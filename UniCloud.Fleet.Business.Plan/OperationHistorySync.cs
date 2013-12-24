using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.Business.Plan
{
    public enum OperationType
    {

        OperationLastYearPlan,//查找上个年度的计划
        OperationNextYearPlan,//查找比当前年度大的计划
    }

    public static class OperationHistorySync
    {
        /// <summary>
        /// 通过批文历史获取运营时间
        /// </summary>
        /// <param name="ApprovalHistoryID"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static DateTime? GetOperationDateByApprovalHistory(Guid? ApprovalHistoryID, FleetEntities dbContext)
        {
            var OperationHistory = dbContext.OperationHistories.Where(p => p.OperationHistoryID == ApprovalHistoryID).FirstOrDefault();
            if (OperationHistory != null)
            {
                return OperationHistory.StartDate;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///同步下一年度计划 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="currentPlanHisotry"></param>
        /// <param name="currentOpenAnnual"></param>
        private static void SynOperationNextYearPlan(FleetEntities dbContext, PlanHistory currentPlanHisotry, Annual currentOpenAnnual)
        {
            //找到大于当前年度的计划，补上当前计划运营历史或者商业数据
            //大于当前有效计划的集合
            var exceedAnnual = dbContext.Annuals.Where(p => p.Year > currentOpenAnnual.Year);
            //需要同步的计划历史
            var searchNextPlanHistories = from t in dbContext.Plans
                                          from a in dbContext.PlanHistories
                                          from r in dbContext.PlanAircrafts
                                          from f in exceedAnnual
                                          where
                                              t.AnnualID == f.AnnualID &&
                                              t.PlanID == a.PlanID
                                              && r.PlanAircraftID == a.PlanAircraftID &&
                                              r.PlanAircraftID == currentPlanHisotry.PlanAircraftID
                                          select a;


            searchNextPlanHistories.ToList().ForEach
                (
                    f =>
                    {
                        if (currentPlanHisotry.GetType() == typeof(OperationPlan) && f.GetType() == typeof(OperationPlan) && (f as OperationPlan).OperationHistory == null)
                        {
                            var operationPlan = f as OperationPlan;
                            if (operationPlan != null)
                            {
                                var plan = currentPlanHisotry as OperationPlan;
                                if (plan != null)
                                    operationPlan.OperationHistoryID = plan.OperationHistoryID;
                            }
                        }
                        if (currentPlanHisotry.GetType() != typeof(ChangePlan) ||
                            f.GetType() != typeof(ChangePlan) || (f as ChangePlan).AircraftBusiness != null)
                            return;
                        var changePlan = f as ChangePlan;
                        if (changePlan != null)
                        {
                            var plan = currentPlanHisotry as ChangePlan;
                            if (plan != null)
                                changePlan.AircraftBusinessID = plan.AircraftBusinessID;
                        }
                    }
                );
        }

        /// <summary>
        /// 同步上一年度计划
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="currentPlanHisotry"></param>
        /// <param name="StartDate"></param>
        /// <param name="currentOpenAnnual"></param>
        private static void SynOperationLastYearPlan(FleetEntities dbContext, PlanHistory currentPlanHisotry, DateTime StartDate, Annual currentOpenAnnual)
        {
            //找到上个年度的有效计划，补上当前计划运营历史或者商业数据
            //上个年度有效的打开的年度
            var lastOpenAnnual = dbContext.Annuals.FirstOrDefault(p => p.Year == currentOpenAnnual.Year - 1);
            //需要同步的计划历史
            var needSyncPlanHisotry = from t in dbContext.Plans
                                      from a in dbContext.PlanHistories
                                      from r in dbContext.PlanAircrafts
                                      where
                                          t.IsValid == true && t.AnnualID == lastOpenAnnual.AnnualID &&
                                          t.PlanID == a.PlanID
                                          && r.PlanAircraftID == a.PlanAircraftID &&
                                          r.PlanAircraftID == currentPlanHisotry.PlanAircraftID
                                      select a;
            var searchPlanHistory = needSyncPlanHisotry.FirstOrDefault();
            if (searchPlanHistory != null)
            {
                //如果运营日期在上一执行年度，则需要反向同步
                if (lastOpenAnnual != null && StartDate.Year <= lastOpenAnnual.Year)
                {
                    if (currentPlanHisotry != null && (currentPlanHisotry.GetType() == typeof(OperationPlan) && searchPlanHistory.GetType() == typeof(OperationPlan) && (searchPlanHistory as OperationPlan).OperationHistory == null))
                    {
                        var operationPlan = searchPlanHistory as OperationPlan;
                        if (operationPlan != null)
                            operationPlan.OperationHistoryID = (currentPlanHisotry as OperationPlan).OperationHistoryID;
                    }
                    if (currentPlanHisotry.GetType() == typeof(ChangePlan) && searchPlanHistory.GetType() == typeof(ChangePlan) && (searchPlanHistory as ChangePlan).AircraftBusiness == null)
                    {
                        var changePlan = searchPlanHistory as ChangePlan;
                        if (changePlan != null)
                            changePlan.AircraftBusinessID = (currentPlanHisotry as ChangePlan).AircraftBusinessID;
                    }
                }
                else //如果不需要反向同步的需要,要将可能由于运营时间得改动,有做过反向同步的给取消掉
                {
                    if (currentPlanHisotry.GetType() == typeof(OperationPlan) && searchPlanHistory.GetType() == typeof(OperationPlan) && (searchPlanHistory as OperationPlan).OperationHistory != null)
                    {
                        var operationPlan = searchPlanHistory as OperationPlan;
                        if (operationPlan != null)
                            operationPlan.OperationHistoryID = null;
                    }
                    if (currentPlanHisotry.GetType() == typeof(ChangePlan) && searchPlanHistory.GetType() == typeof(ChangePlan) && (searchPlanHistory as ChangePlan).AircraftBusiness != null)
                    {
                        var changePlan = searchPlanHistory as ChangePlan;
                        if (changePlan != null)
                            changePlan.AircraftBusinessID = null;
                    }
                }
            }
        }

        /// <summary>
        /// 同步计划的运营历史操作
        /// </summary>
        /// <param name="StartDate">开始时间</param>
        /// <param name="planHistoryID">计划明细ID</param>
        /// <param name="tag">标志</param>
        /// <param name="dbContext">域上下文</param>
        public static void SyncOperationHistroy(FleetEntities dbContext, Guid planHistoryID, OperationType tag)
        {
            //当前计划历史
            var currentPlanHisotry = dbContext.PlanHistories.FirstOrDefault(p => p.PlanHistoryID == planHistoryID);
            if (currentPlanHisotry == null || currentPlanHisotry.ApprovalHistoryID == null) return;
            //运行日期
            DateTime? StartDate = GetOperationDateByApprovalHistory(currentPlanHisotry.ApprovalHistoryID, dbContext);
            if (StartDate == null) return;
            //当前有效的年度
            var currentOpenAnnual = (from t in dbContext.Plans.Include("Annuals")
                                     from r in dbContext.PlanHistories
                                     where
                                         t.PlanID == r.PlanID &&
                                         r.PlanHistoryID == currentPlanHisotry.PlanHistoryID
                                     select t.Annual).FirstOrDefault();
            if (currentOpenAnnual == null) return;
            switch (tag)
            {
                case OperationType.OperationLastYearPlan:
                    SynOperationLastYearPlan(dbContext, currentPlanHisotry, (DateTime)StartDate, currentOpenAnnual);
                    break;
                case OperationType.OperationNextYearPlan:
                    SynOperationNextYearPlan(dbContext, currentPlanHisotry, currentOpenAnnual);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 同步计划的运营历史操作
        /// </summary>
        /// <param name="dbContext">域上下文</param>
        /// <param name="planHistoryID">计划明细ID</param>
        public static void SyncOperationHistroy(FleetEntities dbContext, Guid planHistoryID)
        {
            //当前计划历史
            var currentPlanHisotry = dbContext.PlanHistories.FirstOrDefault(p => p.PlanHistoryID == planHistoryID);
            if (currentPlanHisotry == null || currentPlanHisotry.ApprovalHistoryID == null) return;
            //运行日期
            DateTime? StartDate = GetOperationDateByApprovalHistory(currentPlanHisotry.ApprovalHistoryID, dbContext);
            if (StartDate == null) return;
            //当前有效的年度
            var currentOpenAnnual = (from t in dbContext.Plans.Include("Annuals")
                                     from r in dbContext.PlanHistories
                                     where
                                         t.PlanID == r.PlanID &&
                                         r.PlanHistoryID == currentPlanHisotry.PlanHistoryID
                                     select t.Annual).FirstOrDefault();
            if (currentOpenAnnual == null) return;
            //同步上一年度计划
            SynOperationLastYearPlan(dbContext, currentPlanHisotry, (DateTime)StartDate, currentOpenAnnual);
            //同步下一年度计划
            SynOperationNextYearPlan(dbContext, currentPlanHisotry, currentOpenAnnual);
        }
    }

}

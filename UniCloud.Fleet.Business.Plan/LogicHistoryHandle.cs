using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniCloud.Fleet.Models;
namespace UniCloud.Fleet.Business.Plan
{
    /// <summary>
    /// 飞机历史数据处理
    /// </summary>
    public static class LogicHistoryHandle
    {
        /// <summary>
        /// 判断是否要发送当前所有权历史的前一条记录
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentOwnershipHistory"></param>
        /// <returns></returns>
        public static OwnershipHistory GetOwnershipHistory(Aircraft aircraft, OwnershipHistory currentOwnershipHistory, FleetEntities dbContext)
        {
            if (aircraft == null || aircraft.OwnershipHistorys.Count <= 1)
            {
                return null;
            }
            else
            {
                //当前飞机所有权人历史的前一条记录
                var oh = dbContext.OwnershipHistories.Where(
                        o => o.OwnershipHistoryID != currentOwnershipHistory.OwnershipHistoryID && o.AircraftID == aircraft.AircraftID).OrderByDescending(
                            o => o.StartDate).FirstOrDefault();
                if (oh != null && currentOwnershipHistory.StartDate != null)
                {
                    return oh;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 判断是否要发送当前商业数据的前一条记录
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentAircraftBusiness"></param>
        /// <returns></returns>
        public static AircraftBusiness GetAircraftBusiness(Aircraft aircraft, AircraftBusiness currentAircraftBusiness, FleetEntities dbContext)
        {
            if (aircraft == null || aircraft.AircraftBusinesses.Count <= 1)
            {
                return null;
            }
            else
            {
                //当前商业数据的前一条记录
                var ab = dbContext.AircraftBusinesses.Where(
                        o => o.AircraftBusinessID != currentAircraftBusiness.AircraftBusinessID && o.AircraftID == aircraft.AircraftID).OrderByDescending(
                            o => o.StartDate).FirstOrDefault();

                if (ab != null && currentAircraftBusiness.StartDate != null)
                {
                    return ab;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 判断是否要发送当前运营历史的前一条记录
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentOperationHistory"></param>
        /// <returns></returns>
        public static OperationHistory GetOperationHistory(Aircraft aircraft, OperationHistory currentOperationHistory, FleetEntities dbContext)
        {
            if (aircraft == null || aircraft.OperationHistories.Count <= 1)
            {
                return null;
            }
            else
            {
                //当前运营历史的前一条记录
                var oh = dbContext.OperationHistories.Where(
                        o => o.OperationHistoryID != currentOperationHistory.OperationHistoryID && o.AircraftID == aircraft.AircraftID).OrderByDescending(
                            o => o.StartDate).FirstOrDefault();
                if (oh != null && currentOperationHistory.StartDate != null)
                {
                    return oh;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 设置前一条的所有权历史的结束时间为当前所有权历史的开始时间减去一天
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentOwnershipHistory"></param>
        public static void SetPreviousOwershipHistory(Aircraft aircraft, OwnershipHistory currentOwnershipHistory, FleetEntities dbContext)
        {
            if (currentOwnershipHistory != null && currentOwnershipHistory.StartDate != null)
            {
                //获取前一条所有权历史
                var formerOwershipHistory = GetOwnershipHistory(aircraft, currentOwnershipHistory, dbContext);
                if (formerOwershipHistory == null) return;
                //设置前一条所有权历史的结束时间为当前所有权历史的开始时间减去一天
                formerOwershipHistory.EndDate = currentOwnershipHistory.StartDate.Value.AddDays(-1);
            }
        }

        /// <summary>
        /// 设置前一条的商业数据的结束时间为当前商业数据的开始时间减去一天
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentAircraftBusiness"></param>
        public static void SetPreviousrAircraftBusiness(Aircraft aircraft, AircraftBusiness currentAircraftBusiness, FleetEntities dbContext)
        {
            if (currentAircraftBusiness != null && currentAircraftBusiness.StartDate != null)
            {
                //获取前一条商业数据
                var formerAircraftBusiness = GetAircraftBusiness(aircraft, currentAircraftBusiness, dbContext);
                if (formerAircraftBusiness == null) return;
                //设置前一条商业数据的结束时间为当前商业数据的开始时间减去一天
                formerAircraftBusiness.EndDate = currentAircraftBusiness.StartDate.Value.AddDays(-1);
            }
        }

        /// <summary>
        /// 设置前一条的商业数据的结束时间为当前商业数据的开始时间减去一天
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="currentOperationHistory"> </param>
        public static void SetPreviousOperationHistory(Aircraft aircraft, OperationHistory currentOperationHistory, FleetEntities dbContext)
        {
            if (currentOperationHistory != null && currentOperationHistory.StartDate != null)
            {
                //获取前一条商业数据
                var formerOperationHistory = GetOperationHistory(aircraft, currentOperationHistory, dbContext);
                if (formerOperationHistory == null) return;
                //设置前一条商业数据的结束时间为当前商业数据的开始时间减去一天
                formerOperationHistory.EndDate = currentOperationHistory.StartDate.Value.AddDays(-1);
            }
        }
        /// <summary>
        /// 同步申请明细到计划历史
        /// </summary>
        /// <param name="planHistoryID">需要操作计划明细ID</param>
        /// <param name="dbContext">域上下文</param>
        public static void SyncApprovalToPlanHisotry(Guid planHistoryID, FleetEntities dbContext)
        {
            //对上个年度的发布计划进行同步申请的操作
            //当前计划历史
            var currentPlanHisotry =
                dbContext.PlanHistories.FirstOrDefault(p => p.PlanHistoryID == planHistoryID);
            //当前有效的年度
            var currentOpenAnnual = (from t in dbContext.Plans.Include("Annuals")
                                     from r in dbContext.PlanHistories
                                     where
                                         t.PlanID == r.PlanID &&
                                         r.PlanHistoryID == currentPlanHisotry.PlanHistoryID
                                     select t.Annual).FirstOrDefault();
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
            if (searchPlanHistory != null && currentPlanHisotry != null)
            {
                searchPlanHistory.ApprovalHistoryID = currentPlanHisotry.ApprovalHistoryID;
            }
        }
    }
}


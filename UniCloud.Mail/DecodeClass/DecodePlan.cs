using System.Collections.Generic;
using System.Linq;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Business.Plan;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodePlan : DecodeObject
    {
        public DecodePlan()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.Plan";
            _TypeName = "Plan";
        }
        #endregion

        #region 分解计划对象

        public override void Decode(object obj)
        {
            //接收的计划
            Plan NewPlan = (Plan)obj;
            //获取原计划
            Plan OriginPlan = this._FE.Plans.Where(q => q.PlanID == NewPlan.PlanID).FirstOrDefault();
            //设置接受计划的当前版本为True
            NewPlan.IsCurrentVersion = true;
            //获取该计划的年度
            Annual currentAnnual = this._FE.Annuals.FirstOrDefault(p => p.AnnualID == NewPlan.AnnualID);
            //更新年度
            UpdateAnnual(currentAnnual);
            //设置当前版本
            SetPlanIsCurrentVersion(NewPlan);
            //处理航空公司
            UpdateAirlinesForPlan(NewPlan);
            //同步计划数据
            SyncOperationHistroy(NewPlan);
            //计划已经存在
            if (OriginPlan != null)
            {
                //删除计划历史
                DeletePlanHistories(NewPlan, OriginPlan);
                //更新计划历史
                UpdatePlanHistories(NewPlan, OriginPlan);
                //计划明细自己保存
                NewPlan.PlanHistories = null;
                //更新计划
                this.Update<Plan>(_FE, OriginPlan, NewPlan);
            }
            else
            {
                //更新计划历史
                AddPlanHistories(NewPlan);
                //计划明细自己保存
                NewPlan.PlanHistories = null;
                //增加计划
                this._FE.Plans.Add(NewPlan);
            }
        }

        private void SyncOperationHistroy(Plan NewPlan)
        {
            if (NewPlan == null) return;
            foreach (PlanHistory NewPlanHistory in NewPlan.PlanHistories)
            {
                try
                {
                    OperationHistorySync.SyncOperationHistroy(_FE, NewPlanHistory.PlanHistoryID);
                }
                catch
                {
                }
            }
        }

        private void UpdateAirlinesForPlan(Plan NewPlan)
        {
            if (!this._FE.Owners.Any(p => p.OwnerID == NewPlan.AirlinesID))
            {
                //航空公司不存在
                if (NewPlan.Airlines != null)
                {
                    NewPlan.Airlines.OperationHistories = null;
                    NewPlan.Airlines.OwnershipHistorys = null;
                    NewPlan.Airlines.Requests = null;
                    NewPlan.Airlines.SubOperationHistories = null;
                    NewPlan.Airlines.Plans = null;
                    NewPlan.Airlines.SubAirlines = null;
                    NewPlan.Airlines.MailAddresses = null;
                    this._FE.Owners.Add(NewPlan.Airlines);
                }
            }
            //处理航空公司
            NewPlan.Airlines = null;
        }


        private void UpdatePlanHistoryRelativeHistory(PlanHistory NewPlanHistory)
        {
            //处理批文历史
            UpdateApprovalHistoryForPlanHistory(NewPlanHistory);
            //处理运营计划历史
            UpdateOperationHistoryForPlanHistory(NewPlanHistory);
            //处理变更计划历史
            UpdateChangeHistoryForPlanHistory(NewPlanHistory);
        }

        private void UpdateApprovalHistoryForPlanHistory(PlanHistory NewPlanHistory)
        {
            //处理批文历史
            if (NewPlanHistory.ApprovalHistoryID != null && !this._FE.ApprovalHistories.Any(p => p.ApprovalHistoryID == NewPlanHistory.ApprovalHistoryID))
            {
                NewPlanHistory.ApprovalHistoryID = null;
            }
            NewPlanHistory.ApprovalHistory = null;
        }

        private void UpdateOperationHistoryForPlanHistory(PlanHistory NewPlanHistory)
        {
            //处理运营计划历史
            if (NewPlanHistory is OperationPlan)
            {
                OperationPlan OperationPlan = NewPlanHistory as OperationPlan;
                if (OperationPlan.OperationHistoryID != null && !this._FE.OperationHistories.Any(p => p.OperationHistoryID == OperationPlan.OperationHistoryID))
                {
                    OperationPlan.OperationHistoryID = null;
                }
                OperationPlan.OperationHistory = null;
            }
        }

        private void UpdateChangeHistoryForPlanHistory(PlanHistory NewPlanHistory)
        {
            //处理变更计划历史
            if (NewPlanHistory is ChangePlan)
            {
                ChangePlan ChangePlan = NewPlanHistory as ChangePlan;
                if (ChangePlan.AircraftBusinessID != null && !this._FE.AircraftBusinesses.Any(p => p.AircraftBusinessID == ChangePlan.AircraftBusinessID))
                {
                    ChangePlan.AircraftBusinessID = null;
                }
                ChangePlan.AircraftBusiness = null;
            }
        }

        //增加计划明细
        private void AddPlanHistories(Plan NewPlan)
        {
            for (int i = NewPlan.PlanHistories.Count() - 1; i >= 0; i--)
            {
                var NewPlanHistory = NewPlan.PlanHistories.ElementAt(i);
                if (!NewPlanHistory.IsSubmit) continue;
                //处理航空公司
                NewPlanHistory.Airlines = null;
                //更新计划飞机
                UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                NewPlanHistory.PlanAircraft = null;
                //处理计划历史相关历史数据
                UpdatePlanHistoryRelativeHistory(NewPlanHistory);
                //处理计划明细
                PlanHistory OriginPlanHistory = this._FE.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
                if (OriginPlanHistory != null)
                {
                    //更新计划明细
                    NewPlanHistory.Plan = null;
                    this.Update<PlanHistory>(_FE, OriginPlanHistory, NewPlanHistory);
                }
                else
                {
                    //增加计划明细
                    NewPlanHistory.Plan = null;
                    this._FE.PlanHistories.Add(NewPlanHistory);
                }
            }
        }

        //更新计划明细
        private void UpdatePlanHistories(Plan NewPlan, Plan OriginPlan)
        {
            for (int i = NewPlan.PlanHistories.Count() - 1; i >= 0; i--)
            {
                var NewPlanHistory = NewPlan.PlanHistories.ElementAt(i);
                if (!NewPlanHistory.IsSubmit) continue;
                //处理航空公司
                NewPlanHistory.Airlines = null;
                //更新计划飞机
                UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                NewPlanHistory.PlanAircraft = null;
                //处理计划历史相关数据
                UpdatePlanHistoryRelativeHistory(NewPlanHistory);
                //处理计划明细
                PlanHistory OriginPlanHistory = OriginPlan.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
                if (OriginPlanHistory != null)
                {
                    //更新计划明细
                    NewPlanHistory.Plan = null;
                    this.Update<PlanHistory>(_FE, OriginPlanHistory, NewPlanHistory);
                }
                else
                {
                    //增加计划明细
                    NewPlanHistory.Plan = null;
                    this._FE.PlanHistories.Add(NewPlanHistory);
                }
            }
        }

        //删除计划历史明细
        private void DeletePlanHistories(Plan NewPlan, Plan OriginPlan)
        {
            if (OriginPlan == null) return;
            //获取原申请明细
            ICollection<PlanHistory> OriginPlanHistories = this._FE.PlanHistories.Where(p => p.PlanID == OriginPlan.PlanID).ToList();
            //对原申请明细进行处理， 不在新申请明细中的需要删除
            for (int i = OriginPlanHistories.Count() - 1; i >= 0; i--)
            {
                var OriginPlanHistory = OriginPlanHistories.ElementAt(i);

                if (!NewPlan.PlanHistories.Any(p => p.PlanHistoryID == OriginPlanHistory.PlanHistoryID))
                {
                    //断开与该计划明细的关系 TODO
                    DeletePlanHistoryRelation(OriginPlanHistory);
                    //删除该申请明细
                    this._FE.PlanHistories.Remove(OriginPlanHistory);
                }
            }
        }

        private void DeletePlanHistoryRelation(PlanHistory OriginPlanHistory)
        {
            //if (OriginPlanHistory != null)
            //{
            //    //计划飞机处理
            //    if (OriginPlanHistory.PlanAircraftID != null)
            //    {
            //    }
            //}
        }

        /// <summary>
        /// 更新年度
        /// </summary>
        /// <param name="annual">计划的当前年度</param>
        private void UpdateAnnual(Annual annual)
        {
            if (annual != null)
            {
                //设置当前年度为打开的年度，其他年度为不能打开的年度
                if (annual != null && annual.IsOpen != false)
                {
                    annual.IsOpen = true;
                    //更新其他年度的年度为不可打开
                    if (this._FE.Annuals.Any(p => p.IsOpen == true && p.AnnualID != annual.AnnualID))
                    {
                        //找到存在该条件的计划，设置可打开的属性为false
                        this._FE.Annuals.Where(p => p.IsOpen == true && p.AnnualID != annual.AnnualID).ToList().ForEach
                            (
                              ann =>
                              {
                                  ann.IsOpen = false;
                                  this.Update<Annual>(_FE, ann);
                              }
                            );
                    }
                    this.Update<Annual>(_FE, annual);
                }
            }
        }

        /// <summary>
        /// 设置计划是否为当前值
        /// </summary>
        /// <param name="planNew"></param>
        private void SetPlanIsCurrentVersion(Plan planNew)
        {
            // 获取需要修改的计划
            IEnumerable<Plan> allPlan = this._FE.Plans.Where(q => q.AnnualID == planNew.AnnualID && q.AirlinesID == planNew.AirlinesID && q.PlanID != planNew.PlanID);
            if (allPlan.Count() > 0)
            {
                //当前年度当前航空公司已存在计划
                if (allPlan.Any(p => p.VersionNumber > planNew.VersionNumber))
                {
                    //当前解析的计划版本如果不是最新的，这个计划的IsCurrentVersion为false
                    planNew.IsCurrentVersion = false;
                }
                else  //新发送的计划版本为当前年度最新的
                {
                    //遍历计划，使以前的版本都变成false，新的计划IsCurrentVersion为true
                    allPlan.ToList().ForEach
                        (
                        ap =>
                        {
                            ap.IsCurrentVersion = false;
                            this.Update<Plan>(_FE, ap);
                        }
                        );
                }
            }
        }


        #endregion
    }
}

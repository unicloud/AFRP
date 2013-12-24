using System;
using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeChangePlan : DecodeObject
    {
        public DecodeChangePlan()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.ChangePlan";
            _TypeName = "ChangePlan";
        }
        #endregion

        #region 解析飞机对象

        public override void Decode(object obj)
        {
            ChangePlan NewChangePlan = (ChangePlan)obj;
            //获取原计划历史
            PlanHistory OriginPlanHistory = this._FE.PlanHistories.Where(q => q.PlanHistoryID == NewChangePlan.PlanHistoryID).FirstOrDefault();

            //更新计划 TODO
            UpdatePlan(NewChangePlan);
            NewChangePlan.Plan = null;

            NewChangePlan.PlanAircraft = null;
            NewChangePlan.ApprovalHistory = null;

            //处理商业数据
            UpdateAircraftBusiness(NewChangePlan);
            NewChangePlan.AircraftBusiness = null;

            //计划历史已经存在
            if (OriginPlanHistory != null)
            {
                //更新计划历史
                this.Update<PlanHistory>(_FE, OriginPlanHistory, NewChangePlan);
            }
            else
            {
                //增加计划历史
                this._FE.PlanHistories.Add(NewChangePlan);
            }
        }

        /// <summary>
        /// 更新商业数据历史
        /// </summary>
        /// <param name="NewChangePlan"></param>
        private void UpdateAircraftBusiness(ChangePlan NewChangePlan)
        {
            if (NewChangePlan.AircraftBusiness != null)
            {
                DecodeAircraftBusiness IDO = new DecodeAircraftBusiness();
                IDO.SetDbContext(this._FE);
                IDO.Decode(NewChangePlan.AircraftBusiness);
            }
        }


        /// <summary>
        /// 更新计划
        /// </summary>
        /// <param name="NewPlanHistory"></param>
        private void UpdatePlan(PlanHistory NewPlanHistory)
        {
            if (NewPlanHistory.PlanID != null)
            {
                Plan OriginPlan = this._FE.Plans.Where(q => q.PlanID == NewPlanHistory.PlanID).FirstOrDefault();
                if (OriginPlan == null)
                {
                    Plan NewPlan = new Plan();
                    NewPlan.PlanID = NewPlanHistory.PlanID;
                    NewPlan.AirlinesID = NewPlanHistory.AirlinesID;
                    if (NewPlanHistory.Annual != null)
                    {
                        NewPlan.AnnualID = NewPlanHistory.Annual.AnnualID;
                    }
                    else
                    {
                        Annual annual = this._FE.Annuals.Where(p=>p.Year== DateTime.Today.Year).FirstOrDefault();
                        NewPlan.AnnualID = annual.AnnualID;
                    }
                    //增加计划历史
                    this._FE.Plans.Add(NewPlan);
                }
            }
        }

        #endregion

    }
}

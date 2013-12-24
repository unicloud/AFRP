using System;
using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeOperationPlan : DecodeObject
    {
        public DecodeOperationPlan()
            : base()
        {
        }


        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.OperationPlan";
            _TypeName = "OperationPlan";
        }
        #endregion

        #region 解析飞机对象


        public override void Decode(object obj)
        {
            OperationPlan NewPlanHistory = (OperationPlan)obj;
            //处理航空公司
            NewPlanHistory.Airlines = null;
            //更新计划 TODO
            UpdatePlan(NewPlanHistory);
            NewPlanHistory.Plan = null; 
            
            NewPlanHistory.PlanAircraft = null;
            NewPlanHistory.ApprovalHistory = null;

            //处理运营权历史
            UpdateOperationHistory(NewPlanHistory);
            NewPlanHistory.OperationHistory = null;

            //获取原计划历史
            PlanHistory OriginPlanHistory = this._FE.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
            //计划历史已经存在
            if (OriginPlanHistory != null)
            {
                //更新计划历史
                this.Update<PlanHistory>(_FE, OriginPlanHistory, NewPlanHistory);
            }
            else
            {
                //增加计划历史
                this._FE.PlanHistories.Add(NewPlanHistory);
            }
        }

        /// <summary>
        /// 更新运营权历史
        /// </summary>
        /// <param name="NewOperationPlan"></param>
        private void UpdateOperationHistory(OperationPlan NewOperationPlan)
        {
            if (NewOperationPlan.OperationHistory != null)
            {
                DecodeOperationHistory IDO = new DecodeOperationHistory();
                IDO.SetDbContext(this._FE);
                IDO.Decode(NewOperationPlan.OperationHistory);
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
                        Annual annual = this._FE.Annuals.Where(p => p.Year == DateTime.Today.Year).FirstOrDefault();
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

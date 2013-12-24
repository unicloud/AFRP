using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodePlanHistory : DecodeObject
    {
        public DecodePlanHistory()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.PlanHistory";
            _TypeName = "PlanHistory";
        }
        #endregion

        #region 解析飞机对象

        public override void Decode(object obj)
        {
            PlanHistory NewPlanHistory = (PlanHistory)obj;
            //处理航空公司
            NewPlanHistory.Airlines = null;
            //获取原飞机
            PlanHistory OriginPlanHistory = this._FE.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
            //计划历史已经存在
            if (OriginPlanHistory != null)
            {
                //更新计划 TODO
                // UpdatePlan(NewAircraft, OriginAircraft);
                NewPlanHistory.PlanAircraft = null;
                NewPlanHistory.Plan = null;
                //更新计划历史
                this.Update<PlanHistory>(_FE, OriginPlanHistory, NewPlanHistory);
            }
            else
            {
                //更新计划 TODO
                // UpdatePlan(NewAircraft, OriginAircraft);
                NewPlanHistory.PlanAircraft = null;
                NewPlanHistory.Plan = null;
                //增加计划历史
                this._FE.PlanHistories.Add(NewPlanHistory);
            }
        }

        #endregion

    }
}

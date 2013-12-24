using System.ServiceModel.DomainServices.Client;
using System.Collections.Generic;
using System.Linq;

namespace UniCloud.Fleet.Models
{
    public sealed partial class Plan : Entity
    {
        /// <summary>
        /// 获取计划相关数据提交状态
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public PlanRelativeSubmitState GetPlanRelativeDataSubmitState()
        {
            PlanRelativeSubmitState psState = PlanRelativeSubmitState.All;

            foreach (PlanHistory ph in this.PlanHistories)
            {
                //处理飞机数据
                psState = ph.GetAircraftSubmitState();
                if (psState != PlanRelativeSubmitState.All) break;
                //处理申请
                psState = ph.GetRequestSubmitState();
                if (psState != PlanRelativeSubmitState.All) break;
            }
            return psState;
        }
    }
}

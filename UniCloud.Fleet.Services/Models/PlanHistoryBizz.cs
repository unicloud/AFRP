using System.ServiceModel.DomainServices.Client;
using System.Collections.Generic;
using System.Linq;

namespace UniCloud.Fleet.Models
{
     public abstract partial class PlanHistory : Entity
    {
        /// <summary>
        /// 获取计划历史对应飞机数据的提交状态
        /// </summary>
        /// <param name="ph"></param>
        /// <returns></returns>
        public PlanRelativeSubmitState GetAircraftSubmitState()
        {
            //处理飞机数据
            if (this.PlanAircraft!=null && this.PlanAircraft.Aircraft != null)
            {
                //运营权历史没有提交
                IEnumerable<OperationHistory> OperationHistories = this.PlanAircraft.Aircraft.OperationHistories;
                if (OperationHistories != null && OperationHistories.Count(q => q.Status < 3) > 0)
                {
                    return PlanRelativeSubmitState.OperationHistoryNot;
                }
                //商业数据历史没有提交
                IEnumerable<AircraftBusiness> AircraftBusinesses = this.PlanAircraft.Aircraft.AircraftBusinesses;
                if (AircraftBusinesses != null && AircraftBusinesses.Count(q => q.Status < 3) > 0)
                {
                    return PlanRelativeSubmitState.BusinessHistoryNot;
                }
            }
            return PlanRelativeSubmitState.All;
        }

         /// <summary>
         /// 获取飞机数据提交状态
         /// </summary>
         /// <returns></returns>
        public string GetAircraftSubmitStateString()
        {
            PlanRelativeSubmitState psState = GetAircraftSubmitState();
            switch (psState)
            {
                case PlanRelativeSubmitState.BusinessHistoryNot:
                case PlanRelativeSubmitState.OperationHistoryNot:
                    return "未提交";
                default:
                    return "正常";
            }
        }


        /// <summary>
        /// 获取计划历史对应申请的提交状态
        /// </summary>
        /// <param name="ph"></param>
        /// <returns></returns>
        public PlanRelativeSubmitState GetRequestSubmitState()
        {
            //处理申请
            if (this.ApprovalHistory != null && this.ApprovalHistory.Request != null)
            {
                //申请还没提交
                if (this.ApprovalHistory.Request.Status < 3)
                {
                    return PlanRelativeSubmitState.RequestNot;
                }
            }
            return PlanRelativeSubmitState.All;
        }

    }
}

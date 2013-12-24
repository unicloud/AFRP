using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.Business.Plan
{
    public  static class ApprovalHistorySync
    {
         /// <summary>
        /// 清除申请明细和所有对应的计划明细之间的关系
         /// </summary>
         /// <param name="planHistoryID">需要操作计划明细ID</param>
         /// <param name="dbContext">域上下文</param>
        public static void RemoveApprovalRelation(Guid approvalHisotryID, FleetEntities dbContext)
        {
            //需要清除关系的计划明细
            var planhistories =
                dbContext.PlanHistories.Where(p => p.ApprovalHistoryID == approvalHisotryID);
           planhistories.ToList().ForEach(p =>
                {
                   p.ApprovalHistoryID = null;
                });
             var currentApprovalHisotry = dbContext.ApprovalHistories.FirstOrDefault(p => p.ApprovalHistoryID ==approvalHisotryID);
             dbContext.ApprovalHistories.Remove(currentApprovalHisotry);
        }
    }
}

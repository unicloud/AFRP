using System.Collections.Generic;
using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeApprovalDoc : DecodeObject
    {
        public DecodeApprovalDoc()
            : base()
        {
        }


        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.ApprovalDoc";
            _TypeName = "ApprovalDoc";
        }
        #endregion

        #region 分解批文对象

        public override void Decode(object obj)
        {
            ApprovalDoc NewApprovalDoc = (ApprovalDoc)obj;
            //获取原批文
            ApprovalDoc OriginApprovalDoc = this._FE.ApprovalDocs.Where(q => q.ApprovalDocID == NewApprovalDoc.ApprovalDocID).FirstOrDefault();
            //批文已经存在
            if (OriginApprovalDoc != null)
            {
                //更新批文明细
                UpdateApprovalDocHistories(NewApprovalDoc, OriginApprovalDoc);
                //明细已单独保存
                NewApprovalDoc.Requests = null;
                //更新批文
                this.Update<ApprovalDoc>(_FE, OriginApprovalDoc, NewApprovalDoc);
            }
            else
            {
                //增加批文明细
                AddApprovalDocHistories(NewApprovalDoc);
                //明细已单独保存
                NewApprovalDoc.Requests = null;
                //增加批文
                this._FE.ApprovalDocs.Add(NewApprovalDoc);
            }
        }

        //增加批文明细
        private void AddApprovalDocHistories(ApprovalDoc NewApprovalDoc)
        {
            foreach (var NewRequest in NewApprovalDoc.Requests)
            {
                //处理航空公司
                NewRequest.Airlines = null;
                //处理批文明细
                Request OriginRequest = this._FE.Requests.Where(p => p.RequestID == NewRequest.RequestID).FirstOrDefault();
                if (OriginRequest == null)
                {
                    //申请对应的批文对象需要设置为空,才能调用ADD方法
                    NewRequest.ApprovalDoc = null;
                    //增加申请明细
                    AddRequestHistories(NewRequest);
                    NewRequest.ApprovalHistories = null;
                    //增加申请
                    this._FE.Requests.Add(NewRequest);
                }
                else
                {
                    //删除申请明细
                    DeleteRequestHistories(NewRequest, OriginRequest);
                    //申请对应的批文对象需要设置为空,才能调用ADD方法
                    NewRequest.ApprovalDoc = null;
                    //更新申请明细
                    UpdateRequestHistories(NewRequest, OriginRequest);
                    NewRequest.ApprovalHistories = null;
                    //更新申请 
                    this.Update<Request>(_FE, OriginRequest, NewRequest);
                }
            }
        }

        //更新批文明细
        private void UpdateApprovalDocHistories(ApprovalDoc NewApprovalDoc, ApprovalDoc OriginApprovalDoc)
        {
            foreach (var NewRequest in NewApprovalDoc.Requests)
            {
                //处理航空公司
                NewRequest.Airlines = null;
                //处理批文明细
                Request OriginRequest = this._FE.Requests.Where(p => p.RequestID == NewRequest.RequestID).FirstOrDefault();
                if (OriginRequest == null)
                {
                    //申请对应的批文对象需要设置为空,才能调用ADD方法
                    NewRequest.ApprovalDoc = null;
                    //增加申请明细
                    AddRequestHistories(NewRequest);
                    NewRequest.ApprovalHistories = null;
                    // 增加批文明细
                    this._FE.Requests.Add(NewRequest);
                }
                else
                {
                    //删除申请明细
                    DeleteRequestHistories(NewRequest, OriginRequest);
                    //申请对应的批文对象需要设置为空,才能调用ADD方法
                    NewRequest.ApprovalDoc = null;
                    //更新申请明细
                    this.UpdateRequestHistories(NewRequest, OriginRequest);
                    NewRequest.ApprovalHistories = null;
                    //更新批文明细
                    this.Update<Request>(_FE, OriginRequest, NewRequest);
                }
            }
        }

        //增加申请明细
        private void AddRequestHistories(Request NewRequest)
        {
            for (int i = NewRequest.ApprovalHistories.Count() - 1; i >= 0; i--)
            {
                var NewApprovalHistory = NewRequest.ApprovalHistories.ElementAt(i);
                //处理航空公司
                NewApprovalHistory.Airlines = null;
                //更新计划飞机
                UpdatePlanAircraft(NewApprovalHistory.PlanAircraft);
                NewApprovalHistory.PlanAircraft = null;
                // 增加申请对应的批文历史
                ApprovalHistory OriginApprovalHistory = this._FE.ApprovalHistories.Where(p => p.ApprovalHistoryID == NewApprovalHistory.ApprovalHistoryID).FirstOrDefault();
                if (OriginApprovalHistory == null)
                {
                    NewApprovalHistory.Request = null;
                    //增加申请明细
                    this._FE.ApprovalHistories.Add(NewApprovalHistory);
                }
                else
                {
                    NewApprovalHistory.Request = null;
                    //更新申请明细
                    this.Update<ApprovalHistory>(_FE, OriginApprovalHistory, NewApprovalHistory);
                }
                //创建管理的批文
                CreateManaApprovalHistory(NewApprovalHistory);
            }
        }

        //创建管理的批文
        private void CreateManaApprovalHistory(ApprovalHistory NewApprovalHistory)
        {
            if (!this._FE.ManaApprovalHistories.Any(p => p.ApprovalHistoryID == NewApprovalHistory.ApprovalHistoryID))
            {
                ManaApprovalHistory m = new ManaApprovalHistory();
                m.ApprovalHistoryID = NewApprovalHistory.ApprovalHistoryID;
                this._FE.ManaApprovalHistories.Add(m);
            }
        }

        //更新申请明细
        private void UpdateRequestHistories(Request NewRequest, Request OriginRequest)
        {

            for (int i = NewRequest.ApprovalHistories.Count() - 1; i >= 0; i--)
            {
                var NewApprovalHistory = NewRequest.ApprovalHistories.ElementAt(i);
                //处理航空公司
                NewApprovalHistory.Airlines = null;
                //更新计划飞机
                UpdatePlanAircraft(NewApprovalHistory.PlanAircraft);
                NewApprovalHistory.PlanAircraft = null;
                //获取原批文历史
                ApprovalHistory OriginApprovalHistory = this._FE.ApprovalHistories.Where(p => p.ApprovalHistoryID == NewApprovalHistory.ApprovalHistoryID).FirstOrDefault();
                if (OriginApprovalHistory == null)
                {
                    NewApprovalHistory.Request = null;
                    // 增加申请对应的批文历史
                    this._FE.ApprovalHistories.Add(NewApprovalHistory);

                }
                else
                {
                    NewApprovalHistory.Request = null;
                    //更新申请明细
                    this.Update<ApprovalHistory>(_FE, OriginApprovalHistory, NewApprovalHistory);
                }
                //创建管理的批文
                CreateManaApprovalHistory(NewApprovalHistory);
            }
        }

        //删除申请明细
        private void DeleteRequestHistories(Request NewRequest, Request OriginRequest)
        {
            if (OriginRequest == null) return;
            //获取原申请明细
            ICollection<ApprovalHistory> OriginApprovalHistories = this._FE.ApprovalHistories.Where(p => p.RequestID == OriginRequest.RequestID).ToList();
            //对原申请明细进行处理， 不在新申请明细中的需要删除
            foreach (var OriginApprovalHistory in OriginApprovalHistories)
            {
                if (!NewRequest.ApprovalHistories.Any(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID))
                {
                    //断开与该申请明细的关系 TODO
                    DeleteApprovalHistoryRelation(OriginApprovalHistory);
                    //删除该申请明细
                    this._FE.ApprovalHistories.Remove(OriginApprovalHistory);
                }
            }
        }

        private void DeleteApprovalHistoryRelation(ApprovalHistory OriginApprovalHistory)
        {
            if (OriginApprovalHistory != null)
            {
                //更新计划历史运营计划
                IQueryable<PlanHistory> PlanHistories = this._FE.PlanHistories.Where(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (PlanHistory PlanHistory in PlanHistories)
                {
                    PlanHistory.ApprovalHistoryID = null;
                    PlanHistory.ApprovalHistory = null;
                }

                //更新运营计划
                IQueryable <OperationPlan> OperationPlans = this._FE.OperationPlans.Where(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (OperationPlan OperationPlan in OperationPlans)
                {
                    OperationPlan.ApprovalHistoryID = null;
                    OperationPlan.ApprovalHistory = null;
                }

                //删除运行历史
                IQueryable<OperationHistory> OperationHistories = this._FE.OperationHistories.Where(p=>p.OperationHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (OperationHistory OperationHistory in OperationHistories)
                {
                    this._FE.OperationHistories.Remove(OperationHistory);
                }
                //删除管理的批文
                IQueryable<ManaApprovalHistory> ManaOperationHistories = this._FE.ManaApprovalHistories.Where(p=>p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (ManaApprovalHistory ManaOperationHistory in ManaOperationHistories)
                {
                    this._FE.ManaApprovalHistories.Remove(ManaOperationHistory);
                }
                //删除批文历史（申请明细）
                IQueryable<ApprovalHistory> ApprovalHistories = this._FE.ApprovalHistories.Where(p=>p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (ApprovalHistory ApprovalHistory in ApprovalHistories)
                {
                    this._FE.ApprovalHistories.Remove(ApprovalHistory);
                }
            }
        }

        #endregion
    }
}

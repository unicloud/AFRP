using System.Linq;
using System.Collections.Generic;
using UniCloud.Fleet.Models;


namespace UniCloud.Mail.DecodeClass
{
    public class DecodeRequest : DecodeObject
    {
        public DecodeRequest()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.Request";
            _TypeName = "Request";
        }
        #endregion

        #region 分解申请对象

        public override void Decode(object obj)
        {
            Request NewRequest = (Request)obj;
            //处理航空公司
            NewRequest.Airlines = null;
            //获取原申请
            Request OriginRequest = this._FE.Requests.Where(q => q.RequestID == NewRequest.RequestID).FirstOrDefault();
            //申请已经存在
            if (OriginRequest != null)
            {
                //删除申请明细
                DeleteRequestHistories(NewRequest, OriginRequest);
                //更新申请明细
                UpdateRequestHistories(NewRequest, OriginRequest);
                NewRequest.ApprovalHistories = null;
                //更新申请对应的批文
                UpdateRequestApprovalDoc(NewRequest);
                //更新申请
                this.Update<Request>(_FE, OriginRequest, NewRequest);
            }
            else
            {
                //增加申请明细
                AddRequestHistories(NewRequest);
                NewRequest.ApprovalHistories = null;
                //更新申请对应的批文
                UpdateRequestApprovalDoc(NewRequest);
                //增加申请
                this._FE.Requests.Add(NewRequest);
            }
        }

        private void UpdateRequestApprovalDoc(Request NewRequest)
        {
            if (NewRequest == null) return;
            //处理批文ID
            if (NewRequest.ApprovalDocID != null)
            {
                ApprovalDoc ApprovalDoc = this._FE.ApprovalDocs.Where(q => q.ApprovalDocID == NewRequest.ApprovalDocID).FirstOrDefault();
                if (ApprovalDoc == null)
                {
                    NewRequest.ApprovalDocID = null;
                }
            };
            //批文对象不需要
            NewRequest.ApprovalDoc = null;
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
                    //增加申请明细
                    NewApprovalHistory.Request = null;
                    this._FE.ApprovalHistories.Add(NewApprovalHistory);
                }
                else
                {
                    //更新申请明细
                    NewApprovalHistory.Request = null;
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
                //航空公司
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
                    // 更新申请对应的批文历史
                    this.Update<ApprovalHistory>(_FE, OriginApprovalHistory, NewApprovalHistory);
                }
            }
        }

        //删除申请明细
        private void DeleteRequestHistories(Request NewRequest, Request OriginRequest)
        {
            if (OriginRequest == null) return;
            //获取原申请明细
            ICollection<ApprovalHistory> OriginApprovalHistories = this._FE.ApprovalHistories.Where(p => p.RequestID == OriginRequest.RequestID).ToList();
            //对原申请明细进行处理， 不在新申请明细中的需要删除
            for (int i = OriginApprovalHistories.Count() - 1; i >= 0; i--)
            {
                var OriginApprovalHistory = OriginApprovalHistories.ElementAt(i);

                if (!NewRequest.ApprovalHistories.Any(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID))
                {
                    //断开与该申请明细的关系 TODO
                    DeleteApprovalHistoryRelation(OriginApprovalHistory);
                    //删除该申请明细
                    this._FE.ApprovalHistories.Remove(OriginApprovalHistory);
                }
            }
        }

        //断开与该申请明细的关系
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
                IQueryable<OperationPlan> OperationPlans = this._FE.OperationPlans.Where(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (OperationPlan OperationPlan in OperationPlans)
                {
                    OperationPlan.ApprovalHistoryID = null;
                    OperationPlan.ApprovalHistory = null;
                }
                //删除运行历史
                IQueryable<OperationHistory> OperationHistories = this._FE.OperationHistories.Where(p => p.OperationHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (OperationHistory OperationHistory in OperationHistories)
                {
                    this._FE.OperationHistories.Remove(OperationHistory);
                }
                //删除管理的批文
                IQueryable<ManaApprovalHistory> ManaOperationHistories = this._FE.ManaApprovalHistories.Where(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (ManaApprovalHistory ManaOperationHistory in ManaOperationHistories)
                {
                    this._FE.ManaApprovalHistories.Remove(ManaOperationHistory);
                }
                //删除批文历史（申请明细）
                IQueryable<ApprovalHistory> ApprovalHistories = this._FE.ApprovalHistories.Where(p => p.ApprovalHistoryID == OriginApprovalHistory.ApprovalHistoryID);
                foreach (ApprovalHistory ApprovalHistory in ApprovalHistories)
                {
                    this._FE.ApprovalHistories.Remove(ApprovalHistory);
                }
            }
        }

        #endregion
    }
}

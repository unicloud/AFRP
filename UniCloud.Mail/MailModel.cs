using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniCloud.Fleet.Models;
using System.ServiceModel.DomainServices.EntityFramework;

namespace UniCloud.Mail
{

    public class MailModel
    {
        private FleetEntities _FE;
        private bool _DataChanged;

        public MailModel()
        {
            _FE = new FleetEntities();
            _DataChanged = false;
        }

        public Boolean DataChanged
        {
            get { return _DataChanged; }
        }

        #region 保存所有实体

        private bool SubmitChange()
        {
            //保存数据
             if (_DataChanged)
            {
                try
                {
                    this._FE.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public bool SaveObjects(List<object> Objects)
        {
            if (Objects == null) return true;

            foreach (var obj in Objects)
            {
                switch (obj.GetType().ToString())
                {
                    case "UniCloud.Fleet.Models.Plan":
                        {
                            _DataChanged = true;
                            //保存计划对象
                            SavePlanObject(obj);
                            if (!SubmitChange()) { return false; }
                            break;
                        };
                    case "UniCloud.Fleet.Models.Request":
                        {
                            _DataChanged = true;
                            //保存申请对象
                            SaveRequestObject(obj);
                            if (!SubmitChange()) { return false; }
                            break;
                        };
                    case "UniCloud.Fleet.Models.ApprovalDoc":
                        {
                            _DataChanged = true;
                            //保存批文对象
                            SaveApprovalDocObject(obj);
                            if (!SubmitChange()) { return false; }
                            break;
                        }
                    default:
                        {
                            break;
                        };
                };
            }

            return true;
        }
        #endregion

        #region 保存计划对象

        private void SavePlanObject(object obj)
        {
            //接收的计划
            Plan planNew = (Plan)obj;
            //获取原计划
            Plan OriginPlan = this._FE.Plans.Where(q => q.PlanID == planNew.PlanID).FirstOrDefault();
            //设置接受计划的当前版本为True
            planNew.IsCurrentVersion = true;
            //获取该计划的年度
            Annual currentAnnual = this._FE.Annuals.FirstOrDefault(p => p.AnnualID == planNew.AnnualID);
            //更新年度
            UpdateAnnual(currentAnnual);
            //设置当前年度
            SetPlanIsCurrentVersion(planNew);
            //计划已经存在
            if (OriginPlan != null)
            {
                //更新计划历史
                UpdatePlanHistories(planNew, OriginPlan);
                //更新计划
                this._FE.Update<Plan>(OriginPlan, planNew);
            }
            else
            {
                //增加计划明细
                this.AddPlanHistories(planNew);
                //计划明细自己保存
                planNew.PlanHistories = null;
                //增加计划
                this._FE.Plans.Add(planNew);
            }
        }

        //增加计划明细
        private void AddPlanHistories(Plan NewPlan)
        {
            foreach (var NewPlanHistory in NewPlan.PlanHistories)
            {
                PlanHistory OriginPlanHistory = this._FE.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
                if (OriginPlanHistory != null)
                {
                    //更新计划飞机
                    UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                    //更新计划明细
                    NewPlanHistory.ApprovalHistoryID = null;
                    NewPlanHistory.ApprovalHistory = null;
                    this._FE.Update<PlanHistory>(OriginPlanHistory, NewPlanHistory);
                }
                else
                {
                    //更新计划飞机
                    UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                    //增加计划明细
                    NewPlanHistory.PlanAircraft = null;
                    NewPlanHistory.Plan = null;
                    NewPlanHistory.ApprovalHistory = null;
                    NewPlanHistory.ApprovalHistoryID = null;
                    this._FE.PlanHistories.Add(NewPlanHistory);
                }
            }
        }

        //更新计划明细
        private void UpdatePlanHistories(Plan NewPlan, Plan OriginPlan)
        {
            foreach (var NewPlanHistory in NewPlan.PlanHistories)
            {
                PlanHistory OriginPlanHistory = OriginPlan.PlanHistories.Where(q => q.PlanHistoryID == NewPlanHistory.PlanHistoryID).FirstOrDefault();
                if (OriginPlanHistory != null)
                {
                    //更新计划飞机
                    UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                    //更新计划明细
                    NewPlanHistory.ApprovalHistoryID = null;
                    NewPlanHistory.ApprovalHistory = null;
                    this._FE.Update<PlanHistory>(OriginPlanHistory, NewPlanHistory);
                }
                else
                {
                    //更新计划飞机
                    UpdatePlanAircraft(NewPlanHistory.PlanAircraft);
                    //增加计划明细
                    NewPlanHistory.PlanAircraft = null;
                    NewPlanHistory.Plan = null;
                    NewPlanHistory.ApprovalHistory = null;
                    NewPlanHistory.ApprovalHistoryID = null;
                    this._FE.PlanHistories.Add(NewPlanHistory);
                }
            }
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
                                  this._FE.Update<Annual>(ann);
                              }
                            );
                    }
                    this._FE.Update<Annual>(annual);
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
                //当前年度当前航空公司已存在
                if (allPlan.Any(p => p.ManageFlagCargo == true && p.ManageFlagPnr == true))
                {
                    //存在被民航局已通过的计划，则当前版本不更新为最新的版本
                    planNew.IsCurrentVersion = false;

                }
                else  //不存在民航局审阅的计划
                {
                    //遍及计划，使以前的版本都变成false，新的计划IsCurrentVersion为true
                    allPlan.ToList().ForEach
                        (
                        ap =>
                        {
                            ap.IsCurrentVersion = false;
                            this._FE.Update<Plan>(ap);
                        }
                        );

                }
            }
        }

        #endregion

        #region 保存申请对象


        private void SaveRequestObject(object obj)
        {
            Request NewRequest = (Request)obj;
            //获取原申请
            Request OriginRequest = this._FE.Requests.Where(q => q.RequestID == NewRequest.RequestID).FirstOrDefault();
            //申请已经存在
            if (OriginRequest != null)
            {
                //更新申请明细
                UpdateRequestHistories(NewRequest, OriginRequest);
                NewRequest.ApprovalHistories = null;
                NewRequest.ApprovalDocID = null;
                //更新申请
                this._FE.Update<Request>(OriginRequest, NewRequest);
            }
            else
            {
                //增加申请明细
                AddRequestHistories(NewRequest);
                NewRequest.ApprovalHistories = null;
                //增加申请
                this._FE.Requests.Add(NewRequest);
            }
        }

        //增加申请明细
        private void AddRequestHistories(Request NewRequest)
        {
            foreach (var NewApprovalHistory in NewRequest.ApprovalHistories)
            {
                // 增加申请对应的批文历史
                ApprovalHistory OriginApprovalHistory = this._FE.ApprovalHistories.Where(p => p.ApprovalHistoryID == NewApprovalHistory.ApprovalHistoryID).FirstOrDefault();
                if (OriginApprovalHistory == null)
                {
                    NewApprovalHistory.Request = null;
                    this._FE.ApprovalHistories.Add(NewApprovalHistory);
                    //创建管理的批文
                     ManaApprovalHistory m = new ManaApprovalHistory();
                     m.ApprovalHistoryID = NewApprovalHistory.ApprovalHistoryID;
                     this._FE.ManaApprovalHistories.Add(m);
                }
                else
                {
                    NewApprovalHistory.Request = null;
                    this._FE.Update<ApprovalHistory>(OriginApprovalHistory, NewApprovalHistory);
                }
            }
        }


        //更新申请明细
        private void UpdateRequestHistories(Request NewRequest, Request OriginRequest)
        {
            foreach (var NewApprovalHistory in NewRequest.ApprovalHistories)
            {
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
                    this._FE.Update<ApprovalHistory>(OriginApprovalHistory, NewApprovalHistory);
                }
            }
        }

        #endregion

        #region 保存批文对象

        private void SaveApprovalDocObject(object obj)
        {
            ApprovalDoc NewApprovalDoc = (ApprovalDoc)obj;
            //获取原批文
            ApprovalDoc OriginApprovalDoc = this._FE.ApprovalDocs.Where(q => q.ApprovalDocID == NewApprovalDoc.ApprovalDocID).FirstOrDefault();
            //批文已经存在
            if (OriginApprovalDoc != null)
            {
                //更新批文明细
                UpdateApprovalDocHistories(NewApprovalDoc, OriginApprovalDoc);
                NewApprovalDoc.Requests = null;
                //更新批文
                this._FE.Update<ApprovalDoc>(OriginApprovalDoc, NewApprovalDoc);
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
                Request OriginRequest = this._FE.Requests.Where(p => p.RequestID == NewRequest.RequestID).FirstOrDefault();
                if (OriginRequest == null)
                {
                    NewRequest.ApprovalDoc = null;
                    //增加申请明细
                    AddRequestHistories(NewRequest);
                    NewRequest.ApprovalHistories = null;
                    //增加申请
                    this._FE.Requests.Add(NewRequest);
                }
                else
                {
                    NewRequest.ApprovalDoc = null;
                    //更新申请明细
                    this.UpdateRequestHistories(NewRequest, OriginRequest);
                    NewRequest.ApprovalHistories = null;
                    //更新申请 
                    this._FE.Update<Request>(OriginRequest, NewRequest);
                }
            }
        }

        //增加批文历史
        private void UpdateApprovalDocHistories(ApprovalDoc NewApprovalDoc, ApprovalDoc OriginApprovalDoc)
        {
            foreach (var NewRequest in NewApprovalDoc.Requests)
            {
                Request OriginRequest = this._FE.Requests.Where(p => p.RequestID == NewRequest.RequestID).FirstOrDefault();
                if (OriginRequest == null)
                {
                    NewRequest.ApprovalDoc = null;
                    //增加申请明细
                    AddRequestHistories(NewRequest);
                    NewRequest.ApprovalHistories = null;
                    // 增加批文明细
                    this._FE.Requests.Add(NewRequest);
                }
                else
                {
                    NewRequest.ApprovalDoc = null;
                    //更新申请明细
                    this.UpdateRequestHistories(NewRequest, OriginRequest);
                    NewRequest.ApprovalHistories = null;
                    //更新批文明细
                    this._FE.Update<Request>(OriginRequest, NewRequest);
                }
            }
        }

        #endregion

        #region 处理飞机数据

        //更新计划飞机
        private void UpdatePlanAircraft(PlanAircraft NewPlanAircraft)
        {
            if (NewPlanAircraft != null)
            {
                PlanAircraft OriginPlanAircraft = this._FE.PlanAircrafts.Where(q => q.PlanAircraftID == NewPlanAircraft.PlanAircraftID).FirstOrDefault();
                //原来为空,新增
                if (OriginPlanAircraft == null)
                {
                    NewPlanAircraft.PlanHistories = null;
                    this._FE.PlanAircrafts.Add(NewPlanAircraft);
                }
                //原来已有,更新
                else
                {
                    NewPlanAircraft.PlanHistories = null;
                    this._FE.Update<PlanAircraft>(OriginPlanAircraft, NewPlanAircraft);
                }
            }
        }

        //更新飞机
        private void UpdateAircraft(Aircraft NewAircraft)
        {
            if (NewAircraft != null)
            {
                Aircraft OriginAircraft = this._FE.Aircrafts.Where(q => q.AircraftID == NewAircraft.AircraftID).FirstOrDefault();
                //原来为空,新增
                if (OriginAircraft == null)
                {
                    this._FE.Aircrafts.Add(NewAircraft);
                }
                //原来已有,更新
                else
                {
                    this._FE.Update<Aircraft>(OriginAircraft, NewAircraft);
                }
            }
        }

        #endregion
    }
}

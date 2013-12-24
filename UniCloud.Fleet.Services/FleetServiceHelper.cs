using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Prism.ViewModel;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.Services
{
    public class FleetServiceHelper : NotificationObject, IDisposable
    {

        public void Dispose()
        {

        }

        #region local

        private Color CreateRandomColor()
        {
            var c = new byte[3];
            var random = new Random();
            for (var i = 0; i < 3; i++)
            {
                c[i] = Convert.ToByte(random.Next(0, 255));
            }
            return Color.FromArgb(255, c[0], c[1], c[2]);
        }

        #region 计划

        /// <summary>
        /// 创建新飞机
        /// </summary>
        /// <param name="planDetail">计划明细</param>
        /// <param name="service"></param>
        private static void CreateAircraft(PlanHistory planDetail, IFleetService service)
        {
            var aircraft = new Aircraft
                {
                    AircraftID = Guid.NewGuid(),
                    AircraftType = planDetail.PlanAircraft.AircraftType,
                    Airlines = service.CurrentAirlines,
                    CreateDate = DateTime.Now,
                    IsOperation = true,
                    SeatingCapacity = planDetail.SeatingCapacity,
                    CarryingCapacity = planDetail.CarryingCapacity,
                };
            planDetail.PlanAircraft.Aircraft = aircraft;
            service.EntityContainer.GetEntitySet<Aircraft>().Add(aircraft);
            CreateOperationHistory(planDetail, aircraft, service);
            CreateAircraftBusiness(planDetail, aircraft, service);
        }

        /// <summary>
        /// 创建新的运营历史
        /// </summary>
        /// <param name="approvalHistory">批文明细</param>
        /// <param name="aircraft">飞机</param>
        /// <param name="service"></param>
        private static void CreateOperationHistory(PlanHistory planDetail, Aircraft aircraft, IFleetService service)
        {
            var operationHistory = new OperationHistory
                {
                    ApprovalHistory = planDetail.ApprovalHistory,
                    Airlines = service.CurrentAirlines,
                    Aircraft = aircraft,
                    ImportCategory = planDetail.ApprovalHistory.ImportCategory,
                    Status = (int)OpStatus.Draft,
                };
            if (planDetail is OperationPlan) (planDetail as OperationPlan).OperationHistory = operationHistory;
            service.EntityContainer.GetEntitySet<OperationHistory>().Add(operationHistory);
            // 更改运营历史状态
            operationHistory.Status = (int)OpStatus.Draft;
        }

        /// <summary>
        /// 创建新的商业数据历史
        /// </summary>
        /// <param name="aircraft">飞机</param>
        /// <param name="service"></param>
        private static void CreateAircraftBusiness(PlanHistory planDetail, Aircraft aircraft, IFleetService service)
        {
            var aircraftBusiness = new AircraftBusiness
                {
                    AircraftBusinessID = Guid.NewGuid(),
                    Aircraft = aircraft,
                    AircraftType = aircraft.AircraftType,
                    ImportCategory = aircraft.ImportCategory,
                    SeatingCapacity = aircraft.SeatingCapacity,
                    CarryingCapacity = aircraft.CarryingCapacity,
                    Status = (int)OpStatus.Draft,
                };

            if (planDetail is ChangePlan) (planDetail as ChangePlan).AircraftBusiness = aircraftBusiness;
            service.EntityContainer.GetEntitySet<AircraftBusiness>().Add(aircraftBusiness);
            // 更改商业数据历史状态
            aircraftBusiness.Status = (int)OpStatus.Draft;
        }

        #endregion

        #endregion

        #region 计划

        /// <summary>
        /// 创建新年度计划
        /// </summary>
        /// <param name="title"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Plan CreateNewYearPlan(string title, IFleetService service)
        {
            // 打开新年度
            var newAnnual =
                service.EntityContainer.GetEntitySet<Annual>()
                       .FirstOrDefault(a => a.Year == service.CurrentAnnual.Year + 1);
            if (newAnnual == null)
            {
                RadWindow.Alert(new DialogParameters
                {
                    Header = "提醒",
                    OkButtonContent = "确认",
                    Content = new TextBlock
                        {
                            Text = "新年度不能为空！",
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                            FontSize = 13,
                            TextWrapping = TextWrapping.Wrap,
                            Width = 250,
                        },
                });
                return null;
            }
            service.CurrentAnnual.IsOpen = false;
            newAnnual.IsOpen = true;
            service.SetCurrentAnnual();
            // 获取当前计划
            var curPlan = service.CurrentPlan;
            // 从当前计划复制生成新年度计划
            var planNew = new Plan
                {
                    PlanID = Guid.NewGuid(),
                    Title = title,
                    CreateDate = DateTime.Now,
                    Annual = newAnnual,
                    Airlines = curPlan.Airlines,
                    VersionNumber = 1,
                    Status = (int)PlanStatus.Draft,
                    PublishStatus = (int)PlanPublishStatus.Draft,
                };
            //获取需要滚动到下一年度的计划明细项
            var planHistories = (curPlan == null || curPlan.PlanHistories == null) ? null
                                        : curPlan.PlanHistories.Where(o => o.PlanAircraft == null ||
                                           (o.PlanAircraft != null && (o.PlanAircraft.Status != (int)ManageStatus.Prepare
                                                                       && o.PlanAircraft.Status != (int)ManageStatus.Operation
                                                                       && o.PlanAircraft.Status != (int)ManageStatus.Retired)));

            // 从当前计划往新版本计划复制运营计划
            var operationPlans = planHistories.OfType<OperationPlan>()
                                        .Where(op => op.OperationHistory == null || op.OperationHistory.EndDate == null)
                                        .Select(op => new OperationPlan
                                            {
                                                Plan = planNew,
                                                PlanHistoryID = Guid.NewGuid(),
                                                PlanAircraft = op.PlanAircraft,
                                                Airlines = op.Airlines,
                                                AircraftType = op.AircraftType,
                                                ApprovalHistory = op.ApprovalHistory,
                                                IsSubmit = op.IsSubmit,
                                                IsValid = op.IsValid,
                                                ActionCategory = op.ActionCategory,
                                                TargetCategory = op.TargetCategory,
                                                Annual = op.Annual,
                                                PerformMonth = op.PerformMonth,
                                                SeatingCapacity = op.SeatingCapacity,
                                                CarryingCapacity = op.CarryingCapacity,
                                                OperationHistory = op.OperationHistory,
                                            }).ToList();
            operationPlans.ForEach(op => planNew.PlanHistories.Add(op));
            // 从当前计划往新版本计划复制变更计划
            var changePlans = planHistories.OfType<ChangePlan>()
                                     .Where(cp => cp.AircraftBusiness == null || cp.AircraftBusiness.EndDate == null)
                                     .Select(cp => new ChangePlan
                                         {
                                             PlanID = planNew.PlanID,
                                             PlanHistoryID = Guid.NewGuid(),
                                             PlanAircraft = cp.PlanAircraft,
                                             Airlines = cp.Airlines,
                                             AircraftType = cp.AircraftType,
                                             ApprovalHistory = cp.ApprovalHistory,
                                             IsSubmit = cp.IsSubmit,
                                             IsValid = cp.IsValid,
                                             ActionCategory = cp.ActionCategory,
                                             TargetCategory = cp.TargetCategory,
                                             Annual = cp.Annual,
                                             PerformMonth = cp.PerformMonth,
                                             SeatingCapacity = cp.SeatingCapacity,
                                             CarryingCapacity = cp.CarryingCapacity,
                                             AircraftBusiness = cp.AircraftBusiness,
                                         }).ToList();
            changePlans.ForEach(cp => planNew.PlanHistories.Add(cp));
            service.EntityContainer.GetEntitySet<Plan>().Add(planNew);
            service.SetCurrentPlan();
            return planNew;
        }

        /// <summary>
        /// 创建当前新版本计划
        /// </summary>
        /// <param name="title"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Plan CreateNewVersionPlan(string title, IFleetService service)
        {
            // 获取当前计划
            var curPlan = service.CurrentPlan;
            // 从当前计划复制生成新版本计划
            var planNew = new Plan
                {
                    PlanID = Guid.NewGuid(),
                    Title = title,
                    CreateDate = DateTime.Now,
                    Annual = curPlan.Annual,
                    Airlines = curPlan.Airlines,
                    VersionNumber = curPlan.VersionNumber + 1,
                    Status = (int)PlanStatus.Draft,
                    PublishStatus = (int)PlanPublishStatus.Draft,
                };
            // 从当前计划往新版本计划复制运营计划
            curPlan.PlanHistories.OfType<OperationPlan>().Select(op => new OperationPlan
                {
                    Plan = planNew,
                    PlanHistoryID = Guid.NewGuid(),
                    PlanAircraft = op.PlanAircraft,
                    Airlines = op.Airlines,
                    AircraftType = op.AircraftType,
                    ApprovalHistory = op.ApprovalHistory,
                    IsSubmit = op.IsSubmit,
                    IsValid = op.IsValid,
                    ActionCategory = op.ActionCategory,
                    TargetCategory = op.TargetCategory,
                    Annual = op.Annual,
                    PerformMonth = op.PerformMonth,
                    SeatingCapacity = op.SeatingCapacity,
                    CarryingCapacity = op.CarryingCapacity,
                    OperationHistory = op.OperationHistory,
                }).ToList().ForEach(op => planNew.PlanHistories.Add(op));
            // 从当前计划往新版本计划复制变更计划
            curPlan.PlanHistories.OfType<ChangePlan>().Select(cp => new ChangePlan
                {
                    PlanID = planNew.PlanID,
                    PlanHistoryID = Guid.NewGuid(),
                    PlanAircraft = cp.PlanAircraft,
                    Airlines = cp.Airlines,
                    AircraftType = cp.AircraftType,
                    ApprovalHistory = cp.ApprovalHistory,
                    IsSubmit = cp.IsSubmit,
                    IsValid = cp.IsValid,
                    ActionCategory = cp.ActionCategory,
                    TargetCategory = cp.TargetCategory,
                    Annual = cp.Annual,
                    PerformMonth = cp.PerformMonth,
                    SeatingCapacity = cp.SeatingCapacity,
                    CarryingCapacity = cp.CarryingCapacity,
                    AircraftBusiness = cp.AircraftBusiness,
                }).ToList().ForEach(cp => planNew.PlanHistories.Add(cp));

            service.EntityContainer.GetEntitySet<Plan>().Add(planNew);
            service.SetCurrentPlan();
            return planNew;
        }

        /// <summary>
        /// 创建运营计划明细
        /// 同一架计划飞机在一份计划中的明细项不得超过两条，且两条不得为同种操作类型（含运营计划与变更计划）
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="planAircraft"></param>
        /// <param name="actionType"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal OperationPlan CreateOperationPlan(Plan plan, PlanAircraft planAircraft, string actionType, IFleetService service)
        {
            if (plan == null) return null;
            // 创建新的计划历史
            var planDetail = new OperationPlan
                {
                    PlanHistoryID = Guid.NewGuid(),
                    Plan = plan,
                    ActionType = actionType,
                    Airlines = service.CurrentAirlines,
                    Annual = service.CurrentAnnual,
                    PerformMonth = 1,
                };
            // 1、计划飞机为空
            if (planAircraft == null)
            {
                // 创建新的计划飞机
                var pa = new PlanAircraft
                    {
                        PlanAircraftID = Guid.NewGuid(),
                        Airlines = service.CurrentAirlines,
                        Status = (int)ManageStatus.Plan,
                        IsOwn = true
                    };
                planDetail.PlanAircraft = pa;
            }
            // 2、计划飞机非空
            else
            {
                // 获取计划飞机的所有计划明细集合
                var phs = planAircraft.PlanHistories;
                // 获取计划飞机在当前计划中的计划明细集合
                var planDetails = phs.Where(ph => ph.Plan == plan).ToList();
                // 2.1、不是针对现有飞机的计划明细
                if (planAircraft.Aircraft == null)
                {
                    if (phs.Any())
                    {
                        // 获取计划飞机的最后一条计划明细，用于复制数据
                        var planHistory =
                            phs.OrderBy(ph => ph.Annual.Year)
                               .ThenBy(ph => ph.Plan.VersionNumber)
                               .LastOrDefault();
                        if (planHistory != null)
                        {
                            // 1、计划飞机在当前计划中没有明细项
                            if (!planDetails.Any())
                            {
                                planDetail.AircraftType = planAircraft.AircraftType;
                                planDetail.ActionCategory = planHistory.ActionCategory;
                                planDetail.TargetCategory = planHistory.TargetCategory;
                                planDetail.SeatingCapacity = planHistory.SeatingCapacity;
                                planDetail.CarryingCapacity = planHistory.CarryingCapacity;
                                planAircraft.Status = (int)ManageStatus.Plan;
                            }
                            // 2、计划飞机在当前计划中已有明细项
                            else
                            {
                                planDetail.AircraftType = planAircraft.AircraftType;
                                planDetail.SeatingCapacity = -planHistory.SeatingCapacity;
                                planDetail.CarryingCapacity = -planHistory.CarryingCapacity;
                            }
                        }
                    }
                }
                // 2.2、是针对现有飞机的计划明细，肯定是退出计划，无需改变计划飞机管理状态
                else
                {
                    planDetail.AircraftType = planAircraft.AircraftType;
                    planDetail.SeatingCapacity = planAircraft.Aircraft.SeatingCapacity;
                    planDetail.CarryingCapacity = planAircraft.Aircraft.CarryingCapacity;
                }
                planDetail.PlanAircraft = planAircraft;
            }

            return planDetail;
        }

        /// <summary>
        /// 创建变更计划明细
        /// 同一架计划飞机在一份计划中的明细项不得超过两条，且两条不得为同种操作类型（含运营计划与变更计划）
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="planAircraft"></param>
        /// <param name="actionType"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal ChangePlan CreateChangePlan(Plan plan, PlanAircraft planAircraft, string actionType, IFleetService service)
        {
            if (plan == null || planAircraft == null)
                return null;
            // 创建新的变更计划明细
            var planDetail = new ChangePlan
                {
                    Plan = plan,
                    ActionType = actionType,
                    PlanHistoryID = Guid.NewGuid(),
                    PlanAircraft = planAircraft,
                    AircraftType = planAircraft.AircraftType,
                    Annual = service.CurrentAnnual,
                    PerformMonth = 1,
                    Airlines = service.CurrentAirlines,
                };
            // 获取飞机的当前商业数据，赋予新创建的变更计划明细
            var abs = planAircraft.Aircraft.AircraftBusinesses;
            if (abs.Any())
            {
                var aircraftBusiness = abs.FirstOrDefault(a => a.EndDate == null);
                if (aircraftBusiness != null)
                {
                    planDetail.ActionCategory = aircraftBusiness.ImportCategory;
                    planDetail.TargetCategory = aircraftBusiness.ImportCategory;
                    planDetail.SeatingCapacity = aircraftBusiness.SeatingCapacity;
                    planDetail.CarryingCapacity = aircraftBusiness.CarryingCapacity;
                }
            }

            return planDetail;
        }

        /// <summary>
        /// 移除计划明细项
        /// </summary>
        /// <param name="planDetail"></param>
        /// <param name="service"></param>
        internal void RemovePlanDetail(PlanHistory planDetail, IFleetService service)
        {
            if (planDetail != null)
            {
                // 获取计划飞机
                var planAircraft = planDetail.PlanAircraft;
                // 获取计划飞机的明细项集合
                var planAircraftHistories = planAircraft.PlanHistories;
                // 获取计划飞机在当前计划中的明细项集合
                var planDetails = planAircraft.PlanHistories.Where(ph => ph.Plan == service.CurrentPlan).ToList();

                // 1、已有飞机（只有变更与退出计划）
                if (planAircraft.Aircraft != null)
                {
                    // 1.1、计划飞机在当前计划中只有一条明细项
                    if (planDetails.Count == 1)
                        planAircraft.Status = (int)ManageStatus.Operation;
                    // 1.2、计划飞机在当前计划中超过一条明细项，即一条变更、一条退出
                    else
                    {
                        // 移除的是变更计划，计划飞机改为运营状态（可能之前也是运营状态）
                        if (planDetail.ActionType == "变更")
                            planAircraft.Status = (int)ManageStatus.Operation;
                        // 移除的是退出计划，不做任何改变
                    }
                }
                // 2、没有飞机（只有引进与退出计划）
                // 2.1、计划飞机相关的明细项数量为1
                // 删除相关计划飞机。
                else if (planAircraftHistories.Count == 1)
                {
                    service.EntityContainer.GetEntitySet<PlanAircraft>().Remove(planAircraft);
                }
                // 2.2、计划飞机相关的计划历史数量不为1（即超过1）
                // 2.2.1、计划飞机在当前计划中只有一条明细项
                // 计划飞机的管理状态改为预备
                else if (planDetails.Count == 1)
                {
                    planAircraft.Status = (int)ManageStatus.Prepare;
                }
                // 2.2.2、计划飞机在当前计划中超过一条明细项，即一条引进、一条退出
                // 不改变计划飞机状态

                service.EntityContainer.GetEntitySet<PlanHistory>().Remove(planDetail);
            }
        }

        /// <summary>
        /// 完成计划项
        /// </summary>
        /// <param name="planDetail"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Aircraft CompletePlan(PlanHistory planDetail, IFleetService service)
        {
            Aircraft aircraft;
            OperationHistory operationHistory;
            if (planDetail == null)
            {
                throw new ArgumentNullException("planDetail");
            }
            var actionName = planDetail.ActionCategory.ActionName;
            if (actionName == null)
            {
                return null;
            }
            // 根据引进方式调用不同的操作
            switch (actionName)
            {
                case "购买":
                    // 创建新飞机
                    CreateAircraft(planDetail, service);
                    break;
                case "融资租赁":
                    // 创建新飞机
                    CreateAircraft(planDetail, service);
                    break;
                case "经营租赁":
                    // 创建新飞机
                    CreateAircraft(planDetail, service);
                    break;
                case "湿租":
                    // 创建新飞机
                    CreateAircraft(planDetail, service);
                    break;
                case "经营租赁续租":
                    // 创建新运营历史
                    CreateOperationHistory(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "湿租续租":
                    // 创建新运营历史
                    CreateOperationHistory(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "出售":
                    // 更改运营历史状态
                    aircraft = planDetail.PlanAircraft.Aircraft;
                    operationHistory = aircraft.OperationHistories.LastOrDefault(oh => oh.EndDate == null);
                    if (operationHistory != null)
                    {
                        operationHistory.Status = (int)OpStatus.Draft;
                        operationHistory.ExportCategory =
                            service.AllActionCategories.FirstOrDefault(ac => ac.ActionName == "出售");

                        if (planDetail is OperationPlan) (planDetail as OperationPlan).OperationHistory = operationHistory;
                    }
                    break;
                case "出租":
                    // 更改运营历史状态
                    aircraft = planDetail.PlanAircraft.Aircraft;
                    operationHistory = aircraft.OperationHistories.LastOrDefault(oh => oh.EndDate == null);
                    if (operationHistory != null)
                    {
                        operationHistory.Status = (int)OpStatus.Draft;
                        operationHistory.ExportCategory =
                            service.AllActionCategories.FirstOrDefault(ac => ac.ActionName == "出租");

                        if (planDetail is OperationPlan) (planDetail as OperationPlan).OperationHistory = operationHistory;
                    }
                    break;
                case "退租":
                    // 更改运营历史状态
                    aircraft = planDetail.PlanAircraft.Aircraft;
                    operationHistory = aircraft.OperationHistories.LastOrDefault(oh => oh.EndDate == null);
                    if (operationHistory != null)
                    {
                        operationHistory.Status = (int)OpStatus.Draft;
                        operationHistory.ExportCategory =
                            service.AllActionCategories.FirstOrDefault(ac => ac.ActionName == "退租");

                        if (planDetail is OperationPlan) (planDetail as OperationPlan).OperationHistory = operationHistory;
                    }
                    break;
                case "退役":
                    // 更改运营历史状态
                    aircraft = planDetail.PlanAircraft.Aircraft;
                    operationHistory = aircraft.OperationHistories.LastOrDefault(oh => oh.EndDate == null);
                    if (operationHistory != null)
                    {
                        operationHistory.Status = (int)OpStatus.Draft;
                        operationHistory.ExportCategory =
                            service.AllActionCategories.FirstOrDefault(ac => ac.ActionName == "退役");

                        if (planDetail is OperationPlan) (planDetail as OperationPlan).OperationHistory = operationHistory;
                    }
                    break;
                case "货改客":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "客改货":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "售后经营租赁":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "售后融资租赁":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "一般改装":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
                case "租转购":
                    // 创建商业数据历史
                    CreateAircraftBusiness(planDetail, planDetail.PlanAircraft.Aircraft, service);
                    break;
            }
            // 更改计划飞机状态
            planDetail.PlanAircraft.Status = (int)ManageStatus.Operation;
            // 刷新计划完成状态
            RaisePropertyChanged(() => planDetail.CompleteStatus);

            return planDetail.PlanAircraft.Aircraft;
        }

        /// <summary>
        /// 获取当前打开年度的计划
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private static IEnumerable<Plan> GetCurrentPlans(IFleetService service)
        {
            // 获取当年计划集合，按版本排序
            var currentPlans =
                service.EntityContainer.GetEntitySet<Plan>()
                       .Where(p => p.Annual.IsOpen == true)
                       .OrderBy(p => p.VersionNumber);
            return currentPlans;
        }

        /// <summary>
        /// 获取当前打开年度发布中计划
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal IEnumerable<Plan> GetLatestPublishingPlan(IFleetService service)
        {
            // 获取当年计划集合
            var currentPlans = GetCurrentPlans(service).ToList();
            //没有则返回为空
            if (currentPlans == null || !currentPlans.Any())
            {
                return null;
            }
            //否则取发布中计划
            return currentPlans.Where(
                p =>
                p.PublishStatus > (int)PlanPublishStatus.Draft &&
                p.PublishStatus < (int)PlanPublishStatus.Submited);
        }

        /// <summary>
        /// 获取当前打开年度最后一份已发布的计划
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Plan GetLatestPublishedPlan(IFleetService service)
        {
            // 获取当年计划集合
            var currentPlans = GetCurrentPlans(service).ToList();
            //没有则返回为空
            if (currentPlans == null || !currentPlans.Any())
            {
                return null;
            }
            return currentPlans.LastOrDefault(p => p.PublishStatus == (int)PlanPublishStatus.Submited);
        }

        /// <summary>
        /// 获取所有有效的计划
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal IEnumerable<Plan> GetAllValidPlan(IFleetService service)
        {
            var validPlans = service.EntityContainer.GetEntitySet<Plan>().Where(p => p.IsValid);
            return validPlans;
        }

        #endregion

        #region 批文管理

        /// <summary>
        /// 创建新申请
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Request CreateNewRequest(IFleetService service)
        {
            var request = new Request
                {
                    RequestID = Guid.NewGuid(),
                    Airlines = service.CurrentAirlines,
                    Status = (int)ReqStatus.Draft,
                    CreateDate = DateTime.Now,
                };
            service.EntityContainer.GetEntitySet<Request>().Add(request);
            return request;
        }

        /// <summary>
        /// 创建新申请明细
        /// </summary>
        /// <param name="request"></param>
        /// <param name="planHistory"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal ApprovalHistory CreateNewRequestDetail(Request request, PlanHistory planHistory, IFleetService service)
        {
            // 创建新的申请明细
            var requestDetail = new ApprovalHistory
                {
                    ApprovalHistoryID = Guid.NewGuid(),
                    Request = request,
                    PlanAircraft = planHistory.PlanAircraft,
                    ImportCategory = planHistory.TargetCategory,
                    Airlines = planHistory.Airlines,
                    Annual = planHistory.Annual,
                    RequestDeliverMonth = planHistory.PerformMonth,
                    SeatingCapacity = planHistory.SeatingCapacity,
                    CarryingCapacity = planHistory.CarryingCapacity,
                };
            // 把申请明细赋给关联的计划明细
            planHistory.ApprovalHistory = requestDetail;
            // 计划飞机管理状态修改为申请
            requestDetail.PlanAircraft.Status = (int)ManageStatus.Request;

            service.EntityContainer.GetEntitySet<ApprovalHistory>().Add(requestDetail);
            return requestDetail;
        }

        /// <summary>
        /// 移除申请明细
        /// </summary>
        /// <param name="requestDetail"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal void RemoveRequestDetail(ApprovalHistory requestDetail, IFleetService service)
        {
            // 获取相关的计划明细
            var planHistories =
                service.EntityContainer.GetEntitySet<PlanHistory>()
                       .Where(ph => ph.ApprovalHistoryID == requestDetail.ApprovalHistoryID).ToList();
            // 相关计划明细的申请明细置为空
            planHistories.ForEach(ph => ph.ApprovalHistory = null);
            // 相关计划飞机的管理状态改为计划
            requestDetail.PlanAircraft.Status = (int)ManageStatus.Plan;
            // 移除申请明细
            service.EntityContainer.GetEntitySet<ApprovalHistory>().Remove(requestDetail);
        }

        /// <summary>
        /// 创建新批文
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal ApprovalDoc CreateNewApprovalDoc(IFleetService service)
        {
            var approvalDoc = new ApprovalDoc
                {
                    ApprovalDocID = Guid.NewGuid(),
                    Status = (int)OpStatus.Draft,
                };
            service.EntityContainer.GetEntitySet<ApprovalDoc>().Add(approvalDoc);
            return approvalDoc;
        }

        /// <summary>
        /// 往批文添加申请
        /// </summary>
        /// <param name="approvalDoc"></param>
        /// <param name="request"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Request AddRequestToApprovalDoc(ApprovalDoc approvalDoc, Request request, IFleetService service)
        {
            // 把申请赋给相关批文
            request.ApprovalDoc = approvalDoc;
            // 申请状态改为已审批
            request.Status = (int)ReqStatus.Examined;
            // 相关申请明细对应计划飞机置为批准，其管理状态置为批文
            request.ApprovalHistories.ToList().ForEach(ah =>
                {
                    ah.IsApproved = true;
                    ah.PlanAircraft.Status = (int)ManageStatus.Approval;
                });
            return request;
        }

        /// <summary>
        /// 移除申请
        /// </summary>
        /// <param name="request"></param>
        /// <param name="service"></param>
        internal void RemoveRequest(Request request, IFleetService service)
        {
            // 从相关批文移除申请
            request.ApprovalDoc = null;
            // 申请状态改为已提交
            request.Status = (int)ReqStatus.Submited;
            // 相关申请明细对应计划飞机置为未批准，其管理状态置为申请
            request.ApprovalHistories.ToList().ForEach(ah =>
                {
                    ah.IsApproved = false;
                    ah.PlanAircraft.Status = (int)ManageStatus.Request;
                });
        }

        /// <summary>
        /// 批准申请
        /// </summary>
        /// <param name="requestDetail"></param>
        /// <param name="service"></param>
        internal void ApproveRequest(ApprovalHistory requestDetail, IFleetService service)
        {
            // 相关计划飞机的管理状态改为批文
            requestDetail.PlanAircraft.Status = (int)ManageStatus.Approval;
        }

        /// <summary>
        /// 拒绝申请
        /// </summary>
        /// <param name="requestDetail"></param>
        /// <param name="service"></param>
        internal void RejectRequest(ApprovalHistory requestDetail, IFleetService service)
        {
            // 相关计划飞机的管理状态改为计划
            requestDetail.PlanAircraft.Status = (int)ManageStatus.Plan;
        }

        #endregion

        #region 运营管理

        /// <summary>
        /// 创建新的所有权历史记录
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal OwnershipHistory CreateNewOwnership(Aircraft aircraft, IFleetService service)
        {
            var ownership = new OwnershipHistory
                {
                    OwnershipHistoryID = Guid.NewGuid(),
                    Aircraft = aircraft,
                    Owner = service.CurrentAirlines,
                    StartDate = DateTime.Now,
                    Status = (int)OpStatus.Draft,
                };
            service.EntityContainer.GetEntitySet<OwnershipHistory>().Add(ownership);
            return ownership;
        }

        /// <summary>
        /// 移除所有权历史记录
        /// </summary>
        /// <param name="ownership"></param>
        /// <param name="service"></param>
        internal void RemoveOwnership(OwnershipHistory ownership, IFleetService service)
        {
            var ownweships =
                service.EntityContainer.GetEntitySet<OwnershipHistory>()
                       .Where(os => os.Aircraft == ownership.Aircraft)
                       .OrderBy(os => os.StartDate)
                       .ToList();
            var count = ownweships.Count;
            // 所有权历史至少要保留一条
            if (count > 1)
            {
                service.EntityContainer.GetEntitySet<OwnershipHistory>().Remove(ownership);
                // 修改之前记录的结束日期
                ownweships[count - 2].EndDate = null;
            }
        }

        /// <summary>
        /// 创建新的运力分配
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal SubOperationHistory CreateNewSubOperation(OperationHistory operation, IFleetService service)
        {
            var subOperation = new SubOperationHistory
                {
                    SubOperationHistoryID = Guid.NewGuid(),
                    OperationHistory = operation,
                    StartDate = DateTime.Now,
                    Status = (int)OpStatus.Draft,
                };
            service.EntityContainer.GetEntitySet<SubOperationHistory>().Add(subOperation);
            return subOperation;
        }

        /// <summary>
        /// 移除运力分配记录
        /// </summary>
        /// <param name="subOperation"></param>
        /// <param name="service"></param>
        internal void RemoveSubOperation(SubOperationHistory subOperation, IFleetService service)
        {
            var subOperations =
                service.EntityContainer.GetEntitySet<SubOperationHistory>()
                       .Where(soh => soh.OperationHistory == subOperation.OperationHistory)
                       .OrderBy(soh => soh.StartDate)
                       .ToList();
            var count = subOperations.Count;
            // 运力分配历史至少要保留一条
            if (count > 1)
            {
                service.EntityContainer.GetEntitySet<SubOperationHistory>().Remove(subOperation);
                // 修改之前记录的结束日期
                subOperations[count - 2].EndDate = null;
            }
        }

        /// <summary>
        /// 创建分公司
        /// </summary>
        /// <param name="airlines"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Airlines CreateFilialeAirlines(Airlines airlines, IFleetService service)
        {
            // 创建并添加分公司
            var air = new Airlines
                {
                    OwnerID = Guid.NewGuid(),
                    MasterID = airlines.OwnerID,
                    IATACode = airlines.IATACode,
                    ICAOCode = airlines.ICAOCode,
                    SubType = 0,
                    IsValid = true,
                    Status = (int)FilialeStatus.InUse,
                };
            airlines.SubAirlines.Add(air);
            service.EntityContainer.GetEntitySet<Owner>().Add(air);
            return air;
        }

        /// <summary>
        /// 移除分公司
        /// </summary>
        /// <param name="airlines"></param>
        /// <param name="service"></param>
        internal void RemoveFilialeAirlines(Airlines airlines, IFleetService service)
        {
            // 如果分公司已经有相关操作，只能假删除
            if (airlines.SubOperationHistories.Any())
            {
                airlines.Status = (int)FilialeStatus.Deleted;
            }
            else
            {
                service.EntityContainer.GetEntitySet<Owner>().Remove(airlines);
            }
        }

        /// <summary>
        /// 创建子公司
        /// </summary>
        /// <param name="airlines"></param>
        /// <param name="service"></param>
        internal void CreateSubAirlines(Airlines airlines, IFleetService service)
        {
            airlines.MasterID = service.CurrentAirlines.OwnerID;
            airlines.SubType = 1;
        }

        /// <summary>
        /// 移除子公司
        /// </summary>
        /// <param name="airlines"></param>
        /// <param name="service"></param>
        internal void RemoveSubAirlines(Airlines airlines, IFleetService service)
        {
            airlines.SubType = 0;
            airlines.MasterID = null;
        }

        /// <summary>
        /// 创建新供应商
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal Owner CreateSupplier(IFleetService service)
        {
            var supplier = new Owner
                {
                    OwnerID = Guid.NewGuid(),
                    IsValid = true,
                    SupplierType = 1,
                };
            service.EntityContainer.GetEntitySet<Owner>().Add(supplier);
            return supplier;
        }

        /// <summary>
        /// 移除供应商
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="service"></param>
        internal void RemoveSupplier(Owner supplier, IFleetService service)
        {
            service.EntityContainer.GetEntitySet<Owner>().Remove(supplier);
        }


        #endregion

    }
}

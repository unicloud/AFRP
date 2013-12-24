using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Xml.Linq;

namespace UniCloud.Fleet.Models
{
    #region 基础配置

    public partial class XmlConfig : Entity
    {
        public XElement LocalXmlContent
        {
            get { return XElement.Parse(this.ConfigContent); }
            set { this.ConfigContent = value.ToString(); }
        }
    }

    #endregion

    #region 参与者



    #endregion

    #region 飞机

    public partial class Aircraft : Entity
    {
        private string _title;
        public string Title
        {
            get { return this._title; }
            set
            {
                this._title = value;
            }
        }
        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ImportCategory != null)
                {
                    return this.ImportCategory.ActionType + "：" + this.ImportCategory.ActionName;
                }
                else
                {
                    return null;
                }
            }
        }

        //座位
        private decimal _thenSeatingCapacity;
        public decimal ThenSeatingCapacity
        {
            get { return this._thenSeatingCapacity; }
            set
            {
                this._thenSeatingCapacity = value;
            }
        }
        //商载
        private decimal _thenCarryingCapacity;
        public decimal ThenCarryingCapacity
        {
            get { return this._thenCarryingCapacity; }
            set
            {
                this._thenCarryingCapacity = value;
            }
        }
        //座级
        private string _thenRegional;
        public string ThenRegional
        {
            get { return this._thenRegional; }
            set
            {
                this._thenRegional = value;
            }
        }
        //机型
        private string _thenAircraftTypeName;
        public string ThenAircraftTypeName
        {
            get { return this._thenAircraftTypeName; }
            set
            {
                this._thenAircraftTypeName = value;
            }
        }
        //引进方式
        private string _thenActionName;
        public string ThenActionName
        {
            get { return this._thenActionName; }
            set
            {
                this._thenActionName = value;
            }
        }
        //运营权人
        private string _thenAirlineName;
        public string ThenAirlineName
        {
            get { return this._thenAirlineName; }
            set
            {
                this._thenAirlineName = value;
            }
        }

        private string _thenOwnerName;
        public string ThenOwnerName
        {
            get { return this._thenOwnerName; }
            set
            {
                this._thenOwnerName = value;
            }
        }
    }

    public partial class PlanHistory : Entity
    {
        #region 属性

        /// <summary>
        /// 是否已申请
        /// </summary>
        public string IsApply
        {
            get
            {
                if (this.ApprovalHistoryID != null)
                {
                    return "已申请";
                }
                else
                {
                    return "未申请";
                }

            }
        }

        [Display(Name = "执行时间")]
        public string PerformTime
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.PerformMonth : null; }
        }
        partial void OnPerformAnnualIDChanged()
        {
            this.RaisePropertyChanged("PerformTime");
        }
        partial void OnPerformMonthChanged()
        {
            this.RaisePropertyChanged("PerformTime");
        }

        [Display(Name = "净增客机")]
        public int DeltaPnr
        {
            get
            {
                if (this.AircraftType == null || this.ActionCategory == null)
                    return 0;
                if (this.AircraftType.AircraftCategory.Category == "客机")
                {
                    switch (this.ActionCategory.ActionName)
                    {
                        case "购买": return 1;
                        case "融资租赁": return 1;
                        case "经营租赁": return 1;
                        case "湿租": return 1;
                        case "经营租赁续租": return 0;
                        case "湿租续租": return 0;
                        case "退役": return -1;
                        case "出售": return -1;
                        case "退租": return -1;
                        case "一般改装": return 0;
                        case "货改客": return 1;
                        case "售后回租": return 0;
                        case "租转购": return 0;
                        default: return 0;
                    }
                }
                else
                {
                    if (this.ActionCategory.ActionName == "客改货")
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        [Display(Name = "净增货机")]
        public int DeltaCargo
        {
            get
            {
                if (this.AircraftType == null || this.ActionCategory == null)
                    return 0;
                if (this.AircraftType.AircraftCategory.Category == "货机")
                {
                    switch (this.ActionCategory.ActionName)
                    {
                        case "购买": return 1;
                        case "融资租赁": return 1;
                        case "经营租赁": return 1;
                        case "湿租": return 1;
                        case "经营租赁续租": return 0;
                        case "湿租续租": return 0;
                        case "退役": return -1;
                        case "出售": return -1;
                        case "退租": return -1;
                        case "一般改装": return 0;
                        case "客改货": return 1;
                        case "售后回租": return 0;
                        case "租转购": return 0;
                        default: return 0;
                    }
                }
                else
                {
                    if (this.ActionCategory.ActionName == "货改客")
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        [Display(Name = "是否不同")]
        public bool IsDifferent { get; set; } //用于计划查询中比较的计划历史是否相同


        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ActionCategory != null)
                {
                    return this.ActionCategory.ActionType + "：" + this.ActionCategory.ActionName;
                }
                else
                {
                    return null;
                }
            }
        }


        #endregion

    }

    public partial class ChangePlan : PlanHistory
    {
        [Display(Name = "完成情况")]
        public string IsPerform
        {
            get
            {
                if (this.AircraftBusiness == null)
                {
                    return "未完成";
                }
                else
                {
                    return "已完成";
                }
            }
        }
    }

    public partial class OperationPlan : PlanHistory
    {
        [Display(Name = "完成情况")]
        public string IsPerform
        {
            get
            {
                if (this.OperationHistory == null)
                {
                    return "未完成";
                }
                else
                {
                    return "已完成";
                }
            }
        }
    }

    public partial class ApprovalHistory : Entity
    {
        [Display(Name = "申请交付时间")]
        public string RequestDeliver
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.RequestDeliverMonth : null; }
        }

        [Display(Name = "飞机投入运营情况")]
        public string RequstOperation
        {
            get
            {
                if (this.OperationHistory == null)
                {
                    return null;
                }
                else
                {
                    return "飞机运营情况";
                }
            }
        }

        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ImportCategory != null)
                {
                    return this.ImportCategory.ActionType + "：" + this.ImportCategory.ActionName;
                }
                else
                {
                    return null;
                }
            }
        }


        partial void OnRequestDeliverAnnualIDChanged()
        {
            this.RaisePropertyChanged("RequestDeliver");
        }
        partial void OnRequestDeliverMonthChanged()
        {
            this.RaisePropertyChanged("RequestDeliver");
        }
    }

    public sealed partial class OperationHistory : Entity
    {
        #region 属性

        [Display(Name = "引进操作")]
        public string ImportCategoryOperation
        {
            get
            {
                if (this.ImportCategory != null)
                {
                    return this.ImportCategory.ActionType + "：" + this.ImportCategory.ActionName;
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "退出操作")]
        public string ExporCategoryOperation
        {
            get
            {
                if (this.ExportCategory != null)
                {
                    return this.ExportCategory.ActionType + "：" + this.ExportCategory.ActionName;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
    }


    public partial class Agreement : Entity
    {
        [Display(Name = "计划启动谈判")]
        public string PlanNegotiateStart
        {
            get { return this.PlanAnnual != null ? this.PlanAnnual.Year + "/" + this.PlanAgreeMonth : null; }
        }
        partial void OnPlanAgreeAnnualIDChanged()
        {
            this.RaisePropertyChanged("PlanNegotiateStart");
        }
        partial void OnPlanAgreeMonthChanged()
        {
            this.RaisePropertyChanged("PlanNegotiateStart");
        }

        [Display(Name = "实际启动谈判")]
        public string ActualnegotiateStart
        {
            get { return this.ActualAnnual != null ? this.ActualAnnual.Year + "/" + this.ActualAgreeMonth : null; }
        }
        partial void OnActualAgreeAnnualIDChanged()
        {
            this.RaisePropertyChanged("ActualnegotiateStart");
        }
        partial void OnActualAgreeMonthChanged()
        {
            this.RaisePropertyChanged("ActualnegotiateStart");
        }
    }

    public partial class AgreementDetail : Entity
    {
        [Display(Name = "协议交付时间")]
        public string AgreementDeliver
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.PlanDeliverMonth : null; }
        }
        partial void OnPlanDeliverAnnualIDChanged()
        {
            this.RaisePropertyChanged("AgreementDeliver");
        }
        partial void OnPlanDeliverMonthChanged()
        {
            this.RaisePropertyChanged("AgreementDeliver");
        }
    }

    #endregion

    #region 机队管理

    public partial class Annual : Entity
    {
        [Display(Name = "计划总数")]
        public int PlanCount
        {
            get { return this.Plans.Count; }
        }
    }

    #region 计划
    public partial class Plan : Entity
    {
        [Display(Name = "客机计划")]
        public List<PlanHistory> AirlinerPlanHistories
        {
            get
            {
                if (this.PlanHistories.Any(p => p.AircraftType.AircraftCategory.Category == "客机"))
                {
                    return this.PlanHistories.Where(p => p.AircraftType.AircraftCategory.Category == "客机" &&
                      ((p.PlanAircraft == null) || (p.PlanAircraft != null && p.PlanAircraft.Status != (int)ManageStatus.Operation))).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "货机计划")]
        public List<PlanHistory> CargoAircraftPlanHistories
        {
            get
            {
                if (this.PlanHistories.Any(p => p.AircraftType.AircraftCategory.Category == "货机"))
                {
                    return this.PlanHistories.Where(p => p.AircraftType.AircraftCategory.Category == "货机" &&
                       ((p.PlanAircraft == null) || (p.PlanAircraft != null && p.PlanAircraft.Status != (int)ManageStatus.Operation))).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "计划完成情况")]
        public string PlanIsFinished
        {
            get
            {
                if(this.CargoAircraftPlanHistories == null && this.ManageFlagPnr==true)
                {
                    return "已完成";
                }
                else if(this.AirlinerPlanHistories == null && this.ManageFlagCargo== true)
                {
                    return "已完成";
                }
                else if (this.ManageFlagCargo == true && this.ManageFlagPnr == true)
                {
                    return "已完成";
                }
                else
                {
                    return "未完成";
                }
            }
        }

        [Display(Name = "计划发布情况")]
        public string PlanIsPublished
        {
            get
            {
                if (this.IsValid)
                {
                    return "已发布";
                }
                else
                {
                    return "未发布";
                }
            }
        }

    }


    #endregion

    #region 批文

    public partial class ApprovalDoc : Entity
    {
        public Airlines Airlines
        {
            get
            {
                if (this.Requests != null && this.Requests.Count != 0)
                {
                    return this.Requests.First().Airlines;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    #endregion


    #endregion
}

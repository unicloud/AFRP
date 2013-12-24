using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Operation;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using System.IO;
namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpPlanPerformViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpPlanPerformViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpPlanPerformViewModel()
        {

            //弹出窗口的设置
            radWindow.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            radWindow.Height = 300;
            radWindow.Width = 600;
            radWindow.ResizeMode = ResizeMode.CanResize;

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly UniCloud.AFRP.Views.AfrpPlanPerformView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpPlanPerformView>();
        //初始化RadWindow 需添加引用集 Telerik.Windows.Controls.Navigation
        private RadWindow radWindow = new RadWindow();
        private RadGridView plan, planDetail;
        private int i; //导出数据源格式判断

        #region Property



        #endregion

        #region Method

        #region Local 方法 CreateRadWindows -- 创建子窗体

        /// <summary>
        /// 初始化RadWindow控件的Content的值
        /// </summary>
        /// <param name="windowsType">窗体的类型，主要有三个：1、运行历史 2、商业数据 3、申请</param>
        /// <param name="obj">RadGridView控件的数据集合</param>
        /// <returns></returns>
        public Grid CreateRadWindows(string windowsType, object obj)
        {
            Grid grid = new Grid();
            RadGridView gridview;
            if (windowsType == "运营历史")
            {
                gridview = new RadGridView();
                gridview.ShowGroupPanel = true;
                gridview.AutoGenerateColumns = false;
                gridview.IsReadOnly = true;
                gridview.IsFilteringAllowed = true;

                gridview.RowIndicatorVisibility = Visibility.Collapsed;
                var OperationHistories = new List<OperationHistory>();
                OperationHistories.Add((obj as OperationPlan).OperationHistory);
                gridview.ItemsSource = OperationHistories;
                var gv1 = new GridViewDataColumn
                    {
                        Header = "航空公司",
                        DataMemberBinding =
                            new System.Windows.Data.Binding("Airlines.ShortName")
                    };

                var gv2 = new GridViewDataColumn
                    {
                        Header = "飞机号",
                        DataMemberBinding =
                            new System.Windows.Data.Binding("Aircraft.RegNumber")
                    };

                var gv3 = new GridViewDataColumn
                    {
                        Header = "机型",
                        DataMemberBinding =
                            new System.Windows.Data.Binding("Aircraft.AircraftType.Name")
                    };

                var gv4 = new GridViewDataColumn { Header = "技术接收日期" };
                var bing4 = new System.Windows.Data.Binding("TechReceiptDate")
                    {
                        StringFormat
                            = "yyyy/M/d"
                    };
                gv4.DataMemberBinding = bing4;

                var gv5 = new GridViewDataColumn { Header = "接收日期" };
                var bing5 = new System.Windows.Data.Binding("ReceiptDate") { StringFormat = "yyyy/M/d" };
                gv5.DataMemberBinding = bing5;

                var gv6 = new GridViewDataColumn { Header = "运营日期" };
                var bing6 = new System.Windows.Data.Binding("StartDate") { StringFormat = "yyyy/M/d" };
                gv6.DataMemberBinding = bing6;

                var gv7 = new GridViewDataColumn { Header = "退出停场日期" };
                var bing7 = new System.Windows.Data.Binding("StopDate") { StringFormat = "yyyy/M/d" };
                gv7.DataMemberBinding = bing7;

                var gv8 = new GridViewDataColumn { Header = "技术交付日期" };
                var bing8 = new System.Windows.Data.Binding("TechDeliveryDate") { StringFormat = "yyyy/M/d" };
                gv8.DataMemberBinding = bing8;

                var gv9 = new GridViewDataColumn { Header = "退出日期" };
                var bing9 = new System.Windows.Data.Binding("EndDate") { StringFormat = "yyyy/M/d" };
                gv9.DataMemberBinding = bing9;

                var gv10 = new GridViewDataColumn { Header = "起租日期" };
                var bing10 = new System.Windows.Data.Binding("OnHireDate") { StringFormat = "yyyy/M/d" };
                gv10.DataMemberBinding = bing10;

                var gv11 = new GridViewDataColumn
                    {
                        Header = "引进方式",
                        DataMemberBinding =
                            new System.Windows.Data.Binding("ImportCategory.ActionName")
                    };

                var gv12 = new GridViewDataColumn
                    {
                        Header = "退出方式",
                        DataMemberBinding =
                            new System.Windows.Data.Binding("ExportCategory.ActionName")
                    };

                var gv13 = new GridViewDataColumn
                    {
                        Header = "备注",
                        DataMemberBinding = new System.Windows.Data.Binding("Note")
                    };


                SerialColumn gv14 = new SerialColumn();
                ResourceDictionary dictionary = new ResourceDictionary();
                dictionary.Source = new Uri("/UniCloud.Infrastructure;component/Resources/CafmStyle.xaml",
                                            UriKind.Relative);
                gv14.Header = "序号";
                gv14.CellStyle = (Style)dictionary["style"];


                gridview.Columns.Add(gv14);
                gridview.Columns.Add(gv1);
                gridview.Columns.Add(gv2);
                gridview.Columns.Add(gv3);
                gridview.Columns.Add(gv4);
                gridview.Columns.Add(gv5);
                gridview.Columns.Add(gv6);
                gridview.Columns.Add(gv7);
                gridview.Columns.Add(gv8);
                gridview.Columns.Add(gv9);
                gridview.Columns.Add(gv10);
                gridview.Columns.Add(gv11);
                gridview.Columns.Add(gv12);
                gridview.Columns.Add(gv13);

            }
            else if (windowsType == "商业数据")
            {
                gridview = new RadGridView();
                gridview.ShowGroupPanel = true;
                gridview.AutoGenerateColumns = false;
                gridview.IsReadOnly = true;
                gridview.IsFilteringAllowed = true;
                gridview.RowIndicatorVisibility = Visibility.Collapsed;
                List<AircraftBusiness> aircraftBusiness = new List<AircraftBusiness>();
                aircraftBusiness.Add((obj as ChangePlan).AircraftBusiness);
                gridview.ItemsSource = aircraftBusiness;
                GridViewDataColumn gv1 = new GridViewDataColumn();
                gv1.Header = "商业数据开始";
                System.Windows.Data.Binding bing1 = new System.Windows.Data.Binding("StartDate");
                bing1.StringFormat = "yyyy/M/d";
                gv1.DataMemberBinding = bing1;

                GridViewDataColumn gv2 = new GridViewDataColumn();
                gv2.Header = "商业数据结束";
                System.Windows.Data.Binding bing2 = new System.Windows.Data.Binding("StartDate");
                bing2.StringFormat = "yyyy/M/d";
                gv2.DataMemberBinding = bing2;

                GridViewDataColumn gv3 = new GridViewDataColumn();
                gv3.Header = "航空公司";
                gv3.DataMemberBinding = new System.Windows.Data.Binding("Airlines.ShortName");

                GridViewDataColumn gv4 = new GridViewDataColumn();
                gv4.Header = "飞机号";
                gv4.DataMemberBinding = new System.Windows.Data.Binding("Aircraft.RegNumber");

                GridViewDataColumn gv5 = new GridViewDataColumn();
                gv5.Header = "机型";
                gv5.DataMemberBinding = new System.Windows.Data.Binding("Aircraft.AircraftType.Name");

                GridViewDataColumn gv6 = new GridViewDataColumn();
                gv6.Header = "座位数";
                gv6.DataMemberBinding = new System.Windows.Data.Binding("SeatingCapacity");

                GridViewDataColumn gv7 = new GridViewDataColumn();
                gv7.Header = "商载（吨）";
                gv7.DataMemberBinding = new System.Windows.Data.Binding("CarryingCapacity");


                SerialColumn gv8 = new SerialColumn();
                ResourceDictionary dictionary = new ResourceDictionary();
                dictionary.Source = new Uri("/UniCloud.Infrastructure;component/Resources/CafmStyle.xaml",
                                            UriKind.Relative);
                gv8.Header = "序号";
                gv8.CellStyle = (Style)dictionary["style"];


                gridview.Columns.Add(gv8);
                gridview.Columns.Add(gv1);
                gridview.Columns.Add(gv2);
                gridview.Columns.Add(gv3);
                gridview.Columns.Add(gv4);
                gridview.Columns.Add(gv5);
                gridview.Columns.Add(gv6);
                gridview.Columns.Add(gv7);

            }
            else //申请
            {

                gridview = new RadGridView();
                gridview.ShowGroupPanel = true;
                gridview.AutoGenerateColumns = false;
                gridview.IsReadOnly = true;
                gridview.IsFilteringAllowed = true;
                gridview.RowIndicatorVisibility = Visibility.Collapsed;
                List<ApprovalHistory> approvalHistories = new List<ApprovalHistory>();
                approvalHistories.Add((obj as PlanHistory).ApprovalHistory);
                gridview.ItemsSource = approvalHistories;
                GridViewDataColumn gv1 = new GridViewDataColumn();
                gv1.Header = "座级";
                System.Windows.Data.Binding bing1 =
                    new System.Windows.Data.Binding("PlanAircraft.AircraftType.AircraftCategory.Regional");
                gv1.DataMemberBinding = bing1;

                GridViewDataColumn gv2 = new GridViewDataColumn();
                gv2.Header = "机型";
                System.Windows.Data.Binding bing2 = new System.Windows.Data.Binding("PlanAircraft.AircraftType.Name");
                gv2.DataMemberBinding = bing2;

                GridViewDataColumn gv3 = new GridViewDataColumn();
                gv3.Header = "引进方式";
                gv3.DataMemberBinding = new System.Windows.Data.Binding("ImportCategory.ActionName");

                GridViewDataColumn gv4 = new GridViewDataColumn();
                gv4.Header = "座位数";
                gv4.DataMemberBinding = new System.Windows.Data.Binding("SeatingCapacity");

                GridViewDataColumn gv5 = new GridViewDataColumn();
                gv5.Header = "商载（吨）";
                gv5.DataMemberBinding = new System.Windows.Data.Binding("CarryingCapacity");


                GridViewDataColumn gv6 = new GridViewDataColumn();
                gv6.Header = "申请交付时间";
                gv6.UniqueName = "performTime";
                gv6.DataMemberBinding = new System.Windows.Data.Binding("RequestDeliver");

                SerialColumn gv7 = new SerialColumn();
                ResourceDictionary dictionary = new ResourceDictionary();
                dictionary.Source = new Uri("/UniCloud.Infrastructure;component/Resources/CafmStyle.xaml",
                                            UriKind.Relative);
                gv7.Header = "序号";
                gv7.CellStyle = (Style)dictionary["style"];


                gridview.Columns.Add(gv7);
                gridview.Columns.Add(gv1);
                gridview.Columns.Add(gv2);
                gridview.Columns.Add(gv3);
                gridview.Columns.Add(gv4);
                gridview.Columns.Add(gv5);
                gridview.Columns.Add(gv6);
            }
            if (gridview != null)
            {

                var radContext = new RadContextMenu();
                radContext.ItemClick += radContext_ItemClick;
                var radMenuItem = new RadMenuItem { Header = "导出数据", };
                radContext.Items.Add(radMenuItem);
                RadContextMenu.SetContextMenu(gridview, radContext); //为控件绑定右键菜单
            }

            grid.Children.Add(gridview);
            return grid;

        }

        /// <summary>
        /// 子窗体导出功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void radContext_ItemClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var radContextMenu = sender as RadContextMenu;
            if (radContextMenu != null)
            {
                var gridview = radContextMenu.UIElement as RadGridView;
                if (gridview != null)
                {
                    gridview.ElementExporting -= this.ElementExporting;
                    gridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            gridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }

                }
            }

        }
        #endregion

        #region Local 方法 GetPlanPerforms --获取计划完成情况

        /// <summary>
        ///  获取计划完成情况
        /// </summary>
        /// <param name="currentPlan">当前计划</param>
        public void GetPlanPerforms(Plan currentPlan)
        {
            decimal planPerformCount = 0; //计划总完成的情况
            decimal planTotalCount = 0; //计划总项
            if (currentPlan.PlanHistories == null || currentPlan.PlanHistories.Count == 0)
            {
                Performance = 0;
            }
            else
            {
                //运营计划的某年执行情况
                Func<OperationPlan, bool> exprOperationPlanImportAndExport = p =>
                    {
                        var performPlan = p as OperationPlan;
                        var isCurrentYearPerform = performPlan.OperationHistory != null &&
                                                   ((performPlan.OperationHistory.StartDate != null &&
                                                     performPlan.OperationHistory.StartDate.Value.Year ==
                                                     currentPlan.Annual.Year) ||
                                                    (performPlan.OperationHistory.EndDate != null &&
                                                     performPlan.OperationHistory.EndDate.Value.Year ==
                                                     currentPlan.Annual.Year));

                        return p.Annual.Year == currentPlan.Annual.Year && isCurrentYearPerform;
                    };
                //变更计划某年执行情况
                Func<ChangePlan, bool> exprChangePlanImportAndExport = p =>
                    {
                        var performPlan = p as ChangePlan;
                        var isCurrentYearPerform = performPlan.AircraftBusiness != null &&
                                                   ((performPlan.AircraftBusiness.StartDate != null &&
                                                     performPlan.AircraftBusiness.StartDate.Value.Year ==
                                                     currentPlan.Annual.Year) ||
                                                    (performPlan.AircraftBusiness.EndDate != null &&
                                                     performPlan.AircraftBusiness.EndDate.Value.Year ==
                                                     currentPlan.Annual.Year));

                        return p.Annual.Year == currentPlan.Annual.Year && isCurrentYearPerform;
                    };
                //运营计划执行统计
                planPerformCount += currentPlan.PlanHistories.OfType<OperationPlan>().Count(exprOperationPlanImportAndExport);
                //变更计划执行统计
                planPerformCount += currentPlan.PlanHistories.OfType<ChangePlan>().Count(exprChangePlanImportAndExport);
                //某年计划执行条数
                planTotalCount = GetPerformPlanHistory(currentPlan).Count;
                //统计执行百分比
                Performance = planTotalCount == 0 ? 0 : Math.Round(planPerformCount * 100 / planTotalCount, 2);
            }
        }

        /// <summary>
        ///计划执行情况
        /// </summary>
        /// <param name="currentPlan"></param>
        /// <param name="plans"></param>
        /// <returns></returns>
        private List<PlanHistory> GetPerformPlanHistory(Plan currentPlan)
        {
            //获取当年执行情况
            Func<PlanHistory, bool> funcPlanHisotry = p => p.Annual.Year == currentPlan.Annual.Year;
            return currentPlan.PlanHistories.ToList().Where(funcPlanHisotry).ToList(); ;
        }
        #endregion

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.plan = this._view.plan as RadGridView;
            this.planDetail = this._view.planDetail as RadGridView;
        }
        #endregion

        #endregion

        #region ViewModel

        #region 加载实体Plan集合

        /// <summary>
        /// Plan集合
        /// </summary>
        public IEnumerable<Plan> ViewPlan
        {
            get
            {
                return this._service.GetAllValidPlan();
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadPlan()
        {
            this._service.LoadPlan(new QueryBuilder<Plan>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewPlan);

                }
            }, null);
        }

        #endregion

        #region 加载实体OperationHistory集合

        /// <summary>
        /// 获取计划历史，由于加载计划历史服务端Includ不到OperationHistory，故需要在前台加载OperationHistory，程序会自动完成匹配
        /// </summary>
        public IEnumerable<OperationHistory> ViewOperationHistory
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<OperationHistory>();
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadOperationHistory()
        {
            this._service.LoadOperationHistory(new QueryBuilder<OperationHistory>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewOperationHistory);
                }
            }, null);
        }

        #endregion

        #region 加载实体AircraftBusiness集合

        /// <summary>
        /// 获取商业数据，由于加载计划历史服务端Includ不到AircraftBusiness，故需要在前台加载AircraftBusiness，程序会自动完成匹配
        /// </summary>
        public IEnumerable<AircraftBusiness> ViewAircraftBusiness
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<AircraftBusiness>();
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraftBusiness()
        {
            this._service.LoadAircraftBusiness(new QueryBuilder<AircraftBusiness>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewAircraftBusiness);
                }
            }, null);
        }

        #endregion

        #region 获取实体集合 ApprovalHistory

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<ApprovalHistory> ViewApprovalHistory
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<ApprovalHistory>();
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadRequest()
        {
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewApprovalHistory);
                }
            }, null);
        }

        #endregion

        #region Property

        #region ViewModel 属性 忙闲状态

        private bool _isBusy;
        /// <summary>
        /// 忙闲状态
        /// </summary>
        public bool IsBusy
        {
            get { return this._isBusy; }
            private set
            {
                if (this._isBusy != value)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => this.IsBusy);
                }
            }
        }

        #endregion

        #region ViewModel 属性 SelPlan --选中的计划

        private Plan _selPlan;
        /// <summary>
        /// 选中的计划
        /// </summary>
        public Plan SelPlan
        {
            get { return _selPlan; }
            set
            {
                _selPlan = value;
                if (value != null)
                {
                    PlanHistoryPerforms = GetPerformPlanHistory(value);
                    GetPlanPerforms(value);
                    PerformAirlineHeader = value.Annual.Year + "年度计划完成情况（执行率：" + Convert.ToString(Performance) + "%）";
                }
                else
                {
                    PerformAirlineHeader = "当前计划完成情况（单位：%）";
                }
                this.RaisePropertyChanged(() => this.SelPlan);


            }
        }
        #endregion

        #region ViewModel 属性 PlanHistoryPerforms --获取执行的计划历史

        private List<PlanHistory> _planHistoryPerforms = new List<PlanHistory>();
        /// <summary>
        /// 获取执行的计划历史
        /// </summary>
        public List<PlanHistory> PlanHistoryPerforms
        {
            get { return _planHistoryPerforms; }
            set
            {

                if (PlanHistoryPerforms != value)
                {
                    _planHistoryPerforms = value;
                    this.RaisePropertyChanged(() => this.PlanHistoryPerforms);

                }
            }
        }
        #endregion

        #region ViewModel 属性 Performance --完成情况

        private decimal _performance;
        /// <summary>
        /// 完成情况
        /// </summary>
        public decimal Performance
        {
            get { return _performance; }
            set
            {

                if (Performance != value)
                {
                    _performance = value;
                    this.RaisePropertyChanged(() => this.Performance);

                }
            }
        }
        #endregion

        #region ViewModel 属性 PerformAirlineHeader --计划执行情况标题

        private string _performAirlineHeader = "当前计划完成情况（单位：%）";
        /// <summary>
        /// 计划执行情况标题
        /// </summary>
        public string PerformAirlineHeader
        {
            get { return _performAirlineHeader; }
            set
            {

                if (PerformAirlineHeader != value)
                {
                    _performAirlineHeader = value;
                    this.RaisePropertyChanged(() => this.PerformAirlineHeader);

                }
            }
        }
        #endregion

        #region ViewModel 属性 IsContextMenuOpen --控制右键菜单的打开

        private bool _isContextMenuOpen = true;
        /// <summary>
        /// 控制右键菜单的打开
        /// </summary>
        public bool IsContextMenuOpen
        {
            get { return this._isContextMenuOpen; }
            set
            {

                if (this._isContextMenuOpen != value)
                {
                    _isContextMenuOpen = value;
                    this.RaisePropertyChanged(() => this.IsContextMenuOpen);

                }
            }
        }
        #endregion

        #endregion

        #region Command

        #region ViewModel 命令 ViewAttachmentCommand --查看附件

        // 查看附件
        public DelegateCommand<object> ViewAttachmentCommand { get; private set; }
        private void OnViewAttachment(object obj)
        {
            Plan plan = obj as Plan;
            if (plan != null)
            {
                AttachmentOperation.OpenAttachment<Plan>(plan);
            }
        }

        public bool CanViewAttachment(object obj)
        {
            return true;
        }
        #endregion

        #region ViewModel 命令 ViewOperationOrAircraftHistoryCommand --查看运行历史或者商业数据明细或者申请

        // 查看运行历史或者商业数据明细
        public DelegateCommand<object> ViewOperationOrAircraftHistoryCommand { get; private set; }
        private void OnViewOperationOrAircraftHistory(object obj)
        {
            if (obj != null && obj.GetType() == typeof(OperationPlan))
            {
                var planHistory = obj as OperationPlan;
                //RadWindow 标题
                radWindow.Header = "运营历史";
                //RadWindow 内容
                radWindow.Content = CreateRadWindows("运营历史", planHistory);
            }
            else if (obj != null && obj.GetType() == typeof(ChangePlan))
            {
                var planHistory = obj as ChangePlan;
                //RadWindow 标题
                radWindow.Header = "商业数据历史";
                //RadWindow 内容
                radWindow.Content = CreateRadWindows("商业数据", planHistory);

            }

            //RadWindow单例
            if (!radWindow.IsOpen)
            {
                new CommonMethod().ShowRadWindow(radWindow);
            }

        }

        public bool CanViewOperationOrAircraftHistory(object obj)
        {
            return true;
        }
        #endregion

        #region ViewModel 命令 ViewOpertationApprovalHistoryCommand --查看批文历史

        // 查看批文历史
        public DelegateCommand<object> ViewOpertationApprovalHistoryCommand { get; private set; }
        private void OnViewOpertationApprovalHistory(object obj)
        {
            var planHistory = obj;
            if (planHistory != null)
            {
                //RadWindow 标题
                radWindow.Header = "申请";
                //RadWindow 内容
                radWindow.Content = CreateRadWindows("申请", planHistory);

                //RadWindow单例
                if (!radWindow.IsOpen)
                {
                    new CommonMethod().ShowRadWindow(radWindow);
                }
            }

        }

        public bool CanViewOpertationApprovalHistory(object obj)
        {
            return true;
        }
        #endregion

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "PlanDetailExport" && planDetail != null)
            {
                planDetail.ElementExporting -= this.ElementExporting;
                planDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        planDetail.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "PlanExport" && plan != null)
            {
                plan.ElementExporting -= this.ElementExporting;
                plan.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);

                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        plan.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
        }
        /// <summary>
        /// 设置导出样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ElementExporting(object sender, GridViewElementExportingEventArgs e)
        {
            e.Width = 120;
            if (e.Element == ExportElement.Row)
            { i = 1; }
            else if (e.Element == ExportElement.Cell &&
                e.Value != null)
            {
                var gridViewColumn = e.Context as GridViewColumn;
                if (gridViewColumn != null && gridViewColumn.UniqueName == "performTime")
                {
                    e.Value = DateTime.Parse(e.Value.ToString()).ToString("yyyy年M月");
                }
                i++;
            }

        }

        bool CanExportGridView(object sender)
        {
            return true;
        }

        #endregion

        #endregion

        #region Methods

        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }
        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();
            ViewAttachmentCommand = new DelegateCommand<object>(this.OnViewAttachment, this.CanViewAttachment);
            ViewOperationOrAircraftHistoryCommand = new DelegateCommand<object>(this.OnViewOperationOrAircraftHistory, this.CanViewOperationOrAircraftHistory);
            ViewOpertationApprovalHistoryCommand = new DelegateCommand<object>(this.OnViewOpertationApprovalHistory, this.CanViewOpertationApprovalHistory);
            this.ExportGridViewCommand = new DelegateCommand<object>(this.OnExportGridView, this.CanExportGridView);//导出GridView数据
        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {

        }

        #endregion

        #region INavigationAware Members

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadPlan();
            LoadAircraftBusiness();
            LoadOperationHistory();
            LoadRequest();
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            if (radWindow.IsOpen)
            {
                radWindow.Close();
            }
            continuationCallback(true);

        }

        #endregion
    }
}

using CAAC.Fleet.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Charting;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmPerformAnalyseViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmPerformAnalyseViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmPerformAnalyseViewModel()
        {
            //弹出窗口的设置
            radWindow.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            radWindow.Height = 200;
            radWindow.Width = 551;
            radWindow.ResizeMode = ResizeMode.CanResize;

        }

        #region Local

        [Import]
        public IRegionManager regionManager;

        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmPerformAnalyseView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmPerformAnalyseView>();
        //初始化RadWindow 需添加引用集 Telerik.Windows.Controls.Navigation
        private RadWindow radWindow = new RadWindow();

        CommonMethod commonmethod = new CommonMethod();
        private Grid TrendGrid;//趋势图区域
        private RadGridView ExportRadgridview, planDetail, plan; //初始化RadGridView

        #region Property



        #endregion

        #region Method

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.TrendGrid = _view.TrendGrid as Grid;
            this.plan = _view.plan as RadGridView;
            this.planDetail = _view.planDetail as RadGridView;
        }

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
                dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
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
                dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
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
            else  //申请
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
                System.Windows.Data.Binding bing1 = new System.Windows.Data.Binding("PlanAircraft.AircraftType.AircraftCategory.Regional");
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
                gv5.Header = "机型";
                gv5.DataMemberBinding = new System.Windows.Data.Binding("AircraftType.Name");


                GridViewDataColumn gv6 = new GridViewDataColumn();
                gv6.Header = "商载（吨）";
                gv6.DataMemberBinding = new System.Windows.Data.Binding("CarryingCapacity");


                GridViewDataColumn gv7 = new GridViewDataColumn();
                gv7.Header = "申请交付时间";
                gv7.DataMemberBinding = new System.Windows.Data.Binding("RequestDeliver");

                SerialColumn gv8 = new SerialColumn();
                ResourceDictionary dictionary = new ResourceDictionary();
                dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
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
            grid.Children.Add(gridview);
            return grid;

        }
        #endregion

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            if (!this.IsBusy)
            {
                FleetPerformAnalyseTrendCollection = CreateFleetPerformAnalyseTrendCollection();
            }
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合  XmlConfig

        /// <summary>
        /// XML文件集合
        /// </summary>
        public IEnumerable<XmlConfig> XmlConfigCollection
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<XmlConfig>();
            }
        }

        private bool _isBusyXmlConfig = true;
        public bool IsBusyXmlConfig
        {
            get { return this._isBusyXmlConfig; }
            private set
            {
                if (this._isBusyXmlConfig != value)
                {
                    this._isBusyXmlConfig = value;
                }
            }
        }


        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadXmlConfig()
        {
            this.IsBusy = true;
            this._service.LoadXmlConfig(new QueryBuilder<XmlConfig>(), lo =>
            {
                this.IsBusyXmlConfig = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyXmlConfig;
                if (lo.Error != null)
                {
                    // 处理加载失败
                }
                else
                {
                    this.RaisePropertyChanged(() => this.XmlConfigCollection);
                    InitializeData();
                }
            }, null);
        }

        #endregion

        #region 加载实体集合 Plan

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public List<Plan> PlanCollection
        {

            get { return this._service.EntityContainer.GetEntitySet<Plan>().Where(e => e.IsCurrentVersion == true).ToList(); }

        }

        private bool _isBusyPlan = true;
        public bool IsBusyPlan
        {
            get { return this._isBusyPlan; }
            private set
            {
                if (this._isBusyPlan != value)
                {
                    this._isBusyPlan = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadPlan()
        {
            this.IsBusy = true;
            this._service.LoadPlan(new QueryBuilder<Plan>(), lo =>
            {
                this.IsBusyPlan = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyXmlConfig;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.PlanCollection);
                    InitializeData();
                }
            }, null);
        }

        #endregion

        #region Property

        #region ViewModel 属性 IsBusy

        private bool _isBusy;
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

        #region ViewModel 属性 FleetPerformAnalyseTrendCollection --计划执行趋势图的数据源集合

        private List<FleetPerformAnalyseTrend> _FleetPerformAnalyseTrendCollection;
        /// <summary>
        /// 计划执行趋势图的数据源集合
        /// </summary>
        public List<FleetPerformAnalyseTrend> FleetPerformAnalyseTrendCollection
        {
            get { return _FleetPerformAnalyseTrendCollection; }
            set
            {

                if (FleetPerformAnalyseTrendCollection != value)
                {
                    _FleetPerformAnalyseTrendCollection = value;

                    RadCartesianChart radcartesianchart = this.TrendGrid.Children[0] as RadCartesianChart;
                    StackPanel stackpanel = (this.TrendGrid.Children[1] as ScrollViewer).Content as StackPanel;

                    radcartesianchart.Series.Clear();
                    stackpanel.Children.Clear();

                    if (FleetPerformAnalyseTrendCollection != null)
                    {
                        int i = 0;
                        foreach (var GroupItem in FleetPerformAnalyseTrendCollection.GroupBy(p => p.Airlines).ToList())
                        {
                            Telerik.Windows.Controls.ChartView.LineSeries line = new Telerik.Windows.Controls.ChartView.LineSeries();
                            line.StrokeThickness = 3;
                            line.DisplayName = GroupItem.Key;
                            line.Stroke = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            line.CategoryBinding = commonmethod.CreateBinding("Year");
                            line.ValueBinding = commonmethod.CreateBinding("Percent");
                            line.ItemsSource = GroupItem.ToList();
                            if (i != 0)
                            {
                                line.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                this._view.TrendLinearAxis.ElementBrush = line.Stroke;
                            }
                            line.PointTemplate = this._view.Resources["PointTemplate"] as DataTemplate;
                            line.TrackBallInfoTemplate = this._view.Resources["TrackBallInfoTemplate"] as DataTemplate;
                            radcartesianchart.Series.Add(line);

                            StackPanel panel = new StackPanel();
                            panel.Margin = new Thickness(5, 5, 5, 5);
                            panel.Orientation = Orientation.Horizontal;
                            panel.Background = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            CheckBox checkbox = new CheckBox();
                            if (i == 0)
                            {
                                checkbox.IsChecked = true;
                            }
                            else
                            {
                                checkbox.IsChecked = false;
                            }
                            checkbox.Checked += checkbox_Checked;
                            checkbox.Unchecked += checkbox_Unchecked;
                            checkbox.Content = GroupItem.Key;
                            checkbox.Foreground = new SolidColorBrush(Colors.White);
                            checkbox.VerticalAlignment = VerticalAlignment.Center;
                            checkbox.Style = this._view.Resources["LegengCheckBoxStyle"] as Style;
                            panel.Children.Add(checkbox);
                            stackpanel.Children.Add(panel);
                            i = 1;
                        }
                    }

                    //控制趋势图的滚动条
                    int datetimecount = FleetPerformAnalyseTrendCollection.Select(p => p.Year).Distinct().Count();
                    if (FleetPerformAnalyseTrendCollection != null && datetimecount >= 12)
                    {
                        this._view.TrendCategoricalAxis.MajorTickInterval = datetimecount / 6;
                    }
                    else
                    {
                        this._view.TrendCategoricalAxis.MajorTickInterval = 1;
                    }

                    this.RaisePropertyChanged(() => this.FleetPerformAnalyseTrendCollection);

                }
            }
        }
        #endregion

        #region ViewModel 属性 Performance --完成情况

        private decimal _performance = 0;
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

        #region ViewModel 属性 SelPlan --选中计划当前行

        private Plan _selPlan;
        /// <summary>
        /// 选中计划当前行
        /// </summary>
        public Plan SelPlan
        {
            get { return _selPlan; }
            set
            {
                _selPlan = value;
                this.RaisePropertyChanged(() => this.SelPlan);

                //当选择项发生改变，执行的年度的历史也随之改变
                if (SelPlan == null)
                {
                    PerformPlanHistories = null;
                }
                else
                {
                    //当年的计划，到下一年年度才执行
                    PerformPlanHistories = SelPlan.PlanHistories.Where(p => p.Annual.Year == (SelPlan.Annual.Year)).ToList();
                }

                GetPlanPerforms(value);
                if (SelPlan != null)
                {
                    SelectedName = SelPlan.Airlines.ShortName + " " + (SelPlan.Annual.Year) + "年计划执行情况(执行率：" +
                        Convert.ToString(Performance) + "%) ";
                }
                else
                {
                    SelectedName = "计划执行情况(单位：%)";
                }

            }
        }
        #endregion

        #region ViewModel 属性 SelectedName --所选计划的时间和名称

        private string _SelectedName = "计划执行情况(单位：%)";
        /// <summary>
        /// 所选计划的时间和名称
        /// </summary>
        public string SelectedName
        {
            get { return _SelectedName; }
            set
            {

                if (SelectedName != value)
                {
                    _SelectedName = value;
                    this.RaisePropertyChanged(() => this.SelectedName);

                }
            }
        }
        #endregion

        #region ViewModel 属性 PerformPlanHistories --选择计划当年度完成的计划历史

        private List<PlanHistory> _PerformPlanHistories;
        /// <summary>
        /// 选择计划当年度完成的计划历史
        /// </summary>
        public List<PlanHistory> PerformPlanHistories
        {
            get { return _PerformPlanHistories; }
            set
            {

                if (PerformPlanHistories != value)
                {
                    _PerformPlanHistories = value;
                    this.RaisePropertyChanged(() => this.PerformPlanHistories);

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

        #region ViewModel 命令 OpenAttachDocCommand --打开附件

        // 打开附件
        public DelegateCommand<object> OpenAttachDocCommand { get; private set; }
        private void OnOpenAttachDoc(object obj)
        {
            Plan plan = obj as Plan;
            if (plan != null)
            {
                AttachmentOperation.OpenAttachment<Plan>(plan);
            }

        }
        private bool _canOpenAttachDoc = true;
        public bool CanOpenAttachDoc(object obj)
        {
            return _canOpenAttachDoc;
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
                commonmethod.ShowRadWindow(radWindow);
            }

        }
        private bool _canViewOperationOrAircraftHistory = true;
        public bool CanViewOperationOrAircraftHistory(object obj)
        {
            return _canViewOperationOrAircraftHistory;
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
                    commonmethod.ShowRadWindow(radWindow);
                }
            }

        }
        private bool _canViewOpertationApprovalHistory = true;
        public bool CanViewOpertationApprovalHistory(object obj)
        {
            return _canViewOpertationApprovalHistory;
        }
        #endregion

        #region ViewModel 命令 --导出图表

        public DelegateCommand<object> ExportCommand { get; private set; }
        private void OnExport(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Header.ToString() == "导出源数据" && ExportRadgridview != null)
            {
                ExportRadgridview.ElementExporting -= this.ElementExporting;
                ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                if (menu.Name == "TrendGridData")
                {
                    using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                        }
                    }
                }
            }
            else if (menu != null && menu.Header.ToString() == "导出图片")
            {
                if (menu.Name == "TrendGridImage")
                {
                    //导出图片
                    if (this.TrendGrid != null)
                    {
                        commonmethod.ExportToImage(this.TrendGrid);
                    }
                }
            }
        }
        private bool _canExport = true;
        bool CanExport(object sender)
        {
            return this._canExport;
        }

        #endregion

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "PlanExport" && plan != null)
            {
                plan.ElementExporting -= this.ElementExporting;
                plan.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        plan.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "PlanDetailExport" && planDetail != null)
            {
                planDetail.ElementExporting -= this.ElementExporting;
                planDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        planDetail.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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
        }
        private bool _canExportGridView = true;
        bool CanExportGridView(object sender)
        {
            return this._canExportGridView;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// 获取计划执行趋势图的数据源集合
        /// </summary>
        /// <returns>计划趋势图的数据源集合</returns>
        private List<FleetPerformAnalyseTrend> CreateFleetPerformAnalyseTrendCollection()
        {
            List<FleetPerformAnalyseTrend> collection = new List<FleetPerformAnalyseTrend>();

            #region 计划执行XML文件的读写
            var xmlconfig = XmlConfigCollection.FirstOrDefault(p => p.ConfigType == "计划执行");

            XmlConfig colorconfig = this.XmlConfigCollection.FirstOrDefault(p => p.ConfigType == "颜色配置");
            XElement airlinecolor = null;
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
            {
                airlinecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
            }

            if (xmlconfig != null)
            {
                XElement xelement = xmlconfig.XmlContent;
                if (xelement != null)
                {
                    foreach (XElement datetime in xelement.Descendants("DateTime"))
                    {
                        string CurrentTime = datetime.Attribute("EndOfMonth").Value;

                        foreach (XElement type in datetime.Descendants("Type"))
                        {
                            if (type.Attribute("TypeName").Value == "航空公司")
                            {
                                foreach (XElement item in type.Descendants("Item"))
                                {
                                    FleetPerformAnalyseTrend fleetperformanalysetrend = new FleetPerformAnalyseTrend();
                                    fleetperformanalysetrend.Airlines = item.Attribute("Name").Value;
                                    if (airlinecolor != null)
                                    {
                                        fleetperformanalysetrend.Color = airlinecolor.Descendants("Item")
                                            .FirstOrDefault(p => p.Attribute("Name").Value == fleetperformanalysetrend.Airlines).Attribute("Color").Value;
                                    }
                                    fleetperformanalysetrend.Year = CurrentTime;
                                    fleetperformanalysetrend.Percent = Convert.ToDecimal(item.Value);
                                    fleetperformanalysetrend.PercentTip = item.Value + "%";
                                    collection.Add(fleetperformanalysetrend);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            //创建RadGridView
            Dictionary<string, string> columnsList = new Dictionary<string, string>();
            columnsList.Add("Year", "年份");
            columnsList.Add("Airlines", "航空公司");
            columnsList.Add("PercentTip", "完成情况");
            ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, collection, "Aircraft");
            if (collection.Count() > 0)
            {
                return collection;
            }
            else
            {
                return null;
            }

        }

        #region 获取选中计划的完成情况  暂无用

        /// <summary>
        /// 获取指定航空公司和年份的计划完成情况的百分比
        /// </summary>
        /// <param name="EnumerablePlan"></param>
        /// <returns></returns>
        private decimal GetPlanPerformance(IEnumerable<Plan> EnumerablePlan)
        {
            decimal Percent = 0;

            if (EnumerablePlan != null && EnumerablePlan.Count() > 0)
            {
                Plan plan = EnumerablePlan.FirstOrDefault();
                decimal Amount = plan.PlanHistories.Count();
                decimal Finish = plan.PlanHistories.OfType<OperationPlan>().Where(p => p.OperationHistory != null).Count() + plan.PlanHistories.OfType<ChangePlan>().Where(p => p.AircraftBusiness != null).Count();
                if (Amount != 0)
                {
                    Percent = Math.Round(Finish * 100 / Amount, 2);
                }
            }
            return Percent;
        }
        #endregion

        #region  获取指定计划完成情况

        /// <summary>
        /// 获取指定计划的完成情况
        /// </summary>
        public void GetPlanPerforms(Plan currentPlan)
        {
            decimal Finish = 0; //年度的完成计划历史数
            decimal Amount = 0; //年度的所有计划历史数
            if (currentPlan == null || currentPlan.PlanHistories == null || currentPlan.PlanHistories.Count == 0)
            {
                Performance = 0;
            }
            else
            {
                var PlanHistories = currentPlan.PlanHistories.Where(p => p.Annual.Year == currentPlan.Annual.Year);
                if (PlanHistories != null && PlanHistories.Count() > 0)
                {
                    //统计计划第一年的所有计划历史
                    Amount = PlanHistories.Count();
                    Finish = PlanHistories.OfType<OperationPlan>().Count(p => p.OperationHistory != null) +
                        PlanHistories.OfType<ChangePlan>().Count(p => p.AircraftBusiness != null);
                    Performance = Math.Round(Finish * 100 / Amount, 2);
                }
            }
        }

        #endregion


        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }


        #endregion

        #region Class

        /// <summary>
        /// 计划执行的趋势对象
        /// </summary>
        public class FleetPerformAnalyseTrend
        {
            public FleetPerformAnalyseTrend()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string Airlines { get; set; }//航空公司的名称
            public string Color { get; set; }//航空公司的颜色
            public decimal Percent { get; set; }//计划完成的百分比
            public string PercentTip { get; set; }//百分比的显示
            public string Year { get; set; }//时间年份
        }


        /// <summary>
        /// 控制趋势图中折线（饼状）的显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                Grid grid = (((checkbox.Parent as StackPanel).Parent as StackPanel).Parent as ScrollViewer).Parent as Grid;
                (this.TrendGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 控制趋势图中折线（饼状）的隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                Grid grid = (((checkbox.Parent as StackPanel).Parent as StackPanel).Parent as ScrollViewer).Parent as Grid;
                (this.TrendGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;

            }
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.ExportCommand = new DelegateCommand<object>(this.OnExport, this.CanExport);//导出图表源数据（Source data）
            this.ExportGridViewCommand = new DelegateCommand<object>(this.OnExportGridView, this.CanExportGridView);
            SubscribeEvent();
            ViewModelInitializer();

            ViewOperationOrAircraftHistoryCommand = new DelegateCommand<object>(this.OnViewOperationOrAircraftHistory, this.CanViewOperationOrAircraftHistory);
            ViewOpertationApprovalHistoryCommand = new DelegateCommand<object>(this.OnViewOpertationApprovalHistory, this.CanViewOpertationApprovalHistory);


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
            LoadXmlConfig();
            LoadPlan();
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

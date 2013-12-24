using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
//using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Data;
using System.ServiceModel.DomainServices.Client;
using System.Collections;
using System.Windows.Media;
using UniCloud.AFRP.Operation;
using UniCloud.Fleet.Services;
using UniCloud.Fleet.Models;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Converters;
using Microsoft.Practices.Prism.ViewModel;
using Telerik.Windows;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpFleetTrendPnrViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpFleetTrendPnrViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpFleetTrendPnrViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly UniCloud.AFRP.Views.AfrpFleetTrendPnrView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpFleetTrendPnrView>();
        CommonMethod commonmethod = new CommonMethod();
        private Grid LineGrid, BarGrid, AircraftPieGrid, SeatPieGrid, LoadPieGrid;//飞机数饼图区域， 座位数饼图区域， 商载量区域
        private RadDateTimePicker StartDateTimePicker, EndDateTimePicker;//开始时间控件， 结束时间控件
        private int i; //导出数据源格式判断
        private RadGridView ExportRadgridview, aircraftDetail; //初始化RadGridView

        #region Property

        private RadWindow AircraftWindow = new RadWindow(); //用于单击飞机数饼状图的用户提示
        private RadWindow SeatWindow = new RadWindow(); //用于单击座位数饼状图的用户提示
        private RadWindow LoadWindow = new RadWindow(); //用于单击商载量饼状图的用户提示

        #region  属性 SelectedTime --所选的时间点

        private string _SelectedTime = "所选时间";
        /// <summary>
        /// 所选的时间点
        /// </summary>
        public string SelectedTime
        {
            get { return _SelectedTime; }
            set
            {

                if (SelectedTime != value)
                {
                    _SelectedTime = value;
                    AircraftWindow.Close();
                    SeatWindow.Close();
                    LoadWindow.Close();
                    if (SelectedTime == "所选时间")
                    {
                        SelectedTimeAircraft = "所选时间的飞机分布图";
                        SelectedTimeSeat = "所选时间的座位分布图";
                        SelectedTimeLoad = "所选时间的商载分布图";
                    }
                    else
                    {
                        SelectedTimeAircraft = SelectedTime + "末的飞机分布图";
                        SelectedTimeSeat = SelectedTime + "末的座位分布图";
                        SelectedTimeLoad = SelectedTime + "末的商载分布图";
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Method

        /// <summary>
        /// 根据选中饼图的航空公司弹出相应的数据列表窗体
        /// </summary>
        /// <param name="selectedItem">选中点</param>
        /// <param name="radwindow">弹出窗体</param>
        /// <param name="header">窗体标示</param>
        private void GetGridViewDataSourse(PieDataPoint selectedItem, RadWindow radwindow, string header)
        {

            if (selectedItem != null && radwindow != null)
            {
                FleetAircraft fleetaircraft = selectedItem.DataItem as FleetAircraft;
                DateTime time = Convert.ToDateTime(SelectedTime).AddMonths(1).AddDays(-1);
                var aircraft = this.ViewAircraft.Where(o => o.OperationHistories.Any(a =>
                    (a.Airlines.ShortName == this._service.CurrentAirlines.ShortName || a.Airlines.SubType == 2)
                    && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))
                    && o.AircraftBusinesses.Any(a =>
                        a.AircraftType.AircraftCategory.Category == "客机" && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                List<Aircraft> airlineAircrafts = new List<Aircraft>();
                if (fleetaircraft.Aircraft == this._service.CurrentAirlines.ShortName)
                {
                    airlineAircrafts = aircraft.Where(p =>
                    {
                        var aircraftbusiness = p.AircraftBusinesses.FirstOrDefault(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time));
                        if (aircraftbusiness == null) return false;
                        if (aircraftbusiness.AircraftType.AircraftCategory.Category != "客机") return false;

                        var operationHistory = p.OperationHistories.FirstOrDefault(pp => pp.Airlines.ShortName == this._service.CurrentAirlines.ShortName && pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time));
                        if (operationHistory == null)
                        {
                            return false;
                        }
                        else if (operationHistory.SubOperationHistories.Count <= 0)
                        {
                            return true;
                        }
                        else
                        {
                            var suboperationCategory = operationHistory.SubOperationHistories.Where(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time));
                            if (suboperationCategory == null || suboperationCategory.Count() == 0) return true;
                            return suboperationCategory.Any(a => a.Airlines.ShortName == this._service.CurrentAirlines.ShortName);
                        }

                    }).ToList();
                }
                else
                {
                    //分子公司的筛选
                    var aircraftSubCompany = aircraft.Where(p =>
                    {
                        var aircraftbusiness = p.AircraftBusinesses.FirstOrDefault(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time));
                        if (aircraftbusiness == null) return false;
                        if (aircraftbusiness.AircraftType.AircraftCategory.Category != "客机") return false;
                        return p.OperationHistories.Any(pp =>
                                                            pp.Airlines.ShortName == fleetaircraft.Aircraft &&
                                                            pp.Airlines.SubType == 2
                                                            && pp.StartDate <= time &&
                                                   !(pp.EndDate != null && pp.EndDate < time));
                    }).ToList();

                    //分公司的筛选     
                    var aircraftFiliale = aircraft.Where(p =>
                    {
                        var aircraftbusiness = p.AircraftBusinesses.FirstOrDefault(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time));
                        if (aircraftbusiness == null) return false;
                        if (aircraftbusiness.AircraftType.AircraftCategory.Category != "客机") return false;

                        var operationHistory = p.OperationHistories.FirstOrDefault(pp => pp.Airlines.ShortName == this._service.CurrentAirlines.ShortName && pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time));
                        if (operationHistory == null ||
                            operationHistory.SubOperationHistories.Count <= 0)
                        {
                            return false;
                        }
                        else
                        {
                            return
                                operationHistory.SubOperationHistories.Any(
                                    a =>
                                    a.Airlines.ShortName ==
                                    fleetaircraft.Aircraft &&
                                    a.StartDate <= time &&
                                    !(a.EndDate != null && a.EndDate < time));

                        }
                    }).ToList();
                    airlineAircrafts = aircraftSubCompany.Union(aircraftFiliale).ToList();
                };
                //找到子窗体的RadGridView，并为其赋值
                RadGridView rgv = radwindow.Content as RadGridView;
                rgv.ItemsSource = commonmethod.GetAircraftByTime(airlineAircrafts, time);
                radwindow.Header = fleetaircraft.Aircraft + header + "：" + fleetaircraft.ToolTip;
                if (!radwindow.IsOpen)
                {
                    commonmethod.ShowRadWindow(radwindow);
                }
            }
        }



        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.LineGrid = this._view.LineGrid;
            this.BarGrid = this._view.BarGrid;
            this.AircraftPieGrid = this._view.AircraftPieGrid;
            this.SeatPieGrid = this._view.SeatPieGrid;
            this.LoadPieGrid = this._view.LoadPieGrid;
            this.aircraftDetail = _view.aircraftDetail as RadGridView;
            //控制界面起止时间控件的字符串格式化
            this.StartDateTimePicker = _view.StartDateTimePicker as RadDateTimePicker;
            this.EndDateTimePicker = _view.EndDateTimePicker as RadDateTimePicker;
            StartDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
            EndDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
        }


        /// <summary>
        /// 初始化提示窗体
        /// </summary>
        public void InitalizerRadWindows(RadWindow radwindow, string WindowsName, int length)
        {
            //运营计划子窗体的设置
            radwindow.Name = WindowsName;
            radwindow.Top = length;
            radwindow.Left = length;
            radwindow.Height = 250;
            radwindow.Width = 500;
            radwindow.ResizeMode = ResizeMode.CanResize;
            radwindow.Content = commonmethod.CreatOperationGridView();
            radwindow.Closed += new EventHandler<WindowClosedEventArgs>(radwindow_Closed);
        }


        /// <summary>
        /// 弹出窗体关闭时，取消相应饼图的弹出项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void radwindow_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow radwindow = sender as RadWindow;
            Grid grid = new Grid();
            if (radwindow.Name == "Aircraft")
            {
                grid = this.AircraftPieGrid;
            }
            else if (radwindow.Name == "Seat")
            {
                grid = this.SeatPieGrid;
            }
            else if (radwindow.Name == "Load")
            {
                grid = this.LoadPieGrid;
            }

            //更改对应饼图的突出显示
            foreach (var item in (grid.Children[0] as RadPieChart).Series[0].DataPoints)
            {
                item.IsSelected = false;
            }
            //更改对应饼图的标签大小
            foreach (var item in ((grid.Children[1] as ScrollViewer).Content as StackPanel).Children)
            {
                System.Windows.Shapes.Rectangle rectangle = (item as StackPanel).Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 15;
                rectangle.Height = 15;
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            if (!this.IsBusy)
            {
                CreatFleetAircraftTrendCollection();
                SetRadCartesianChartColor();
            }
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合 XmlConfig

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<XmlConfig> ViewXmlConfig
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
                this.IsBusy = this.IsBusyXmlConfig || this.IsBusyAircraft;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewXmlConfig);
                    InitializeData();
                }
            }, null);
        }

        #endregion

        #region 加载实体集合 Aircraft

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Aircraft> ViewAircraft
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Aircraft>();
            }
        }

        private bool _isBusyAircraft = true;
        public bool IsBusyAircraft
        {
            get { return this._isBusyAircraft; }
            private set
            {
                if (this._isBusyAircraft != value)
                {
                    this._isBusyAircraft = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraft()
        {
            this.IsBusy = true;
            this._service.LoadAircraft(new QueryBuilder<Aircraft>(), lo =>
            {
                this.IsBusyAircraft = false;
                this.IsBusy = this.IsBusyAircraft || this.IsBusyXmlConfig;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewAircraft);
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

        #region ViewModel 属性 FleetAircraftTrendLineCollection --折线图的统计总数集合

        private List<FleetAircraftTrend> _FleetAircraftTrendLineCollection;
        /// <summary>
        /// 折线图的统计总数集合
        /// </summary>
        public List<FleetAircraftTrend> FleetAircraftTrendLineCollection
        {
            get { return _FleetAircraftTrendLineCollection; }
            set
            {

                if (FleetAircraftTrendLineCollection != value)
                {
                    _FleetAircraftTrendLineCollection = value;

                    if (FleetAircraftTrendLineCollection != null && FleetAircraftTrendLineCollection.Count() >= 12)
                    {
                        this._view.LineCategoricalAxis.MajorTickInterval = FleetAircraftTrendLineCollection.Count() / 6;
                        this._view.BarCategoricalAxis.MajorTickInterval = FleetAircraftTrendLineCollection.Count() / 6;
                    }
                    else
                    {
                        this._view.LineCategoricalAxis.MajorTickInterval = 1;
                        this._view.BarCategoricalAxis.MajorTickInterval = 1;
                    }
                    this.RaisePropertyChanged(() => this.FleetAircraftTrendLineCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetAircraftTrendBarCollection --柱状图的净增数集合

        private List<FleetAircraftTrend> _FleetAircraftTrendBarCollection;
        /// <summary>
        /// 柱状图的净增数集合
        /// </summary>
        public List<FleetAircraftTrend> FleetAircraftTrendBarCollection
        {
            get { return _FleetAircraftTrendBarCollection; }
            set
            {

                if (FleetAircraftTrendBarCollection != value)
                {
                    _FleetAircraftTrendBarCollection = value;
                    this.RaisePropertyChanged(() => this.FleetAircraftTrendBarCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftCollection --选中时间点的飞机数据集合

        private List<Aircraft> _AircraftCollection;
        /// <summary>
        /// 选中时间点的飞机数分布集合
        /// </summary>
        public List<Aircraft> AircraftCollection
        {
            get { return _AircraftCollection; }
            set
            {

                if (AircraftCollection != value)
                {
                    _AircraftCollection = value;
                    this.RaisePropertyChanged(() => this.AircraftCollection);
                    if (SelectedTime == "所选时间")
                    {
                        SelectedTimeAircraftList = "所选时间的飞机详细";
                    }
                    else
                    {
                        if (AircraftCollection == null || AircraftCollection.Count() == 0)
                        {
                            SelectedTimeAircraftList = SelectedTime + "末的飞机详细（0架）";
                        }
                        else
                        {
                            SelectedTimeAircraftList = SelectedTime + "末的飞机详细（" + AircraftCollection.Count() + "架）";
                        }
                    }
                }
            }
        }
        #endregion

        #region ViewModel 属性 SelectedTimeAircraft --飞机数饼图的标识提示

        private string _SelectedTimeAircraft = "所选时间的飞机分布图";
        /// <summary>
        /// 飞机数饼图的标识提示
        /// </summary>
        public string SelectedTimeAircraft
        {
            get { return _SelectedTimeAircraft; }
            set
            {

                if (SelectedTimeAircraft != value)
                {
                    _SelectedTimeAircraft = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeAircraft);
                }
            }
        }
        #endregion

        #region ViewModel 属性 SelectedTimeSeat--座位数饼图的标识提示

        private string _SelectedTimeSeat = "所选时间的座位分布图";
        /// <summary>
        /// 座位数饼图的标识提示
        /// </summary>
        public string SelectedTimeSeat
        {
            get { return _SelectedTimeSeat; }
            set
            {

                if (SelectedTimeSeat != value)
                {
                    _SelectedTimeSeat = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeSeat);
                }
            }
        }
        #endregion

        #region ViewModel 属性 SelectedTimeLoad --商载量饼图的标识提示

        private string _SelectedTimeLoad = "所选时间的商载分布图";
        /// <summary>
        /// 商载量饼图的标识提示
        /// </summary>
        public string SelectedTimeLoad
        {
            get { return _SelectedTimeLoad; }
            set
            {

                if (SelectedTimeLoad != value)
                {
                    _SelectedTimeLoad = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeLoad);
                }
            }
        }
        #endregion

        #region ViewModel 属性 SelectedTimeAircraft --飞机列表的标识提示

        private string _SelectedTimeAircraftList = "所选时间的飞机详细";
        /// <summary>
        /// 飞机列表的标识提示
        /// </summary>
        public string SelectedTimeAircraftList
        {
            get { return _SelectedTimeAircraftList; }
            set
            {

                if (SelectedTimeAircraftList != value)
                {
                    _SelectedTimeAircraftList = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeAircraftList);
                }
            }
        }
        #endregion

        #region ViewModel 属性 SelectedIndex --时间的统计方式

        private int _SelectedIndex = 0;
        /// <summary>
        /// 时间的统计方式
        /// </summary>
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {

                if (SelectedIndex != value)
                {
                    _SelectedIndex = value;
                    CreatFleetAircraftTrendCollection();
                    this.RaisePropertyChanged(() => this.SelectedIndex);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftMaxValue --柱状图中飞机数轴的最大值

        private double _AircraftMaxValue = 0;
        /// <summary>
        /// 柱状图中飞机数轴的最大值
        /// </summary>
        public double AircraftMaxValue
        {
            get { return _AircraftMaxValue; }
            set
            {

                if (AircraftMaxValue != value)
                {
                    _AircraftMaxValue = value;
                    this.RaisePropertyChanged(() => this.AircraftMaxValue);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftMinValue --柱状图中飞机数轴的最小值

        private double _AircraftMinValue = 0;
        /// <summary>
        /// 柱状图中飞机数轴的最小值
        /// </summary>
        public double AircraftMinValue
        {
            get { return _AircraftMinValue; }
            set
            {
                if (AircraftMinValue != value)
                {
                    _AircraftMinValue = value;
                    this.RaisePropertyChanged(() => this.AircraftMinValue);
                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftStep --柱状图中飞机数轴的节点距离

        private int _AircraftStep = 20;
        /// <summary>
        /// 柱状图中飞机数轴的节点距离
        /// </summary>
        public int AircraftStep
        {
            get { return _AircraftStep; }
            set
            {

                if (AircraftStep != value)
                {
                    _AircraftStep = value;
                    this.RaisePropertyChanged(() => this.AircraftStep);

                }
            }
        }
        #endregion

        #region ViewModel 属性 LoadMaxValue --柱状图中商载量轴的最大值

        private double _LoadMaxValue = 0;
        /// <summary>
        /// 柱状图中商载量轴的最大值
        /// </summary>
        public double LoadMaxValue
        {
            get { return _LoadMaxValue; }
            set
            {

                if (LoadMaxValue != value)
                {
                    _LoadMaxValue = value;
                    this.RaisePropertyChanged(() => this.LoadMaxValue);

                }
            }
        }
        #endregion

        #region ViewModel 属性 LoadMinValue --柱状图中商载量轴的最小值

        private double _LoadMinValue = 0;
        /// <summary>
        /// 柱状图中商载量轴的最小值
        /// </summary>
        public double LoadMinValue
        {
            get { return _LoadMinValue; }
            set
            {

                if (LoadMinValue != value)
                {
                    _LoadMinValue = value;
                    this.RaisePropertyChanged(() => this.LoadMinValue);

                }
            }
        }
        #endregion

        #region ViewModel 属性 LoadStep --柱状图中商载量轴的节点距离

        private int _LoadStep = 200;
        /// <summary>
        /// 柱状图中商载量轴的节点距离
        /// </summary>
        public int LoadStep
        {
            get { return _LoadStep; }
            set
            {

                if (LoadStep != value)
                {
                    _LoadStep = value;
                    this.RaisePropertyChanged(() => this.LoadStep);

                }
            }
        }
        #endregion

        #region ViewModel 属性 SeatMaxValue --柱状图中座位数轴的最大值

        private double _SeatMaxValue = 0;
        /// <summary>
        /// 柱状图中座位数轴的最大值
        /// </summary>
        public double SeatMaxValue
        {
            get { return _SeatMaxValue; }
            set
            {

                if (SeatMaxValue != value)
                {
                    _SeatMaxValue = value;
                    this.RaisePropertyChanged(() => this.SeatMaxValue);

                }
            }
        }
        #endregion

        #region ViewModel 属性 SeatMinValue --柱状图中座位数轴的最小值

        private double _SeatMinValue = 0;
        /// <summary>
        /// 柱状图中座位数轴的最小值
        /// </summary>
        public double SeatMinValue
        {
            get { return _SeatMinValue; }
            set
            {

                if (SeatMinValue != value)
                {
                    _SeatMinValue = value;
                    this.RaisePropertyChanged(() => this.SeatMinValue);

                }
            }
        }
        #endregion

        #region ViewModel 属性 SeatStep --柱状图中座位数轴的节点距离

        private int _SeatStep = 200;
        /// <summary>
        /// 柱状图中座位数轴的节点距离
        /// </summary>
        public int SeatStep
        {
            get { return _SeatStep; }
            set
            {

                if (SeatStep != value)
                {
                    _SeatStep = value;
                    this.RaisePropertyChanged(() => this.SeatStep);

                }
            }
        }
        #endregion

        #region ViewModel 属性 EndDate --结束时间

        private DateTime? _endDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy/M"));
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {

                if (EndDate != value)
                {
                    if (value == null)
                    {
                        EndDateTimePicker.SelectedValue = _endDate;
                        return;
                    }
                    _endDate = value;
                    this.RaisePropertyChanged(() => this.EndDate);
                    CreatFleetAircraftTrendCollection();
                }
            }
        }
        #endregion

        #region ViewModel 属性 StartDate --开始时间

        private DateTime? _startDate = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {

                if (StartDate != value)
                {
                    if (value == null)
                    {
                        StartDateTimePicker.SelectedValue = _startDate;
                        return;
                    }
                    _startDate = value;
                    this.RaisePropertyChanged(() => this.StartDate);
                    CreatFleetAircraftTrendCollection();
                }
            }
        }
        #endregion

        #region ViewModel 属性 IsHidden --控制是否含分公司的饼图区域显示

        private bool _IsHidden = true;
        /// <summary>
        /// 控制是否含分公司的饼图区域显示
        /// </summary>
        public bool IsHidden
        {
            get { return _IsHidden; }
            set
            {
                _IsHidden = value;
                this._view.GridViewPane.IsHidden = !value;
                this.RaisePropertyChanged(() => this.IsHidden);
            }
        }
        #endregion

        #region ViewModel 属性 Visibility --控制是否含分公司的趋势图数据显示

        private System.Windows.Visibility _Visibility = System.Windows.Visibility.Collapsed;
        /// <summary>
        /// 控制是否含分公司的饼图区域显示
        /// </summary>
        public System.Windows.Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                if (Visibility != value)
                {
                    _Visibility = value;
                    this.RaisePropertyChanged(() => this.Visibility);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetAircraftCollection --选中时间点的飞机数分布集合

        private IEnumerable<FleetAircraft> _FleetAircraftCollection;
        /// <summary>
        /// 选中时间点的飞机数分布集合
        /// </summary>
        public IEnumerable<FleetAircraft> FleetAircraftCollection
        {
            get { return _FleetAircraftCollection; }
            set
            {

                if (FleetAircraftCollection != value)
                {
                    _FleetAircraftCollection = value;
                    this.RaisePropertyChanged(() => this.FleetAircraftCollection);
                    SetPieMark(FleetAircraftCollection, this.AircraftPieGrid);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetSeatCollection --选中时间点的座位数分布集合

        private IEnumerable<FleetAircraft> _FleetSeatCollection;
        /// <summary>
        /// 选中时间点的座位数分布集合
        /// </summary>
        public IEnumerable<FleetAircraft> FleetSeatCollection
        {
            get { return _FleetSeatCollection; }
            set
            {

                if (FleetSeatCollection != value)
                {
                    _FleetSeatCollection = value;
                    this.RaisePropertyChanged(() => this.FleetSeatCollection);
                    SetPieMark(FleetSeatCollection, this.SeatPieGrid);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetAircraftCollection --选中时间点的商载量分布集合

        private IEnumerable<FleetAircraft> _FleetLoadCollection;
        /// <summary>
        /// 选中时间点的商载量分布集合
        /// </summary>
        public IEnumerable<FleetAircraft> FleetLoadCollection
        {
            get { return _FleetLoadCollection; }
            set
            {

                if (FleetLoadCollection != value)
                {
                    _FleetLoadCollection = value;
                    this.RaisePropertyChanged(() => this.FleetLoadCollection);
                    SetPieMark(FleetLoadCollection, this.LoadPieGrid);
                }
            }
        }
        #endregion


        #region ViewModel 属性 Zoom --滚动条的对应

        private Size _Zoom = new Size(1, 1);
        /// <summary>
        /// 滚动条的对应
        /// </summary>
        public Size Zoom
        {
            get { return _Zoom; }
            set
            {

                if (Zoom != value)
                {
                    _Zoom = value;
                    this.RaisePropertyChanged(() => this.Zoom);
                }
            }
        }
        #endregion

        #region ViewModel 属性 PanOffset --滚动条的滑动

        private Point _PanOffset;
        /// <summary>
        /// 滚动条的滑动
        /// </summary>
        public Point PanOffset
        {
            get { return _PanOffset; }
            set
            {

                if (PanOffset != value)
                {
                    _PanOffset = value;
                    this.RaisePropertyChanged(() => this.PanOffset);
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

        #region ViewModel 命令 --导出图表

        public DelegateCommand<object> ExportCommand { get; private set; }
        private void OnExport(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Header.ToString() == "导出源数据")
            {
                if (menu.Name == "LineGridData")
                {
                    if (_service.CurrentAirlines.SubAirlines != null && _service.CurrentAirlines.SubAirlines.Any(p => p.SubType == 1))
                    {
                        //当包含子公司时
                        Dictionary<string, string> columnsList = new Dictionary<string, string>();
                        columnsList.Add("DateTime", "时间点");
                        columnsList.Add("AircraftAmount", "期末客机数(子)");
                        columnsList.Add("AircraftAmount1", "期末客机数");
                        columnsList.Add("SeatAmount", "期末座位数(子)");
                        columnsList.Add("SeatAmount1", "期末座位数");
                        columnsList.Add("LoadAmount", "期末商载量(子)");
                        columnsList.Add("LoadAmount1", "期末商载量");
                        ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAircraftTrendLineCollection, "SubFleetTrendPnr");
                    }
                    else
                    {
                        //创建RadGridView
                        Dictionary<string, string> columnsList = new Dictionary<string, string>();
                        columnsList.Add("DateTime", "时间点");
                        columnsList.Add("AircraftAmount1", "期末客机数");
                        columnsList.Add("SeatAmount1", "期末座位数");
                        columnsList.Add("LoadAmount1", "期末商载量");
                        ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAircraftTrendLineCollection, "FleetTrendPnr");
                    }
                    i = 1;
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }
                }
                else if (menu.Name == "BarGridData")
                {
                    if (_service.CurrentAirlines.SubAirlines != null && _service.CurrentAirlines.SubAirlines.Any(p => p.SubType == 1))
                    {
                        //当包含子公司时
                        Dictionary<string, string> columnsList = new Dictionary<string, string>();
                        columnsList.Add("DateTime", "时间点");
                        columnsList.Add("AircraftAmount", "客机净增数(子)");
                        columnsList.Add("AircraftAmount1", "客机净增数(子)");
                        columnsList.Add("SeatAmount", "座位净增数(子)");
                        columnsList.Add("SeatAmount1", "座位净增数");
                        columnsList.Add("LoadAmount", "商载净增量(子)");
                        columnsList.Add("LoadAmount1", "商载净增量");
                        ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAircraftTrendBarCollection, "SubFleetTrendPnr");
                    }
                    else
                    {
                        //创建RadGridView
                        Dictionary<string, string> columnsList = new Dictionary<string, string>();
                        columnsList.Add("DateTime", "时间点");
                        columnsList.Add("AircraftAmount1", "客机净增数");
                        columnsList.Add("SeatAmount1", "座位净增数");
                        columnsList.Add("LoadAmount1", "商载净增量");
                        ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAircraftTrendBarCollection, "FleetTrendPnr");
                    }
                    i = 1;
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }
                }
                else if (menu.Name == "AircraftPieGridData")
                {
                    if (FleetAircraftCollection == null || FleetAircraftCollection.Count() == 0)
                    {
                        return;
                    }

                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("Aircraft", "航空公司");
                    columnsList.Add("Amount", "飞机数（架）");
                    ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAircraftCollection, "PieFleetTrend");

                    i = 1;
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.xlsx)|*.xlsx|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }
                }
                else if (menu.Name == "SeatPieGridData")
                {
                    if (FleetSeatCollection == null || FleetSeatCollection.Count() == 0)
                    {
                        return;
                    }

                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("Aircraft", "航空公司");
                    columnsList.Add("Amount", "座位数");
                    ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetSeatCollection, "PieFleetTrend");

                    i = 1;
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }
                }
                else if (menu.Name == "LoadPieGridData")
                {
                    if (FleetLoadCollection == null || FleetLoadCollection.Count() == 0)
                    {
                        return;
                    }

                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("Aircraft", "航空公司");
                    columnsList.Add("Amount", "商载量（吨）");
                    ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetLoadCollection, "PieFleetTrend");

                    i = 1;
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            ExportRadgridview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                        }
                    }
                }

            }
            else if (menu != null && menu.Header.ToString() == "导出图片")
            {
                if (menu.Name == "LineGridImage" || menu.Name == "BarGridImage")
                {
                    //导出图片
                    if (this.LineGrid != null)
                    {
                        commonmethod.ExportToImage(this.LineGrid.Parent as Grid);
                    }
                }
                else if (menu.Name == "AircraftPieGridImage")
                {
                    //导出图片
                    if (this.AircraftPieGrid != null)
                    {
                        commonmethod.ExportToImage(this.AircraftPieGrid);
                    }
                }
                else if (menu.Name == "SeatPieGridImage")
                {
                    //导出图片
                    if (this.SeatPieGrid != null)
                    {
                        commonmethod.ExportToImage(this.SeatPieGrid);
                    }
                }
                else if (menu.Name == "LoadPieGridImage")
                {
                    //导出图片
                    if (this.LoadPieGrid != null)
                    {
                        commonmethod.ExportToImage(this.LoadPieGrid);
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
            if (e.Element == ExportElement.Cell &&
                e.Value != null)
            {
                if (i % 5 == 3 && i >= 8 && (sender as RadGridView).Name == "FleetTrendPnr")
                {
                    e.Value = DateTime.Parse(e.Value.ToString()).AddMonths(1).AddDays(-1).ToString("yyyy/M/d");
                }
                else if (i % 8 == 3 && i >= 11 && (sender as RadGridView).Name == "SubFleetTrendPnr")
                {
                    e.Value = DateTime.Parse(e.Value.ToString()).AddMonths(1).AddDays(-1).ToString("yyyy/M/d");
                }
            }
            i++;
        }

        bool CanExport(object sender)
        {
            return true;
        }

        #endregion

        #region  增加子窗体的右键导出功能

        public void AddRadMenu(RadWindow rwindow)
        {
            RadContextMenu radcm = new RadContextMenu();//新建右键菜单
            radcm.Opened += radcm_Opened;
            RadMenuItem rmi = new RadMenuItem();//新建右键菜单项
            rmi.Header = "导出表格";
            rmi.Click += new RadRoutedEventHandler(MenuItem_Click);//为菜单项注册事件
            rmi.DataContext = rwindow.Name;
            radcm.Items.Add(rmi);
            RadContextMenu.SetContextMenu(rwindow, radcm);//为控件绑定右键菜单
        }
        void radcm_Opened(object sender, RoutedEventArgs e)
        {
            RadContextMenu radcm = sender as RadContextMenu;
            radcm.StaysOpen = true;
        }
        public void MenuItem_Click(object sender, RadRoutedEventArgs e)
        {
            RadMenuItem rmi = sender as RadMenuItem;
            RadContextMenu radcm = rmi.Parent as RadContextMenu;
            radcm.StaysOpen = false;
            RadGridView rgview = null;
            if (rmi.DataContext.ToString() == "Aircraft")
            {
                rgview = AircraftWindow.Content as RadGridView;

            }
            else if (rmi.DataContext.ToString() == "Seat")
            {
                rgview = SeatWindow.Content as RadGridView;

            }
            else if (rmi.DataContext.ToString() == "Load")
            {
                rgview = LoadWindow.Content as RadGridView;

            }

            if (rgview != null)
            {
                rgview.ElementExporting -= this.ElementExporting;
                rgview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        rgview.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
        }
        #endregion

        #region ViewModel 命令 --导出数据aircraftDetail

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Header.ToString() == "导出数据" && aircraftDetail != null)
            {
                aircraftDetail.ElementExporting -= this.ElementExporting;
                aircraftDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraftDetail.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
        }

        bool CanExportGridView(object sender)
        {
            return true;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// 获取趋势图的颜色配置
        /// </summary>
        private Dictionary<string, string> GetColorDictionary()
        {
            Dictionary<string, string> colordictionary = new Dictionary<string, string>();
            XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "运力变化"))
            {
                XElement capacitycolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "运力变化");
                foreach (var item in capacitycolor.Descendants("Item"))
                {
                    colordictionary.Add(item.Attribute("Name").Value, item.Attribute("Color").Value);
                }
            }
            else
            {
                colordictionary.Add("飞机数（子）", commonmethod.GetRandomColor());
                colordictionary.Add("座位数（子）", commonmethod.GetRandomColor());
                colordictionary.Add("商载量（子）", commonmethod.GetRandomColor());
                colordictionary.Add("飞机数", commonmethod.GetRandomColor());
                colordictionary.Add("座位数", commonmethod.GetRandomColor());
                colordictionary.Add("商载量", commonmethod.GetRandomColor());
            }
            return colordictionary;
        }

        /// <summary>
        /// 获取总数和净增数趋势图的数据源集合
        /// </summary>
        /// <returns></returns>
        private void CreatFleetAircraftTrendCollection()
        {
            List<FleetAircraftTrend> fleetaircrafttrendlinelist = new List<FleetAircraftTrend>();//折线图统计总数的集合
            List<FleetAircraftTrend> fleetaircrafttrendbarlist = new List<FleetAircraftTrend>();//柱状图统计净增的集合

            #region 客机运力XML文件的读写
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "客机运力");
            Dictionary<string, string> colordictionary = GetColorDictionary();
            if (xmlconfig != null)
            {
                XElement xelement = xmlconfig.XmlContent;
                if (xelement != null)
                {
                    //记录上一个时间点的总数，便于统计净增数据
                    int LastAircraftAmount = 0;
                    int LastSeatAmount = 0;
                    int LastLoadAmount = 0;
                    int LastAircraftAmount1 = 0;
                    int LastSeatAmount1 = 0;
                    int LastLoadAmount1 = 0;

                    foreach (XElement datetime in xelement.Descendants("DateTime"))
                    {
                        string CurrentTime = Convert.ToDateTime(datetime.Attribute("EndOfMonth").Value).ToString("yyyy/M");

                        if (SelectedIndex == 1)//按半年统计
                        {
                            if (Convert.ToDateTime(CurrentTime).Month != 6 && Convert.ToDateTime(CurrentTime).Month != 12)
                            {
                                continue;
                            }
                        }
                        else if (SelectedIndex == 2)//按年份统计
                        {
                            if (Convert.ToDateTime(CurrentTime).Month != 12)
                            {
                                continue;
                            }
                        }
                        FleetAircraftTrend fleetaircrafttrenline = new FleetAircraftTrend();//折线图的总数对象
                        fleetaircrafttrenline.DateTime = CurrentTime;
                        FleetAircraftTrend fleetaircrafttrenbar = new FleetAircraftTrend();//柱状图的净增数对象
                        fleetaircrafttrenbar.DateTime = CurrentTime;
                        foreach (XElement type in datetime.Descendants("Type"))
                        {
                            if (type.Attribute("TypeName").Value == "飞机数（子）")
                            {
                                fleetaircrafttrenline.AircraftAmount = Convert.ToInt32(type.Attribute("Amount").Value);
                                //飞机净增数
                                fleetaircrafttrenbar.AircraftAmount = fleetaircrafttrenline.AircraftAmount - LastAircraftAmount;

                                fleetaircrafttrenline.AircraftColor = fleetaircrafttrenbar.AircraftColor = colordictionary["飞机数（子）"];
                            }
                            else if (type.Attribute("TypeName").Value == "座位数（子）")
                            {
                                fleetaircrafttrenline.SeatAmount = Convert.ToInt32(type.Attribute("Amount").Value);
                                //座位净增数
                                fleetaircrafttrenbar.SeatAmount = fleetaircrafttrenline.SeatAmount - LastSeatAmount;

                                fleetaircrafttrenline.SeatColor = fleetaircrafttrenbar.SeatColor = colordictionary["座位数（子）"];
                            }
                            else if (type.Attribute("TypeName").Value == "商载量（子）")
                            {
                                fleetaircrafttrenline.LoadAmount = Convert.ToInt32(type.Attribute("Amount").Value);
                                //商载净增数
                                fleetaircrafttrenbar.LoadAmount = fleetaircrafttrenline.LoadAmount - LastLoadAmount;

                                fleetaircrafttrenline.LoadColor = fleetaircrafttrenbar.LoadColor = colordictionary["商载量（子）"];
                            }
                            else if (type.Attribute("TypeName").Value == "飞机数")
                            {
                                fleetaircrafttrenline.AircraftAmount1 = Convert.ToInt32(type.Attribute("Amount").Value);
                                //飞机净增数
                                fleetaircrafttrenbar.AircraftAmount1 = fleetaircrafttrenline.AircraftAmount1 - LastAircraftAmount1;

                                fleetaircrafttrenline.AircraftColor1 = fleetaircrafttrenbar.AircraftColor1 = colordictionary["飞机数"];
                            }
                            else if (type.Attribute("TypeName").Value == "座位数")
                            {
                                fleetaircrafttrenline.SeatAmount1 = Convert.ToInt32(type.Attribute("Amount").Value);
                                //座位净增数
                                fleetaircrafttrenbar.SeatAmount1 = fleetaircrafttrenline.SeatAmount1 - LastSeatAmount1;

                                fleetaircrafttrenline.SeatColor1 = fleetaircrafttrenbar.SeatColor1 = colordictionary["座位数"];
                            }
                            else if (type.Attribute("TypeName").Value == "商载量")
                            {
                                fleetaircrafttrenline.LoadAmount1 = Convert.ToInt32(type.Attribute("Amount").Value);
                                //商载净增数
                                fleetaircrafttrenbar.LoadAmount1 = fleetaircrafttrenline.LoadAmount1 - LastLoadAmount1;

                                fleetaircrafttrenline.LoadColor1 = fleetaircrafttrenbar.LoadColor1 = colordictionary["商载量"];
                            }
                        }

                        //将当前总数赋值做为下一次计算净增量。
                        LastAircraftAmount = fleetaircrafttrenline.AircraftAmount;
                        LastSeatAmount = fleetaircrafttrenline.SeatAmount;
                        LastLoadAmount = fleetaircrafttrenline.LoadAmount;
                        LastAircraftAmount1 = fleetaircrafttrenline.AircraftAmount1;
                        LastSeatAmount1 = fleetaircrafttrenline.SeatAmount1;
                        LastLoadAmount1 = fleetaircrafttrenline.LoadAmount1;

                        //注：放于此为了正确统计净增量
                        //早于开始时间时执行下一个
                        if (Convert.ToDateTime(CurrentTime) < StartDate)
                        {
                            continue;
                        }
                        //晚于结束时间时跳出循环
                        if (Convert.ToDateTime(CurrentTime) > EndDate)
                        {
                            break;
                        }

                        //添加进相应的数据源集合
                        fleetaircrafttrendlinelist.Add(fleetaircrafttrenline);
                        fleetaircrafttrendbarlist.Add(fleetaircrafttrenbar);
                    }

                    if (fleetaircrafttrendbarlist.Count() > 0)
                    {
                        AircraftMaxValue = fleetaircrafttrendbarlist.Max(p => p.AircraftAmount);
                        SeatMaxValue = fleetaircrafttrendbarlist.Max(p => p.SeatAmount);
                        LoadMaxValue = fleetaircrafttrendbarlist.Max(p => p.LoadAmount);
                        double aircraft = AircraftMaxValue == 0 ? 0 : fleetaircrafttrendbarlist.Min(p => p.AircraftAmount) / AircraftMaxValue;
                        double seat = SeatMaxValue == 0 ? 0 : fleetaircrafttrendbarlist.Min(p => p.SeatAmount) / SeatMaxValue;
                        double load = LoadMaxValue == 0 ? 0 : fleetaircrafttrendbarlist.Min(p => p.LoadAmount) / LoadMaxValue;
                        double minrate = aircraft < seat ? (aircraft < load ? aircraft : load) : (seat < load ? seat : load);
                        if (minrate >= 0)
                        {
                            AircraftMinValue = 0;
                            SeatMinValue = 0;
                            LoadMinValue = 0;
                            if (AircraftMaxValue == 0)
                            {
                                AircraftMaxValue = 10;
                            }
                            if (SeatMaxValue == 0)
                            {
                                SeatMaxValue = 10;
                            }
                            if (LoadMaxValue == 0)
                            {
                                LoadMaxValue = 10;
                            }
                        }
                        else
                        {
                            AircraftMinValue = AircraftMaxValue * minrate;
                            SeatMinValue = SeatMaxValue * minrate;
                            LoadMinValue = LoadMaxValue * minrate;
                        }

                        AircraftStep = Convert.ToInt32(AircraftMaxValue / 2);
                        SeatStep = Convert.ToInt32(SeatMaxValue / 2);
                        LoadStep = Convert.ToInt32(LoadMaxValue / 2);
                    }
                }
            }
            #endregion

            FleetAircraftTrendLineCollection = fleetaircrafttrendlinelist;
            FleetAircraftTrendBarCollection = fleetaircrafttrendbarlist;

            SelectedTime = "所选时间";
            FleetAircraftCollection = null;
            FleetSeatCollection = null;
            FleetLoadCollection = null;
            AircraftCollection = null;
            this.Zoom = new Size(1, 1);
        }

        /// <summary>
        /// 趋势图的选择点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartSelectionBehavior_SelectionChanged(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            Telerik.Charting.DataPoint SelectedPoint = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart.SelectedPoints.FirstOrDefault(p => (p.Presenter as CategoricalSeries).Visibility == Visibility.Visible);
            if (SelectedPoint != null)
            {
                FleetAircraftTrend fleetaircrafttrend = SelectedPoint.DataItem as FleetAircraftTrend;
                if (SelectedTime != fleetaircrafttrend.DateTime)
                {
                    //选中时间点
                    SelectedTime = fleetaircrafttrend.DateTime;

                    DateTime time = Convert.ToDateTime(fleetaircrafttrend.DateTime).AddMonths(1).AddDays(-1);
                    var aircraftlist = this.ViewAircraft
                        .Where(o => o.OperationHistories.Any(a => (a.Airlines.ShortName == this._service.CurrentAirlines.ShortName || a.Airlines.SubType == 2)
                            && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))
                            && o.AircraftBusinesses.Any(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)))
                        .Where(o => o.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))
                            .AircraftType.AircraftCategory.Category == "客机").ToList();
                    AircraftCollection = commonmethod.GetAircraftByTime(aircraftlist, time);

                    #region 客机运力XML文件的读写
                    var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "客机运力");

                    XElement airlinecolor = null;
                    XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                    if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
                    {
                        airlinecolor = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置").XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
                    }
                    if (xmlconfig != null)
                    {
                        List<FleetAircraft> AircraftList = new List<FleetAircraft>();//飞机数饼图集合
                        List<FleetAircraft> SeatList = new List<FleetAircraft>();//座位数饼图集合
                        List<FleetAircraft> LoadList = new List<FleetAircraft>();//商载量饼图集合

                        XElement xelement = xmlconfig.XmlContent.Descendants("DateTime").FirstOrDefault(p => Convert.ToDateTime(p.Attribute("EndOfMonth").Value) == time);
                        if (xelement != null)
                        {
                            foreach (XElement type in xelement.Descendants("Type"))
                            {
                                if (type.Attribute("TypeName").Value == "飞机数")
                                {
                                    foreach (XElement item in type.Descendants("Item"))
                                    {
                                        FleetAircraft fleetaircraft = new FleetAircraft();
                                        fleetaircraft.Aircraft = item.Attribute("Name").Value;
                                        fleetaircraft.Amount = Convert.ToDecimal(item.Value);
                                        fleetaircraft.ToolTip = item.Value + " 架,占 " + item.Attribute("Percent").Value;
                                        if (airlinecolor != null)
                                        {
                                            fleetaircraft.Color = airlinecolor.Descendants("Item")
                                                .FirstOrDefault(p => p.Attribute("Name").Value == fleetaircraft.Aircraft).Attribute("Color").Value;
                                        }
                                        if (fleetaircraft.Amount > 0)
                                        {
                                            AircraftList.Add(fleetaircraft);
                                        }
                                    }
                                }
                                else if (type.Attribute("TypeName").Value == "座位数")
                                {
                                    foreach (XElement item in type.Descendants("Item"))
                                    {
                                        FleetAircraft fleetaircraft = new FleetAircraft();
                                        fleetaircraft.Aircraft = item.Attribute("Name").Value;
                                        fleetaircraft.Amount = Convert.ToDecimal(item.Value);
                                        fleetaircraft.ToolTip = item.Value + " 座,占 " + item.Attribute("Percent").Value;
                                        if (airlinecolor != null)
                                        {
                                            fleetaircraft.Color = airlinecolor.Descendants("Item")
                                                .FirstOrDefault(p => p.Attribute("Name").Value == fleetaircraft.Aircraft).Attribute("Color").Value;
                                        }
                                        if (fleetaircraft.Amount > 0)
                                        {
                                            SeatList.Add(fleetaircraft);
                                        }
                                    }
                                }
                                else if (type.Attribute("TypeName").Value == "商载量")
                                {
                                    foreach (XElement item in type.Descendants("Item"))
                                    {
                                        FleetAircraft fleetaircraft = new FleetAircraft();
                                        fleetaircraft.Aircraft = item.Attribute("Name").Value;
                                        fleetaircraft.Amount = Convert.ToDecimal(item.Value);
                                        fleetaircraft.ToolTip = item.Value + " 吨,占 " + item.Attribute("Percent").Value;
                                        if (airlinecolor != null)
                                        {
                                            fleetaircraft.Color = airlinecolor.Descendants("Item")
                                                .FirstOrDefault(p => p.Attribute("Name").Value == fleetaircraft.Aircraft).Attribute("Color").Value;
                                        }
                                        if (fleetaircraft.Amount > 0)
                                        {
                                            LoadList.Add(fleetaircraft);
                                        }
                                    }
                                }
                            }
                        }

                        FleetAircraftCollection = AircraftList;
                        FleetSeatCollection = SeatList;
                        FleetLoadCollection = LoadList;
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 飞机饼状图的选择点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadPieChartSelectionBehavior_SelectionChanged(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            Telerik.Windows.Controls.ChartView.RadChartBase radchartbase = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart;
            Telerik.Charting.PieDataPoint SelectedPoint = radchartbase.SelectedPoints.FirstOrDefault() as Telerik.Charting.PieDataPoint;

            StackPanel stackpanel = new StackPanel();
            if (radchartbase.EmptyContent.ToString() == "飞机数分布")
            {
                stackpanel = (this.AircraftPieGrid.Children[1] as ScrollViewer).Content as StackPanel;
            }
            else if (radchartbase.EmptyContent.ToString() == "座位数分布")
            {
                stackpanel = (this.SeatPieGrid.Children[1] as ScrollViewer).Content as StackPanel;
            }
            else if (radchartbase.EmptyContent.ToString() == "商载量分布")
            {
                stackpanel = (this.LoadPieGrid.Children[1] as ScrollViewer).Content as StackPanel;
            }

            foreach (var item in stackpanel.Children)
            {
                System.Windows.Shapes.Rectangle itemrectangle = (item as StackPanel).Children[0] as System.Windows.Shapes.Rectangle;
                itemrectangle.Width = 15;
                itemrectangle.Height = 15;
            }

            if (SelectedPoint != null)
            {
                StackPanel childstackpanel = stackpanel.Children
                    .FirstOrDefault(p => ((p as StackPanel).Children[1] as TextBlock).Text == (SelectedPoint.DataItem as FleetAircraft).Aircraft) as StackPanel;
                System.Windows.Shapes.Rectangle rectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 12;
                rectangle.Height = 12;

                if (radchartbase.EmptyContent.ToString() == "飞机数分布")
                {
                    GetGridViewDataSourse(SelectedPoint, AircraftWindow, "飞机数");
                }
                else if (radchartbase.EmptyContent.ToString() == "座位数分布")
                {
                    GetGridViewDataSourse(SelectedPoint, SeatWindow, "座位数");
                }
                else if (radchartbase.EmptyContent.ToString() == "商载量分布")
                {
                    GetGridViewDataSourse(SelectedPoint, LoadWindow, "商载量");
                }
            }
            else
            {
                if (radchartbase.EmptyContent.ToString() == "飞机数分布")
                {
                    AircraftWindow.Close();
                }
                else if (radchartbase.EmptyContent.ToString() == "座位数分布")
                {
                    SeatWindow.Close();
                }
                else if (radchartbase.EmptyContent.ToString() == "商载量分布")
                {
                    LoadWindow.Close();
                }
            }
        }

        /// <summary>
        /// 根据相应的饼图数据生成饼图标签
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetPieMark(IEnumerable<FleetAircraft> ienumerable, Grid grid)
        {
            RadPieChart radpiechart = grid.Children[0] as RadPieChart;
            StackPanel stackpanel = (grid.Children[1] as ScrollViewer).Content as StackPanel;

            radpiechart.Series[0].SliceStyles.Clear();
            stackpanel.Children.Clear();
            if (ienumerable == null)
            {
                return;
            }
            foreach (var item in ienumerable)
            {
                Setter setter = new Setter();
                setter.Property = System.Windows.Shapes.Shape.FillProperty;
                setter.Value = item.Color;
                Style style = new Style();
                style.TargetType = typeof(System.Windows.Shapes.Path);
                style.Setters.Add(setter);
                radpiechart.Series[0].SliceStyles.Add(style);

                StackPanel barpanel = new StackPanel();
                barpanel.MouseLeftButtonDown += PiePanel_MouseLeftButtonDown;
                barpanel.Orientation = Orientation.Horizontal;
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 15;
                rectangle.Height = 15;
                rectangle.Fill = new SolidColorBrush(commonmethod.GetColor(item.Color));
                TextBlock textblock = new TextBlock();
                textblock.Text = item.Aircraft;
                textblock.Style = this._view.Resources.FirstOrDefault(p => p.Key.ToString() == "legendItemStyle").Value as Style;
                barpanel.Children.Add(rectangle);
                barpanel.Children.Add(textblock);
                stackpanel.Children.Add(barpanel);
            }
        }

        /// <summary>
        /// 控制趋势图的Y轴和折线及标签颜色
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetRadCartesianChartColor()
        {
            Dictionary<string, string> colordictionary = GetColorDictionary();
            //控制折线趋势图的Y轴颜色
            foreach (var item in ((LineGrid.Children[0] as RadCartesianChart).Resources["additionalVerticalAxis"] as AxisCollection))
            {
                LinearAxis linearaxis = item as LinearAxis;
                if (linearaxis.Title.ToString() == "飞机数（架）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
                else if (linearaxis.Title.ToString() == "座位数（座）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数"]));
                }
                else if (linearaxis.Title.ToString() == "商载量（吨）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量"]));
                }
            }
            //控制折线趋势图的线条颜色
            foreach (var item in ((LineGrid.Children[0] as RadCartesianChart).Series))
            {
                LineSeries linearseries = item as LineSeries;
                if (linearseries.DisplayName == "期末飞机数（子）")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数（子）"]));
                }
                else if (linearseries.DisplayName == "期末座位数（子）")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数（子）"]));
                }
                else if (linearseries.DisplayName == "期末商载量（子）")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量（子）"]));
                }
                if (linearseries.DisplayName == "期末飞机数")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
                else if (linearseries.DisplayName == "期末座位数")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数"]));
                }
                else if (linearseries.DisplayName == "期末商载量")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量"]));
                }
            }

            //控制折线趋势图的标签颜色
            foreach (var item in ((LineGrid.Children[1] as ScrollViewer).Content as StackPanel).Children)
            {
                StackPanel stackpanel = item as StackPanel;
                CheckBox checkbox = stackpanel.Children[0] as CheckBox;
                if (checkbox.Content.ToString() == "期末飞机数（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数（子）"]));
                }
                else if (checkbox.Content.ToString() == "期末座位数（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数（子）"]));
                }
                else if (checkbox.Content.ToString() == "期末商载量（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量（子）"]));
                }
                if (checkbox.Content.ToString() == "期末飞机数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
                else if (checkbox.Content.ToString() == "期末座位数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数"]));
                }
                else if (checkbox.Content.ToString() == "期末商载量")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量"]));
                }
            }


            //控制柱状趋势图的Y轴颜色
            foreach (var item in ((BarGrid.Children[0] as RadCartesianChart).Resources["additionalVerticalAxis"] as AxisCollection))
            {
                LinearAxis linearaxis = item as LinearAxis;
                if (linearaxis.Title.ToString() == "飞机净增（架）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
                else if (linearaxis.Title.ToString() == "座位净增（座）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数"]));
                }
                else if (linearaxis.Title.ToString() == "商载净增（吨）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量"]));
                }
            }

            //控制柱状趋势图的标签颜色
            foreach (var item in ((BarGrid.Children[1] as ScrollViewer).Content as StackPanel).Children)
            {
                StackPanel stackpanel = item as StackPanel;
                CheckBox checkbox = stackpanel.Children[0] as CheckBox;
                if (checkbox.Content.ToString() == "飞机净增数（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数（子）"]));
                }
                else if (checkbox.Content.ToString() == "座位净增数（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数（子）"]));
                }
                else if (checkbox.Content.ToString() == "商载净增量（子）")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量（子）"]));
                }
                if (checkbox.Content.ToString() == "飞机净增数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
                else if (checkbox.Content.ToString() == "座位净增数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["座位数"]));
                }
                else if (checkbox.Content.ToString() == "商载净增量")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["商载量"]));
                }
            }
        }

        /// <summary>
        /// 饼状图标签的选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PiePanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //选中航空公司的名称
            StackPanel stackpanel = sender as StackPanel;
            string shortname = (stackpanel.Children[1] as TextBlock).Text;

            //修改饼图标签中的突出显示
            foreach (var item in (stackpanel.Parent as StackPanel).Children)
            {
                StackPanel childstackpanel = item as StackPanel;
                System.Windows.Shapes.Rectangle itemrectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                string itemshortname = (childstackpanel.Children[1] as TextBlock).Text;
                if (itemshortname == shortname)
                {
                    if (itemrectangle.Width == 12)
                    {
                        itemrectangle.Width = 15;
                        itemrectangle.Height = 15;
                    }
                    else
                    {
                        itemrectangle.Width = 12;
                        itemrectangle.Height = 12;
                    }
                }
                else
                {
                    itemrectangle.Width = 15;
                    itemrectangle.Height = 15;
                }
            }

            //修改对应饼图块状的突出显示
            RadPieChart radpiechart = (((stackpanel.Parent as StackPanel).Parent as ScrollViewer).Parent as Grid).Children[0] as RadPieChart;
            foreach (var item in radpiechart.Series[0].DataPoints)
            {
                Telerik.Charting.PieDataPoint piedatapoint = item as Telerik.Charting.PieDataPoint;
                if ((piedatapoint.DataItem as FleetAircraft).Aircraft == shortname)
                {

                    piedatapoint.IsSelected = !piedatapoint.IsSelected;
                    if (piedatapoint.IsSelected == true)
                    {
                        if (radpiechart.EmptyContent.ToString() == "飞机数分布")
                        {
                            GetGridViewDataSourse(piedatapoint, AircraftWindow, "飞机数");
                        }
                        else if (radpiechart.EmptyContent.ToString() == "座位数分布")
                        {
                            GetGridViewDataSourse(piedatapoint, SeatWindow, "座位数");
                        }
                        else if (radpiechart.EmptyContent.ToString() == "商载量分布")
                        {
                            GetGridViewDataSourse(piedatapoint, LoadWindow, "商载量");
                        }
                    }
                    else
                    {
                        if (radpiechart.EmptyContent.ToString() == "飞机数分布")
                        {
                            AircraftWindow.Close();
                        }
                        else if (radpiechart.EmptyContent.ToString() == "座位数分布")
                        {
                            SeatWindow.Close();
                        }
                        else if (radpiechart.EmptyContent.ToString() == "商载量分布")
                        {
                            LoadWindow.Close();
                        }
                    }
                }
                else
                {
                    piedatapoint.IsSelected = false; ;

                }
            }
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
                if (grid.Name == "LineGrid")
                {
                    (this.LineGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;
                }
                else if (grid.Name == "BarGrid")
                {
                    (this.BarGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;
                }
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
                if (grid.Name == "LineGrid")
                {
                    (this.LineGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (grid.Name == "BarGrid")
                {
                    (this.BarGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }

        #endregion

        #region Class

        /// <summary>
        /// 趋势图的对象
        /// </summary>
        public class FleetAircraftTrend
        {
            public string Aircraft { get; set; }//飞机相关的名称
            public string DateTime { get; set; }//时间点
            public int AircraftAmount { get; set; }//飞机数的总数（子）
            public int SeatAmount { get; set; }//座位数的总数（子）
            public int LoadAmount { get; set; }//商载量的总数（子）
            public string AircraftColor { get; set; }//飞机数的颜色（子）
            public string SeatColor { get; set; }//座位数的颜色（子）
            public string LoadColor { get; set; }//商载量的颜色（子）

            public int AircraftAmount1 { get; set; }//飞机数的总数
            public int SeatAmount1 { get; set; }//座位数的总数
            public int LoadAmount1 { get; set; }//商载量的总数
            public string AircraftColor1 { get; set; }//飞机数的颜色
            public string SeatColor1 { get; set; }//座位数的颜色
            public string LoadColor1 { get; set; }//商载量的颜色
        }


        /// <summary>
        ///饼图的分布对象
        /// </summary>
        public class FleetAircraft
        {
            public FleetAircraft()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string Aircraft { get; set; }//航空公司的名称
            public decimal Amount { get; set; }//分布的计数
            public string ToolTip { get; set; }//显示的提示
            public string Color { get; set; }//航空公司颜色
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

            InitalizerRadWindows(AircraftWindow, "Aircraft", 200);
            InitalizerRadWindows(SeatWindow, "Seat", 220);
            InitalizerRadWindows(LoadWindow, "Load", 240);
            AddRadMenu(AircraftWindow);
            AddRadMenu(SeatWindow);
            AddRadMenu(LoadWindow);
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
            AircraftWindow.Close();
            SeatWindow.Close();
            LoadWindow.Close();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_service.CurrentAirlines != null && _service.CurrentAirlines.SubAirlines != null && _service.CurrentAirlines.SubAirlines.Any(p => p.SubType == 0 || p.SubType == 2))
            {
                this.IsHidden = false;
            }
            else
            {
                this.IsHidden = true;
            }


            if (_service.CurrentAirlines != null && _service.CurrentAirlines.SubAirlines != null && _service.CurrentAirlines.SubAirlines.Any(p => p.SubType == 1))
            {
                Visibility = System.Windows.Visibility.Visible;
                (this.BarGrid.Children[0] as RadCartesianChart).TooltipTemplate = this._view.Resources["TooltipTemplateChild"] as DataTemplate;
            }
            else
            {
                Visibility = System.Windows.Visibility.Collapsed;
                (this.BarGrid.Children[0] as RadCartesianChart).TooltipTemplate = this._view.Resources["TooltipTemplate"] as DataTemplate;
            }

            LoadXmlConfig();
            LoadAircraft();
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        #endregion
    }
}

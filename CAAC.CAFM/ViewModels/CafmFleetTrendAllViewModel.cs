using CAAC.CAFM.Converters;
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
using Telerik.Charting;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmFleetTrendAllViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmFleetTrendAllViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmFleetTrendAllViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmFleetTrendAllView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmFleetTrendAllView>();
        CommonMethod commonmethod = new CommonMethod();
        private Grid LineGrid, BarGrid, AircraftPieGrid;//折线趋势图区域，柱状趋势图区域， 飞机数饼图区域
        private RadDateTimePicker StartDateTimePicker, EndDateTimePicker;//开始时间控件， 结束时间控件
        private RadGridView ExportRadgridview, aircraftDetail; //初始化RadGridView
        private int i; //导出数据源格式判断

        #region Property

        private RadWindow AircraftWindow = new RadWindow(); //用于单击飞机数饼状图的用户提示

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
                    if (SelectedTime == "所选时间")
                    {
                        SelectedTimeAircraft = "所选时间的飞机分布图";
                    }
                    else
                    {
                        SelectedTimeAircraft = SelectedTime + "末的飞机分布图";
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
                var aircraft = this.ViewAircraft.Where(o => o.OperationHistories.Any(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))
                                      && o.AircraftBusinesses.Any(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));
                List<Aircraft> airlineAircrafts = aircraft.Where(p => p.OperationHistories.FirstOrDefault(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time)).Airlines.ShortName == fleetaircraft.Aircraft).ToList();
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
            this.LineGrid = _view.LineGrid as Grid;
            this.BarGrid = _view.BarGrid as Grid;
            this.AircraftPieGrid = _view.AircraftPieGrid as Grid;
            //控制界面起止时间控件的字符串格式化
            this.StartDateTimePicker = _view.StartDateTimePicker as RadDateTimePicker;
            this.EndDateTimePicker = _view.EndDateTimePicker as RadDateTimePicker;
            this.aircraftDetail = _view.aircraftDetail as RadGridView;
            StartDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
            EndDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
        }

        /// <summary>
        /// 初始化子窗体
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

        #region ViewModel 属性 AircraftCollection --选中时间点的所有飞机数据集合

        private List<Aircraft> _aircraftCollection;
        /// <summary>
        /// 选中时间点的所有飞机数据集合
        /// </summary>
        public List<Aircraft> AircraftCollection
        {
            get { return _aircraftCollection; }
            set
            {
                _aircraftCollection = value;
                this.RaisePropertyChanged(() => this.AircraftCollection);
                if (SelectedTime != "所选时间")
                {
                    if (AircraftCollection == null || AircraftCollection.Count() == 0)
                    {
                        AircraftCount = "飞机明细（0架）";
                    }
                    else
                    {
                        AircraftCount = "飞机明细（" + AircraftCollection.Count() + "架）";
                    }
                }
                else
                {
                    AircraftCount = "飞机明细";
                }
            }
        }

        #endregion

        #region ViewModel 属性 AircraftCount --飞机详细列表的标识栏提示

        private string _AircraftCount = "飞机明细";
        /// <summary>
        /// 飞机详细列表的标识栏提示
        /// </summary>
        public string AircraftCount
        {
            get { return _AircraftCount; }
            set
            {

                if (AircraftCount != value)
                {
                    _AircraftCount = value;
                    this.RaisePropertyChanged(() => this.AircraftCount);

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
                    SetPieMark(_FleetAircraftCollection, this.AircraftPieGrid);
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

        private int _AircraftMaxValue = 0;
        /// <summary>
        /// 柱状图中飞机数轴的最大值
        /// </summary>
        public int AircraftMaxValue
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

        private int _AircraftMinValue = 0;
        /// <summary>
        /// 柱状图中飞机数轴的最小值
        /// </summary>
        public int AircraftMinValue
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
                    if (FleetAircraftTrendLineCollection == null || FleetAircraftTrendLineCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("DateTime", "时间点");
                    columnsList.Add("AircraftAmount", "期末飞机数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, FleetAircraftTrendLineCollection, "FleetTrend");
                }
                else if (menu.Name == "BarGridData")
                {
                    if (FleetAircraftTrendBarCollection == null || FleetAircraftTrendBarCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList2 = new Dictionary<string, string>();
                    columnsList2.Add("DateTime", "时间点");
                    columnsList2.Add("AircraftAmount", "飞机净增数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList2, FleetAircraftTrendBarCollection, "FleetTrend");
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
                    columnsList.Add("Amount", "当前运营货机数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, FleetAircraftCollection, "PieFleetTrend");
                }
                i = 1;
                ExportRadgridview.ElementExporting -= this.ElementExporting;
                ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        ExportRadgridview.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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
                if (i % 3 == 0 && i >= 6 && (sender as RadGridView).Name == "FleetTrend")
                {
                    e.Value = DateTime.Parse(e.Value.ToString()).AddMonths(1).AddDays(-1).ToString("yyyy/M/d");
                }
            }
            i++;
        }

        private bool _canExport = true;
        bool CanExport(object sender)
        {
            return this._canExport;
        }

        #endregion

        #region ViewModel 命令 --导出GridView数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Header.ToString() == "导出数据" && aircraftDetail != null)
            {
                aircraftDetail.ElementExporting -= this.ElementExporting;
                aircraftDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraftDetail.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
        }
        private bool _canExportGridView = true;
        bool CanExportGridView(object sender)
        {
            return this._canExportGridView;
        }

        #endregion

        #region  增加子窗体的右键导出功能

        public void AddRadMenu(RadWindow rwindow)
        {
            RadContextMenu radcm = new RadContextMenu();//新建右键菜单
            RadMenuItem rmi = new RadMenuItem();//新建右键菜单项
            radcm.Opened += radcm_Opened;
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
            RadGridView rgview = null;
            RadContextMenu radcm = rmi.Parent as RadContextMenu;
            radcm.StaysOpen = false;
            if (rmi.DataContext.ToString() == "Aircraft")
            {
                rgview = AircraftWindow.Content as RadGridView;

            }
            if (rgview != null)
            {
                rgview.ElementExporting -= this.ElementExporting;
                rgview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        rgview.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
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
            try
            {
                List<FleetAircraftTrend> fleetaircrafttrendlinelist = new List<FleetAircraftTrend>();//折线图统计总数的集合
                List<FleetAircraftTrend> fleetaircrafttrendbarlist = new List<FleetAircraftTrend>();//柱状图统计净增的集合

                #region 飞机运力XML文件的读写
                var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "飞机运力");
                Dictionary<string, string> colordictionary = GetColorDictionary();
                if (xmlconfig != null)
                {
                    XElement xelement = xmlconfig.XmlContent;
                    if (xelement != null)
                    {
                        //记录上一个时间点的总数，便于统计净增数据
                        int LastAircraftAmount = 0;

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

                            XElement typexelement = datetime.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "飞机数");
                            if (typexelement != null)
                            {
                                fleetaircrafttrenline.AircraftAmount = Convert.ToInt32(typexelement.Attribute("Amount").Value);
                                //飞机净增数
                                fleetaircrafttrenbar.AircraftAmount = fleetaircrafttrenline.AircraftAmount - LastAircraftAmount;

                                fleetaircrafttrenline.AircraftColor = fleetaircrafttrenbar.AircraftColor = colordictionary["飞机数"];
                            }

                            //将当前总数赋值做为下一次计算净增量。
                            LastAircraftAmount = fleetaircrafttrenline.AircraftAmount;

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
                            AircraftMinValue = fleetaircrafttrendbarlist.Min(p => p.AircraftAmount);
                            if (AircraftMinValue >= 0)
                            {
                                AircraftMinValue = 0;
                                if (AircraftMaxValue == 0)
                                {
                                    AircraftMaxValue = 10;
                                }
                            }
                            AircraftStep = Convert.ToInt32(AircraftMaxValue / 2);
                        }
                    }
                }
                #endregion

                SelectedTime = "所选时间";
                FleetAircraftCollection = null;
                AircraftCollection = null;
                this.Zoom = new Size(1, 1);

                FleetAircraftTrendLineCollection = fleetaircrafttrendlinelist;
                FleetAircraftTrendBarCollection = fleetaircrafttrendbarlist;
            }
            catch (Exception)
            {
            }
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
                    var aircraftlist = this.ViewAircraft.
                        Where(o => o.OperationHistories.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time))
                        && o.AircraftBusinesses.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time))).ToList();
                    AircraftCollection = commonmethod.GetAircraftByTime(aircraftlist, time);

                    #region 飞机运力XML文件的读写
                    var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "飞机运力");

                    XElement airlinecolor = null;
                    XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                    if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
                    {
                        airlinecolor = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置").XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
                    }

                    if (xmlconfig != null)
                    {
                        List<FleetAircraft> AircraftList = new List<FleetAircraft>();//飞机数饼图集合

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
                            }
                        }
                        FleetAircraftCollection = AircraftList;
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
            }
            else
            {
                if (radchartbase.EmptyContent.ToString() == "飞机数分布")
                {
                    AircraftWindow.Close();
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
            }
            //控制折线趋势图的线条颜色
            foreach (var item in ((LineGrid.Children[0] as RadCartesianChart).Series))
            {
                LineSeries linearseries = item as LineSeries;
                if (linearseries.DisplayName == "期末飞机数")
                {
                    linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
                }
            }

            //控制折线趋势图的标签颜色
            foreach (var item in ((LineGrid.Children[1] as ScrollViewer).Content as StackPanel).Children)
            {
                StackPanel stackpanel = item as StackPanel;
                CheckBox checkbox = stackpanel.Children[0] as CheckBox;
                if (checkbox.Content.ToString() == "期末飞机数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
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
            }

            //控制柱状趋势图的标签颜色
            foreach (var item in ((BarGrid.Children[1] as ScrollViewer).Content as StackPanel).Children)
            {
                StackPanel stackpanel = item as StackPanel;
                CheckBox checkbox = stackpanel.Children[0] as CheckBox;
                if (checkbox.Content.ToString() == "飞机净增数")
                {
                    stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary["飞机数"]));
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
                    }
                    else
                    {
                        if (radpiechart.EmptyContent.ToString() == "飞机数分布")
                        {
                            AircraftWindow.Close();
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
        ///饼图的分布对象
        /// </summary>
        public class FleetAircraft
        {
            public FleetAircraft()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string Aircraft { get; set; }//飞机相关的名称
            public decimal Amount { get; set; }//分布的计数
            public string ToolTip { get; set; }//显示的提示
            public string Color { get; set; }//航空公司颜色
        }

        /// <summary>
        /// 趋势图的对象
        /// </summary>
        public class FleetAircraftTrend
        {
            //public string Aircraft { get; set; }//飞机相关的名称
            public string DateTime { get; set; }//时间点
            public int AircraftAmount { get; set; }//飞机数的总数
            public string AircraftColor { get; set; }//飞机数的颜色
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.ExportCommand = new DelegateCommand<object>(this.OnExport, this.CanExport);//导出图表源数据（Source data）
            this.ExportGridViewCommand = new DelegateCommand<object>(this.OnExportGridView, this.CanExportGridView);//导出GridView数据
            SubscribeEvent();
            ViewModelInitializer();
            InitalizerRadWindows(AircraftWindow, "Aircraft", 200);
            AddRadMenu(AircraftWindow);
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
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
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

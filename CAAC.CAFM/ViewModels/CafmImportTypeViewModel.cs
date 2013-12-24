using CAAC.CAFM.Converters;
using CAAC.Fleet.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Export(typeof(CafmImportTypeViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmImportTypeViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmImportTypeViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmImportTypeView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmImportTypeView>();
        CommonMethod commonmethod = new CommonMethod();
        private Grid LineGrid, BarGrid, ImportTypePieGrid;//趋势折线图区域，趋势柱状图区域， 引进方式饼图区域
        private RadDateTimePicker StartDateTimePicker, EndDateTimePicker;//开始时间控件， 结束时间控件
        private int i; //导出数据源格式判断
        private RadGridView ExportRadgridview, aircraftDetail; //初始化RadGridView

        #region Property

        private RadWindow ImportTypeWindow = new RadWindow(); //用于单击引进方式饼状图的用户提示

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
                    ImportTypeWindow.Close();
                    if (SelectedTime == "所选时间")
                    {
                        SelectedTimeImportType = "所选时间的引进方式分布图";
                    }
                    else
                    {
                        SelectedTimeImportType = SelectedTime + "末的引进方式分布图";
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
                FleetImportTypeComposition fleetimporttTypecomposition = selectedItem.DataItem as FleetImportTypeComposition;
                DateTime time = Convert.ToDateTime(SelectedTime).AddMonths(1).AddDays(-1);
                var aircraft = this.ViewAircraft.Where(o => o.OperationHistories.Any(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)))
                    .Where(o => o.OperationHistories.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)).ImportCategory.ActionType == "引进");
                List<Aircraft> airlineAircrafts = aircraft.Where(p => this.SameImportType(p.OperationHistories.
                    FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)).ImportCategory.ActionName, fleetimporttTypecomposition.ImportType)).ToList();
                //找到子窗体的RadGridView，并为其赋值
                RadGridView rgv = radwindow.Content as RadGridView;
                rgv.ItemsSource = commonmethod.GetAircraftByTime(airlineAircrafts, time);
                radwindow.Header = header + " " + fleetimporttTypecomposition.ImportType + "：" + fleetimporttTypecomposition.AirTt;
                if (!radwindow.IsOpen)
                {
                    commonmethod.ShowRadWindow(radwindow);
                }
            }
        }

        protected bool SameImportType(string ImportType1, string ImportType2)
        {
            int Index1 = ImportType1.IndexOf("续租");
            if (Index1 >= -1)
            {
                return ImportType1.Contains(ImportType2);
            }
            else
            {
                int Index2 = ImportType2.IndexOf("续租");
                if (Index2 >= -1)
                {
                    return ImportType2.Contains(ImportType1);
                }
                else
                {
                    return ImportType1 == ImportType2;
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
            this.ImportTypePieGrid = _view.ImportTypePieGrid as Grid;
            this.aircraftDetail = _view.aircraftDetail as RadGridView;
            this.aircraftDetail = _view.aircraftDetail as RadGridView;
            this.StartDateTimePicker = _view.StartDateTimePicker as RadDateTimePicker;
            this.EndDateTimePicker = _view.EndDateTimePicker as RadDateTimePicker;
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
            if (radwindow.Name == "ImportType")
            {
                grid = this.ImportTypePieGrid;
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
        /// 根据相应的饼图数据生成饼图标签
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetPieMark(IEnumerable<FleetImportTypeComposition> ienumerable, Grid grid)
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
                textblock.Text = item.ImportType;
                textblock.Style = this._view.Resources.FirstOrDefault(p => p.Key.ToString() == "legendItemStyle").Value as Style;
                barpanel.Children.Add(rectangle);
                barpanel.Children.Add(textblock);
                stackpanel.Children.Add(barpanel);
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
                if ((piedatapoint.DataItem as FleetImportTypeComposition).ImportType == shortname)
                {

                    piedatapoint.IsSelected = !piedatapoint.IsSelected;
                    if (piedatapoint.IsSelected == true)
                    {
                        if (radpiechart.EmptyContent.ToString() == "引进方式分布")
                        {
                            GetGridViewDataSourse(piedatapoint, ImportTypeWindow, "引进方式");
                        }
                    }
                    else
                    {
                        if (radpiechart.EmptyContent.ToString() == "引进方式分布")
                        {
                            ImportTypeWindow.Close();
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
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            if (!this.IsBusy)
            {
                CreateFleetImportTypeTrendCollection();
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

        #region ViewModel 属性 FleetImportTypeTrendCollection --引进方式趋势图的数据源集合

        private List<FleetImportTypeTrend> _FleetImportTypeTrendCollection;
        /// <summary>
        /// 引进方式趋势图的数据源集合
        /// </summary>
        public List<FleetImportTypeTrend> FleetImportTypeTrendCollection
        {
            get { return _FleetImportTypeTrendCollection; }
            set
            {

                if (FleetImportTypeTrendCollection != value)
                {
                    _FleetImportTypeTrendCollection = value;

                    RadCartesianChart radcartesianchart = this.LineGrid.Children[0] as RadCartesianChart;
                    StackPanel stackpanel = (this.LineGrid.Children[1] as ScrollViewer).Content as StackPanel;
                    radcartesianchart.Series.Clear();
                    stackpanel.Children.Clear();

                    if (FleetImportTypeTrendCollection != null)
                    {
                        foreach (var GroupItem in FleetImportTypeTrendCollection.GroupBy(p => p.ImportType).ToList())
                        {

                            Telerik.Windows.Controls.ChartView.LineSeries line = new Telerik.Windows.Controls.ChartView.LineSeries();
                            line.StrokeThickness = 3;
                            line.DisplayName = GroupItem.Key;
                            line.Stroke = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            line.CategoryBinding = commonmethod.CreateBinding("DateTime");
                            line.ValueBinding = commonmethod.CreateBinding("AirNum");
                            line.ItemsSource = GroupItem.ToList();
                            line.PointTemplate = (this.LineGrid.Children[0] as RadCartesianChart).Resources["LinePointTemplate"] as DataTemplate;
                            line.TrackBallInfoTemplate = (this.LineGrid.Children[0] as RadCartesianChart).Resources["LineTrackBallInfoTemplate"] as DataTemplate; ;
                            radcartesianchart.Series.Add(line);

                            StackPanel panel = new StackPanel();
                            panel.Margin = new Thickness(5, 5, 5, 5);
                            panel.Orientation = Orientation.Horizontal;
                            panel.Background = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            CheckBox checkbox = new CheckBox();
                            checkbox.IsChecked = true;
                            checkbox.Checked += checkbox_Checked;
                            checkbox.Unchecked += checkbox_Unchecked;
                            checkbox.Content = GroupItem.Key;
                            checkbox.Foreground=new SolidColorBrush(Colors.White);
                            checkbox.VerticalAlignment = VerticalAlignment.Center;
                            checkbox.Style = this._view.Resources["LegengCheckBoxStyle"] as Style;
                            panel.Children.Add(checkbox);
                            stackpanel.Children.Add(panel);
                        }
                    }

                    this.RaisePropertyChanged(() => this.FleetImportTypeTrendCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftAmountCollection --引进方式趋势图的飞机总数集合

        private List<FleetImportTypeTrend> _AircraftAmountCollection;
        /// <summary>
        /// 引进方式趋势图的数据源集合
        /// </summary>
        public List<FleetImportTypeTrend> AircraftAmountCollection
        {
            get { return _AircraftAmountCollection; }
            set
            {

                if (AircraftAmountCollection != value)
                {
                    _AircraftAmountCollection = value;

                    if (AircraftAmountCollection != null)
                    {
                        //控制趋势图的滚动条
                        if (AircraftAmountCollection != null && AircraftAmountCollection.Count() >= 12)
                        {
                            this._view.LineCategoricalAxis.MajorTickInterval = AircraftAmountCollection.Count() / 6;
                            this._view.BarCategoricalAxis.MajorTickInterval = AircraftAmountCollection.Count() / 6;
                        }
                        else
                        {
                            this._view.LineCategoricalAxis.MajorTickInterval = 1;
                            this._view.BarCategoricalAxis.MajorTickInterval = 1;
                        }
                    }

                    this.RaisePropertyChanged(() => this.AircraftAmountCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetImportTypeCollection--引进方式饼图的数据集合（指定时间点）

        private IEnumerable<FleetImportTypeComposition> _fleetImportTypeCollection;
        /// <summary>
        /// 引进方式饼图的数据集合（指定时间点）
        /// </summary>
        public IEnumerable<FleetImportTypeComposition> FleetImportTypeCollection
        {
            get { return this._fleetImportTypeCollection; }
            private set
            {
                if (this._fleetImportTypeCollection != value)
                {
                    this._fleetImportTypeCollection = value;
                    this.RaisePropertyChanged(() => this.FleetImportTypeCollection);
                    SetPieMark(FleetImportTypeCollection, this.ImportTypePieGrid);
                }
            }
        }

        #endregion

        #region ViewModel 属性 AircraftCollection --引进方式饼图所对应的所有飞机数据（指定时间点）

        private List<Aircraft> _aircraftCollection;
        /// <summary>
        /// 引进方式饼图所对应的所有飞机数据（指定时间点）
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

        #region ViewModel 属性 SelectedTimeImportType--引进方式饼图的标识提示

        private string _SelectedTimeImportType = "所选时间的引进方式分布图";
        /// <summary>
        /// 引进方式饼图的标识提示
        /// </summary>
        public string SelectedTimeImportType
        {
            get { return _SelectedTimeImportType; }
            set
            {

                if (SelectedTimeImportType != value)
                {
                    _SelectedTimeImportType = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeImportType);
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
                    CreateFleetImportTypeTrendCollection();
                    this.RaisePropertyChanged(() => this.SelectedIndex);

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
                    CreateFleetImportTypeTrendCollection();
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
                    CreateFleetImportTypeTrendCollection();
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

        #region ViewModel 属性 AircraftColor --期末飞机数的颜色

        private string _AircraftColor = new CommonMethod().GetRandomColor();
        /// <summary>
        /// 期末飞机数的颜色
        /// </summary>
        public string AircraftColor
        {
            get { return _AircraftColor; }
            set
            {

                if (AircraftColor != value)
                {
                    _AircraftColor = value;
                    this.RaisePropertyChanged(() => this.AircraftColor);
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
                if (menu.Name == "TrendGridData")
                {
                    if (FleetImportTypeTrendCollection == null || FleetImportTypeTrendCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("DateTime", "时间点");
                    columnsList.Add("ImportType", "引进类型");
                    columnsList.Add("AirNum", "飞机数(架)");
                    columnsList.Add("Amount", "期末飞机数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, FleetImportTypeTrendCollection, "ImportType");
                }
                else if (menu.Name == "ImportTypePieGridData")
                {
                    if (FleetImportTypeCollection == null || FleetImportTypeCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("ImportType", "引进方式");
                    columnsList.Add("AirNum", "引进方式对应的飞机数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, FleetImportTypeCollection, "PieImportType");
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
                if (menu.Name == "TrendGridImage")
                {
                    //导出图片
                    if (this.LineGrid != null)
                    {
                        commonmethod.ExportToImage(this.LineGrid.Parent as Grid);
                    }
                }
                else if (menu.Name == "ImportTypePieGridImage")
                {
                    //导出图片
                    if (this.ImportTypePieGrid != null)
                    {
                        commonmethod.ExportToImage(this.ImportTypePieGrid);
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
                if (i % 5 == 3 && i >= 8 && (sender as RadGridView).Name == "ImportType")
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
            if (rmi.DataContext.ToString() == "ImportType")
            {
                rgview = ImportTypeWindow.Content as RadGridView;

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
        /// 获取引进方式趋势图的数据源集合
        /// </summary>
        /// <returns></returns>
        private void CreateFleetImportTypeTrendCollection()
        {
            List<FleetImportTypeTrend> ImportTypeCollection = new List<FleetImportTypeTrend>();
            List<FleetImportTypeTrend> AmountCollection = new List<FleetImportTypeTrend>();

            #region 引进方式XML文件的读写
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "引进方式");

            XElement aircraftcolor = null;
            XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "运力变化"))
            {
                aircraftcolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "运力变化");

                if (aircraftcolor != null)
                {
                    this.AircraftColor = aircraftcolor.Descendants("Item").FirstOrDefault(p => p.Attribute("Name").Value == "飞机数").Attribute("Color").Value;
                }
                else
                {
                    this.AircraftColor = commonmethod.GetRandomColor();
                }
            }

            XElement importcolor = null;
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "引进方式"))
            {
                importcolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "引进方式");
            }



            if (xmlconfig != null)
            {
                XElement xelement = xmlconfig.XmlContent;
                if (xelement != null)
                {
                    foreach (XElement datetime in xelement.Descendants("DateTime"))
                    {
                        string CurrentTime = Convert.ToDateTime(datetime.Attribute("EndOfMonth").Value).ToString("yyyy/M");

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


                        foreach (XElement type in datetime.Descendants("Type"))
                        {
                            if (type.Attribute("TypeName").Value == "引进方式")
                            {
                                string CurrentAmount = type.Attribute("Amount").Value;

                                //飞机总数柱状集合
                                FleetImportTypeTrend aircraftamount = new FleetImportTypeTrend();
                                aircraftamount.DateTime = CurrentTime;
                                aircraftamount.Amount = Convert.ToInt32(CurrentAmount);
                                aircraftamount.Color = this.AircraftColor;
                                AmountCollection.Add(aircraftamount);

                                foreach (XElement item in type.Descendants("Item"))
                                {
                                    //引进方式折线集合
                                    FleetImportTypeTrend fleetimporttypetrend = new FleetImportTypeTrend();
                                    fleetimporttypetrend.ImportType = item.Attribute("Name").Value;
                                    fleetimporttypetrend.DateTime = CurrentTime;
                                    fleetimporttypetrend.AirNum = Convert.ToInt32(item.Value);
                                    fleetimporttypetrend.Amount = Convert.ToInt32(CurrentAmount);
                                    if (importcolor != null)
                                    {
                                        fleetimporttypetrend.Color = importcolor.Descendants("Item")
                                            .FirstOrDefault(p => p.Attribute("Name").Value == fleetimporttypetrend.ImportType).Attribute("Color").Value;
                                    }
                                    ImportTypeCollection.Add(fleetimporttypetrend);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            SelectedTime = "所选时间";
            FleetImportTypeCollection = null;
            AircraftCollection = null;
            this.Zoom = new Size(1, 1);

            if (ImportTypeCollection.Count() > 0 && AmountCollection.Count() > 0)
            {
                FleetImportTypeTrendCollection = ImportTypeCollection;
                AircraftAmountCollection = AmountCollection;
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
                FleetImportTypeTrend FleetImportTypeTrend = SelectedPoint.DataItem as FleetImportTypeTrend;
                if (SelectedTime != FleetImportTypeTrend.DateTime)
                {
                    //选中时间点
                    SelectedTime = FleetImportTypeTrend.DateTime;

                    DateTime time = Convert.ToDateTime(FleetImportTypeTrend.DateTime).AddMonths(1).AddDays(-1);
                    var aircraft = this.ViewAircraft
                        .Where(o => o.OperationHistories.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)))
                        .Where(o => o.OperationHistories.FirstOrDefault(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)).ImportCategory.ActionType == "引进");

                    #region 引进方式XML文件的读写
                    var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "引进方式");

                    XElement importtypecolor = null;
                    XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                    if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "引进方式"))
                    {
                        importtypecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "引进方式");
                    }

                    if (xmlconfig != null)
                    {
                        List<FleetImportTypeComposition> ImportTypeList = new List<FleetImportTypeComposition>();//引进方式饼图集合

                        XElement xelement = xmlconfig.XmlContent.Descendants("DateTime").FirstOrDefault(p => Convert.ToDateTime(p.Attribute("EndOfMonth").Value) == time);
                        if (xelement != null)
                        {
                            foreach (XElement type in xelement.Descendants("Type"))
                            {
                                if (type.Attribute("TypeName").Value == "引进方式")
                                {
                                    foreach (XElement item in type.Descendants("Item"))
                                    {
                                        FleetImportTypeComposition fleetimporttypecomposition = new FleetImportTypeComposition();
                                        fleetimporttypecomposition.ImportType = item.Attribute("Name").Value;
                                        fleetimporttypecomposition.AirNum = Convert.ToInt32(item.Value);
                                        fleetimporttypecomposition.AirTt = item.Value + " 架,占 " + item.Attribute("Percent").Value;
                                        if (importtypecolor != null)
                                        {
                                            fleetimporttypecomposition.Color = importtypecolor.Descendants("Item")
                                                .FirstOrDefault(p => p.Attribute("Name").Value == fleetimporttypecomposition.ImportType).Attribute("Color").Value;
                                        }
                                        if (fleetimporttypecomposition.AirNum > 0)
                                        {
                                            ImportTypeList.Add(fleetimporttypecomposition);
                                        }
                                    }
                                }
                            }
                        }
                        FleetImportTypeCollection = ImportTypeList;
                    }
                    #endregion

                    //飞机详细数据
                    AircraftCollection = commonmethod.GetAircraftByTime(aircraft.ToList(), time);
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
            if (radchartbase.EmptyContent.ToString() == "引进方式分布")
            {
                stackpanel = (this.ImportTypePieGrid.Children[1] as ScrollViewer).Content as StackPanel;
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
                    .FirstOrDefault(p => ((p as StackPanel).Children[1] as TextBlock).Text == (SelectedPoint.DataItem as FleetImportTypeComposition).ImportType) as StackPanel;
                System.Windows.Shapes.Rectangle rectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 12;
                rectangle.Height = 12;

                if (radchartbase.EmptyContent.ToString() == "引进方式分布")
                {
                    GetGridViewDataSourse(SelectedPoint, ImportTypeWindow, "引进方式");
                }
            }
            else
            {
                if (radchartbase.EmptyContent.ToString() == "引进方式分布")
                {
                    ImportTypeWindow.Close();
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
        public class FleetImportTypeComposition
        {
            public FleetImportTypeComposition()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string ImportType { get; set; }
            public decimal AirNum { get; set; }
            public string AirTt { get; set; }
            public string Color { get; set; }
        }

        /// <summary>
        /// 引进方式趋势引进方式
        /// </summary>
        public class FleetImportTypeTrend
        {
            public FleetImportTypeTrend()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string ImportType { get; set; }//引进名称
            public int AirNum { get; set; }//引进方式的飞机数
            public string DateTime { get; set; }//时间点
            public int Amount { get; set; }//飞机总数
            public string Color { get; set; }//引进的颜色
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
            InitalizerRadWindows(ImportTypeWindow, "ImportType", 220);
            AddRadMenu(ImportTypeWindow);
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
            ImportTypeWindow.Close();
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

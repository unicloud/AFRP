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
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Data;
using System.ServiceModel.DomainServices.Client;
using System.Collections;
using Telerik.Windows.Controls;
using Microsoft.Practices.Prism.ViewModel;
using UniCloud.AFRP.Converters;
using UniCloud.AFRP.Operation;
using UniCloud.Fleet.Services;
using UniCloud.Fleet.Models;
using Telerik.Windows;
using Telerik.Charting;
using System.Windows.Media;
using Telerik.Windows.Controls.ChartView;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpFleetAgeViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpFleetAgeViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpFleetAgeViewModel()
        {
            //this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        UniCloud.AFRP.Views.AfrpFleetAgeView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpFleetAgeView>();

        CommonMethod commonmethod = new CommonMethod();
        private Grid LineGrid, AgePieGrid;//趋势折线图区域， 机龄饼图区域

        private Dictionary<string, List<Aircraft>> AircraftByAgeDic = new Dictionary<string, List<Aircraft>>();//机龄饼图的飞机数据分布字典
        private RadDateTimePicker StartDateTimePicker, EndDateTimePicker;//开始时间控件， 结束时间控件
        private int i; //导出数据源格式判断
        private RadGridView ExportRadgridview, aircraftDetail; //初始化RadGridView

        #region Property

        private RadWindow AgeWindow = new RadWindow(); //用于单击机龄饼状图的用户提示

        #region 属性 SelectedTime --所选的时间点

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
                    AgeWindow.Close();

                    if (SelectedTime == "所选时间")
                    {
                        SelectedTimeAge = "所选时间和机型的机龄分布图";
                    }
                    else
                    {
                        SelectedTimeAge = SelectedTime + " 机型：" + SelectedType + "的机龄分布图";
                    }
                }
            }
        }
        #endregion


        #region 属性 SelectedType --所选的机型

        private string _SelectedType = string.Empty;
        /// <summary>
        /// 所选的时间点
        /// </summary>
        public string SelectedType
        {
            get { return _SelectedType; }
            set
            {

                if (SelectedType != value)
                {
                    _SelectedType = value;
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
                DateTime time = Convert.ToDateTime(SelectedTime).AddMonths(1).AddDays(-1);
                FleetAgeComposition fleetagecomposition = selectedItem.DataItem as FleetAgeComposition;
                //找到子窗体的RadGridView，并为其赋值
                RadGridView rgv = radwindow.Content as RadGridView;
                if (this.AircraftByAgeDic != null && this.AircraftByAgeDic.ContainsKey(fleetagecomposition.AgeGroup))
                {
                    rgv.ItemsSource = commonmethod.GetAircraftByTime(AircraftByAgeDic[fleetagecomposition.AgeGroup], time);
                }
                AgeWindow.Header = fleetagecomposition.AgeGroup + "的飞机数：" + fleetagecomposition.ToolTip;
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
            this.AgePieGrid = _view.AgePieGrid as Grid;
            this.aircraftDetail = _view.aircraftDetail as RadGridView;
            //控制界面起止时间控件的字符串格式化
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
            if (radwindow.Name == "Age")
            {
                grid = this.AgePieGrid;
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
        private void SetPieMark(IEnumerable<FleetAgeComposition> ienumerable, Grid grid)
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
                textblock.Text = item.AgeGroup;
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
                if ((piedatapoint.DataItem as FleetAgeComposition).AgeGroup == shortname)
                {

                    piedatapoint.IsSelected = !piedatapoint.IsSelected;
                    if (piedatapoint.IsSelected == true)
                    {
                        if (radpiechart.EmptyContent.ToString() == "机龄分布")
                        {
                            GetGridViewDataSourse(piedatapoint, AgeWindow, "机龄");
                        }
                    }
                    else
                    {
                        if (radpiechart.EmptyContent.ToString() == "机龄分布")
                        {
                            AgeWindow.Close();
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
                CreateFleetAgeTrendCollection();
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

        #region ViewModel 属性 FleetManufacturerTrendCollection --平均机龄趋势图的数据源集合

        private List<FleetAgeTrend> _FleetAgeTrendCollection;
        /// <summary>
        /// 平均机龄趋势图的数据源集合
        /// </summary>
        public List<FleetAgeTrend> FleetAgeTrendCollection
        {
            get { return _FleetAgeTrendCollection; }
            set
            {

                if (FleetAgeTrendCollection != value)
                {
                    _FleetAgeTrendCollection = value;

                    RadCartesianChart radcartesianchart = this.LineGrid.Children[0] as RadCartesianChart;
                    StackPanel stackpanel = (this.LineGrid.Children[1] as ScrollViewer).Content as StackPanel;
                    radcartesianchart.Series.Clear();
                    stackpanel.Children.Clear();
                    if (_FleetAgeTrendCollection != null)
                    {
                        foreach (var GroupItem in _FleetAgeTrendCollection.GroupBy(p => p.AircraftType).ToList())
                        {
                            Telerik.Windows.Controls.ChartView.LineSeries line = new Telerik.Windows.Controls.ChartView.LineSeries() { IsHitTestVisible = true };
                            line.StrokeThickness = 3;
                            line.DisplayName = GroupItem.Key;
                            line.Stroke = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            line.CategoryBinding = commonmethod.CreateBinding("DateTime");
                            line.ValueBinding = commonmethod.CreateBinding("AverageAge");
                            line.ItemsSource = GroupItem.ToList();
                            if (line.DisplayName != "所有机型")
                            {
                                line.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                this._view.LineLinearAxis.ElementBrush = line.Stroke; 
                            }
                            line.PointTemplate = (this.LineGrid.Children[0] as RadCartesianChart).Resources["LinePointTemplate"] as DataTemplate;
                            radcartesianchart.Series.Add(line);

                            StackPanel panel = new StackPanel();
                            panel.Margin = new Thickness(5, 5, 5, 5);
                            panel.Orientation = Orientation.Horizontal;
                            panel.Background = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            CheckBox checkbox = new CheckBox();
                            if (line.DisplayName == "所有机型")
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
                            checkbox.Foreground=new SolidColorBrush(Colors.White);
                            checkbox.VerticalAlignment = VerticalAlignment.Center;
                            checkbox.Style = this._view.Resources["LegengCheckBoxStyle"] as Style;
                            panel.Children.Add(checkbox);
                            stackpanel.Children.Add(panel);
                        }
                    }

                    //控制趋势图的滚动条
                    int datetimecount = FleetAgeTrendCollection.Select(p => p.DateTime).Distinct().Count();
                    if (FleetAgeTrendCollection != null && datetimecount >= 12)
                    {
                        this._view.LineCategoricalAxis.MajorTickInterval = datetimecount / 6;
                    }
                    else
                    {
                        this._view.LineCategoricalAxis.MajorTickInterval = 1;
                    }

                    this.RaisePropertyChanged(() => this.FleetAgeTrendCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetAgeCollection--机龄饼图的数据集合（指定时间点）

        private List<FleetAgeComposition> _fleetAgeCollection;
        /// <summary>
        /// 平均机龄饼图饼图的数据集合（指定时间点）
        /// </summary>
        public List<FleetAgeComposition> FleetAgeCollection
        {
            get { return this._fleetAgeCollection; }
            private set
            {
                if (this._fleetAgeCollection != value)
                {
                    this._fleetAgeCollection = value;
                    this.RaisePropertyChanged(() => this.FleetAgeCollection);
                    SetPieMark(FleetAgeCollection, this.AgePieGrid);
                }
            }
        }

        #endregion

        #region ViewModel 属性 AircraftCollection --机龄饼图所对应的所有飞机数据（指定时间点）

        private List<Aircraft> _aircraftCollection;
        /// <summary>
        ///机龄饼图所对应的所有飞机数据（指定时间点）
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

        #region ViewModel 属性 SelectedTimeAge --机龄饼图的标识提示

        private string _SelectedTimeAge = "所选时间和机型的机龄分布图";
        /// <summary>
        /// 机龄饼图的标识提示
        /// </summary>
        public string SelectedTimeAge
        {
            get { return _SelectedTimeAge; }
            set
            {

                if (SelectedTimeAge != value)
                {
                    _SelectedTimeAge = value;
                    this.RaisePropertyChanged(() => this.SelectedTimeAge);
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
                    CreateFleetAgeTrendCollection();
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
                    CreateFleetAgeTrendCollection();
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
                    CreateFleetAgeTrendCollection();
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

        #region ViewModel 命令 PieDeployCommand --机龄饼图的配置

        // 机龄饼图的配置
        public DelegateCommand<object> PieDeployCommand { get; private set; }
        private void OnPieDeploy(object obj)
        {
            UniCloud.AFRP.Views.AfrpAgeDeployView window = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpAgeDeployView>();
            window.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            window.Closed += new EventHandler<WindowClosedEventArgs>(window_Closed);
            window.ShowDialog();
        }

        /// <summary>
        /// 机龄配置窗体关闭后刷新饼图的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void window_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow window = sender as RadWindow;
            if (window.Tag != null && Convert.ToBoolean(window.Tag))
            {
                if (SelectedTime != "所选时间")
                {
                    AgeWindow.Close();
                    DateTime time = Convert.ToDateTime(SelectedTime).AddMonths(1).AddDays(-1);
                    CreateFleetAgeCollection(SelectedType, time);
                }
                window.Tag = false;
            }
        }
        private bool _canPieDeploy = true;
        public bool CanPieDeploy(object obj)
        {
            return _canPieDeploy;
        }
        #endregion

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
                    if (FleetAgeTrendCollection == null || FleetAgeTrendCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("DateTime", "时间点");
                    columnsList.Add("AircraftType", "机型");
                    columnsList.Add("AverageAge", "机型平均机龄");
                    ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAgeTrendCollection, "LineAge");
                }
                else if (menu.Name == "AgePieGridData")
                {
                    if (FleetAgeCollection == null || FleetAgeCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("AgeGroup", "机龄范围");
                    columnsList.Add("GroupCount", "飞机数(架)");
                    ExportRadgridview = ImageAndGridOperation.CreatDataGridView(columnsList, FleetAgeCollection, "PieAge");
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
            else if (menu != null && menu.Header.ToString() == "导出图片")
            {
                if (menu.Name == "LineGridImage")
                {
                    //导出图片
                    if (this.LineGrid != null)
                    {
                        commonmethod.ExportToImage(this.LineGrid);
                    }
                }
                else if (menu.Name == "AgePieGridImage")
                {
                    //导出图片
                    if (this.AgePieGrid != null)
                    {
                        commonmethod.ExportToImage(this.AgePieGrid);
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
                if (i % 4 == 3 && i >= 7 && (sender as RadGridView).Name == "LineAge")
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
            if (rmi.DataContext.ToString() == "Age")
            {
                rgview = AgeWindow.Content as RadGridView;

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


        #endregion

        #region Methods


        /// <summary>
        /// 获取平均机龄趋势图的数据源集合
        /// </summary>
        /// <returns></returns>
        private void CreateFleetAgeTrendCollection()
        {
            List<FleetAgeTrend> collection = new List<FleetAgeTrend>();

            #region 平均机龄XML文件的读写
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄分析");

            string aircraftcolor = string.Empty;
            XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "运力变化"))
            {
                aircraftcolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "运力变化")
                    .Descendants("Item").FirstOrDefault(p => p.Attribute("Name").Value == "飞机数").Attribute("Color").Value;
            }

            XElement typecolor = null;
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "机型"))
            {
                typecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机型");
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
                            FleetAgeTrend fleetagetrend = new FleetAgeTrend();//折线图的总数对象
                            fleetagetrend.AircraftType = type.Attribute("TypeName").Value;
                            fleetagetrend.AverageAge = Math.Round(Convert.ToDouble(type.Attribute("Amount").Value), 4);
                            fleetagetrend.DateTime = CurrentTime;
                            if (fleetagetrend.AircraftType == "所有机型")
                            {
                                if (!string.IsNullOrEmpty(aircraftcolor))
                                {
                                    fleetagetrend.Color = aircraftcolor;
                                }
                            }
                            else
                            {
                                if (typecolor != null)
                                {
                                    fleetagetrend.Color = typecolor.Descendants("Item")
                                        .FirstOrDefault(p => p.Attribute("Name").Value == fleetagetrend.AircraftType).Attribute("Color").Value;
                                }
                            }
                            //添加进相应的数据源集合
                            collection.Add(fleetagetrend);
                        }
                    }
                }
            }
            #endregion

            SelectedType = string.Empty;
            SelectedTime = "所选时间";
            //对界面数据集合进行重新初始化
            FleetAgeCollection = null;
            AircraftByAgeDic = new Dictionary<string, List<Aircraft>>();
            AircraftCollection = null;

            FleetAgeTrendCollection = collection;
        }

        /// <summary>
        /// 根据选中时间和机型生成相应的饼图和飞机数据
        /// </summary>
        /// <param name="AircraftType">选中机型</param>
        /// <param name="time">选中时间</param>
        public void CreateFleetAgeCollection(string AircraftType, DateTime time)
        {
            //机龄的饼图分布集合
            List<FleetAgeComposition> AgeCompositionList = new List<FleetAgeComposition>();
            //机龄饼图的飞机数据分布字典
            AircraftByAgeDic = new Dictionary<string, List<Aircraft>>();

            IEnumerable<Aircraft> aircraft = null;
            if (AircraftType == "所有机型")
            {
                aircraft = this.ViewAircraft.Where(p => p.FactoryDate != null)
                    .Where(o => o.AircraftBusinesses.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)) && o.FactoryDate <= time && !(o.ExportDate != null && o.ExportDate < time));
            }
            else
            {
                aircraft = this.ViewAircraft.Where(p => p.FactoryDate != null)
                    .Where(o => o.AircraftBusinesses.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)) && o.FactoryDate <= time && !(o.ExportDate != null && o.ExportDate < time))
                    .Where(o => o.AircraftBusinesses.FirstOrDefault(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)).AircraftType.Name == AircraftType);
            }

            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄配置");

            XElement agecolor = null;
            XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "机龄"))
            {
                agecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机龄");
            }
            if (xmlconfig != null && aircraft != null && aircraft.Count() > 0)
            {
                XElement xelement = xmlconfig.XmlContent;
                foreach (XElement item in xelement.Descendants("Item"))
                {
                    int StartYear = Convert.ToInt32(item.Attribute("Start").Value);
                    int EndYear = Convert.ToInt32(item.Attribute("End").Value);
                    //设置相应机龄范围的飞机数据，用于弹出窗体的数据显示
                    var AircraftByAge = aircraft.Where(p =>
                        EndYear * 12 > (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month)
                        && (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month) >= StartYear * 12).ToList();
                    if (AircraftByAge != null && AircraftByAge.Count() > 0)
                    {
                        FleetAgeComposition AgeComposition = new FleetAgeComposition();
                        AgeComposition.AgeGroup = item.Value;
                        AgeComposition.GroupCount = AircraftByAge.Count();
                        AgeComposition.ToolTip = AgeComposition.GroupCount + " 架，占" + (AircraftByAge.Count() * 100 / aircraft.Count()).ToString("##0") + "%";
                        if (agecolor != null)
                        {
                            AgeComposition.Color = agecolor.Descendants("Item")
                                .FirstOrDefault(p => p.Attribute("Name").Value == AgeComposition.AgeGroup).Attribute("Color").Value;
                        }
                        AgeCompositionList.Add(AgeComposition);
                        AircraftByAgeDic.Add(AgeComposition.AgeGroup, AircraftByAge);
                    }
                }
            }

            FleetAgeCollection = AgeCompositionList;
            if (aircraft != null)
            {
                //飞机详细数据集合
                AircraftCollection = commonmethod.GetAircraftByTime(aircraft.ToList(), time);
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
                FleetAgeTrend fleetagetrend = SelectedPoint.DataItem as FleetAgeTrend;
                if (SelectedTime != fleetagetrend.DateTime || SelectedType != fleetagetrend.AircraftType)
                {
                    //选中机型
                    SelectedType = fleetagetrend.AircraftType;
                    //选中时间点
                    SelectedTime = fleetagetrend.DateTime;

                    DateTime time = Convert.ToDateTime(fleetagetrend.DateTime).AddMonths(1).AddDays(-1);
                    CreateFleetAgeCollection(SelectedType, time);
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
            if (radchartbase.EmptyContent.ToString() == "机龄分布")
            {
                stackpanel = (this.AgePieGrid.Children[1] as ScrollViewer).Content as StackPanel;
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
                    .FirstOrDefault(p => ((p as StackPanel).Children[1] as TextBlock).Text == (SelectedPoint.DataItem as FleetAgeComposition).AgeGroup) as StackPanel;
                System.Windows.Shapes.Rectangle rectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 12;
                rectangle.Height = 12;

                if (radchartbase.EmptyContent.ToString() == "机龄分布")
                {
                    GetGridViewDataSourse(SelectedPoint, AgeWindow, "机龄");
                }
            }
            else
            {
                if (radchartbase.EmptyContent.ToString() == "机龄分布")
                {
                    AgeWindow.Close();
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
                    RadCartesianChart chart = this.LineGrid.Children[0] as RadCartesianChart;
                    chart.Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;

                    Size size = chart.Zoom;
                    chart.Zoom = new Size(size.Width + 0.01, size.Height);
                    chart.Zoom = size;
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
            }
        }

        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }
        #endregion

        #region Class

        /// <summary>
        ///机龄饼图的分布对象
        /// </summary>
        public class FleetAgeComposition
        {
            public FleetAgeComposition()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string AgeGroup { get; set; }
            public decimal GroupCount { get; set; }
            public string ToolTip { get; set; }
            public string Color { get; set; }
        }

        /// <summary>
        /// 平均机龄趋势对象
        /// </summary>
        public class FleetAgeTrend
        {
            public FleetAgeTrend()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string AircraftType { get; set; }//机型名称
            public double AverageAge { get; set; }//机型的平均年龄
            public string DateTime { get; set; }//时间点
            public string Color { get; set; }//机型的颜色
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            //机龄饼图的配置
            this.PieDeployCommand = new DelegateCommand<object>(this.OnPieDeploy, this.CanPieDeploy);
            this.ExportCommand = new DelegateCommand<object>(this.OnExport, this.CanExport);//导出图表源数据（Source data）
            this.ExportGridViewCommand = new DelegateCommand<object>(this.OnExportGridView, this.CanExportGridView);
            SubscribeEvent();
            ViewModelInitializer();
            InitalizerRadWindows(AgeWindow, "Age", 220);
            AddRadMenu(AgeWindow);
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
            AgeWindow.Close();
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

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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmFleetRegisteredViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmFleetRegisteredViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmFleetRegisteredViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmFleetRegisteredView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmFleetRegisteredView>();
        CommonMethod commonmethod = new CommonMethod();
        private Grid MonthGrid, YearGrid;
        private RadDateTimePicker StartDateTimePicker, EndDateTimePicker;//开始时间控件， 结束时间控件
        private RadGridView MonthExportRadgridview, YearExportRadgridview; //初始化RadGridView
        private int i; //导出数据源格式判断

        #region Property

        #endregion

        #region Method

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.MonthGrid = _view.MonthGrid as Grid;
            this.YearGrid = _view.YearGrid as Grid;
            this.StartDateTimePicker = _view.StartDateTimePicker as RadDateTimePicker;
            this.EndDateTimePicker = _view.EndDateTimePicker as RadDateTimePicker;
            StartDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
            EndDateTimePicker.Culture.DateTimeFormat.ShortDatePattern = "yyyy/M";
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            if (!this.IsBusy)
            {
                CreatFleetRegisteredTrendMonthCollection();
                CreatFleetRegisteredTrendYearCollection();
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
                this.IsBusy = this.IsBusyXmlConfig;
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

        #region ViewModel 属性 FleetAircraftTrendMonthCollection --月平均在册的机型飞机数集合

        private List<FleetRegisteredTrend> _FleetRegisteredTrendMonthCollection;
        /// <summary>
        /// 月平均在册的机型飞机数集合
        /// </summary>
        public List<FleetRegisteredTrend> FleetRegisteredTrendMonthCollection
        {
            get { return _FleetRegisteredTrendMonthCollection; }
            set
            {

                if (FleetRegisteredTrendMonthCollection != value)
                {
                    _FleetRegisteredTrendMonthCollection = value;

                    RadCartesianChart radcartesianchart = this.MonthGrid.Children[0] as RadCartesianChart;
                    StackPanel stackpanel = (this.MonthGrid.Children[1] as ScrollViewer).Content as StackPanel;

                    radcartesianchart.Series.Clear();
                    stackpanel.Children.Clear();

                    if (FleetRegisteredTrendMonthCollection != null)
                    {
                        foreach (var GroupItem in FleetRegisteredTrendMonthCollection.GroupBy(p => p.AircraftType).ToList())
                        {
                            LineSeries line = new LineSeries();
                            line.StrokeThickness = 3;
                            line.DisplayName = GroupItem.Key;
                            line.Stroke = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            line.CategoryBinding = commonmethod.CreateBinding("DateTime");
                            line.ValueBinding = commonmethod.CreateBinding("RegisteredCount");
                            line.ItemsSource = GroupItem.ToList();
                            if (line.DisplayName != "所有机型")
                            {
                                line.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                this._view.MonthLinearAxis.ElementBrush = line.Stroke;
                            }
                            line.PointTemplate = this._view.Resources["PointTemplate"] as DataTemplate;
                            line.TrackBallInfoTemplate = this._view.Resources["TrackBallInfoTemplate"] as DataTemplate; ;
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
                    int datetimecount = FleetRegisteredTrendMonthCollection.Select(p => p.DateTime).Distinct().Count();
                    if (FleetRegisteredTrendMonthCollection != null && datetimecount >= 12)
                    {
                        this._view.MonthCategoricalAxis.MajorTickInterval = datetimecount / 6;
                    }
                    else
                    {
                        this._view.MonthCategoricalAxis.MajorTickInterval = 1;
                    }

                    this.RaisePropertyChanged(() => this.FleetRegisteredTrendMonthCollection);
                }
            }
        }
        #endregion

        #region ViewModel 属性 FleetRegisteredTrendYearCollection --年平均在册的机型飞机数集合

        private List<FleetRegisteredTrend> _FleetRegisteredTrendYearCollection;
        /// <summary>
        /// 年平均在册的机型飞机数集合
        /// </summary>
        public List<FleetRegisteredTrend> FleetRegisteredTrendYearCollection
        {
            get { return _FleetRegisteredTrendYearCollection; }
            set
            {

                if (FleetRegisteredTrendYearCollection != value)
                {
                    _FleetRegisteredTrendYearCollection = value;

                    RadCartesianChart radcartesianchart = this.YearGrid.Children[0] as RadCartesianChart;
                    StackPanel stackpanel = (this.YearGrid.Children[1] as ScrollViewer).Content as StackPanel;

                    radcartesianchart.Series.Clear();
                    stackpanel.Children.Clear();

                    if (FleetRegisteredTrendYearCollection != null)
                    {
                        foreach (var GroupItem in FleetRegisteredTrendYearCollection.GroupBy(p => p.AircraftType).ToList())
                        {
                            Telerik.Windows.Controls.ChartView.LineSeries line = new Telerik.Windows.Controls.ChartView.LineSeries();
                            line.StrokeThickness = 3;
                            line.DisplayName = GroupItem.Key;
                            line.Stroke = new SolidColorBrush(commonmethod.GetColor(GroupItem.FirstOrDefault().Color));
                            line.CategoryBinding = commonmethod.CreateBinding("DateTime");
                            line.ValueBinding = commonmethod.CreateBinding("RegisteredCount");
                            line.ItemsSource = GroupItem.ToList();
                            if (line.DisplayName != "所有机型")
                            {
                                line.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                this._view.YearLinearAxis.ElementBrush = line.Stroke;
                            }
                            line.PointTemplate = this._view.Resources["PointTemplate"] as DataTemplate;
                            line.TrackBallInfoTemplate = this._view.Resources["TrackBallInfoTemplate"] as DataTemplate; ;
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
                    int datetimecount = FleetRegisteredTrendYearCollection.Select(p => p.DateTime).Distinct().Count();
                    if (FleetRegisteredTrendYearCollection != null && datetimecount >= 12)
                    {
                        this._view.YearCategoricalAxis.MajorTickInterval = datetimecount / 6;
                    }
                    else
                    {
                        this._view.YearCategoricalAxis.MajorTickInterval = 1;
                    }

                    this.RaisePropertyChanged(() => this.FleetRegisteredTrendYearCollection);
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
                    CreatFleetRegisteredTrendMonthCollection();
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
                    CreatFleetRegisteredTrendMonthCollection();
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
                if (menu.Name == "MonthGridData")
                {
                    i = 1;
                    MonthExportRadgridview.ElementExporting -= this.ElementExporting;
                    MonthExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            MonthExportRadgridview.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                        }
                    }
                }
                else if (menu.Name == "YearGridData")
                {
                    i = 1;
                    YearExportRadgridview.ElementExporting -= this.ElementExporting;
                    YearExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                    using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                    {
                        if (stream != null)
                        {
                            YearExportRadgridview.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                        }
                    }
                }
            }
            else if (menu != null && menu.Header.ToString() == "导出图片")
            {
                if (menu.Name == "MonthGridImage")
                {
                    //导出图片
                    if (this.MonthGrid != null)
                    {
                        commonmethod.ExportToImage(this.MonthGrid);
                    }
                }
                else if (menu.Name == "YearGridImage")
                {
                    //导出图片
                    if (this.YearGrid != null)
                    {
                        commonmethod.ExportToImage(this.MonthGrid);
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
                if (i % 4 == 3 && i >= 7)
                {
                    RadGridView a = sender as RadGridView;
                    if (a.Name == "FleetRegisteredMonth")
                    {
                        e.Value = DateTime.Parse(e.Value.ToString()).AddMonths(1).AddDays(-1).ToString("yyyy/M/d");
                    }
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

        #endregion

        #region Methods

        /// <summary>
        /// 获取月平均在册飞机数趋势图的数据源集合
        /// </summary>
        /// <returns></returns>
        private void CreatFleetRegisteredTrendMonthCollection()
        {
            List<FleetRegisteredTrend> fleetregisteredtrendmonthlist = new List<FleetRegisteredTrend>();//月在册的机型飞机数集合
            List<FleetRegisteredTrend> AllFleetRegeisteredtrendMonthList = new List<FleetRegisteredTrend>();

            #region 飞机运力XML文件的读写
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "在册分析");

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

                        XElement typexelement = datetime.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机型");
                        if (typexelement != null)
                        {
                            //平均在册总飞机数的柱状数据
                            FleetRegisteredTrend amounttrendmnth = new FleetRegisteredTrend();//月平均在册的总飞机数对象
                            amounttrendmnth.DateTime = CurrentTime;
                            amounttrendmnth.RegisteredCount = Convert.ToDouble(typexelement.Attribute("Amount").Value);
                            amounttrendmnth.AircraftType = "所有机型";
                            if (!string.IsNullOrEmpty(aircraftcolor))
                            {
                                amounttrendmnth.Color = aircraftcolor;
                            }
                            fleetregisteredtrendmonthlist.Add(amounttrendmnth);

                            foreach (XElement item in typexelement.Descendants("Item"))
                            {
                                FleetRegisteredTrend fleetregisteredtrenmonth = new FleetRegisteredTrend();//月平均在册的机型飞机数对象
                                fleetregisteredtrenmonth.DateTime = CurrentTime;
                                fleetregisteredtrenmonth.AircraftType = item.Attribute("Name").Value;
                                fleetregisteredtrenmonth.RegisteredCount = Convert.ToDouble(item.Value);
                                if (typecolor != null)
                                {
                                    fleetregisteredtrenmonth.Color = typecolor.Descendants("Item")
                                        .FirstOrDefault(p => p.Attribute("Name").Value == fleetregisteredtrenmonth.AircraftType).Attribute("Color").Value;
                                }
                                fleetregisteredtrendmonthlist.Add(fleetregisteredtrenmonth);
                            }
                        }
                    }
                }
            }
            #endregion

            FleetRegisteredTrendMonthCollection = fleetregisteredtrendmonthlist;
            AllFleetRegeisteredtrendMonthList.AddRange(FleetRegisteredTrendMonthCollection);
            //创建RadGridView
            Dictionary<string, string> columnsList = new Dictionary<string, string>();
            columnsList.Add("DateTime", "时间点");
            columnsList.Add("AircraftType", "机型");
            columnsList.Add("RegisteredCount", "月平均在册飞机数");
            MonthExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AllFleetRegeisteredtrendMonthList, "FleetRegisteredMonth");

        }


        /// <summary>
        /// 获取年平均在册飞机数趋势图的数据源集合
        /// </summary>
        /// <returns></returns>
        private void CreatFleetRegisteredTrendYearCollection()
        {
            List<FleetRegisteredTrend> fleetregisteredtrendmonthlist = new List<FleetRegisteredTrend>();//月平均在册的机型飞机数集合
            List<FleetRegisteredTrend> AllFleetRegisteredtrendMonthlist = new List<FleetRegisteredTrend>();//月平均在册的总飞机数集合
            #region 飞机运力XML文件的读写
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "在册分析");

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

                        XElement typexelement = datetime.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机型");
                        if (typexelement != null)
                        {
                            //平均在册总飞机数的柱状数据
                            FleetRegisteredTrend amounttrendmnth = new FleetRegisteredTrend();//月平均在册的总飞机数对象
                            amounttrendmnth.DateTime = CurrentTime;
                            amounttrendmnth.RegisteredCount = Convert.ToDouble(typexelement.Attribute("Amount").Value);
                            amounttrendmnth.AircraftType = "所有机型";
                            if (!string.IsNullOrEmpty(aircraftcolor))
                            {
                                amounttrendmnth.Color = aircraftcolor;
                            }
                            fleetregisteredtrendmonthlist.Add(amounttrendmnth);

                            foreach (XElement item in typexelement.Descendants("Item"))
                            {
                                FleetRegisteredTrend fleetregisteredtrenmonth = new FleetRegisteredTrend();//月平均在册的机型飞机数对象
                                fleetregisteredtrenmonth.DateTime = CurrentTime;
                                fleetregisteredtrenmonth.AircraftType = item.Attribute("Name").Value;
                                fleetregisteredtrenmonth.RegisteredCount = Convert.ToDouble(item.Value);
                                if (typecolor != null)
                                {
                                    fleetregisteredtrenmonth.Color = typecolor.Descendants("Item")
                                        .FirstOrDefault(p => p.Attribute("Name").Value == fleetregisteredtrenmonth.AircraftType).Attribute("Color").Value;
                                }
                                fleetregisteredtrendmonthlist.Add(fleetregisteredtrenmonth);
                            }
                        }
                    }
                }
            }
            #endregion




            List<FleetRegisteredTrend> fleetregisteredtrendyearlist = new List<FleetRegisteredTrend>();//年平均在册的机型飞机数集合
            if (fleetregisteredtrendmonthlist != null)
            {
                //获取按机型分类的年平均在册飞机数的集合
                fleetregisteredtrendmonthlist.GroupBy(p => p.AircraftType).ToList().ForEach(p =>
                    fleetregisteredtrendyearlist.AddRange(p.GroupBy(pp => Convert.ToDateTime(pp.DateTime).Year).Select(o => new FleetRegisteredTrend
                       {
                           DateTime = o.Key.ToString(),
                           AircraftType = p.Key,
                           RegisteredCount = Math.Round(o.Sum(a => a.RegisteredCount * Convert.ToDateTime(a.DateTime).AddMonths(1).AddDays(-1).Day) / (new DateTime(o.Key + 1, 1, 1) - new DateTime(o.Key, 1, 1)).TotalDays, 4),
                           Color = p.FirstOrDefault().Color
                       })
                    )
                );
            }

            FleetRegisteredTrendYearCollection = fleetregisteredtrendyearlist;
            AllFleetRegisteredtrendMonthlist.AddRange(FleetRegisteredTrendYearCollection);
            //创建RadGridView
            Dictionary<string, string> columnsList2 = new Dictionary<string, string>();
            columnsList2.Add("DateTime", "年份");
            columnsList2.Add("AircraftType", "机型");
            columnsList2.Add("RegisteredCount", "年平均在册飞机数");
            YearExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList2, AllFleetRegisteredtrendMonthlist, "FleetRegisteredYear");
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
                if (grid.Name == "MonthGrid")
                {
                    RadCartesianChart chart = this.MonthGrid.Children[0] as RadCartesianChart;
                    chart.Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;

                    Size size = chart.Zoom;
                    chart.Zoom = new Size(size.Width + 0.01, size.Height);
                    chart.Zoom = size;
                }
                else if (grid.Name == "YearGrid")
                {
                    RadCartesianChart chart = this.YearGrid.Children[0] as RadCartesianChart;
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
                if (grid.Name == "MonthGrid")
                {
                    (this.MonthGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (grid.Name == "YearGrid")
                {
                    (this.YearGrid.Children[0] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
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
        public class FleetRegisteredTrend
        {
            public FleetRegisteredTrend()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string AircraftType { get; set; }//机型名称
            public double RegisteredCount { get; set; }//机型的平均在册飞机数
            public string DateTime { get; set; }//时间点
            public string Amount { get; set; }//所有机型在册飞机数
            public string Color { get; set; }//颜色
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.ExportCommand = new DelegateCommand<object>(this.OnExport, this.CanExport);//导出图表源数据（Source data）
            SubscribeEvent();
            ViewModelInitializer();
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

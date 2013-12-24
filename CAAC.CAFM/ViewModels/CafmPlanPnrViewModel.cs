using CAAC.CAFM.Converters;
using CAAC.Fleet.Services;
using CAAC.Infrastructure;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
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
using Telerik.Windows.Data;
using UniCloud.CAFM.CustomerClass;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmPlanPnrViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmPlanPnrViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmPlanPnrViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;

        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private bool _isAnnotation = true;//是否标注完成
        private int performYear = 0; //完成年份
        private IEnumerable<PlanHistory> allAirlinerPlanHistories; //获取计划中的客机计划历史

        private List<AircraftStatistic> realOwerAircraftList = new List<AircraftStatistic>(); //航空公司实际运行飞机

        private RadWindow opertationHistoryWindow = new RadWindow(); //单击运营飞机区域的用户提示，需添加引用集 Telerik.Windows.Controls.Navigation
        private RadWindow planHistoryWindow = new RadWindow();//单击净增飞机区域的用户提示
        private RadWindow totalOpertationWindow = new RadWindow();//单击某年度所有飞机区域的用户提示
        private RadWindow totalPlanHistoryWindow = new RadWindow();//单击某年度所有飞机区域的用户提示
        private readonly CAAC.CAFM.Views.CafmPlanPnrView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmPlanPnrView>();

        CommonMethod commonmethod = new CommonMethod();
        private Grid TrendGrid, AircraftPieGrid, SeatPieGrid, LoadPieGrid;//趋势图区域，飞机数饼图区域， 座位数饼图区域， 商载量区域
        private RadGridView ExportRadgridview, viewPlan; //初始化RadGridView

        #region Property



        #endregion

        #region Method

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.TrendGrid = this._view.TrendGrid;
            this.AircraftPieGrid = this._view.AircraftPieGrid;
            this.SeatPieGrid = this._view.SeatPieGrid;
            this.LoadPieGrid = this._view.LoadPieGrid;
            this.viewPlan = this._view.viewPlan as RadGridView;
        }

        /// <summary>
        /// 运营飞机饼状图选择项
        /// </summary>
        /// <param name="selectedItem">选中点</param>
        private void ShowOpertationHistoryWindow(PieDataPoint selectedItem)
        {
            if (selectedItem != null)
            {
                AircraftStatistic selAc = selectedItem.DataItem as AircraftStatistic;
                List<Aircraft> airlineAircrafts = ViewAircraft.Where(p =>
                    p.Airlines.ShortName == selAc.AirlineName).ToList();

                opertationHistoryWindow.Header = selAc.AirlineName +
                    "当前在运营：" + airlineAircrafts.Count.ToString() + "架；份额： " +
                    selAc.TotalAircraftNumberPercent + "%";
                //找到子窗体的RadGridView，并为ItemsSource赋值
                RadGridView rd = opertationHistoryWindow.Content as RadGridView;
                rd.ItemsSource = airlineAircrafts;
                if (!opertationHistoryWindow.IsOpen)
                {
                    commonmethod.ShowRadWindow(opertationHistoryWindow);
                }
            }
        }

        /// <summary>
        /// 所有飞机饼状图选择项
        /// </summary>
        /// <param name="selectedItem">选中点</param>
        private void ShowtotalWindow(PieDataPoint selectedItem)
        {
            if (selectedItem != null)
            {
                AircraftStatistic selAc = selectedItem.DataItem as AircraftStatistic;
                //运营的飞机
                IEnumerable<Aircraft> operationAircrafts = ViewAircraft.Where(p =>
                    p.Airlines.ShortName == selAc.AirlineName);
                //找到运营子窗体的RadGridView，并为ItemsSource赋值
                RadGridView operationDetail = totalOpertationWindow.Content as RadGridView;
                operationDetail.ItemsSource = operationAircrafts;
                if (this.ViewPlan != null && allAirlinerPlanHistories != null)
                {
                    //获取选择航空公司的计划历史 
                    IEnumerable<PlanHistory> airlinePlanHistory = allAirlinerPlanHistories.Where(p =>
                        p.Plan.Airlines.ShortName == selAc.AirlineName &&
                        p.Annual.Year <= performYear).OrderBy(ob => ob.Annual.Year);
                    //找到计划子窗体的RadGridView，并为ItemsSource赋值
                    RadGridView rd = totalPlanHistoryWindow.Content as RadGridView;
                    if (airlinePlanHistory != null && airlinePlanHistory.Count() != 0)
                    {
                        rd.ItemsSource = airlinePlanHistory;
                    }
                    else
                    {
                        rd.ItemsSource = null;
                    }
                }
                string selYear = string.IsNullOrWhiteSpace(TotalAirlineContent) || TotalAirlineContent.Length < 4 ? "0" : TotalAirlineContent.Substring(2, 4);
                //子窗体的标题
                totalOpertationWindow.Header = selAc.AirlineName +
                    "当前在运营：" + operationAircrafts.Count() + "架";
                totalPlanHistoryWindow.Header = selAc.AirlineName + "至" +
                    selYear + "年计划净增：" + selAc.IncreaseAmount + "架；期末份额： " +
                    selAc.TotalAircraftNumberPercent + "%";

                if (!totalOpertationWindow.IsOpen)
                {
                    commonmethod.ShowRadWindow(totalOpertationWindow);
                }
                if (!totalPlanHistoryWindow.IsOpen)
                {
                    commonmethod.ShowRadWindow(totalPlanHistoryWindow);
                }
            }
        }

        /// <summary>
        /// 运营飞机饼状图选择项
        /// </summary>
        /// <param name="selectedItem">选中点</param>
        private void ShowplanHistoryWindow(PieDataPoint selectedItem)
        {
            if (selectedItem != null)
            {
                if (this.ViewPlan != null && allAirlinerPlanHistories != null)
                {
                    AircraftSeattingStatisticByYear selAc = selectedItem.DataItem as AircraftSeattingStatisticByYear;
                    //获取选择航空公司的计划历史 
                    IEnumerable<PlanHistory> airlinePlanHistory = allAirlinerPlanHistories.Where(p =>
                        p.Plan.Airlines.ShortName == selAc.AirlineName &&
                        p.AircraftType.AircraftCategory.Regional == selAc.SeattingType &&
                        p.Annual.Year == selAc.Year);

                    //子窗体的标题
                    planHistoryWindow.Header = selAc.AirlineName +
                        selAc.Year + "年" + selAc.SeattingType + "净增：" +
                        selAc.AircraftAmount.ToString() + "架；份额： " +
                        selAc.TotalAircraftNumberPercent + "%";

                    //找到子窗体的RadGridView，并为ItemsSource赋值
                    RadGridView rd = planHistoryWindow.Content as RadGridView;
                    rd.ItemsSource = airlinePlanHistory;
                    //计划历史子窗体打开
                    if (!planHistoryWindow.IsOpen)
                    {
                        commonmethod.ShowRadWindow(planHistoryWindow);
                    }

                }
            }
        }

        /// <summary>
        /// 弹出窗体关闭时，取消相应饼图的弹出项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void radWindows_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow radwindow = sender as RadWindow;
            Grid grid = new Grid();
            if (radwindow.Name == "opertationHistoryWindow")
            {
                grid = this.AircraftPieGrid;
            }
            else if (radwindow.Name == "totalOpertationWindow" || radwindow.Name == "totalPlanHistoryWindow")
            {
                grid = this.SeatPieGrid;
                this.totalOpertationWindow.Close();
                this.totalPlanHistoryWindow.Close();
            }
            else if (radwindow.Name == "planHistoryWindow")
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
                InitializeAirlinePlanStatistics();
                InitializeCurrentOwerAircraft();
                SetRadCartesianChartColor();
                GetNoRequstAirline();
            }
        }

        #region Local 方法 CreatOperationGridView  --创建运营飞机

        /// <summary>
        ///创建运营飞机
        /// </summary>
        /// <returns></returns>
        public RadGridView CreatOperationGridView()
        {
            RadGridView rgView = new RadGridView();
            rgView.ShowGroupPanel = true;
            rgView.AutoGenerateColumns = false;
            rgView.IsReadOnly = true;
            rgView.Name = "aircraftDetail";
            rgView.RowIndicatorVisibility = Visibility.Collapsed;
            rgView.IsFilteringAllowed = true;

            GridViewDataColumn gvColumn1 = new GridViewDataColumn();
            gvColumn1.Header = "航空公司";
            gvColumn1.DataMemberBinding = new System.Windows.Data.Binding("Airlines.ShortName");


            GridViewDataColumn gvColumn2 = new GridViewDataColumn();
            gvColumn2.Header = "注册号";
            gvColumn2.DataMemberBinding = new System.Windows.Data.Binding("RegNumber");


            GridViewDataColumn gvColumn3 = new GridViewDataColumn();
            gvColumn3.Header = "机型";
            gvColumn3.DataMemberBinding = new System.Windows.Data.Binding("AircraftType.Name");

            GridViewDataColumn gvColumn4 = new GridViewDataColumn();
            gvColumn4.Header = "引进方式";
            System.Windows.Data.Binding bingding4 = new System.Windows.Data.Binding("ImportCategory.ActionName");
            gvColumn4.DataMemberBinding = bingding4;

            GridViewDataColumn gvColumn5 = new GridViewDataColumn();
            gvColumn5.Header = "引进日期";
            gvColumn5.DataMemberBinding = new System.Windows.Data.Binding("ImportDate");
            gvColumn5.DataFormatString = "yyyy/M/d";


            GridViewDataColumn gvColumn6 = new GridViewDataColumn();
            gvColumn6.Header = "出厂日期";
            gvColumn6.DataMemberBinding = new System.Windows.Data.Binding("FactoryDate");
            gvColumn6.DataFormatString = "yyyy/M/d";

            SerialColumn gvColumn7 = new SerialColumn();
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
            gvColumn7.Header = "序号";
            gvColumn7.CellStyle = (Style)dictionary["style"];


            rgView.Columns.Add(gvColumn7);
            rgView.Columns.Add(gvColumn1);
            rgView.Columns.Add(gvColumn2);
            rgView.Columns.Add(gvColumn3);
            rgView.Columns.Add(gvColumn4);
            rgView.Columns.Add(gvColumn5);
            rgView.Columns.Add(gvColumn6);

            return rgView;
        }

        #endregion

        #region Local 方法 CreatePlanHistorityGridView --创建计划历史

        /// <summary>
        /// 创建计划历史
        /// </summary>
        public RadGridView CreatePlanHistorityGridView()
        {

            RadGridView rgView = new RadGridView();
            rgView.ShowGroupPanel = true;
            rgView.AutoGenerateColumns = false;
            rgView.IsReadOnly = true;
            rgView.Name = "planHistotry";
            rgView.RowIndicatorVisibility = Visibility.Collapsed;
            rgView.IsFilteringAllowed = true;

            GridViewDataColumn gvColumn0 = new GridViewDataColumn();
            gvColumn0.Header = "申报单位";
            gvColumn0.DataMemberBinding = new System.Windows.Data.Binding("Plan.Airlines.ShortName");
            //Telerik.Windows.Data.CountFunction cf = new Telerik.Windows.Data.CountFunction();
            //cf.Caption = "总计(项)：";
            //gvColumn0.AggregateFunctions.Add(cf);


            GridViewDataColumn gvColumn1 = new GridViewDataColumn();
            gvColumn1.Header = "执行年份";
            gvColumn1.DataMemberBinding = new System.Windows.Data.Binding("Annual.Year");

            GridViewDataColumn gvColumn2 = new GridViewDataColumn();
            gvColumn2.Header = "执行月份";
            gvColumn2.DataMemberBinding = new System.Windows.Data.Binding("PerformMonth");


            GridViewDataColumn gvColumn3 = new GridViewDataColumn();
            gvColumn3.Header = "活动类别";
            gvColumn3.DataMemberBinding = new System.Windows.Data.Binding("ActionCategoryOperation");


            GridViewDataColumn gvColumn4 = new GridViewDataColumn();
            gvColumn4.Header = "引进/退出方式";
            System.Windows.Data.Binding bingding4 = new System.Windows.Data.Binding("TargetCategory.ActionName");
            gvColumn4.DataMemberBinding = bingding4;

            GridViewDataColumn gvColumn5 = new GridViewDataColumn();
            gvColumn5.Header = "机型";
            gvColumn5.DataMemberBinding = new System.Windows.Data.Binding("AircraftType.Name");


            GridViewDataColumn gvColumn6 = new GridViewDataColumn();
            gvColumn6.Header = "净增座位(座)";
            gvColumn6.DataMemberBinding = new System.Windows.Data.Binding("SeatingCapacity");

            GridViewDataColumn gvColumn7 = new GridViewDataColumn();
            gvColumn7.Header = "净增商载(吨)";
            gvColumn7.DataMemberBinding = new System.Windows.Data.Binding("CarryingCapacity");

            GridViewDataColumn gvColumn8 = new GridViewDataColumn();
            gvColumn8.Header = "备注";
            gvColumn8.DataMemberBinding = new System.Windows.Data.Binding("Note");


            GridViewDataColumn gvColumn9 = new GridViewDataColumn();
            gvColumn9.Header = "申请状态";
            gvColumn9.DataMemberBinding = new System.Windows.Data.Binding("IsApply");

            //默认分组
            GroupDescriptor countryDescriptor = new GroupDescriptor();
            countryDescriptor.Member = "IsApply";
            countryDescriptor.DisplayContent = "申请状态";
            rgView.GroupDescriptors.Add(countryDescriptor);
            rgView.AutoExpandGroups = true;


            SerialColumn gvColumn10 = new SerialColumn();
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
            gvColumn10.Header = "序号";
            gvColumn10.CellStyle = (Style)dictionary["style"];


            rgView.Columns.Add(gvColumn10);
            rgView.Columns.Add(gvColumn0);
            rgView.Columns.Add(gvColumn1);
            rgView.Columns.Add(gvColumn2);
            rgView.Columns.Add(gvColumn3);
            rgView.Columns.Add(gvColumn4);
            rgView.Columns.Add(gvColumn5);
            rgView.Columns.Add(gvColumn6);
            rgView.Columns.Add(gvColumn7);
            rgView.Columns.Add(gvColumn8);
            rgView.Columns.Add(gvColumn9);

            return rgView;
        }
        #endregion

        #region Local 方法 InitalizerRadWindows --初始化子窗体

        /// <summary>
        /// 初始化子窗体
        /// </summary>
        /// <param name="radWindows">需要初始化的窗体</param>
        /// <param name="windowsName">窗体名称</param>
        /// <param name="length">窗体距离</param>
        public void InitalizerRadWindows(RadWindow radWindows, string windowsName, int length)
        {

            //子窗体的设置
            radWindows.Height = 200;
            radWindows.Width = 450;
            radWindows.Name = windowsName;
            radWindows.ResizeMode = ResizeMode.CanResize;
            if (windowsName == "totalPlanHistoryWindow")
            {
                radWindows.Top = totalOpertationWindow.Top;
                radWindows.Left = length;
            }
            else
            {
                radWindows.Top = length;
                radWindows.Left = length;

            }

            //子窗体的Content,Conent根据不同的子窗体而不同
            RadGridView contentGridView;
            if (windowsName == "totalOpertationWindow" || windowsName == "opertationHistoryWindow")
            {
                contentGridView = CreatOperationGridView();
            }
            else
            {
                contentGridView = CreatePlanHistorityGridView();
            }
            radWindows.Content = contentGridView;
            radWindows.Closed += new EventHandler<WindowClosedEventArgs>(radWindows_Closed);

        }

        #endregion

        #region Local 方法 InitializeAirlinePlanStatistics --获取AirlinePlanStatistics统计集合

        /// <summary>
        /// 获取AirlinePlanStatistics统计集合
        /// </summary>
        public void InitializeAirlinePlanStatistics()
        {

            if (this.ViewPlan == null)
            {
                AirlinePlanSatisfiedDataString = "加载完成，数据为空";
            }
            else
            {
                //计划历史统计，If语句排除计划历史为空的客机计划
                if (allAirlinerPlanHistories != null)
                {
                    List<AirlinePlanStatistics> alllPlanhistory = new List<AirlinePlanStatistics>();

                    //统计基于年度的客机计划
                    var airlinePlanSatisfiedList = allAirlinerPlanHistories.OrderBy(p => p.Annual.Year).GroupBy(gb => gb.Annual.Year)
                       .Select(p => new AirlinePlanStatistics
                       {
                           Year = p.Key,
                           SeattingNumberThan100 = p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan100) == null ? 0 : p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan100).Sum(m => m.DeltaPnr),
                           SeattingNumberThan250 = p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan250) == null ? 0 : p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan250).Sum(m => m.DeltaPnr),
                           SeattingNumberLess100 = p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberLess100) == null ? 0 : p.Where(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberLess100).Sum(m => m.DeltaPnr),
                       }
                       ).ToList();

                    Dictionary<string, string> colordictionary = GetColorDictionary();
                    //总体趋势图为计划和实际运营，两部分组成
                    airlinePlanSatisfiedList.ForEach
                        (
                          air =>
                          {
                              AirlinePlanStatistics aps = new AirlinePlanStatistics();
                              aps.Year = air.Year;

                              aps.TotalName = "总飞机数";
                              aps.Less100Name = this.SeattingLess100LegendLabel;
                              aps.Than100Name = this.SeattingThan100LegendLabel;
                              aps.Than250Name = this.SeattingThan250LegendLabel;

                              aps.TotalColor = colordictionary[aps.TotalName];
                              aps.Less100Color = colordictionary[aps.Less100Name];
                              aps.Than100Color = colordictionary[aps.Than100Name];
                              aps.Than250Color = colordictionary[aps.Than250Name];

                              aps.RealIncreaseLess100 = air.SeattingNumberLess100;
                              aps.RealLess100ToolTip = aps.Less100Name + ":" + aps.RealIncreaseLess100 + "架";

                              aps.RealIncreaseThan100 = air.SeattingNumberThan100;
                              aps.RealThan100ToolTip = aps.Than100Name + ":" + aps.RealIncreaseThan100 + "架";

                              aps.RealIncreaseThan250 = air.SeattingNumberThan250;
                              aps.RealThan250ToolTip = aps.Than250Name + ":" + aps.RealIncreaseThan250 + "架";

                              aps.RealTotalAircraft = aps.RealIncreaseLess100 + aps.RealIncreaseThan100 + aps.RealIncreaseThan250;
                              aps.RealTotalToolTip = aps.TotalName + ":" + aps.RealTotalAircraft + "架";

                              aps.SeattingNumberLess100 = airlinePlanSatisfiedList.Where(p => p.Year <= air.Year)
                                  .Sum(m => m.SeattingNumberLess100)
                                  + ViewAircraft.Count(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberLess100);
                              aps.SeattingLess100ToolTip = aps.Less100Name + ":" + aps.SeattingNumberLess100 + "架";

                              aps.SeattingNumberThan100 = airlinePlanSatisfiedList.Where(p => p.Year <= air.Year)
                                  .Sum(m => m.SeattingNumberThan100)
                                  + ViewAircraft.Count(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan100);
                              aps.SeattingThan100ToolTip = aps.Than100Name + ":" + aps.SeattingNumberThan100 + "架";

                              aps.SeattingNumberThan250 = airlinePlanSatisfiedList.Where(p => p.Year <= air.Year)
                                  .Sum(m => m.SeattingNumberThan250)
                                   + ViewAircraft.Count(w => w.AircraftType.AircraftCategory.Regional == AppConfig.seattingNumberThan250);
                              aps.SeattingThan250ToolTip = aps.Than250Name + ":" + aps.SeattingNumberThan250 + "架";

                              aps.SeattingTotalAircraft = aps.SeattingNumberLess100 + aps.SeattingNumberThan100 + aps.SeattingNumberThan250;
                              aps.SeattingTotalToolTip = aps.TotalName + ":" + aps.SeattingTotalAircraft + "架";

                              alllPlanhistory.Add(aps);
                          }
                        );
                    AirlinePlanSatisfiedCollection = alllPlanhistory;
                }
            }
        }
        #endregion

        #region Local 方法 InitializeCurrentOwerAircraft --统计航空公司目前实际拥有运营的飞机的集合

        /// <summary>
        /// 统计航空公司目前实际拥有运营的飞机的集合
        /// </summary>
        public void InitializeCurrentOwerAircraft()
        {

            AircraftStatisticCollection.Clear();
            //获取实际运营中的飞机

            if (ViewAircraft != null && ViewAircraft.Count() > 0)
            {
                //运营中飞机总数
                decimal aircraftCount = ViewAircraft.Count();
                //统计现有航空那个公司拥有的飞机
                realOwerAircraftList = ViewAircraft.GroupBy(t => t.Airlines.ShortName).Select(p =>
                    new AircraftStatistic
                    {
                        AirlineName = p.Key,
                        AircraftAmount = p.Count()
                    }).ToList();

                XElement airlinecolor = null;
                XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
                {
                    airlinecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
                }

                //统计现有航空那个公司拥有的飞机百分比
                var aircraftsByAirline = ViewAircraft.GroupBy(t => t.Airlines.ShortName).Select(p =>
                    new AircraftStatistic
                    {
                        AirlineName = p.Key,
                        AircraftAmount = p.Count(),
                        Color = airlinecolor == null ? new CommonMethod().GetRandomColor() : airlinecolor.Descendants("Item").FirstOrDefault(pp => pp.Attribute("Name").Value == p.Key).Attribute("Color").Value,
                        TotalAircraftNumberPercent = aircraftCount == 0 ? 0 : Math.Round(p.Count() * 100 / aircraftCount, 2)
                    });

                AircraftStatisticCollection = aircraftsByAirline.OrderBy(a => a.AircraftAmount).ToList();

            }


        }
        #endregion

        #region Local 方法  InitializeAirlinePlanDetailCollection --获取当年航空公司所有的飞机(包括运营中的飞机和计划的飞机）

        /// <summary>
        /// 获取当年航空公司所有的飞机(包括运营中的飞机和计划的飞机）
        /// </summary>
        /// <param name="year">选择年度</param>
        public void InitializeCurrentTotalAircraftCollection(int year)
        {
            AircraftStatisticByYearCollection.Clear();
            List<AircraftStatistic> totalAircraftStatisticList = new List<AircraftStatistic>();

            //判断客机计划是否存在，如果存在，当前的飞机数等于运营中的飞机加计划的飞机数
            if (this.ViewPlan != null && allAirlinerPlanHistories != null && allAirlinerPlanHistories.Where(p => p.Annual.Year <= year) != null)
            {

                //获取小于或等于今年计划飞机数的统计
                var aircraftStatisticList = allAirlinerPlanHistories.Where(p => p.Annual.Year <= year).GroupBy(gb => gb.Plan.Airlines.ShortName)
                   .Select(p => new AircraftStatistic
                   {
                       AirlineName = p.Key,
                       IncreaseAmount = p.Sum(m => m.DeltaPnr),
                       AircraftAmount = p.Sum(m => m.DeltaPnr)
                   }
                   ).ToList();


                //获取计划和实际运营飞机的飞机总集合
                var allAirline = aircraftStatisticList.Select(p => p.AirlineName).Union(realOwerAircraftList.Select(p => p.AirlineName)).Distinct().ToList();

                allAirline.ForEach
                    (
                       al =>
                       {
                           AircraftStatistic totalAircraftStatistic = new AircraftStatistic(); //定义总飞机数的统计
                           //若该航空公司归属于计划和实际运营的
                           if (aircraftStatisticList.Any(p => p.AirlineName == al) && realOwerAircraftList.Any(p => p.AirlineName == al))
                           {
                               AircraftStatistic planAircraft = aircraftStatisticList.FirstOrDefault(p => p.AirlineName == al);
                               AircraftStatistic realAircraft = realOwerAircraftList.FirstOrDefault(p => p.AirlineName == al);

                               totalAircraftStatistic.AircraftAmount = planAircraft.AircraftAmount + realAircraft.AircraftAmount;
                               totalAircraftStatistic.IncreaseAmount = planAircraft.IncreaseAmount;
                               totalAircraftStatistic.AirlineName = al;
                           }
                           else
                           {
                               //若该航空公司归属于计划
                               if (aircraftStatisticList.Any(p => p.AirlineName == al) && !realOwerAircraftList.Any(p => p.AirlineName == al))
                               {
                                   AircraftStatistic planAircraft = aircraftStatisticList.FirstOrDefault(p => p.AirlineName == al);
                                   totalAircraftStatistic.AircraftAmount = planAircraft.AircraftAmount;
                                   totalAircraftStatistic.IncreaseAmount = planAircraft.IncreaseAmount;
                                   totalAircraftStatistic.AirlineName = al;
                               }
                               //若该航空公司归属于实际运营
                               else if (!aircraftStatisticList.Any(p => p.AirlineName == al) && realOwerAircraftList.Any(p => p.AirlineName == al))
                               {
                                   AircraftStatistic realAircraft = realOwerAircraftList.FirstOrDefault(p => p.AirlineName == al);
                                   totalAircraftStatistic.AircraftAmount = realAircraft.AircraftAmount;
                                   totalAircraftStatistic.AirlineName = al;
                               }
                           }
                           totalAircraftStatisticList.Add(totalAircraftStatistic);
                       }
                    );


                XElement airlinecolor = null;
                XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
                {
                    airlinecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
                }

                //获取选择年度计划飞机的总数
                decimal aircraftStatisticCount = totalAircraftStatisticList.Sum(p => p.AircraftAmount);
                totalAircraftStatisticList.ForEach
                    (
                      at =>
                      {
                          at.Color = airlinecolor == null ? new CommonMethod().GetRandomColor() : airlinecolor.Descendants("Item").FirstOrDefault(pp => pp.Attribute("Name").Value == at.AirlineName).Attribute("Color").Value;
                          at.TotalAircraftNumberPercent = aircraftStatisticCount == 0 ? 0 : Math.Round(at.AircraftAmount * 100 / aircraftStatisticCount, 2);
                      }
                    );

                AircraftStatisticByYearCollection = totalAircraftStatisticList;
            }
            else //如果计划历史不存在，当前的飞机数等于运营中的飞机数
            {
                AircraftStatisticByYearCollection = AircraftStatisticCollection;
            }
        }
        #endregion

        #region Local 方法 InitializeAircraftSeatting --获取当年航空公司座级的集合

        /// <summary>
        /// 获取当年航空公司座级的集合
        /// </summary>
        /// <param name="year">选择年度</param>
        /// <param name="seattingType">座级类型</param>
        public void InitializeAircraftSeatting(int year, string seattingType)
        {
            AircraftSeattingStatisticByYearCollection.Clear();
            //选择年度机型的统计，判断排除客机计划不存在
            if (this.ViewPlan != null && allAirlinerPlanHistories != null)
            {

                //基于年份、航空公司的每个年度每家航空公司飞机的统计
                var increaseAircraftStatistic = allAirlinerPlanHistories.Where(p =>
                    p.AircraftType.AircraftCategory.Regional == seattingType)
                    .GroupBy(gb => new { gb.Plan.Airlines.ShortName, gb.Annual.Year })
                   .Select(p => new AircraftSeattingStatisticByYear
                   {
                       AirlineName = p.Key.ShortName,
                       Year = p.Key.Year,
                       SeattingType = seattingType,
                       AircraftAmount = p.Sum(m => m.DeltaPnr),
                   }).ToList();

                if (increaseAircraftStatistic.Any())
                {
                    XElement airlinecolor = null;
                    XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                    if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "航空公司"))
                    {
                        airlinecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "航空公司");
                    }

                    //选择年份增长的飞机
                    var increaseAircraftByYear = increaseAircraftStatistic.Where(p => p.Year == year).ToList();
                    //座级的总数
                    decimal increaseAircraftSeattingCount = increaseAircraftByYear.Sum(p => p.AircraftAmount);

                    //遍历座级集合，获取百分比
                    increaseAircraftByYear.ForEach
                       (
                           ia =>
                           {
                               ia.Color = airlinecolor == null ? new CommonMethod().GetRandomColor() : airlinecolor.Descendants("Item").FirstOrDefault(pp => pp.Attribute("Name").Value == ia.AirlineName).Attribute("Color").Value;
                               ia.TotalAircraftNumberPercent = increaseAircraftSeattingCount == 0 ? 0 : Math.Round((ia.AircraftAmount * 100 / increaseAircraftSeattingCount), 2);
                           }
                       );

                    AircraftSeattingStatisticByYearCollection = increaseAircraftByYear;

                }

            }



        }
        #endregion

        #region Local 方法 GetNoRequstAirline --获取未申报的航空公司

        /// <summary>
        /// 获取未申报的航空公司
        /// </summary>
        public void GetNoRequstAirline()
        {
            if (ViewPlan != null)
            {
                //已经申报航空公司
                var alreadlyRequstAirline = this.ViewPlan.Select(p => p.Airlines);
                //未申报航空公司
                NoRequstAirlineCollection = this._service.EntityContainer.GetEntitySet<Owner>().OfType<Airlines>()
                    .Where(p => p.IsValid == true && p.MasterID == null).Except(alreadlyRequstAirline).ToList(); //未申报航空公司
                NoRequstAirlineTitle = NoRequstAirlineCollection == null || NoRequstAirlineCollection.Count == 0 ? "未申报公司" : NoRequstAirlineCollection.Count + "家未申报公司";
            }
            else
            {
                NoRequstAirlineCollection = this._service.EntityContainer.GetEntitySet<Owner>().OfType<Airlines>()
                    .Where(p => p.IsValid == true && p.MasterID == null).ToList();
            }
        }
        #endregion


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
                this.IsBusy = this.IsBusyXmlConfig || this.IsBusyAircraft || this.IsBusyPlan;
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

        #region 加载实体Plan集合

        /// <summary>
        /// Plan集合
        /// </summary>
        public IEnumerable<Plan> ViewPlan
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>()
                    .Where(p => p.IsCurrentVersion == true && p.Annual.IsOpen == true && p.Airlines.IsValid == true && p.Airlines.MasterID == null);
            }
        }

        /// <summary>
        ///客机计划集合
        /// </summary>
        public IEnumerable<Plan> ViewPnrPlan
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>()
                    .Where(p => p.IsCurrentVersion == true && p.Annual.IsOpen == true && p.Airlines.IsValid == true && p.Airlines.MasterID == null && p.AirlinerPlanHistories!=null && p.AirlinerPlanHistories.Count>0);
            }
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
                this.IsBusy = this.IsBusyPlan || this.IsBusyAircraft || this.IsBusyXmlConfig;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewPlan);
                    this.RaisePropertyChanged(() => this.ViewPnrPlan);
                    
                    //获取计划中的客机计划历史
                    if (this.ViewPlan != null)
                    {
                        allAirlinerPlanHistories = this.ViewPlan.Where(p =>
                            p.AirlinerPlanHistories != null) == null ? null : this.ViewPlan.Where(p =>
                                p.AirlinerPlanHistories != null).SelectMany(p => p.AirlinerPlanHistories);
                    }
                    InitializeData();
                }
            }, null);
        }

        #endregion

        #region 加载实体Aircraft集合

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Aircraft> ViewAircraft
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Aircraft>().Where(p => p.IsOperation == true && p.AircraftType.AircraftCategory.Category == "客机"); ;
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
                this.IsBusy = this.IsBusyPlan || this.IsBusyAircraft || this.IsBusyXmlConfig;
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

        #region ViewModel 属性 AirlinePlanSatisfiedDataString --计划净增统计数NoDataString字符串显示

        private string _airlinePlanSatisfiedDataString = "正在加载，请稍后...";
        /// <summary>
        /// 计划净增统计数NoDataString字符串显示
        /// </summary>
        public string AirlinePlanSatisfiedDataString
        {
            get { return _airlinePlanSatisfiedDataString; }
            set
            {

                if (AirlinePlanSatisfiedDataString != value)
                {
                    _airlinePlanSatisfiedDataString = value;
                    this.RaisePropertyChanged(() => this.AirlinePlanSatisfiedDataString);

                }
            }
        }
        #endregion

        #region ViewModel 属性 RealAirlineOperatoinNoDataString --航空公司实际运营飞机NoDataString 字符串显示

        private string _realAirlineOperatoinNoDataString = "正在加载，请稍后";
        /// <summary>
        /// 航空公司实际运营飞机NoDataString 字符串显示
        /// </summary>
        public string RealAirlineOperatoinNoDataString
        {
            get { return _realAirlineOperatoinNoDataString; }
            set
            {

                if (RealAirlineOperatoinNoDataString != value)
                {
                    _realAirlineOperatoinNoDataString = value;
                    this.RaisePropertyChanged(() => this.RealAirlineOperatoinNoDataString);

                }
            }
        }
        #endregion

        #region ViewModel 属性 TotalAirlineAircraftNoDataString --航空公司计划与运营飞机NoDataString 字符串显示

        private string _totalAirlineAircraftNoDataString = "单击计划净增统计图，查看明细";
        /// <summary>
        /// 航空公司计划与运营飞机NoDataString 字符串显示
        /// </summary>
        public string TotalAirlineAircraftNoDataString
        {
            get { return _totalAirlineAircraftNoDataString; }
            set
            {

                if (TotalAirlineAircraftNoDataString != value)
                {
                    _totalAirlineAircraftNoDataString = value;
                    this.RaisePropertyChanged(() => this.TotalAirlineAircraftNoDataString);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftTypeNoDataString --机型NoDataString字符串显示

        private string _AircraftTypeNoDataString = "单击计划净增统计图，查看明细";
        /// <summary>
        /// 机型NoDataString字符串显示
        /// </summary>
        public string AircraftTypeNoDataString
        {
            get { return _AircraftTypeNoDataString; }
            set
            {

                if (AircraftTypeNoDataString != value)
                {
                    _AircraftTypeNoDataString = value;
                    this.RaisePropertyChanged(() => this.AircraftTypeNoDataString);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftTypeConent --飞机机型饼状图的标题

        private string _aircraftTypeConent = "所选年度客机座级分布";
        /// <summary>
        /// 飞机机型饼状图的标题
        /// </summary>
        public string AircraftTypeConent
        {
            get { return _aircraftTypeConent; }
            set
            {

                if (AircraftTypeConent != value)
                {
                    _aircraftTypeConent = value;
                    this.RaisePropertyChanged(() => this.AircraftTypeConent);

                }
            }
        }
        #endregion

        #region ViewModel 属性 TotalAirlineContent --航空公司选择年度飞机饼状图的标题

        private string _totalAirlineContent = "所选年度客机分布";
        /// <summary>
        ///航空公司选择年度飞机饼状图的标题
        /// </summary>
        public string TotalAirlineContent
        {
            get { return _totalAirlineContent; }
            set
            {

                if (TotalAirlineContent != value)
                {
                    _totalAirlineContent = value;
                    this.RaisePropertyChanged(() => this.TotalAirlineContent);

                }
            }
        }
        #endregion


        #region ViewModel 属性 AirlinePlanSatisfiedCollection --各航空公司根据年份统计的计划集合

        private List<AirlinePlanStatistics> _airlinePlanSatisfiedCollection = new List<AirlinePlanStatistics>();
        /// <summary>
        /// 各航空公司根据年份统计的计划集合
        /// </summary>
        public List<AirlinePlanStatistics> AirlinePlanSatisfiedCollection
        {
            get { return _airlinePlanSatisfiedCollection; }
            set
            {

                if (AirlinePlanSatisfiedCollection != value)
                {
                    _airlinePlanSatisfiedCollection = value;
                    this.RaisePropertyChanged(() => this.AirlinePlanSatisfiedCollection);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftStatisticCollection --每个航空公司所占市场份额的集合

        private List<AircraftStatistic> _aircraftStatisticCollection = new List<AircraftStatistic>();
        /// <summary>
        /// 每个航空公司所占市场份额的集合
        /// </summary>
        public List<AircraftStatistic> AircraftStatisticCollection
        {
            get { return _aircraftStatisticCollection; }
            set
            {

                if (AircraftStatisticCollection != value)
                {
                    _aircraftStatisticCollection = value;
                    if (value.Count == 0)
                    {
                        RealAirlineOperatoinNoDataString = "加载完成，数据为空";
                    }
                    this.RaisePropertyChanged(() => this.AircraftStatisticCollection);
                    SetPieMark1(AircraftStatisticCollection, this.AircraftPieGrid);
                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftStatisticCollection --选择年度航空公司所有的飞机（包括计划的飞机）

        private List<AircraftStatistic> _aircraftStatisticByYearCollection = new List<AircraftStatistic>();
        /// <summary>
        /// 每个航空公司所占市场份额的集合
        /// </summary>
        public List<AircraftStatistic> AircraftStatisticByYearCollection
        {
            get { return _aircraftStatisticByYearCollection; }
            set
            {

                if (AircraftStatisticByYearCollection != value)
                {
                    _aircraftStatisticByYearCollection = value;
                    if (value.Count == 0)
                    {
                        TotalAirlineAircraftNoDataString = "加载完成，数据为空";
                    }
                    this.RaisePropertyChanged(() => this.AircraftStatisticByYearCollection);
                    SetPieMark1(AircraftStatisticByYearCollection, this.SeatPieGrid);
                }
            }
        }
        #endregion

        #region ViewModel 属性 PlanCollcetion --计划集合

        private List<Plan> _planCollcetion;
        /// <summary>
        /// 计划集合
        /// </summary>
        public List<Plan> PlanCollcetion
        {
            get { return _planCollcetion; }
            set
            {

                if (PlanCollcetion != value)
                {
                    _planCollcetion = value;
                    this.RaisePropertyChanged(() => this.PlanCollcetion);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AircraftSeattingStatisticByYearCollection --选择年份机型的统计集合

        private List<AircraftSeattingStatisticByYear> _aircraftSeattingStatisticByYearCollection = new List<AircraftSeattingStatisticByYear>();
        /// <summary>
        /// 选择年份机型的统计集合
        /// </summary>
        public List<AircraftSeattingStatisticByYear> AircraftSeattingStatisticByYearCollection
        {
            get { return _aircraftSeattingStatisticByYearCollection; }
            set
            {

                if (AircraftSeattingStatisticByYearCollection != value)
                {
                    _aircraftSeattingStatisticByYearCollection = value;
                    if (value.Count == 0)
                    {
                        AircraftTypeNoDataString = "加载完成，数据为空";
                    }
                    this.RaisePropertyChanged(() => this.AircraftSeattingStatisticByYearCollection);
                    SetPieMark2(AircraftSeattingStatisticByYearCollection, this.LoadPieGrid);
                }
            }
        }
        #endregion

        #region ViewModel 属性 NoRequstAirlineCollection --未申报航空公司

        private List<Airlines> _noRequstAirlineCollection;
        /// <summary>
        /// 未申报航空公司
        /// </summary>
        public List<Airlines> NoRequstAirlineCollection
        {
            get { return _noRequstAirlineCollection; }
            set
            {

                if (NoRequstAirlineCollection != value)
                {
                    _noRequstAirlineCollection = value;
                    this.RaisePropertyChanged(() => this.NoRequstAirlineCollection);

                }
            }
        }
        #endregion


        #region ViewModel 属性 NoRequstAirlineTitle --未申报航空标题

        private string _noRequstAirlineTitle = "未申报公司";
        /// <summary>
        /// 未申报航空标题
        /// </summary>
        public string NoRequstAirlineTitle
        {
            get { return _noRequstAirlineTitle; }
            set
            {

                if (NoRequstAirlineTitle != value)
                {
                    _noRequstAirlineTitle = value;
                    this.RaisePropertyChanged(() => this.NoRequstAirlineTitle);

                }
            }
        }
        #endregion

        #region ViewModel 属性 SeattingLess100LegendLabel --100座级以下图例标题

        /// <summary>
        ///100座级以下图例标题
        /// </summary>
        public string SeattingLess100LegendLabel
        {
            get
            {
                return AppConfig.seattingNumberLess100;
            }
        }
        #endregion

        #region ViewModel 属性 SeattingThan100LegendLabel --100座级以下图例标题

        /// <summary>
        ///100座级以下图例标题
        /// </summary>
        public string SeattingThan100LegendLabel
        {
            get
            {
                return AppConfig.seattingNumberThan100;
            }
        }
        #endregion

        #region ViewModel 属性 SeattingThan250LegendLabel --250座级以下图例标题

        /// <summary>
        ///100座级以下图例标题
        /// </summary>
        public string SeattingThan250LegendLabel
        {
            get
            {
                return AppConfig.seattingNumberThan250;
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
        private bool _canViewAttachment = true;
        public bool CanViewAttachment(object obj)
        {
            return _canViewAttachment;
        }
        #endregion

        #region ViewModel 命令 ManageFlagPlanCommand --标记是否完成

        // 标记是否完成
        public DelegateCommand<object> ManageFlagPlanCommand { get; private set; }
        private void OnManageFlagPlan(object obj)
        {
            if (_isAnnotation)
            {
                _isAnnotation = false;
                this._service.SubmitChanges(sm =>
                {
                    if (sm.Error != null)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "标记失败,请检查！", MessageType = MessageType.Fail });
                    }
                    else if (!sm.Cancelled)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "标记成功！", MessageType = MessageType.Success });
                    }
                    _isAnnotation = true;
                }, null);
            }
        }
        private bool _canManageFlagPlan = true;
        public bool CanManageFlagPlan(object obj)
        {
            return _canManageFlagPlan;
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
                    if (AirlinePlanSatisfiedCollection == null || AirlinePlanSatisfiedCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("Year", "年份");
                    columnsList.Add("SeattingNumberThan250", "250座以上计划客机数");
                    columnsList.Add("SeattingNumberThan100", "100-200座计划客机数");
                    columnsList.Add("SeattingNumberLess100", "100座以下计划客机数");
                    columnsList.Add("SeattingTotalAircraft", "总计划飞机数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AirlinePlanSatisfiedCollection, "AircraftPlan");
                }
                else if (menu.Name == "BarGridData")
                {
                    if (AirlinePlanSatisfiedCollection == null || AirlinePlanSatisfiedCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("Year", "年份");
                    columnsList.Add("RealIncreaseThan250", "250座以上客机净增数");
                    columnsList.Add("RealIncreaseThan100", "100-201座客机净增数");
                    columnsList.Add("RealIncreaseLess100", "101座以下客机净增数");
                    columnsList.Add("RealTotalAircraft", "总飞机净增数");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AirlinePlanSatisfiedCollection, "AircraftPlan");
                }
                else if (menu.Name == "AircraftPieGridData")
                {
                    if (AircraftStatisticCollection == null || AircraftStatisticCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("AirlineName", "航空公司");
                    columnsList.Add("TotalAircraftNumberPercent", "当前运营客机数占比（%）");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AircraftStatisticCollection, "AircraftPlan");
                }
                else if (menu.Name == "SeatPieGridData")
                {
                    if (AircraftStatisticByYearCollection == null || AircraftStatisticByYearCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("AirlineName", "航空公司");
                    columnsList.Add("AircraftAmount", "某年计划后总客机数");
                    columnsList.Add("TotalAircraftNumberPercent", "占比（%）");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AircraftStatisticByYearCollection, "AircraftPlan");
                }
                else if (menu.Name == "LoadPieGridData")
                {
                    if (AircraftSeattingStatisticByYearCollection == null || AircraftSeattingStatisticByYearCollection.Count() == 0)
                    {
                        return;
                    }
                    //创建RadGridView
                    Dictionary<string, string> columnsList = new Dictionary<string, string>();
                    columnsList.Add("AirlineName", "航空公司");
                    columnsList.Add("AircraftAmount", "某年某座级的净增客机数");
                    columnsList.Add("TotalAircraftNumberPercent", "占比（%）");
                    ExportRadgridview = ImageAndGirdOperation.CreatDataGridView(columnsList, AircraftSeattingStatisticByYearCollection, "AircraftPlan");
                }
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    ExportRadgridview.ElementExporting -= this.ElementExporting;
                    ExportRadgridview.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
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
                    if (this.TrendGrid != null)
                    {
                        commonmethod.ExportToImage(this.TrendGrid);
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
        private bool _canExport = true;
        bool CanExport(object sender)
        {
            return this._canExport;
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
            if (rmi.DataContext.ToString() == "opertationHistoryWindow")
            {
                rgview = opertationHistoryWindow.Content as RadGridView;

            }
            else if (rmi.DataContext.ToString() == "planHistoryWindow")
            {
                rgview = planHistoryWindow.Content as RadGridView;
            }
            else if (rmi.DataContext.ToString() == "totalOpertationWindow")
            {
                rgview = totalOpertationWindow.Content as RadGridView;
            }
            else if (rmi.DataContext.ToString() == "totalPlanHistoryWindow")
            {
                rgview = totalPlanHistoryWindow.Content as RadGridView;
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

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "ViewPlanExport" && viewPlan != null)
            {
                viewPlan.ElementExporting -= this.ElementExporting;
                viewPlan.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        viewPlan.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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

        #region Method

        /// <summary>
        /// 趋势图的选择点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChartSelectionBehavior_SelectionChanged(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            string type = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart.EmptyContent.ToString();
            Telerik.Charting.DataPoint SelectedPoint = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart.SelectedPoints.FirstOrDefault(p => (p.Presenter as CategoricalSeries).Visibility == Visibility.Visible);
            if (SelectedPoint != null)
            {
                string name = string.Empty;
                if (type == "客机计划变化趋势")
                {
                    name = ((SelectedPoint.Presenter as CategoricalSeries).ValueBinding as PropertyNameDataPointBinding).PropertyName;
                }
                else if (type == "客机计划净增趋势")
                {
                    name = ((SelectedPoint.Presenter as BarSeries).ValueBinding as PropertyNameDataPointBinding).PropertyName;
                }

                AirlinePlanStatistics selAc = SelectedPoint.DataItem as AirlinePlanStatistics;
                //连续单击同一个年份，只需统计第一次即可
                if (performYear != selAc.Year)
                {
                    performYear = selAc.Year;
                    InitializeCurrentTotalAircraftCollection(selAc.Year);//获取选择年度各个航空所有的飞机(包括计划中的飞机)
                    TotalAirlineContent = "截止" + selAc.Year.ToString() + "年客机分布";

                }

                //当前点击的是100-200座级柱状图
                if (name == "RealIncreaseThan100" || name == "SeattingNumberThan100")
                {
                    InitializeAircraftSeatting(selAc.Year, AppConfig.seattingNumberThan100);
                    AircraftTypeConent = selAc.Year + "年净增" + AppConfig.seattingNumberThan100 + "分布";
                }
                //当前点击的是250座级柱状图
                if (name == "RealIncreaseThan250" || name == "SeattingNumberThan250")
                {
                    InitializeAircraftSeatting(selAc.Year, AppConfig.seattingNumberThan250);
                    AircraftTypeConent = selAc.Year + "年净增" + AppConfig.seattingNumberThan250 + "分布";
                }
                //当前点击的是100座级以下柱状图
                if (name == "RealIncreaseLess100" || name == "SeattingNumberLess100")
                {
                    InitializeAircraftSeatting(selAc.Year, AppConfig.seattingNumberLess100);
                    AircraftTypeConent = selAc.Year + "年净增" + AppConfig.seattingNumberLess100 + "分布";
                }
                if (name == "SeattingTotalAircraft")
                {
                    AircraftSeattingStatisticByYearCollection = new List<AircraftSeattingStatisticByYear>();
                    AircraftTypeConent = "所选年度客机座级分布";
                }
            }
        }

        /// <summary>
        /// 飞机饼状图的选择点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadPieChartSelectionBehavior_SelectionChanged1(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            Telerik.Windows.Controls.ChartView.RadChartBase radchartbase = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart;
            Telerik.Charting.PieDataPoint SelectedPoint = radchartbase.SelectedPoints.FirstOrDefault() as Telerik.Charting.PieDataPoint;

            StackPanel stackpanel = new StackPanel();
            if (radchartbase.EmptyContent.ToString() == "当前运营客机分布")
            {
                stackpanel = (this.AircraftPieGrid.Children[1] as ScrollViewer).Content as StackPanel;
            }
            else if (radchartbase.EmptyContent.ToString() == this.TotalAirlineContent)
            {
                stackpanel = (this.SeatPieGrid.Children[1] as ScrollViewer).Content as StackPanel;
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
                    .FirstOrDefault(p => ((p as StackPanel).Children[1] as TextBlock).Text == (SelectedPoint.DataItem as AircraftStatistic).AirlineName) as StackPanel;
                System.Windows.Shapes.Rectangle rectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 12;
                rectangle.Height = 12;

                if (radchartbase.EmptyContent.ToString() == "当前运营客机分布")
                {
                    ShowOpertationHistoryWindow(SelectedPoint);
                }
                else if (radchartbase.EmptyContent.ToString() == this.TotalAirlineContent)
                {
                    ShowtotalWindow(SelectedPoint);
                }
            }
            else
            {
                if (radchartbase.EmptyContent.ToString() == "当前运营客机分布")
                {
                    opertationHistoryWindow.Close();
                }
                else if (radchartbase.EmptyContent.ToString() == this.TotalAirlineContent)
                {
                    totalOpertationWindow.Close();
                    totalPlanHistoryWindow.Close();
                }
            }
        }

        /// <summary>
        /// 飞机饼状图的选择点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RadPieChartSelectionBehavior_SelectionChanged2(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            Telerik.Windows.Controls.ChartView.RadChartBase radchartbase = (sender as Telerik.Windows.Controls.ChartView.ChartSelectionBehavior).Chart;
            Telerik.Charting.PieDataPoint SelectedPoint = radchartbase.SelectedPoints.FirstOrDefault() as Telerik.Charting.PieDataPoint;

            StackPanel stackpanel = new StackPanel();
            stackpanel = (this.LoadPieGrid.Children[1] as ScrollViewer).Content as StackPanel;

            foreach (var item in stackpanel.Children)
            {
                System.Windows.Shapes.Rectangle itemrectangle = (item as StackPanel).Children[0] as System.Windows.Shapes.Rectangle;
                itemrectangle.Width = 15;
                itemrectangle.Height = 15;
            }

            if (SelectedPoint != null)
            {
                StackPanel childstackpanel = stackpanel.Children
                    .FirstOrDefault(p => ((p as StackPanel).Children[1] as TextBlock).Text == (SelectedPoint.DataItem as AircraftSeattingStatisticByYear).AirlineName) as StackPanel;
                System.Windows.Shapes.Rectangle rectangle = childstackpanel.Children[0] as System.Windows.Shapes.Rectangle;
                rectangle.Width = 12;
                rectangle.Height = 12;

                ShowplanHistoryWindow(SelectedPoint);
            }
            else
            {
                planHistoryWindow.Close();
            }
        }

        /// <summary>
        /// 根据相应的饼图数据生成饼图标签
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetPieMark1(IEnumerable<AircraftStatistic> ienumerable, Grid grid)
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
                barpanel.MouseLeftButtonDown += PiePanel_MouseLeftButtonDown1;
                barpanel.Orientation = Orientation.Horizontal;
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 15;
                rectangle.Height = 15;
                rectangle.Fill = new SolidColorBrush(commonmethod.GetColor(item.Color));
                TextBlock textblock = new TextBlock();
                textblock.Text = item.AirlineName;
                textblock.Style = this._view.Resources.FirstOrDefault(p => p.Key.ToString() == "legendItemStyle").Value as Style;
                barpanel.Children.Add(rectangle);
                barpanel.Children.Add(textblock);
                stackpanel.Children.Add(barpanel);
            }
        }

        /// <summary>
        /// 根据相应的饼图数据生成饼图标签
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetPieMark2(IEnumerable<AircraftSeattingStatisticByYear> ienumerable, Grid grid)
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
                barpanel.MouseLeftButtonDown += PiePanel_MouseLeftButtonDown2;
                barpanel.Orientation = Orientation.Horizontal;
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 15;
                rectangle.Height = 15;
                rectangle.Fill = new SolidColorBrush(commonmethod.GetColor(item.Color));
                TextBlock textblock = new TextBlock();
                textblock.Text = item.AirlineName;
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
        void PiePanel_MouseLeftButtonDown1(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                if ((piedatapoint.DataItem as AircraftStatistic).AirlineName == shortname)
                {

                    piedatapoint.IsSelected = !piedatapoint.IsSelected;
                    if (piedatapoint.IsSelected == true)
                    {
                        if (radpiechart.EmptyContent.ToString() == "当前运营客机分布")
                        {
                            ShowOpertationHistoryWindow(piedatapoint);
                        }
                        else if (radpiechart.EmptyContent.ToString() == this.TotalAirlineContent)
                        {
                            ShowtotalWindow(piedatapoint);
                        }
                    }
                    else
                    {
                        if (radpiechart.EmptyContent.ToString() == "当前运营客机分布")
                        {
                            opertationHistoryWindow.Close();
                        }
                        else if (radpiechart.EmptyContent.ToString() == this.TotalAirlineContent)
                        {
                            totalOpertationWindow.Close();
                            totalPlanHistoryWindow.Close();
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
        /// 饼状图标签的选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PiePanel_MouseLeftButtonDown2(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                if ((piedatapoint.DataItem as AircraftSeattingStatisticByYear).AirlineName == shortname)
                {

                    piedatapoint.IsSelected = !piedatapoint.IsSelected;
                    if (piedatapoint.IsSelected == true)
                    {
                        ShowplanHistoryWindow(piedatapoint);
                    }
                    else
                    {
                        planHistoryWindow.Close();
                    }
                }
                else
                {
                    piedatapoint.IsSelected = false; ;

                }
            }
        }

        /// <summary>
        /// 获取趋势图的颜色配置
        /// </summary>
        private Dictionary<string, string> GetColorDictionary()
        {
            Dictionary<string, string> colordictionary = new Dictionary<string, string>();

            XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "运力变化"))
            {
                string aircraftcolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "运力变化")
                    .Descendants("Item").FirstOrDefault(p => p.Attribute("Name").Value == "飞机数").Attribute("Color").Value;
                colordictionary.Add("总飞机数", aircraftcolor);
            }
            else
            {
                colordictionary.Add("总飞机数", commonmethod.GetRandomColor());
            }

            if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "座级"))
            {
                XElement regionalcolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "座级");
                foreach (var item in regionalcolor.Descendants("Item"))
                {
                    colordictionary.Add(item.Attribute("Name").Value, item.Attribute("Color").Value);
                }
            }
            else
            {
                colordictionary.Add("中型货机", commonmethod.GetRandomColor());
                colordictionary.Add("100-200座客机", commonmethod.GetRandomColor());
                colordictionary.Add("250座以上客机", commonmethod.GetRandomColor());
                colordictionary.Add("大型货机", commonmethod.GetRandomColor());
                colordictionary.Add("100座以下客机", commonmethod.GetRandomColor());
            }

            return colordictionary;
        }


        /// <summary>
        /// 控制趋势图的Y轴和折线及标签颜色
        /// </summary>
        /// <param name="ienumerable">饼图数据集合</param>
        private void SetRadCartesianChartColor()
        {
            Dictionary<string, string> colordictionary = GetColorDictionary();

            //控制趋势图的标签颜色
            foreach (var item in ((TrendGrid.Children[0] as ScrollViewer).Content as StackPanel).Children)
            {
                StackPanel stackpanel = item as StackPanel;
                CheckBox checkbox = stackpanel.Children[0] as CheckBox;
                stackpanel.Background = new SolidColorBrush(commonmethod.GetColor(colordictionary[checkbox.Content.ToString()]));
            }

            //控制折线趋势图的Y轴颜色
            foreach (var item in ((TrendGrid.Children[1] as RadCartesianChart).Resources["additionalVerticalAxis"] as AxisCollection))
            {
                LinearAxis linearaxis = item as LinearAxis;
                if (linearaxis.Title.ToString() == "总飞机数(架)")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["总飞机数"]));
                }
                else if (linearaxis.Title.ToString() == "座级飞机数（架）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["250座以上客机"]));
                }
            }
            //控制折线趋势图的线条颜色
            foreach (var item in ((TrendGrid.Children[1] as RadCartesianChart).Series))
            {
                LineSeries linearseries = item as LineSeries;
                linearseries.Stroke = new SolidColorBrush(commonmethod.GetColor(colordictionary[linearseries.DisplayName]));
            }


            //控制柱状趋势图的Y轴颜色
            foreach (var item in ((TrendGrid.Children[2] as RadCartesianChart).Resources["additionalVerticalAxis"] as AxisCollection))
            {
                LinearAxis linearaxis = item as LinearAxis;
                if (linearaxis.Title.ToString() == "总净增数(架)")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["总飞机数"]));
                }
                else if (linearaxis.Title.ToString() == "座级净增数（架）")
                {
                    linearaxis.ElementBrush = new SolidColorBrush(commonmethod.GetColor(colordictionary["250座以上客机"]));
                }
            }
        }




        /// <summary>
        /// 控制趋势图中折线（柱状）的显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                (this.TrendGrid.Children[1] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;
                if (checkbox.Content.ToString() != "总飞机数")
                {
                    (this.TrendGrid.Children[2] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 控制趋势图中折线（柱状）的隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                (this.TrendGrid.Children[1] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
                if (checkbox.Content.ToString() != "总飞机数")
                {
                    (this.TrendGrid.Children[2] as RadCartesianChart).Series.FirstOrDefault(p => p.DisplayName == checkbox.Content.ToString()).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

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
            //初始化子窗体
            InitalizerRadWindows(opertationHistoryWindow, "opertationHistoryWindow", 200);
            InitalizerRadWindows(planHistoryWindow, "planHistoryWindow", 220);
            InitalizerRadWindows(totalOpertationWindow, "totalOpertationWindow", 240);
            InitalizerRadWindows(totalPlanHistoryWindow, "totalPlanHistoryWindow", 690);
            AddRadMenu(opertationHistoryWindow);
            AddRadMenu(planHistoryWindow);
            AddRadMenu(totalOpertationWindow);
            AddRadMenu(totalPlanHistoryWindow);
            ManageFlagPlanCommand = new DelegateCommand<object>(this.OnManageFlagPlan, this.CanManageFlagPlan);
            ViewAttachmentCommand = new DelegateCommand<object>(this.OnViewAttachment, this.CanViewAttachment);
            this.ExportCommand = new DelegateCommand<object>(this.OnExport, this.CanExport);//导出图表源数据（Source data）
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
            //导航时，如果子窗体还未关闭，就强制关闭
            if (opertationHistoryWindow.IsOpen)
            {
                opertationHistoryWindow.Close();
            }
            if (planHistoryWindow.IsOpen)
            {
                planHistoryWindow.Close();
            }
            if (totalOpertationWindow.IsOpen)
            {
                totalOpertationWindow.Close();
            }
            if (totalPlanHistoryWindow.IsOpen)
            {
                totalPlanHistoryWindow.Close();
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadXmlConfig();
            LoadPlan();
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

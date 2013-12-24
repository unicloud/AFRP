using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using CAAC.CAFM.ViewModels;

namespace UniCloud.CAFM.CustomerClass
{
    /// <summary>
    /// 基于客机计划年份统计的飞机
    /// </summary>
    public class AirlinePlanStatistics
    {
        public AirlinePlanStatistics()
        {
            CommonMethod commonmethod = new CommonMethod();
            this.TotalColor = commonmethod.GetRandomColor();
            this.Than250Color = commonmethod.GetRandomColor();
            this.Than100Color = commonmethod.GetRandomColor();
            this.Less100Color = commonmethod.GetRandomColor();
        }
        public string TotalName { get; set; }//飞机总数的名称
        public string Than250Name { get; set; }//座级超过250的名称
        public string Than100Name { get; set; }//座级100-200的名称
        public string Less100Name { get; set; }//座级小于100的名称

        public string TotalColor { get; set; }//飞机总数的颜色
        public string Than250Color { get; set; }//座级超过250的颜色
        public string Than100Color { get; set; }//座级100-200的颜色
        public string Less100Color { get; set; }//座级小于100的颜色


        public decimal SeattingTotalAircraft { get; set; } //计划飞机总数
        public decimal SeattingNumberThan250 { get; set; } //计划座级超过250的飞机数
        public decimal SeattingNumberThan100 { get; set; } //计划座级100-200的飞机数
        public decimal SeattingNumberLess100 { get; set; } //计划座级小于100的飞机数
        public string SeattingTotalToolTip { get; set; }//飞机总数的提示
        public string SeattingThan250ToolTip { get; set; }//座级超过250的提示
        public string SeattingThan100ToolTip { get; set; }//座级100-200的提示
        public string SeattingLess100ToolTip { get; set; }//座级小于100的提示

        public decimal RealTotalAircraft { get; set; } //实际飞机总数
        public decimal RealIncreaseThan250 { get; set; } //实际计划座级超过250的飞机数
        public decimal RealIncreaseThan100 { get; set; } //实际计划座级超过100-200的飞机数
        public decimal RealIncreaseLess100 { get; set; } //计划座级小于100的飞机数
        public string RealTotalToolTip { get; set; }//飞机总数的颜色
        public string RealThan250ToolTip { get; set; }//座级超过250的提示
        public string RealThan100ToolTip { get; set; }//座级100-200的提示
        public string RealLess100ToolTip { get; set; }//座级小于100的提示

        public int Year { get; set; }
    }

    /// <summary>
    /// 基于货机计划年份统计的飞机
    /// </summary>
    public class CargoPlanStatistics
    {

        public CargoPlanStatistics()
        {
            CommonMethod commonmethod = new CommonMethod();
            this.TotalColor = commonmethod.GetRandomColor();
            this.LargeColor = commonmethod.GetRandomColor();
            this.MidsizeColor = commonmethod.GetRandomColor();
        }
        public string TotalName { get; set; }//飞机总数的名称
        public string LargeName { get; set; }//大型货机的名称
        public string MidsizeName { get; set; }//中型货机的名称

        public string TotalColor { get; set; }//飞机总数的颜色
        public string LargeColor { get; set; }//大型货机的颜色
        public string MidsizeColor { get; set; }//中型货机的颜色

        public decimal TotalAircraft { get; set; } //计划所有飞机
        public decimal LargeCargoAircraftNumber { get; set; } //计划大型货机数
        public decimal MidsizeCargoAircraftNumber { get; set; } //计划中型货机数
        public string TotalToolTip { get; set; }//计划所有飞机的颜色
        public string LargeToolTip { get; set; }//计划大型货机数的颜色
        public string MidsizeToolTip { get; set; }//计划中型货机数的颜色

        public decimal IncreaseTotalAircraft { get; set; } //实际计划所有飞机
        public decimal IncreaseLargeCargoNumber { get; set; } //实际计划大型货机飞机数
        public decimal IncreaseMidsizeCargoNumber { get; set; } //实际计划中型货机飞机数
        public string IncreaseTotalToolTip { get; set; }//实际计划所有飞机的颜色
        public string IncreaseLargeToolTip { get; set; }//实际计划大型货机数的颜色
        public string IncreaseMidsizeToolTip { get; set; }//实际计划中型货机数的颜色


        public int Year { get; set; }
    }

    /// <summary>
    /// 飞机的统计
    /// </summary>
    public class AircraftStatistic
    {
        public decimal TotalAircraftNumberPercent { get; set; } //每个航空公司所占飞机的具体份额
        public string AirlineName { get; set; } //航空公司
        public string Color { get; set; } //航空公司的颜色
        public int Year { get; set; }
        public int AircraftAmount { get; set; } // 某年期末数
        public int IncreaseAmount { get; set; } // 某年净增数
    }

    /// <summary>
    /// 飞机座级类别
    /// </summary>
    public class AircraftSeattingStatisticByYear
    {
        public string SeattingType { get; set; } //座级类别
        public string AirlineName { get; set; } //航空公司
        public string Color { get; set; } //航空公司的颜色
        public int Year { get; set; } //选择年份
        public int AircraftAmount { get; set; } // 某座级某年净增数
        public decimal TotalAircraftNumberPercent { get; set; }//某个座级某年份所占比例
    }

    /// <summary>
    /// 飞机货机类别
    /// </summary>
    public class AircraftCargoStatisticByYear
    {
        public string CargoType { get; set; } //座级类别
        public string AirlineName { get; set; } //航空公司
        public string Color { get; set; } //航空公司的颜色
        public int Year { get; set; } //选择年份
        public int AircraftAmount { get; set; } // 某货机型号某年净增数
        public decimal TotalAircraftNumberPercent { get; set; }//某个座级某年份所在比例
    }

}

using System;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Xml.Linq;
using System.Linq;

namespace UniCloud.Fleet.XmlConfigs
{
    public class FleetAgeXml : BaseXml
    {

        public FleetAgeXml(XmlConfigService XmlService)
            : base(XmlService)
        {
            _ConfigType = "机龄分析";
        }

        protected override void GenerateXmlContent(XElement RootNode)
        {
            //所有飞机
            var AllAircraft = this.GetAllAircraft().Where(o => o.FactoryDate != null).ToList();
            //所有机型
            var AircraftTypeList = this.GetAllAircraftTypeList();
            //所有座级
            var AircraftRegionalList = this.GetAllAircraftRegionalList();
            //所有航空公司
            var AllAirlinesList = this.GetAllAirLinesShortNameList(); 
            //按月生成每个月的数据
            DateTime startTime = GetMonthEndofDateTime(Convert.ToDateTime(AllAircraft.Min(p => p.FactoryDate)));
            DateTime endTime = GetMonthEndofDateTime(DateTime.Now);
            
            startTime = DeleteXmlByYear(RootNode, startTime);

            if (startTime.Year < 1900) startTime = endTime;
            for (DateTime time = startTime; time <= endTime; time = GetMonthEndofDateTime(time.AddMonths(1)))
            {
                //生成时间节点
                XElement DateTimeNode = new XElement("DateTime", new XAttribute("EndOfMonth", FormatDate(time)));
                RootNode.Add(DateTimeNode);
                //每个月份可计算机龄的飞机集合
                var MonthAircraft = AllAircraft.Where(o => 
                    o.OperationHistories.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)) 
                    && o.AircraftBusinesses.Any(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)) 
                    && o.FactoryDate <= time && !(o.ExportDate != null && o.ExportDate < time)).ToList();

                //当前时间点的所有飞机的平均年龄
                double AverageAge = 0;
                if (MonthAircraft != null && MonthAircraft.Count() > 0)
                {
                    AverageAge = MonthAircraft.ToList().Average(p => (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month));
                }

                //座级
                XElement RegionalNode = new XElement("Type", new XAttribute("TypeName", "座级"), new XAttribute("Amount", AverageAge));
                DateTimeNode.Add(RegionalNode);
                foreach (string AircraftRegional in AircraftRegionalList)
                {
                    GetAgeNodeByRegional(ref RegionalNode, MonthAircraft, time, AircraftRegional);
                }
                //机型
                XElement TypeNode = new XElement("Type", new XAttribute("TypeName", "机型"), new XAttribute("Amount", AverageAge));
                DateTimeNode.Add(TypeNode);
                foreach (string AircraftType in AircraftTypeList)
                {
                    GetAgeNodeByType(ref TypeNode, MonthAircraft, time, AircraftType);
                }
                //航空公司
                XElement AirlinesNode = new XElement("Type", new XAttribute("TypeName", "航空公司"), new XAttribute("Amount", AverageAge));
                DateTimeNode.Add(AirlinesNode);
                foreach (string Airlines in AllAirlinesList)
                {
                    GetAgeNodeByAirlines(ref AirlinesNode, MonthAircraft, time, Airlines);
                }
            }
        }

        /// <summary>
        /// 根据参数生成相应机型的平均机龄XML节点
        /// </summary>
        /// <param name="DateTimeNode">Xml的时间节点</param>
        /// <param name="MonthAircraft">月份飞机集合</param>
        /// <param name="time">选中时间点</param>
        /// <param name="AircraftType">机型</param>
        private void GetAgeNodeByType(ref XElement TypeNode, IEnumerable<Aircraft> MonthAircraft, DateTime time, string AircraftType)
        {
            IEnumerable<Aircraft> aircraft = MonthAircraft;
                aircraft = MonthAircraft.Where(o => o.AircraftBusinesses.FirstOrDefault(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)).AircraftType.Name == AircraftType);
            //当前时间点的机型对应的平均年龄
            double AverageAge = 0;
            if (aircraft != null && aircraft.Count() > 0)
            {
                AverageAge = aircraft.ToList().Average(p => (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month));
            }
            //机型节点
            XElement ItemNode = new XElement("Item", new XAttribute("Name", AircraftType), Math.Round(AverageAge, 4));
            TypeNode.Add(ItemNode);
        }


        /// <summary>
        /// 根据参数生成相应座级的平均机龄XML节点
        /// </summary>
        /// <param name="DateTimeNode">Xml的时间节点</param>
        /// <param name="MonthAircraft">月份飞机集合</param>
        /// <param name="time">选中时间点</param>
        /// <param name="AircraftType">机型</param>
        private void GetAgeNodeByRegional(ref XElement RegionalNode, IEnumerable<Aircraft> MonthAircraft, DateTime time, string AircraftRegional)
        {
            IEnumerable<Aircraft> aircraft = MonthAircraft;

            aircraft = MonthAircraft.Where(o => o.AircraftBusinesses.FirstOrDefault(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)).AircraftType.AircraftCategory.Regional == AircraftRegional);
            //当前时间点的座级对应的平均年龄
            double AverageAge = 0;
            if (aircraft != null && aircraft.Count() > 0)
            {
                AverageAge = aircraft.ToList().Average(p => (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month));
            }
            //座级节点
            XElement ItemNode = new XElement("Item", new XAttribute("Name", AircraftRegional), Math.Round(AverageAge, 4));
            RegionalNode.Add(ItemNode);
        }


        /// <summary>
        /// 根据参数生成相应航空公司的平均机龄XML节点
        /// </summary>
        /// <param name="DateTimeNode">Xml的时间节点</param>
        /// <param name="MonthAircraft">月份飞机集合</param>
        /// <param name="time">选中时间点</param>
        /// <param name="AircraftType">机型</param>
        private void GetAgeNodeByAirlines(ref XElement AirlinesNode, IEnumerable<Aircraft> MonthAircraft, DateTime time, string Airlines)
        {
            IEnumerable<Aircraft> aircraft = MonthAircraft;
            //机型座位级别分别位于运营权和商业数据历史
            aircraft = MonthAircraft.Where(o => o.OperationHistories.FirstOrDefault(p => p.StartDate <= time && !(p.EndDate != null && p.EndDate < time)).Airlines.ShortName == Airlines);
            //当前时间点的航空公司对应的平均年龄
            double AverageAge = 0;
            if (aircraft != null && aircraft.Count() > 0)
            {
                AverageAge = aircraft.ToList().Average(p => (time.Year - Convert.ToDateTime(p.FactoryDate).Year) * 12 + (time.Month - Convert.ToDateTime(p.FactoryDate).Month));
            }
            //航空公司节点
            XElement ItemNode = new XElement("Item", new XAttribute("Name", Airlines), Math.Round(AverageAge, 4));
            AirlinesNode.Add(ItemNode);
        }



        protected override XmlConfig GetXmlConfig()
        {
            if (AllXmlConfigs != null)
            {
                var SpeXmlConfigs = AllXmlConfigs.Where(p => p.ConfigType == _ConfigType);
                if (SpeXmlConfigs == null || SpeXmlConfigs.Count() < 1)
                {
                    XmlConfig XmlConfig1 = new XmlConfig();
                    XmlConfig1.XmlConfigID = Guid.NewGuid();
                    XmlConfig1.ConfigType = _ConfigType;
                    XElement RootNode = new XElement("FleetAge");
                    XmlConfig1.XmlContent = RootNode;
                    //设置编辑状态
                    this.EditState = TEditState.esNew;
                    return XmlConfig1;
                }
                else
                {
                    XmlConfig XmlConfig1 = (XmlConfig)SpeXmlConfigs.FirstOrDefault();
                    if (XmlConfig1.XmlContent == null)
                    {
                        XElement RootNode = new XElement("FleetAge");
                        XmlConfig1.XmlContent = RootNode;
                    }
                    //设置编辑状态
                    this.EditState = TEditState.esEdit;
                    return XmlConfig1;
                }
            }
            else
            {
                //设置编辑状态
                this.EditState = TEditState.esNone;
                return null;
            }
        }

    }
}
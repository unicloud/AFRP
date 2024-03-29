﻿using System;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Xml.Linq;
using System.Linq;

namespace UniCloud.Fleet.XmlConfigs
{

    public class FleetTrendCargoXml : BaseXml
    {

        public FleetTrendCargoXml(XmlConfigService XmlService)
            : base(XmlService)
        {
            _ConfigType = "货机运力";
        }

        protected override void GenerateXmlContent(XElement RootNode)
        {
            //清空节点原有数据
            RootNode.RemoveAll();
            //所有飞机
            var AllAircraft = this.GetAllAircraft().ToList();
            //当前航空公司
            string AirlinesName = GetCurrentAirlinesShortName();
            //按月生成每个月的数据
            DateTime startTime = GetOperationStartDate();
            DateTime endTime = GetOperationEndDate();
            for (DateTime time = startTime; time <= endTime; time = GetMonthEndofDateTime(time.AddMonths(1)))
            {
                //生成时间节点
                XElement DateTimeNode = new XElement("DateTime", new XAttribute("EndOfMonth", FormatDate(time)));
                RootNode.Add(DateTimeNode);

                //当前时间点飞机(包括子公司)
                var MonthAircraft = AllAircraft.Where(p =>
                    p.AircraftBusinesses.Any(q =>
                        q.AircraftType.AircraftCategory.Category == "货机" &&
                        q.StartDate <= time && !(q.EndDate != null && q.EndDate < time)) &&
                        p.OperationHistories.Any(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time)));

                //当前时间点的飞机商业数据
                var MonthBusinessHistory = MonthAircraft.Select(o =>
                            o.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                //飞机数
                XElement AircraftNode = new XElement("Type", new XAttribute("TypeName", "飞机数（子）"), new XAttribute("Amount", MonthAircraft.Count()));
                DateTimeNode.Add(AircraftNode);

                //商载量
                decimal CarryingCapacity = (MonthBusinessHistory == null) || MonthBusinessHistory.Count() == 0 ? 0 : MonthBusinessHistory.Sum(p => p == null ? 0 : p.CarryingCapacity);
                XElement CarryingCapacityNode = new XElement("Type", new XAttribute("TypeName", "商载量（子）"), new XAttribute("Amount", FormatDecimal(CarryingCapacity)));
                DateTimeNode.Add(CarryingCapacityNode);


                //当前时间点飞机(不包括子公司)
                var MonthAircraft1 = AllAircraft.Where(p =>
                    p.AircraftBusinesses.Any(q =>
                        q.AircraftType.AircraftCategory.Category == "货机" &&
                        q.StartDate <= time && !(q.EndDate != null && q.EndDate < time)) &&
                        p.OperationHistories.Any(pp => (pp.Airlines.ShortName == AirlinesName || pp.Airlines.SubType == 2)  &&
                            pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time)));

                //当前时间点飞机的运营权历史
                var MonthOperationHistoriy = MonthAircraft1.Select(o =>
                            o.OperationHistories.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));
                //当前时间点飞机的商业数据历史
                var MonthAircraftBusiness = MonthAircraft1.Select(o =>
                            o.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                //飞机数
                XElement AircraftNode1 = new XElement("Type", new XAttribute("TypeName", "飞机数"), new XAttribute("Amount", MonthAircraft1.Count()));
                DateTimeNode.Add(AircraftNode1);

                //商载量
                decimal CarryingCapacity1 = (MonthBusinessHistory == null) || MonthBusinessHistory.Count() == 0 ?
                    0 : MonthBusinessHistory.Sum(p => p == null ? 0 : p.CarryingCapacity);
                XElement CarryingCapacityNode1 = new XElement("Type", new XAttribute("TypeName", "商载量"), new XAttribute("Amount", FormatDecimal(CarryingCapacity1)));
                DateTimeNode.Add(CarryingCapacityNode1);

                if (MonthOperationHistoriy != null)
                {
                    //只属于母公司的运营权历史
                    var OperationHistoriySuperior = MonthOperationHistoriy.Where(p => p.Airlines.ShortName == AirlinesName &&
                        (p.SubOperationHistories == null || !p.SubOperationHistories.Any(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)) || p.SubOperationHistories.Any(a => a.Airlines.ShortName == AirlinesName && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))));
                    if (OperationHistoriySuperior != null && OperationHistoriySuperior.Count() > 0)
                    {
                        AircraftNode1.Add(new XElement("Item", new XAttribute("Name", AirlinesName), new XAttribute("Percent", GetPercent(OperationHistoriySuperior.Count(), MonthOperationHistoriy.Count())), OperationHistoriySuperior.Count()));

                        //母公司的商业数据历史
                        var MonthAircraftBusinessSuperior = OperationHistoriySuperior.Select(p => p.Aircraft.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                        //母公司的商载量
                        decimal CarryingCapacityFiliale1 = (MonthAircraftBusinessSuperior == null) || MonthAircraftBusinessSuperior.Count() == 0 ?
                            0 : MonthAircraftBusinessSuperior.Sum(p => p == null ? 0 : p.CarryingCapacity);
                        XElement CarryingCapacityNodeFiliale1 = new XElement("Item", new XAttribute("Name", AirlinesName), new XAttribute("Percent", GetPercent(CarryingCapacityFiliale1, CarryingCapacity1)), CarryingCapacityFiliale1);
                        CarryingCapacityNode1.Add(CarryingCapacityNodeFiliale1);
                    }


                    //属于分公司的运营权历史
                    var SubOperationHistory = MonthOperationHistoriy.Where(p => p.Airlines.ShortName == AirlinesName && p.SubOperationHistories != null && p.SubOperationHistories.Any(a => a.Airlines.ShortName != AirlinesName && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)))
                        .Select(p => p.SubOperationHistories.FirstOrDefault(a => a.Airlines.ShortName != AirlinesName && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));
                    if (SubOperationHistory != null && SubOperationHistory.Count() > 0)
                    {
                        foreach (var item in SubOperationHistory.GroupBy(p => p.Airlines.ShortName).ToList())
                        {
                            //分公司的飞机数
                            AircraftNode1.Add(new XElement("Item", new XAttribute("Name", item.Key), new XAttribute("Percent", GetPercent(item.Count(), MonthOperationHistoriy.Count())), item.Count()));

                            //分公司的商业数据历史
                            var MonthAircraftBusinessFiliale = item.Select(p => p.OperationHistory.Aircraft.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                            //分公司的商载量
                            decimal CarryingCapacityFiliale1 = (MonthAircraftBusinessFiliale == null) || MonthAircraftBusinessFiliale.Count() == 0 ?
                                0 : MonthAircraftBusinessFiliale.Sum(p => p == null ? 0 : p.CarryingCapacity);
                            XElement CarryingCapacityNodeFiliale1 = new XElement("Item", new XAttribute("Name", item.Key), new XAttribute("Percent", GetPercent(CarryingCapacityFiliale1, CarryingCapacity1)), CarryingCapacityFiliale1);
                            CarryingCapacityNode1.Add(CarryingCapacityNodeFiliale1);
                        }
                    }


                    //属于分子公司的运营权历史
                    var OperationHistoriyMember = MonthOperationHistoriy.Where(a => a.Airlines.ShortName != AirlinesName && a.Airlines.SubType == 2 && a.StartDate <= time && !(a.EndDate != null && a.EndDate < time));
                    if (OperationHistoriyMember != null && OperationHistoriyMember.Count() > 0)
                    {
                        foreach (var item in OperationHistoriyMember.GroupBy(p => p.Airlines.ShortName).ToList())
                        {
                            AircraftNode1.Add(new XElement("Item", new XAttribute("Name", item.Key), new XAttribute("Percent", GetPercent(item.Count(), MonthOperationHistoriy.Count())), item.Count()));

                            //分子公司的商业数据历史
                            var MonthAircraftBusinessMember = item.Select(p => p.Aircraft.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)));

                            //分子公司的商载量
                            decimal CarryingCapacityFiliale1 = (MonthAircraftBusinessMember == null) || MonthAircraftBusinessMember.Count() == 0 ?
                                0 : MonthAircraftBusinessMember.Sum(p => p == null ? 0 : p.CarryingCapacity);
                            XElement CarryingCapacityNodeFiliale1 = new XElement("Item", new XAttribute("Name", item.Key), new XAttribute("Percent", GetPercent(CarryingCapacityFiliale1, CarryingCapacity1)), CarryingCapacityFiliale1);
                            CarryingCapacityNode1.Add(CarryingCapacityNodeFiliale1);
                        }
                    }
                }
            }
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
                    XElement RootNode = new XElement("FleetTrendCargo");
                    XmlConfig1.XmlContent = RootNode;
                    //设置编辑状态
                    this.EditState = TEditState.esNew;
                    return XmlConfig1;
                }
                else
                {
                    XmlConfig XmlConfig1 = (XmlConfig)SpeXmlConfigs.FirstOrDefault();
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

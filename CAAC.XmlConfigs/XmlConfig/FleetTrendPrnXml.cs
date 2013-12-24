using System;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Xml.Linq;
using System.Linq;

namespace UniCloud.Fleet.XmlConfigs
{

    public class FleetTrendPrnXml : BaseXml
    {

        public FleetTrendPrnXml(XmlConfigService service)
            : base(service)
        {
            _ConfigType = "客机运力";
        }

        protected override void GenerateXmlContent(XElement RootNode)
        {
            //所有飞机
            var AllAircraft = this.GetAllAircraft().ToList();
            //所有航空公司
            var AllAirlinesList = this.GetAllAirLinesShortNameList();
            //按月生成每个月的数据
            DateTime startTime = GetOperationStartDate();
            DateTime endTime = GetOperationEndDate();
            startTime = DeleteXmlByYear(RootNode, startTime);

            for (DateTime time = startTime; time <= endTime; time = GetMonthEndofDateTime(time.AddMonths(1)))
            {
                //生成时间节点
                XElement DateTimeNode = new XElement("DateTime", new XAttribute("EndOfMonth", FormatDate(time)));
                RootNode.Add(DateTimeNode);

                //得到time对应时间的所有客机,且在该时间点处于运行状态

                var MonthAircraft = AllAircraft.Where(p =>
                        p.AircraftBusinesses.Any(q =>
                            q.AircraftType.AircraftCategory.Category == "客机" &&
                                q.StartDate <= time && !(q.EndDate != null && q.EndDate < time)) &&
                        p.OperationHistories.Any(pp => pp.StartDate <= time && !(pp.EndDate != null && pp.EndDate < time))).ToList();

                //计算time对应时间所有飞机数量
                int Amount = MonthAircraft.Count();
                //飞机数节点
                XElement AirNumNode = new XElement("Type", new XAttribute("TypeName", "飞机数"), new XAttribute("Amount", Amount));
                DateTimeNode.Add(AirNumNode);

                //当前时间点的飞机商业数据
                var MonthBusinessHistory = MonthAircraft.Select(o =>
                            o.AircraftBusinesses.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time))).ToList();

                //座位数
                decimal SeatingCapacity = MonthBusinessHistory == null || MonthBusinessHistory.Count() == 0 ? 0 : MonthBusinessHistory.Sum(p => p == null ? 0 : p.SeatingCapacity);
                XElement SeatingCapacityNode = new XElement("Type", new XAttribute("TypeName", "座位数"), new XAttribute("Amount", FormatDecimal(SeatingCapacity)));
                DateTimeNode.Add(SeatingCapacityNode);

                //商载量
                decimal CarryingCapacity = (MonthBusinessHistory == null) || MonthBusinessHistory.Count() == 0 ? 0 : MonthBusinessHistory.Sum(p => p == null ? 0 : p.CarryingCapacity);
                XElement CarryingCapacityNode = new XElement("Type", new XAttribute("TypeName", "商载量"), new XAttribute("Amount", FormatDecimal(CarryingCapacity)));
                DateTimeNode.Add(CarryingCapacityNode);

                if (MonthBusinessHistory != null)
                {
                    //所有航空公司数据
                    foreach (var AirlinesName in AllAirlinesList)
                    {
                        if (AirlinesName == null || AirlinesName == "") continue;
                        var AirlinesBusiness = MonthBusinessHistory.Where(p => p != null
                            && p.Aircraft.OperationHistories.FirstOrDefault(a => a.StartDate <= time && !(a.EndDate != null && a.EndDate < time)).Airlines.ShortName == AirlinesName).ToList();

                        // 每个航空的飞机数量
                        int AirNum = AirlinesBusiness.Count();
                        XElement AirLinesNode = new XElement("Item", new XAttribute("Name", AirlinesName), new XAttribute("Percent", GetPercent(AirNum, Amount)), AirNum);
                        AirNumNode.Add(AirLinesNode);

                        //每个航空的座位数
                        decimal AirSeatingCapacity = (AirlinesBusiness == null) || AirlinesBusiness.Count() == 0 ? 0 : AirlinesBusiness.Sum(p => p == null ? 0 : p.SeatingCapacity);
                        XElement AirLinesSeatingCapacityNode = new XElement("Item",
                                    new XAttribute("Name", AirlinesName),
                                    new XAttribute("Percent", GetPercent(AirSeatingCapacity, SeatingCapacity)),
                                    FormatDecimal(AirSeatingCapacity));
                        SeatingCapacityNode.Add(AirLinesSeatingCapacityNode);

                        //每个航空的商载量
                        decimal AirCarryingCapacity = (AirlinesBusiness == null) || AirlinesBusiness.Count() == 0 ? 0 : AirlinesBusiness.Sum(p => p == null ? 0 : p.CarryingCapacity);
                        XElement AirLinesCarryingCapacityNode = new XElement("Item",
                                    new XAttribute("Name", AirlinesName),
                                    new XAttribute("Percent", GetPercent(AirCarryingCapacity, CarryingCapacity)),
                                    FormatDecimal(AirCarryingCapacity));
                        CarryingCapacityNode.Add(AirLinesCarryingCapacityNode);

                    } // end foreach
                } //end if 
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
                    XElement RootNode = new XElement("FleetTrendPrn");
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
                        XElement RootNode = new XElement("FleetTrendPrn");
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

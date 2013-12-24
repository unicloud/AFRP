using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.XmlConfigs
{
    /// <summary>
    /// 运营飞机的统计
    /// </summary>
    public class OperationAircraftXml : BaseXml
    {
        public OperationAircraftXml(XmlConfigService service)
            : base(service)
        {
            _ConfigType = "当前运营飞机";
        }
        protected override void GenerateXmlContent(XElement RootNode)
        {
            //清空节点原有数据
            RootNode.RemoveAll();
            //获取所有的飞机
            var OprerationAf = this.GetAllAircraft().Where(p => p.IsOperation == true);
            //计划中的客机
            var AircraftPnr = OprerationAf == null ?
                    null :
                    OprerationAf.Where(p => p.AircraftType.AircraftCategory.Category == "客机");

            //计划中的货机机
            var AircraftCargo = OprerationAf == null ?
                    null :
                    OprerationAf.Where(p => p.AircraftType.AircraftCategory.Category == "货机");

            if (AircraftPnr != null || AircraftPnr.Any())
            {
                //客机总计划数
                decimal totalAfCount = AircraftPnr.Count();
                //客机的子节点
                XElement PnrNode = new XElement("Pnr", new XAttribute("Amount", totalAfCount));
                //根据航空公司分组，统计客机的各个航空公司所在的比例
                foreach (var Item in AircraftPnr.GroupBy(t => t.Airlines.ShortName))
                {
                    //获取节点
                    XElement ItemNode = new XElement("Type",
                         new XAttribute(Item.Key.ToString(), Item.Count()),
                         new XAttribute("Percent", GetPercent(Item.Count(), totalAfCount)));
                    PnrNode.Add(ItemNode);
                }
                RootNode.Add(PnrNode);
            }

            if (AircraftCargo != null || AircraftCargo.Any())
            {
                //货机总计划数
                decimal totalAfCount = AircraftCargo.Count();
                //货机的子节点
                XElement CargoNode = new XElement("Cargo", new XAttribute("Amount", totalAfCount));
                //根据航空公司分组，统计货机的各个航空公司所在的比例
                foreach (var Item in AircraftCargo.GroupBy(t => t.Airlines.ShortName))
                {
                    //获取节点
                    XElement ItemNode = new XElement("Type",
                         new XAttribute(Item.Key.ToString(), Item.Count()),
                         new XAttribute("Percent", GetPercent(Item.Count(), totalAfCount)));
                    CargoNode.Add(ItemNode);
                }
                RootNode.Add(CargoNode);
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
                    XElement RootNode = new XElement("OperationAircrfat");
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
                        XElement RootNode = new XElement("OperationAircrfat");
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

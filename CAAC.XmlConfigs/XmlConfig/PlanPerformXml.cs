using System;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Xml.Linq;
using System.Linq;

namespace UniCloud.Fleet.XmlConfigs
{

    public class PlanPerformXml : BaseXml
    {

        public PlanPerformXml(XmlConfigService service)
            : base(service)
        {
            _ConfigType = "计划执行";
        }


        /// <summary>
        /// 获取指定航空公司和年份的计划完成情况的百分比
        /// </summary>
        /// <param name="EnumerablePlan"></param>
        /// <returns></returns>
        private decimal GetPlanPerformance(IEnumerable<Plan> EnumerablePlan)
        {
            decimal Percent = 0;

            if (EnumerablePlan != null && EnumerablePlan.Count() > 0)
            {
                Plan plan = EnumerablePlan.FirstOrDefault();
                decimal Amount = plan.PlanHistories.Count();
                decimal Finish = plan.PlanHistories.OfType<OperationPlan>().Where(p => p.OperationHistory != null).Count() + 
                            plan.PlanHistories.OfType<ChangePlan>().Where(p => p.AircraftBusiness != null).Count();  
                if (Amount != 0)
                {
                    Percent = Math.Round(Finish * 100 / Amount, 2);
                }

            }
            return Percent;
        }

        protected override void GenerateXmlContent(XElement RootNode)
        {
            //清空节点原有数据
            RootNode.RemoveAll();
            //当前版本的计划
            var AllPlan = GetAllPlan().ToList(); 
            //航空公司列表
            IEnumerable<string> AirLinesList = AllPlan.Select(p => p.Airlines.ShortName).Distinct();
            //开始年
            int BeginYear = AllPlan.Count() == 0 ? DateTime.Now.Year : AllPlan.Min(q => q.Annual.Year);
            //结束年
            int EndYear = AllPlan.Count() == 0 ? DateTime.Now.Year : AllPlan.Max(q => q.Annual.Year);

            //按年生成每年的数据
            for (int year = BeginYear; year <= EndYear; year++)
            {
                //时间节点
                XElement DateTimeNode = new XElement("DateTime", new XAttribute("EndOfMonth", year.ToString()));
                RootNode.Add(DateTimeNode);

                //年度计划
                var YearPlan = AllPlan.Where(p => p.Annual.Year == year);
                //航空公司节点
                XElement TypeNode = new XElement("Type", new XAttribute("TypeName", "航空公司"));
                DateTimeNode.Add(TypeNode);
                //每个航空公司节点
                foreach (var name in AirLinesList) // 航空公司
                {
                    var AirlinesPlan = YearPlan.Where(p => p.Airlines.ShortName == name);
                    decimal AirlinesPercent = GetPlanPerformance(AirlinesPlan);
                    XElement AirlinesNode = new XElement("Item", new XAttribute("Name", name), FormatDecimal(AirlinesPercent));
                    TypeNode.Add(AirlinesNode);
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
                    XElement RootNode = new XElement("PerformAnalyse");
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
                        XElement RootNode = new XElement("PerformAnalyse");
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

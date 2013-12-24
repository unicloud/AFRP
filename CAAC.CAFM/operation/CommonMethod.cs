using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using CAAC.CAFM.Converters;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using System.Linq;

namespace CAAC.CAFM.ViewModels
{
    public class CommonMethod
    {
        /// <summary>
        /// 显示弹出窗体
        /// </summary>
        /// <param name="radwindow"></param>
        public void ShowRadWindow(RadWindow radwindow)
        {
            radwindow.WindowState = WindowState.Normal;
            radwindow.Show();
        }


        /// <summary>
        /// 将字符串转换成Color的ARGB类型 
        /// </summary>
        /// <param name="colorName"></param>
        /// <returns></returns>
        public  Color GetColor(string colorName)
        {
            if (colorName.StartsWith("#"))
                colorName = colorName.Replace("#", string.Empty);
            int v = int.Parse(colorName, System.Globalization.NumberStyles.HexNumber);
            return new Color()
            {
                A = Convert.ToByte((v >> 24) & 255),
                R = Convert.ToByte((v >> 16) & 255),
                G = Convert.ToByte((v >> 8) & 255),
                B = Convert.ToByte((v >> 0) & 255)
            };
        }

        /// <summary>
        /// 获取随机的ARGB颜色
        /// </summary>
        /// <returns></returns>
        public string GetRandomColor()
        {
            List<string> colorlist = new List<string>();
            colorlist.Add("#FFE51400");
            colorlist.Add("#FF339933");
            colorlist.Add("#FF1BA1E2");
            colorlist.Add("#FFF09609");
            colorlist.Add("#FF8CBF26");
            colorlist.Add("#FF00ABA9");
            colorlist.Add("#FFFF0097");
            colorlist.Add("#FFE671B8");
            colorlist.Add("#FF996600");
            colorlist.Add("#FFA200FF");
            Guid randSeedGuid = Guid.NewGuid();
            Random rand = new Random(BitConverter.ToInt32(randSeedGuid.ToByteArray(), 0));
            return colorlist[rand.Next(10)];
        }

        /// <summary>
        ///创建运营飞机RadGridView
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
            gvColumn1.Header = "注册号";
            gvColumn1.DataMemberBinding = new System.Windows.Data.Binding("RegNumber");


            GridViewDataColumn gvColumn2 = new GridViewDataColumn();
            gvColumn2.Header = "运营权人";
            gvColumn2.DataMemberBinding = new System.Windows.Data.Binding("ThenAirlineName");

            GridViewDataColumn gvColumn3 = new GridViewDataColumn();
            gvColumn3.Header = "所有权人";
            gvColumn3.DataMemberBinding = new System.Windows.Data.Binding("ThenOwnerName");

            GridViewDataColumn gvColumn4 = new GridViewDataColumn();
            gvColumn4.Header = "制造商";
            gvColumn4.DataMemberBinding = new System.Windows.Data.Binding("AircraftType.Manufacturer.Name");

            GridViewDataColumn gvColumn5 = new GridViewDataColumn();
            gvColumn5.Header = "座级";
            gvColumn5.DataMemberBinding = new System.Windows.Data.Binding("ThenRegional");

            GridViewDataColumn gvColumn6 = new GridViewDataColumn();
            gvColumn6.Header = "机型";
            gvColumn6.DataMemberBinding = new System.Windows.Data.Binding("ThenAircraftTypeName");

            GridViewDataColumn gvColumn7 = new GridViewDataColumn();
            gvColumn7.Header = "引进方式";
            gvColumn7.DataMemberBinding = new System.Windows.Data.Binding("ThenActionName");

            GridViewDataColumn gvColumn8 = new GridViewDataColumn();
            gvColumn8.Header = "出厂日期";
            System.Windows.Data.Binding bingding8 = new System.Windows.Data.Binding("FactoryDate");
            bingding8.StringFormat = "yyyy/M/d";
            gvColumn8.DataMemberBinding = bingding8;


            GridViewDataColumn gvColumn9 = new GridViewDataColumn();
            gvColumn9.Header = "座位数(座)";
            gvColumn9.DataMemberBinding = new System.Windows.Data.Binding("ThenSeatingCapacity");


            GridViewDataColumn gvColumn10 = new GridViewDataColumn();
            gvColumn10.Header = "商载量(吨)";
            gvColumn10.DataMemberBinding = new System.Windows.Data.Binding("ThenCarryingCapacity");
            //System.Windows.Data.Binding bingding10 = new System.Windows.Data.Binding("AircraftBusinesses");
            //bingding10.Converter = new SelectedTimeConverter();
            //bingding10.ConverterParameter = "CarryingCapacity";
            //gvColumn10.DataMemberBinding = bingding10;

            SerialColumn gvColumn11 = new SerialColumn();
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("/CAAC.Infrastructure;component/Resources/CafmStyle.xaml", UriKind.Relative);
            gvColumn11.Header = "序号";
            gvColumn11.CellStyle = (Style)dictionary["style"];


            rgView.Columns.Add(gvColumn11);
            rgView.Columns.Add(gvColumn1);
            rgView.Columns.Add(gvColumn2);
            rgView.Columns.Add(gvColumn3);
            rgView.Columns.Add(gvColumn4);
            rgView.Columns.Add(gvColumn5);
            rgView.Columns.Add(gvColumn6);
            rgView.Columns.Add(gvColumn7);
            rgView.Columns.Add(gvColumn8);
            rgView.Columns.Add(gvColumn9);
            rgView.Columns.Add(gvColumn10);
            return rgView;
        }

        /// <summary>
        /// 获取指定时间点的当时飞机信息
        /// </summary>
        /// <param name="list"></param>
        /// <param name="selecttime"></param>
        /// <returns></returns>
        public List<Aircraft> GetAircraftByTime(List<Aircraft> list, DateTime selecttime)
        {
            if (list == null)
                return null;

            List<Aircraft> AircraftList = new List<Aircraft>();
            foreach (var item in list)
            {
                item.ThenSeatingCapacity = 0;
                item.ThenCarryingCapacity = 0;
                item.ThenRegional = string.Empty;
                item.ThenAircraftTypeName = string.Empty;
                item.ThenActionName = string.Empty;
                item.ThenAirlineName = string.Empty;
                item.ThenOwnerName = string.Empty;


                AircraftBusiness aircraftbusiness = item.AircraftBusinesses.FirstOrDefault(p => p.StartDate <= selecttime && !(p.EndDate != null && p.EndDate < selecttime));
                if (aircraftbusiness != null)
                {
                    //座位
                    item.ThenSeatingCapacity = aircraftbusiness.SeatingCapacity;
                    //商载
                    item.ThenCarryingCapacity = aircraftbusiness.CarryingCapacity;
                    //座级
                    item.ThenRegional = aircraftbusiness.AircraftType.AircraftCategory.Regional;
                    //机型
                    item.ThenAircraftTypeName = aircraftbusiness.AircraftType.Name;
                }

                OperationHistory operationhistory = item.OperationHistories.FirstOrDefault(p => p.StartDate <= selecttime && !(p.EndDate != null && p.EndDate < selecttime));
                if (operationhistory != null)
                {
                    //引进方式
                    item.ThenActionName = operationhistory.ImportCategory.ActionName;

                    //运营权人名称
                    if (operationhistory.SubOperationHistories != null && operationhistory.SubOperationHistories.Any(p => p.StartDate <= selecttime && !(p.EndDate != null && p.EndDate < selecttime)))
                    {
                        SubOperationHistory suboperationhistory = operationhistory.SubOperationHistories.FirstOrDefault(p => p.StartDate <= selecttime && !(p.EndDate != null && p.EndDate < selecttime));
                        item.ThenAirlineName = suboperationhistory != null ? suboperationhistory.Airlines.Name : operationhistory.Airlines.Name;
                    }
                    else
                    {
                        item.ThenAirlineName = operationhistory.Airlines.Name;
                    }
                }
                //所有权人
                OwnershipHistory ownershiphistory = item.OwnershipHistorys.FirstOrDefault(p => p.StartDate <= selecttime && !(p.EndDate != null && p.EndDate < selecttime));
                if (ownershiphistory != null)
                {
                    item.ThenOwnerName = ownershiphistory.Owner.Name;
                }

                AircraftList.Add(item);
            }
            return AircraftList;
        }


        /// <summary>
        /// 根据传入的名称创建Binding的对象
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Telerik.Windows.Controls.ChartView.PropertyNameDataPointBinding CreateBinding(string propertyName)
        {
            Telerik.Windows.Controls.ChartView.PropertyNameDataPointBinding binding = new Telerik.Windows.Controls.ChartView.PropertyNameDataPointBinding();
            binding.PropertyName = propertyName;
            return binding;
        }


        /// <summary>
        /// 根据传入的控件导出相应的图片
        /// </summary>
        /// <param name="element">导出的控件</param>
        public void ExportToImage(FrameworkElement element)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                DefaultExt = "png",
                Filter = "Png (*.png)|*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    Telerik.Windows.Media.Imaging.ExportExtensions.ExportToImage(
                         element, stream, new Telerik.Windows.Media.Imaging.PngBitmapEncoder());
                }
            }
        }
    }
}

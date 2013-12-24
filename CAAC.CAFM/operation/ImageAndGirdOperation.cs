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
using System.Collections;
using System.Collections.Generic;

namespace CAAC.CAFM.ViewModels
{
    /// <summary>
    /// 图表导出操作
    /// </summary>
    public static class ImageAndGirdOperation
    {
        /// <summary>
        /// 保存成文件流
        /// </summary>
        /// <param name="filter">保存格式</param>
        /// <returns></returns>
        public static Stream DowmLoadDialogStream(string filter)
        {
            //保存对话框
            SaveFileDialog dialog = new SaveFileDialog();
            //保存的格式
            dialog.Filter = filter;

            if (!(bool)dialog.ShowDialog())
                return null;
            //获取保存的文件流
            return dialog.OpenFile();
        }

        /// <summary>
        /// 设置导出样式
        /// </summary>
        /// <returns></returns>
        public static GridViewExportOptions SetGridViewExportOptions()
        {
            GridViewExportOptions exportOptions = new GridViewExportOptions();
            exportOptions.Format = ExportFormat.Html;
            exportOptions.Encoding = System.Text.Encoding.UTF8;
            exportOptions.ShowColumnFooters = false;
            exportOptions.ShowColumnHeaders = true;
            exportOptions.ShowGroupFooters = false;
            return exportOptions;

        }


        /// <summary>
        /// 创建RadGridView
        /// </summary>
        /// <param name="structs">RadGridView列</param>
        /// <param name="itemsSource">数据源</param>
        /// <param name="itemsSource">RadGridView名称</param>

        /// <returns></returns>
        public static RadGridView CreatDataGridView(Dictionary<string, string> structs, IEnumerable<object> itemsSource, string headername)
        {
            RadGridView rgView = new RadGridView();
            rgView.ShowGroupPanel = false;
            rgView.AutoGenerateColumns = false;
            rgView.IsReadOnly = true;
            rgView.Name = headername;
            rgView.RowIndicatorVisibility = Visibility.Collapsed;

            foreach (var item in structs.Keys)
            {
                GridViewDataColumn gvColumn = new GridViewDataColumn();
                gvColumn.Header = structs[item].ToString();
                gvColumn.IsFilterable = false;
                gvColumn.IsSortable = false;
                gvColumn.DataMemberBinding = new System.Windows.Data.Binding(item);
                rgView.Columns.Add(gvColumn);

            }
            rgView.ItemsSource = itemsSource;


            return rgView;
        }
    }
}

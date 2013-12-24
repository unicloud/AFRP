using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace UniCloud.AFRP.Operation
{
    /// <summary>
    /// 图表导出操作
    /// </summary>
    public static class ImageAndGridOperation
    {
        /// <summary>
        /// 保存成文件流
        /// </summary>
        /// <param name="filter">保存格式</param>
        /// <returns></returns>
        public static Stream DownloadDialogStream(string filter)
        {
            //保存对话框
            var dialog = new SaveFileDialog {Filter = filter};

            var showDialog = dialog.ShowDialog();
            if (showDialog != null && !(bool)showDialog)
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
            var exportOptions = new GridViewExportOptions
                {
                    Format = ExportFormat.Html,
                    Encoding = System.Text.Encoding.UTF8,
                    ShowColumnFooters = false,
                    ShowColumnHeaders = true,
                    ShowGroupFooters = false
                };
            return exportOptions;
        }

        /// <summary>
        /// 创建RadGridView
        /// </summary>
        /// <param name="structs">RadGridView列</param>
        /// <param name="itemsSource">数据源</param>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public static RadGridView CreatDataGridView(Dictionary<string, string> structs, IEnumerable<object> itemsSource, string headerName)
        {
            var rgView = new RadGridView
                {
                    ShowGroupPanel = false,
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    Name = headerName,
                    RowIndicatorVisibility = Visibility.Collapsed
                };

            foreach (var gvColumn in structs.Keys.Select(item => new GridViewDataColumn
                {
                    Header = structs[item],
                    IsFilterable = false,
                    IsSortable = false,
                    DataMemberBinding = new System.Windows.Data.Binding(item)
                }))
            {
                rgView.Columns.Add(gvColumn);
            }
            rgView.ItemsSource = itemsSource;

            return rgView;
        }

    }
}

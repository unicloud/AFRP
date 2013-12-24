using System;
using System.IO;
using System.Runtime.InteropServices.Automation;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;
using UniCloud.Security.Services;
using UniCloud.Utility.Export;

using ExportFormat = UniCloud.Utility.Export.ExportFormat;

namespace UniCloud.Fleet.Services
{
    public abstract class ViewModelBase : NotificationObject
    {

        #region ctor

        protected ViewModelBase()
        {
            this.GroupHeaderBackground = Color.FromArgb(255, 216, 216, 216);
            this.HeaderBackground = Color.FromArgb(255, 127, 127, 127);
            this.RowBackground = Color.FromArgb(255, 251, 247, 255);

            this.ViewAttachCommand = new DelegateCommand<object>(OnViewAttach);
            this.ExcelExportCommand = new DelegateCommand<object>(OnExcelExport);
            this.WordExportCommand = new DelegateCommand<object>(OnWordExport);
            this.ChartExportCommand = new DelegateCommand<object>(OnChartExport);
            this.ChartDataExportCommand = new DelegateCommand<object>(OnChartDataExport);
        }

        #endregion

        #region Local

        private const string Path = @"c:\UniCloud\AttachDoc\";
        protected readonly IFleetService service = ServiceLocator.Current.GetInstance<IFleetService>();
        protected readonly IAuthServices authService = ServiceLocator.Current.GetInstance<IAuthServices>();

        #region Method

        /// <summary>
        /// 打开附件
        /// </summary>
        /// <param name="sender">需要打开附件的对象</param>
        private static void OpenAttachment(object sender)
        {
            var filename = string.Empty;
            if (!Directory.Exists("C:\\UniCloud\\AttachDoc"))
            {
                // 文件夹不存在则创建
                Directory.CreateDirectory("C:\\UniCloud\\AttachDoc");
            }

            var buffbyte = new byte[] { };
            if (sender is Plan)
            {
                var plan = sender as Plan;
                filename = Path + Regex.Replace(plan.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = plan.AttachDoc;
            }
            else if (sender is Request)
            {
                var req = sender as Request;
                filename = Path + Regex.Replace(req.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = req.AttachDoc;
            }
            else if (sender is ApprovalDoc)
            {
                var approvalDoc = sender as ApprovalDoc;
                filename = Path + Regex.Replace(approvalDoc.ApprovalDocFileName, @"[^\w\.]", "");
                buffbyte = approvalDoc.AttachDoc;
            }

            try
            {
                var fs = new FileStream(filename, FileMode.Create);
                if (sender is Plan)
                {
                    var plan = sender as Plan;
                    fs.Write(buffbyte, 0, plan.AttachDoc.Length);
                }
                else if (sender is Request)
                {
                    var req = sender as Request;
                    fs.Write(buffbyte, 0, req.AttachDoc.Length);
                }
                else if (sender is ApprovalDoc)
                {
                    var approvalDoc = sender as ApprovalDoc;
                    fs.Write(buffbyte, 0, approvalDoc.AttachDoc.Length);
                }
                fs.Close();

                using (var shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    if (File.Exists(filename))
                    {
                        shell.Run(filename);
                    }
                }
            }
            catch (IOException)
            {
                using (var shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    if (File.Exists(filename))
                    {
                        shell.Run(filename);
                    }
                }
            }
        }

        /// <summary>
        /// 设置提醒对话框
        /// </summary>
        /// <param name="header">对话框标题</param>
        /// <param name="okContent">Ok按钮显示内容</param>
        /// <param name="content">显示内容</param>
        /// <param name="fontSize">字号</param>
        /// <param name="width">对话框宽度</param>
        /// <param name="callBack">关闭对话框后执行的操作</param>
        /// <returns>提醒对话框</returns>
        protected DialogParameters SetAlter(
            string header,
            string okContent,
            string content,
            int fontSize,
            int width,
            Action callBack)
        {
            var alter = new DialogParameters
                {
                    Header = header,
                    OkButtonContent = okContent,
                    Content = new TextBlock
                        {
                            Text = content,
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                            FontSize = fontSize,
                            TextWrapping = TextWrapping.Wrap,
                            Width = width,
                        },
                    Closed = (o, e) => callBack(),
                };
            return alter;
        }

        /// <summary>
        /// 设置确认对话框
        /// </summary>
        /// <param name="header">对话框标题</param>
        /// <param name="okContent">Ok按钮显示内容</param>
        /// <param name="cancelContent">Cancel按钮显示内容</param>
        /// <param name="content">显示内容</param>
        /// <param name="fontSize">字号</param>
        /// <param name="width">对话框宽度</param>
        /// <param name="closed">关闭对话框后执行的操作</param>
        /// <returns>确认对话框</returns>
        protected DialogParameters SetConfirm(
            string header,
            string okContent,
            string cancelContent,
            string content,
            int fontSize,
            int width,
            EventHandler<WindowClosedEventArgs> closed)
        {
            var confirm = new DialogParameters
            {
                Header = header,
                OkButtonContent = okContent,
                CancelButtonContent = cancelContent,
                Content = new TextBlock
                {
                    Text = content,
                    FontFamily = new FontFamily("Microsoft YaHei UI"),
                    FontSize = fontSize,
                    TextWrapping = TextWrapping.Wrap,
                    Width = width,
                },
                Closed = closed,
            };
            return confirm;
        }

        #endregion

        #endregion

        #region ViewModel

        #region Property

        #region 数据导出背景色

        private Color _headerBackground;
        private Color _rowBackground;
        private Color _groupHeaderBackground;

        /// <summary>
        /// 分组栏背景色
        /// </summary>
        public Color GroupHeaderBackground
        {
            get
            {
                return this._groupHeaderBackground;
            }
            set
            {
                if (this._groupHeaderBackground != value)
                {
                    this._groupHeaderBackground = value;
                    RaisePropertyChanged(() => this.GroupHeaderBackground);
                }
            }
        }

        /// <summary>
        /// 标题背景色
        /// </summary>
        public Color HeaderBackground
        {
            get
            {
                return this._headerBackground;
            }
            set
            {
                if (this._headerBackground != value)
                {
                    this._headerBackground = value;
                    RaisePropertyChanged(() => HeaderBackground);
                }
            }
        }

        /// <summary>
        /// 行背景色
        /// </summary>
        public Color RowBackground
        {
            get
            {
                return this._rowBackground;
            }
            set
            {
                if (this._rowBackground != value)
                {
                    this._rowBackground = value;
                    RaisePropertyChanged(() => RowBackground);
                }
            }
        }

        #endregion

        #endregion

        #region Command

        #region 查看附件

        public DelegateCommand<object> ViewAttachCommand { get; private set; }

        private static void OnViewAttach(object sender)
        {
            if (sender is Plan)
            {
                var plan = sender as Plan;
                OpenAttachment(plan);
            }
            if (sender is Request)
            {
                var request = sender as Request;
                OpenAttachment(request);
            }
            if (sender is ApprovalDoc)
            {
                var approvalDoc = sender as ApprovalDoc;
                OpenAttachment(approvalDoc);
            }
        }

        #endregion

        #region 导出Excel

        public DelegateCommand<object> ExcelExportCommand { get; private set; }

        private void OnExcelExport(object sender)
        {
            var grid = sender as RadGridView;
            if (grid != null)
            {
                grid.Export(new ExportSettings
                    {
                        GroupHeaderBackground = this.GroupHeaderBackground,
                        HeaderBackground = this.HeaderBackground,
                        RowBackground = this.RowBackground,
                        Format = (ExportFormat)Enum.Parse(typeof(ExportFormat), "Excel", false),
                        FileName = (string)grid.Tag
                    });
            }
        }

        #endregion

        #region 导出Word

        public DelegateCommand<object> WordExportCommand { get; private set; }

        private void OnWordExport(object sender)
        {
            var grid = sender as RadGridView;
            if (grid != null)
            {
                grid.Export(new ExportSettings
                {
                    GroupHeaderBackground = this.GroupHeaderBackground,
                    HeaderBackground = this.HeaderBackground,
                    RowBackground = this.RowBackground,
                    Format = (ExportFormat)Enum.Parse(typeof(ExportFormat), "Word", false)
                });
            }
        }

        #endregion

        #region 导出图表

        public DelegateCommand<object> ChartExportCommand { get; private set; }

        private static void OnChartExport(object sender)
        {

        }

        #endregion

        #region 图表数据导出Excel

        public DelegateCommand<object> ChartDataExportCommand { get; private set; }

        private static void OnChartDataExport(object sender)
        {

        }

        #endregion

        #endregion

        #region Method



        #endregion

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Navigation;

namespace UniCloud.UniAuth.Views
{
    [Export(typeof(ConfirmRadWindow))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public partial class ConfirmRadWindow : RadWindow
    {
        public ConfirmRadWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 创建新的子窗体
        /// </summary>
        /// <param name="text">子窗体的提示内容</param>
        /// <param name="OnClosed">子窗体的Closed事件</param>
        public static void Show(string text, EventHandler<WindowClosedEventArgs> OnClosed)
        {
            ConfirmRadWindow crw = new ConfirmRadWindow();
            crw.UserConfrimText.Text = text;
            crw.Closed += OnClosed;
            crw.ShowDialog();
        }

        /// <summary>
        /// 创建新的子窗体
        /// </summary>
        /// <param name="header">子窗体的标题</param>
        /// <param name="text">子窗体的提示内容</param>
        /// <param name="OnClosed">子窗体的Closed事件</param>
        public static void Show(string header, string text, EventHandler<WindowClosedEventArgs> OnClosed)
        {
            ConfirmRadWindow crw = new ConfirmRadWindow();
            crw.Header = header;
            crw.UserConfrimText.Text = text;
            crw.Closed += OnClosed;
            crw.ShowDialog();
        }

        /// <summary>
        /// 创建新的子窗体
        /// </summary>
        /// <param name="header">子窗体的标题</param>
        /// <param name="text">子窗体的提示内容</param>
        /// <param name="source">子窗体要显示的图片路径</param>
        /// <param name="OnClose">子窗体的Closed事件</param>
        public static void Show(string header, string text, Uri source, EventHandler<WindowClosedEventArgs> OnClosed)
        {
            ConfirmRadWindow crw = new ConfirmRadWindow();
            crw.Header = header;
            crw.Image.Source = new BitmapImage(source);
            crw.UserConfrimText.Text = text;
            crw.Closed += OnClosed;
            crw.ShowDialog();
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult =(bool?) true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = (bool?)false;
            this.Close();
        }
    }
}

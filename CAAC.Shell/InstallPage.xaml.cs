using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CAAC.Shell
{
    public partial class InstallPage : UserControl
    {
        public InstallPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(InstallPage_Loaded);
            App.Current.InstallStateChanged += new EventHandler(Current_InstallStateChanged);
        }

        void InstallPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInstallState();
        }

        void Current_InstallStateChanged(object sender, EventArgs e)
        {
            UpdateInstallState();
        }

        private void UpdateInstallState()
        {
            switch (App.Current.InstallState)
            {
                case InstallState.NotInstalled:
                    btnInstall.Visibility = Visibility.Visible;
                    tbMessage.Text = "请点击“安装”按钮安装该软件";
                    break;
                case InstallState.Installing:
                    btnInstall.Visibility = System.Windows.Visibility.Collapsed;
                    tbMessage.Text = "正在安装.......";
                    break;
                case InstallState.InstallFailed:
                    btnInstall.Visibility = Visibility.Visible;
                    tbMessage.Text = "对不起，安装失败，请重新安装";
                    tbMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    break;
                case InstallState.Installed:
                    btnInstall.Visibility = System.Windows.Visibility.Collapsed;
                    tbMessage.Text = "您已经安装了该软件,请从本地启动该软件";
                    tbMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    break;

            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.InstallState == InstallState.NotInstalled)
            {
                App.Current.Install();
            }
        }
    }
}

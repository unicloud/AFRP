using System;
using System.Windows;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Telerik.Windows.Controls;

namespace CAAC.Shell
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            StyleManager.ApplicationTheme = new Windows8Theme();
            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = CAAC.Shell.Localization.GridViewResources.ResourceManager
                 
            };
            InitializeComponent();

            if (Application.Current.InstallState == InstallState.Installed)
            {
                Application.Current.CheckAndDownloadUpdateAsync();
                Application.Current.CheckAndDownloadUpdateCompleted += new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
            }
        }
        /// <summary>
        /// 服务端发生更新，客户端也需更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            var message = new MessageOperationLibrary.Events.VersionUpdateEvent();
            if (e.UpdateAvailable && e.Error == null)
            {
                message.Message = "应用新版本已经下载成功，将在下次启动时生效。";
                MessageEventAggregatorRepository.EventAggregator.Publish<VersionUpdateEvent>(message);

            }
            else if (e.Error != null)
            {
                message.Message = "更新版本出错,请重新启动。";
                MessageEventAggregatorRepository.EventAggregator.Publish<VersionUpdateEvent>(message);
            }
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (App.Current.IsRunningOutOfBrowser && App.Current.HasElevatedPermissions)
            {
                Bootstrapper bootstrapper = new Bootstrapper();
                bootstrapper.Run();
            }
            else
            {
                this.RootVisual = new InstallPage();
            }           
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                MessageBox.Show("在"
                       + "出现以下错误信息:"
                       + Environment.NewLine
                        + Environment.NewLine
                       + e.ExceptionObject.Message);
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}

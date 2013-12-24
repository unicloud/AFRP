namespace CAAC.Fleet.MailService
{
    partial class ServiceInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.MailServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.MailServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // MailServiceInstaller
            // 
            this.MailServiceInstaller.Description = "机队邮件系统服务，接收各航空公司上报的数据";
            this.MailServiceInstaller.DisplayName = "机队邮件系统服务";
            this.MailServiceInstaller.ServiceName = "AircraftMailService";
            this.MailServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.MailServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MailServiceInstaller_AfterInstall);
            // 
            // MailServiceProcessInstaller
            // 
            this.MailServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MailServiceProcessInstaller.Password = null;
            this.MailServiceProcessInstaller.Username = null;
            // 
            // ServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MailServiceInstaller,
            this.MailServiceProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller MailServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller MailServiceProcessInstaller;
    }
}
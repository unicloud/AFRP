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
            this.XmlConfigServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.XmlConfigServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // XmlConfigServiceInstaller
            // 
            this.XmlConfigServiceInstaller.Description = "航空公司机队数据系统服务，生成统计分析相关数据";
            this.XmlConfigServiceInstaller.DisplayName = "航空公司机队数据系统服务";
            this.XmlConfigServiceInstaller.ServiceName = "东航数据服务";
            this.XmlConfigServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.XmlConfigServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.XmlConfigServiceInstaller_AfterInstall);
            // 
            // XmlConfigServiceProcessInstaller
            // 
            this.XmlConfigServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.XmlConfigServiceProcessInstaller.Password = null;
            this.XmlConfigServiceProcessInstaller.Username = null;
            // 
            // ServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.XmlConfigServiceInstaller,
            this.XmlConfigServiceProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller XmlConfigServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller XmlConfigServiceProcessInstaller;
    }
}
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;

namespace UniCloud.Shell
{
    public class Bootstrapper : MefBootstrapper
    {
        protected override void ConfigureAggregateCatalog()
        {
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Security.Services.IAuthServices).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Fleet.Services.IFleetService).Assembly));
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.RootVisual = (UIElement)this.Shell;
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.GetExportedValue<ShellView>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var moduleCatalog = new ModuleCatalog();

            moduleCatalog.AddModule(new ModuleInfo
            {
                InitializationMode = InitializationMode.OnDemand,
                Ref = "UniCloud.AFRP.xap",
                ModuleName = "AFRPModule",
                ModuleType = "UniCloud.AFRP.AFRPModule, UniCloud.AFRP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            moduleCatalog.AddModule(new ModuleInfo
            {
                InitializationMode = InitializationMode.OnDemand,
                Ref = "UniCloud.UniAuth.xap",
                ModuleName = "UniAuthModule",
                ModuleType = "UniCloud.UniAuth.UniAuthModule, UniCloud.UniAuth, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            return moduleCatalog;

        }

    }
}
using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;

namespace CAAC.Shell
{
    public class Bootstrapper : MefBootstrapper
    {
        protected override void ConfigureAggregateCatalog()
        {
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(UniCloud.Security.Services.AuthServices).Assembly));

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
            ModuleCatalog moduleCatalog = new ModuleCatalog();

            moduleCatalog.AddModule(new ModuleInfo
            {
                InitializationMode = InitializationMode.OnDemand,
                Ref = "CAAC.CAFM.xap",
                ModuleName = "CAFMModule",
                ModuleType = "CAAC.CAFM.CAFMModule, CAAC.CAFM, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
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
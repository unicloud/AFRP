using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;


namespace UniCloud.UniAuth
{
    [ModuleExport(typeof(UniAuthModule))]
    public class UniAuthModule : IModule
    {
        [Import]
        public IRegionManager regionManager;
        public void Initialize()
        {
            RegisterView();
        }
        void RegisterView()
        {
            regionManager.RegisterViewWithRegion(UniCloud.Infrastructure.RegionNames.MainRegion, typeof(Views.HomeView));
            //授权
            regionManager.RegisterViewWithRegion(UniCloud.Infrastructure.RegionNames.MainRegion, typeof(Views.RolesFunctionMtnView));
            regionManager.RegisterViewWithRegion(UniCloud.Infrastructure.RegionNames.MainRegion, typeof(Views.UserRoleAllotView));
            regionManager.RegisterViewWithRegion(UniCloud.Infrastructure.RegionNames.MainRegion, typeof(Views.UserMaintainView));
            regionManager.RegisterViewWithRegion(UniCloud.Infrastructure.RegionNames.MainRegion, typeof(Views.UserChangePwdView));
        }
    }
}

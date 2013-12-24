using CAAC.Fleet.Services;
using CAAC.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM
{
    [ModuleExport(typeof(CAFMModule))]
    public class CAFMModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();

        public void Initialize()
        {


            // 加载静态数据
            this._service.LoadOwner(new QueryBuilder<Owner>(), lo => { }, null);
            this._service.LoadAircraftCategory(new QueryBuilder<AircraftCategory>(), lo => { }, null);
            this._service.LoadAircraftType(new QueryBuilder<AircraftType>(), lo => { }, null);
            this._service.LoadActionCategory(new QueryBuilder<ActionCategory>(), lo => { }, null);
            this._service.LoadAnnual(new QueryBuilder<Annual>(), lo => { }, null);

            // 加载飞机数据
            this._service.LoadAircraft(new QueryBuilder<Aircraft>(), lo => { }, null);
            this._service.LoadPlanAircraft(new QueryBuilder<PlanAircraft>(), lo => { }, null);

            //string filePath;
            //using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            //{
            //    if (!store.DirectoryExists("CAFM"))
            //    {
            //        store.CreateDirectory("CAFM");
            //        filePath = Path.Combine("CAFM", CAFMStrings.BasicDataConfigFileName);
            //        SetConfig(filePath);
            //    }
            //    else
            //    {
            //        filePath = Path.Combine("CAFM", CAFMStrings.BasicDataConfigFileName);
            //        //if (store.FileExists(filePath))
            //        //{
            //        //    RegisterView();
            //        //}
            //        //else
            //        //{
            //        //    SetConfig(filePath);
            //        //}
            //        SetConfig(filePath);
            //    }
            //}
            RegisterView();
        }

        private void SetConfig(string filePath)
        {
            this._service.LoadXmlConfig(new QueryBuilder<XmlConfig>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    XElement doc = lo.Entities.Where(x => x.ConfigType == CAFMStrings.BasicDataConfigType).Select(x => x.XmlContent).FirstOrDefault();
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(filePath, FileMode.Create, store))
                        {
                            doc.Save(isoStream);
                        }
                    }
                    RegisterView();
                }
            }, null);
        }

        private void RegisterView()
        {

            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.HomeView));

            //// 计划管理
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPlanPnrView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPlanCargoView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPlanPublishView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPlanPerformView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPlanQueryView));
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmAdjustedPlanQueryView));
            // 批文管理
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmApprovalView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmApprovalCheckView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmRequestView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmApprovalManageView));

            // 统计分析
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetTrendAllView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetTrendPnrView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetTrendCargoView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmSupplierView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmManufacturerView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmAircraftTypeView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmImportTypeView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetQueryView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmPerformAnalyseView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetRegisteredView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmFleetAgeView));

            //// 系统管理
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmLogView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmColorSettingView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.CafmMailSettingView));


           
        }

    }
}

using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using UniCloud.Infrastructure;

namespace UniCloud.AFRP
{
    [ModuleExport(typeof(AFRPModule))]
    public class AFRPModule : IModule
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
            this._service.LoadPlanAircraft(new QueryBuilder<PlanAircraft>().Where(p => p.IsOwn), lo => { }, null);
            RegisterView();
        }

        private void RegisterView()
        {

            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.HomeView));

            // 计划编制
            //准备
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanPrepareView));
            //制定
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanLayView));
            //报送
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanSendView));
            //发布
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanPublishView));

            // 计划执行
            //申请批文
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpRequestView));
            //维护批文
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpApprovalView));
            //完成计划
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpDeliverView));

            // 飞机数据
            //变更所有权
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpAircraftOwnershipView));
            //分配飞机运力
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetAllotView));

            // 合同管理

            // 计划执行分析
            //计划
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanQueryView));
            //计划执行
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpPlanPerformView));
            //申请
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpRequestQueryView));
            //批文
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpApprovalQueryView));

            // 统计分析
            //飞机运力变化
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetTrendAllView));
            //客机运力变化
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetTrendPnrView));
            //货机运力变化
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetTrendCargoView));
            //机队结构
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetStructureView));
            //引进方式
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpImportTypeView));
            //飞机数据
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetQueryView));
            //机龄分析
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetAgeView));
            //在册飞机
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFleetRegisteredView));

            // 系统管理
            //维护邮件账号
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpMailSettingView));
            //维护基础颜色
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpColorSettingView));
            //维护供应商
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpSupplierSettingView));
            //维护分公司
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpFilialeSettingView));
            //维护子公司
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.AfrpSubsidiarySettingView));
        }
    }
}

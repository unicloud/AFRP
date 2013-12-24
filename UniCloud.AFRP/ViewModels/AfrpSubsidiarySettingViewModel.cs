using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;

namespace UniCloud.AFRP.ViewModels
{

    [Export(typeof(AfrpSubsidiarySettingViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpSubsidiarySettingViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

        #region Property

        #endregion

        #region Method

        #region 加载数据

        private void LoadCompleted()
        {
            this._loadedXmlConfig = false;

            this.RefreshView();
        }

        private bool _loadedXmlConfig;

        private void LoadXmlConfig()
        {
            this.service.LoadXmlConfig(new QueryBuilder<XmlConfig>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedXmlConfig = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedXmlConfig)
                    {
                        this.LoadCompleted();
                    }
                }
            }, null);
        }

        #endregion

        #region 管理按钮

        protected override void RefreshButtonState()
        {
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            var selOwner = this.SelOwner;
            this._needReFreshViewOwner = true;
            this.RaiseViewOwner();
            this.SelOwner = selOwner ?? this.ViewOwner.FirstOrDefault();
            var selAirlines = this.SelAirlines;
            this._needReFreshViewAirlines = true;
            this.RaiseViewAirlines();
            this.SelAirlines = selAirlines ?? this.ViewAirlines.FirstOrDefault();

            this.RefreshButtonState();
        }

        internal void ConvertToSubAirlines(Airlines airlines)
        {
            this.service.CreateSubAirlines(airlines);
            this.RefreshView();
        }

        internal void ConvertToMasterAirlines(Airlines airlines)
        {
            this.service.RemoveSubAirlines(airlines);
            this.RefreshView();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 航空公司

        /// <summary>
        /// 航空公司集合
        /// </summary>
        public IEnumerable<Airlines> ViewAirlines
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<Owner>().OfType<Airlines>().Where(a => a.MasterID == null);
            }
        }

        private bool _needReFreshViewAirlines = true;

        private void RaiseViewAirlines()
        {
            if (this._needReFreshViewAirlines)
            {
                RaisePropertyChanged(() => this.ViewAirlines);
                this._needReFreshViewAirlines = false;
            }
        }

        private Airlines _selAirlines;

        /// <summary>
        /// 选中的航空公司
        /// </summary>
        public Airlines SelAirlines
        {
            get { return this._selAirlines; }
            set
            {
                if (this._selAirlines != value)
                {
                    this._selAirlines = value;
                    this.RaisePropertyChanged(() => this.SelAirlines);
                }
            }
        }

        #endregion

        #region 子公司

        /// <summary>
        /// 子公司集合
        /// </summary>
        public IEnumerable<Airlines> ViewOwner
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<Owner>()
                        .OfType<Airlines>()
                        .Where(a => a.SubType == 1 && a.MasterID != null);
            }
        }

        private bool _needReFreshViewOwner = true;

        private void RaiseViewOwner()
        {
            if (this._needReFreshViewOwner)
            {
                RaisePropertyChanged(() => this.ViewOwner);
                this._needReFreshViewOwner = false;
            }
        }

        private Airlines _selOwner;

        /// <summary>
        /// 选中的子公司
        /// </summary>
        public Airlines SelOwner
        {
            get { return this._selOwner; }
            set
            {
                if (this._selOwner != value)
                {
                    this._selOwner = value;
                    this.RaisePropertyChanged(() => this.SelOwner);
                }
            }
        }

        #endregion

        #endregion

        #region Property

        #endregion

        #region Command

        #region 重载命令

        #region 实体变更

        protected override void OnEntityHasChanges()
        {
            this.RefreshButtonState();
        }

        #endregion

        #region 保存

        #endregion

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
            this.RefreshView();
        }

        #endregion

        #region 确认

        #endregion

        #region 取消

        #endregion

        #region 添加附件

        #endregion

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {

        }

        #endregion

        #region INavigationAware Members

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadXmlConfig();
        }

        #endregion

    }
}

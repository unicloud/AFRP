using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;

namespace UniCloud.AFRP.ViewModels
{

    [Export(typeof(AfrpSupplierSettingViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpSupplierSettingViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {
        
        #region Local

        [Import]
        public IRegionManager regionManager;

        private IDictionary<string, bool> _viewButtonValidity;

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
            this.AddCommand.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            var selOwner = this.SelOwner;
            this._needReFreshViewOwner = true;
            this.RaiseViewOwner();
            this.SelOwner = selOwner ?? this.ViewOwner.FirstOrDefault();
            this.RefreshButtonState();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 供应商

        /// <summary>
        /// 供应商集合
        /// </summary>
        public IEnumerable<Owner> ViewOwner
        {
            get { return this.service.EntityContainer.GetEntitySet<Owner>().Where(o => o.SupplierType != 0 && o.IsValid); }
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

        private Owner _selOwner;

        /// <summary>
        /// 选中的供应商
        /// </summary>
        public Owner SelOwner
        {
            get { return this._selOwner; }
            set
            {
                if (this._selOwner != value)
                {
                    this._selOwner = value;
                    this.RaisePropertyChanged(() => this.SelOwner);
                    this.RefreshButtonState();
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

        #region 增加供应商

        /// <summary>
        /// 增加供应商
        /// </summary>
        public DelegateCommand<object> AddCommand { get; private set; }

        private void OnAdd(object obj)
        {
            var supplier = this.service.CreateSupplier();
            this._needReFreshViewOwner = true;
            this.RaiseViewOwner();
            this.SelOwner = supplier;
        }

        private bool CanAdd(object obj)
        {
            if (!this._viewButtonValidity["增加供应商"]) return false;
            // 无未保存内容时，按钮可用
            return !this.service.EntityContainer.HasChanges;
        }

        #endregion

        #region 移除供应商

        /// <summary>
        /// 移除供应商
        /// </summary>
        public DelegateCommand<object> RemoveCommand { get; private set; }

        private void OnRemove(object obj)
        {
            this.service.RemoveSupplier(this.SelOwner);
            this.RefreshView();
            this.SelOwner = this.ViewOwner.FirstOrDefault();
        }

        private bool CanRemove(object obj)
        {
            if (!this._viewButtonValidity["移除供应商"]) return false;
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中供应商未发生过业务的，按钮可用
            return this.SelOwner != null && !this.SelOwner.OwnershipHistorys.Any();
        }

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.AddCommand = new DelegateCommand<object>(this.OnAdd, this.CanAdd);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpSupplierSettingView).ToString());
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

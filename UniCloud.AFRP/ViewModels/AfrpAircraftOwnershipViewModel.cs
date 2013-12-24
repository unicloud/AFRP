using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpAircraftOwnershipViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpAircraftOwnershipViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
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
            this._loadedOwnershipHistory = false;
            this._loadedAircraft = false;
            this._loadedOperationHistory = false;
            this._loadedAircraftBusiness = false;
            this.RefreshView();
        }

        private bool _loadedOwnershipHistory;

        private void LoadOwnershipHistory()
        {
            this.service.LoadOwnershipHistory(new QueryBuilder<OwnershipHistory>(), lo =>
                {
                    if (lo.Error == null)
                    {
                        this._loadedOwnershipHistory = true;
                        // 判断是否所有加载操作都完成
                        if (this._loadedOwnershipHistory && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        private bool _loadedAircraft;

        private void LoadAircraft()
        {
            this.service.LoadAircraft(new QueryBuilder<Aircraft>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedAircraft = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedOwnershipHistory && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                    {
                        this.LoadCompleted();
                    }
                }
            }, null);
        }

        private bool _loadedOperationHistory;

        private void LoadOperationHistory()
        {
            this.service.LoadOperationHistory(new QueryBuilder<OperationHistory>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedOperationHistory = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedOwnershipHistory && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                    {
                        this.LoadCompleted();
                    }
                }
            }, null);
        }

        private bool _loadedAircraftBusiness;

        private void LoadAircraftBusiness()
        {
            this.service.LoadAircraftBusiness(new QueryBuilder<AircraftBusiness>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedAircraftBusiness = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedOwnershipHistory && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
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
            this.NewCommand.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
            this.CommitCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
            this.SendCommand.RaiseCanExecuteChanged();
            this.EditCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            var selAircraft = this.SelAircraft;
            this._needReFreshViewAircraft = true;
            this.RaiseViewAircraft();
            this.SelAircraft = selAircraft ?? this.ViewAircraft.FirstOrDefault();
            this.RefreshButtonState();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 所有权历史

        /// <summary>
        /// 所有权历史集合
        /// </summary>
        public IEnumerable<OwnershipHistory> ViewOwnershipHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<OwnershipHistory>()
                        .OrderBy(oh => oh.StartDate)
                        .Where(os => os.Aircraft == this.SelAircraft);
            }
        }

        private bool _needReFreshViewOwnershipHistory = true;

        private void RaiseViewOwnershipHistory()
        {
            if (this._needReFreshViewOwnershipHistory)
            {
                RaisePropertyChanged(() => this.ViewOwnershipHistory);
                this._needReFreshViewOwnershipHistory = false;
            }
        }

        private OwnershipHistory _selOwnershipHistory;

        /// <summary>
        /// 选中的所有权历史
        /// </summary>
        public OwnershipHistory SelOwnershipHistory
        {
            get { return this._selOwnershipHistory; }
            set
            {
                if (this._selOwnershipHistory != value)
                {
                    this._selOwnershipHistory = value;
                    this.RaisePropertyChanged(() => this.SelOwnershipHistory);
                    this.RefreshButtonState();
                }
            }
        }

        #endregion

        #region 运营飞机

        /// <summary>
        /// 运营飞机集合
        /// </summary>
        public IEnumerable<Aircraft> ViewAircraft
        {
            get { return this.service.EntityContainer.GetEntitySet<Aircraft>().Where(a => a.IsOperation); }
        }

        private bool _needReFreshViewAircraft = true;

        private void RaiseViewAircraft()
        {
            if (this._needReFreshViewAircraft)
            {
                RaisePropertyChanged(() => this.ViewAircraft);
                this._needReFreshViewAircraft = false;
            }
        }

        private Aircraft _selAircraft;

        /// <summary>
        /// 选中的运营飞机
        /// </summary>
        public Aircraft SelAircraft
        {
            get { return this._selAircraft; }
            set
            {
                if (this._selAircraft != value)
                {
                    this._selAircraft = value;
                    this.RaisePropertyChanged(() => this.SelAircraft);
                    if (this._selAircraft != null)
                    {
                        this._needReFreshViewOwnershipHistory = true;
                        this.RaiseViewOwnershipHistory();
                        this._needReFreshViewOperationHistory = true;
                        this.RaiseViewOperationHistory();
                        this._needReFreshViewAircraftBusiness = true;
                        this.RaiseViewAircraftBusiness();
                        RaisePropertyChanged(() => this.OwnershipTitle);
                        RaisePropertyChanged(() => this.OperationTitle);
                        RaisePropertyChanged(() => this.AircraftBusinessTitle);
                        this.SelOwnershipHistory = this.ViewOwnershipHistory.LastOrDefault();
                    }
                }
            }
        }

        #endregion

        #region 飞机运营历史

        /// <summary>
        /// 飞机运营历史集合
        /// </summary>
        public IEnumerable<OperationHistory> ViewOperationHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<OperationHistory>()
                        .OrderBy(oh => oh.StartDate)
                        .Where(oh => oh.Aircraft == this.SelAircraft);
            }
        }

        private bool _needReFreshViewOperationHistory = true;

        private void RaiseViewOperationHistory()
        {
            if (this._needReFreshViewOperationHistory)
            {
                RaisePropertyChanged(() => this.ViewOperationHistory);
                this._needReFreshViewOperationHistory = false;
            }
        }

        private OperationHistory _selOperationHistory;

        /// <summary>
        /// 选中的飞机运营历史
        /// </summary>
        public OperationHistory SelOperationHistory
        {
            get { return this._selOperationHistory; }
            set
            {
                if (this._selOperationHistory != value)
                {
                    this._selOperationHistory = value;
                    this.RaisePropertyChanged(() => this.SelOperationHistory);
                }
            }
        }

        #endregion

        #region 飞机商业数据历史

        /// <summary>
        /// 飞机商业数据历史集合
        /// </summary>
        public IEnumerable<AircraftBusiness> ViewAircraftBusiness
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<AircraftBusiness>()
                        .OrderBy(ab => ab.StartDate)
                        .Where(ab => ab.Aircraft == this.SelAircraft);
            }
        }

        private bool _needReFreshViewAircraftBusiness = true;

        private void RaiseViewAircraftBusiness()
        {
            if (this._needReFreshViewAircraftBusiness)
            {
                RaisePropertyChanged(() => this.ViewAircraftBusiness);
                this._needReFreshViewAircraftBusiness = false;
            }
        }

        private AircraftBusiness _selAircraftBusiness;

        /// <summary>
        /// 选中的飞机商业数据历史
        /// </summary>
        public AircraftBusiness SelAircraftBusiness
        {
            get { return this._selAircraftBusiness; }
            set
            {
                if (this._selAircraftBusiness != value)
                {
                    this._selAircraftBusiness = value;
                    this.RaisePropertyChanged(() => this.SelAircraftBusiness);
                }
            }
        }

        #endregion

        #endregion

        #region Property

        #region 所有权历史

        /// <summary>
        /// 所有权历史
        /// </summary>
        public string OwnershipTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "所有权历史";
                }
                return "飞机所有权历史";
            }
        }

        #endregion

        #region 运营历史

        /// <summary>
        /// 运营历史
        /// </summary>
        public string OperationTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "运营历史";
                }
                return "飞机运营历史";
            }
        }

        #endregion

        #region 商业数据历史

        public string AircraftBusinessTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "商业数据历史";
                }
                return "飞机商业数据历史";
            }
        }

        #endregion

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
            this._needReFreshViewOwnershipHistory = true;
            this.RaiseViewOwnershipHistory();
            this.SelOwnershipHistory = this.ViewOwnershipHistory.LastOrDefault();
        }

        #endregion

        #region 确认

        #endregion

        #region 取消

        #endregion

        #region 添加附件

        #endregion

        #endregion

        #region 创建新所有权

        /// <summary>
        /// 创建新所有权
        /// </summary>
        public DelegateCommand<object> NewCommand { get; private set; }

        private void OnNew(object obj)
        {
            var ownership = this.service.CreateNewOwnership(this.SelAircraft);
            this._needReFreshViewOwnershipHistory = true;
            this.RaiseViewOwnershipHistory();
            this.SelOwnershipHistory = ownership;
        }

        private bool CanNew(object obj)
        {
            if (!this._viewButtonValidity["创建新所有权"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) { return false; }
            // 选中飞机非空，且该飞机所有的所有权记录都已提交时，按钮可用
            return this.SelAircraft != null && this.SelAircraft.OwnershipHistorys.All(os => os.Status == (int)OpStatus.Submited);
        }

        #endregion

        #region 移除所有权

        /// <summary>
        /// 移除所有权
        /// </summary>
        public DelegateCommand<object> RemoveCommand { get; private set; }

        private void OnRemove(object obj)
        {
            this.service.RemoveOwnership(this.SelOwnershipHistory);
            this._needReFreshViewOwnershipHistory = true;
            this.RaiseViewOwnershipHistory();
            this.SelOwnershipHistory = this.ViewOwnershipHistory.LastOrDefault();
        }

        private bool CanRemove(object obj)
        {
            if (!this._viewButtonValidity["移除所有权"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中所有权为空时，按钮不可用
            if (this.SelOwnershipHistory == null) return false;
            // 选中所有权还没有提交的，按钮可用
            return this.SelOwnershipHistory.Status < (int)OpStatus.Submited;
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            this.SelOwnershipHistory.Status = (int)OpStatus.Checking;
            this.RefreshButtonState();
        }

        private bool CanCommit(object obj)
        {
            if (!this._viewButtonValidity["提交审核"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中所有权为空时，按钮不可用
            if (this.SelOwnershipHistory == null) return false;
            // 选中所有权的所有权人或者开始日期为空时，按钮不可用
            if (this.SelOwnershipHistory.StartDate == null || this.SelOwnershipHistory.Owner == null) return false;
            // 选中所有权的状态为草稿时，按钮可用
            return this.SelOwnershipHistory.Status == (int)OpStatus.Draft;
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            this.SelOwnershipHistory.Status = (int)OpStatus.Checked;
            this.RefreshButtonState();
        }

        private bool CanExamine(object obj)
        {
            if (!this._viewButtonValidity["审核"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中所有权为空时，按钮不可用
            if (this.SelOwnershipHistory == null) return false;
            // 选中所有权的所有权人或者开始日期为空时，按钮不可用
            if (this.SelOwnershipHistory.StartDate == null || this.SelOwnershipHistory.Owner == null) return false;
            // 选中所有权的状态为审核时，按钮可用
            return this.SelOwnershipHistory.Status == (int)OpStatus.Checking;
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            var content = "是否向【民航局】报送" + this.SelOwnershipHistory.Aircraft.RegNumber + "所有权记录？";
            RadWindow.Confirm(this.SetConfirm("确认报送所有权记录", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                    if (this.SelOwnershipHistory != null && this.SelOwnershipHistory.Status != (int)OpStatus.Submited)
                    {
                        this.SelOwnershipHistory.Status = (int)OpStatus.Submited;
                    }
                    this.service.SubmitChanges(sc =>
                    {
                        if (sc.Error == null)
                        {
                            // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                            this.service.TransferOwnershipHistory(this.SelOwnershipHistory.OwnershipHistoryID, tp => { }, null);
                            RefreshButtonState();
                        }
                    }, null);
                }
            }));
        }

        private bool CanSend(object obj)
        {
            if (!this._viewButtonValidity["发送"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中所有权为空时，按钮不可用
            if (this.SelOwnershipHistory == null) return false;
            // 选中所有权的状态为已审核或已提交时，按钮可用
            return this.SelOwnershipHistory.Status == (int)OpStatus.Checked ||
                   this.SelOwnershipHistory.Status == (int)OpStatus.Submited;
        }

        #endregion

        #region 修改所有权

        /// <summary>
        /// 修改所有权
        /// </summary>
        public DelegateCommand<object> EditCommand { get; private set; }

        private void OnEdit(object obj)
        {
            const string content = "确认后所有权记录的状态将改为草稿并允许编辑，是否要对该所有权记录进行修改？";
            RadWindow.Confirm(this.SetConfirm("确认修改所有权记录", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    this.SelOwnershipHistory.Status = (int)OpStatus.Draft;
                    this.service.SubmitChanges(sc => { }, null);
                    RefreshButtonState();
                }
            }));
        }

        private bool CanEdit(object obj)
        {
            if (!this._viewButtonValidity["修改所有权"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中所有权为空时，按钮不可用
            if (this.SelOwnershipHistory == null) return false;
            // 选中所有权的状态不是草稿，且是最后一条所有权记录时，按钮可用
            return this.SelOwnershipHistory.Status != (int)OpStatus.Draft && this.SelOwnershipHistory.EndDate == null;
        }

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.NewCommand = new DelegateCommand<object>(this.OnNew, this.CanNew);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);
            this.SendCommand = new DelegateCommand<object>(this.OnSend, this.CanSend);
            this.EditCommand = new DelegateCommand<object>(this.OnEdit, this.CanEdit);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpAircraftOwnershipView).ToString());
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
            LoadOwnershipHistory();
            LoadAircraft();
            LoadOperationHistory();
            LoadAircraftBusiness();
        }

        #endregion

    }
}

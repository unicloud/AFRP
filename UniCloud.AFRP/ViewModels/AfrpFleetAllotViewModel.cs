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
    [Export(typeof(AfrpFleetAllotViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpFleetAllotViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
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
            this._loadedAircraft = false;
            this._loadedOperationHistory = false;

            this.RefreshView();
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
                        if (this._loadedAircraft && this._loadedOperationHistory)
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
                        if (this._loadedAircraft && this._loadedOperationHistory)
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
            this.CommitCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
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
                    this._needReFreshViewOperationHistory = true;
                    this.RaiseViewOperationHistory();
                    this.SelOperationHistory = this.ViewOperationHistory.LastOrDefault();
                    RaisePropertyChanged(() => this.OperationTitle);
                    RaisePropertyChanged(() => this.SubOperationTitle);
                }
            }
        }

        #endregion

        #region 运营历史

        /// <summary>
        /// 运营历史集合
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
        /// 选中的运营历史
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
                    this._needReFreshViewSubOperationHistory = true;
                    this.RaiseViewSubOperationHistory();
                    this.SelSubOperationHistory = this.ViewSubOperationHistory.LastOrDefault();
                }
            }
        }

        #endregion

        #region 子运营历史

        /// <summary>
        /// 子运营历史集合
        /// </summary>
        public IEnumerable<SubOperationHistory> ViewSubOperationHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<SubOperationHistory>()
                        .OrderBy(soh => soh.StartDate)
                        .Where(soh => soh.OperationHistory == this.SelOperationHistory);
            }
        }

        private bool _needReFreshViewSubOperationHistory = true;

        private void RaiseViewSubOperationHistory()
        {
            if (this._needReFreshViewSubOperationHistory)
            {
                RaisePropertyChanged(() => this.ViewSubOperationHistory);
                this._needReFreshViewSubOperationHistory = false;
            }
        }

        private SubOperationHistory _selSubOperationHistory;

        /// <summary>
        /// 选中的子运营历史
        /// </summary>
        public SubOperationHistory SelSubOperationHistory
        {
            get { return this._selSubOperationHistory; }
            set
            {
                if (this._selSubOperationHistory != value)
                {
                    this._selSubOperationHistory = value;
                    this.RaisePropertyChanged(() => this.SelSubOperationHistory);
                }
            }
        }

        #endregion

        #endregion

        #region Property

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

        #region 运力分配历史

        /// <summary>
        /// 运力分配历史
        /// </summary>
        public string SubOperationTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "运力分配历史";
                }
                return "飞机运力分配历史";
            }
        }

        #endregion

        #region 无分公司时隐藏运力分配

        private bool _hideSubOperation;

        /// <summary>
        /// 无分公司时隐藏运力分配
        /// </summary>
        public bool HideSubOperation
        {
            get { return this._hideSubOperation; }
            private set
            {
                if (this._hideSubOperation != value)
                {
                    this._hideSubOperation = value;
                    this.RaisePropertyChanged(() => this.HideSubOperation);
                }
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
            this._needReFreshViewSubOperationHistory = true;
            this.RaiseViewSubOperationHistory();
        }

        #endregion

        #region 确认

        #endregion

        #region 取消

        #endregion

        #region 添加附件

        #endregion

        #endregion

        #region 创建新运力分配

        /// <summary>
        /// 创建新运力分配
        /// </summary>
        public DelegateCommand<object> NewCommand { get; private set; }

        private void OnNew(object obj)
        {
            var subOperation = this.service.CreateNewSubOperation(this.SelOperationHistory);
            this._needReFreshViewSubOperationHistory = true;
            this.RaiseViewSubOperationHistory();
            this.SelSubOperationHistory = subOperation;
        }

        private bool CanNew(object obj)
        {
            if (!this._viewButtonValidity["创建新运力分配"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中的运营历史为空时，按钮不可用
            if (this.SelOperationHistory == null) return false;
            // 当前航空公司存在分公司，且所有运力分配记录已审核时，按钮可用
            return this.service.CurrentAirlines.SubAirlines.Any(sa => sa.SubType == 0) &&
                   this.SelOperationHistory.SubOperationHistories.All(soh => soh.Status == (int)OpStatus.Checked);
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            this.SelSubOperationHistory.Status = (int)OpStatus.Checking;
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
            // 选中的运力分配为空时，按钮不可用
            if (this.SelSubOperationHistory == null) return false;
            // 选中运力分配记录的分公司或者开始日期为空时，按钮不可用
            if (this.SelSubOperationHistory.StartDate == null || this.SelSubOperationHistory.Airlines == null) return false;
            // 选中运力分配的状态为草稿时，按钮可用
            return this.SelSubOperationHistory.Status == (int)OpStatus.Draft;
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            this.SelSubOperationHistory.Status = (int)OpStatus.Checked;
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
            // 选中的运力分配为空时，按钮不可用
            if (this.SelSubOperationHistory == null) return false;
            // 选中运力分配记录的分公司或者开始日期为空时，按钮不可用
            if (this.SelSubOperationHistory.StartDate == null || this.SelSubOperationHistory.Airlines == null) return false;
            // 选中运力分配的状态为审核时，按钮可用
            return this.SelSubOperationHistory.Status == (int)OpStatus.Checking;
        }

        #endregion

        #region 修改运力分配

        /// <summary>
        /// 修改运力分配
        /// </summary>
        public DelegateCommand<object> EditCommand { get; private set; }

        private void OnEdit(object obj)
        {
            const string content = "确认后运力分配记录的状态将改为草稿并允许编辑，是否要对该运力分配记录进行修改？";
            RadWindow.Confirm(this.SetConfirm("确认修改运力分配记录", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    this.SelSubOperationHistory.Status = (int)OpStatus.Draft;
                    this.service.SubmitChanges(sc => { }, null);
                    RefreshButtonState();
                }
            }));
        }

        private bool CanEdit(object obj)
        {
            if (!this._viewButtonValidity["修改运力分配"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中的运力分配为空时，按钮不可用
            if (this.SelSubOperationHistory == null) return false;
            // 选中运力的状态不是草稿，且是最后一条运力分配记录时，按钮可用
            return this.SelSubOperationHistory.Status != (int)OpStatus.Draft && this.SelSubOperationHistory.EndDate == null;
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
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);
            this.EditCommand = new DelegateCommand<object>(this.OnEdit, this.CanEdit);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpFleetAllotView).ToString());
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
            LoadAircraft();
            LoadOperationHistory();
        }

        #endregion

    }
}

using System.IO;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.Services
{
    public abstract class EditViewModelBase : ViewModelBase
    {

        #region ctor

        protected EditViewModelBase()
        {
            SaveCommand = new DelegateCommand<object>(OnSave, CanSave);
            AbortCommand = new DelegateCommand<object>(OnAbort, CanAbort);
            AttachCommand = new DelegateCommand<object>(OnAttach, CanAttach);
            OkCommand = new DelegateCommand<object>(OnOk, CanOk);
            CancelCommand = new DelegateCommand<object>(OnCancel, CanCancel);

            service.EntityContainer.PropertyChanged += EntityContainer_PropertyChanged;
        }


        #endregion

        #region Local

        #region Method

        private void EntityContainer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasChanges")
            {
                SaveCommand.RaiseCanExecuteChanged();
                AbortCommand.RaiseCanExecuteChanged();
                this.OnEntityHasChanges();
            }
        }

        /// <summary>
        /// 实体发生变化时调用的操作
        /// </summary>
        protected virtual void OnEntityHasChanges()
        {

        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="sender">需要添加附件的对象</param>
        private static void UploadAttachment(object sender)
        {
            var ofd = new OpenFileDialog { Multiselect = false, Filter = "可用文件|*.docx;*.doc;*.xls;*.xlsx;*.pdf" };
            if (ofd.ShowDialog() == true)
            {
                Stream fs = ofd.File.OpenRead();
                var buffbyte = new byte[fs.Length];
                fs.Read(buffbyte, 0, (int)fs.Length);
                fs.Close();
                if (sender is Plan)
                {
                    var plan = sender as Plan;
                    plan.AttachDoc = buffbyte;
                    plan.AttachDocFileName = ofd.File.Name;
                }
                else if (sender is Request)
                {
                    var req = sender as Request;
                    req.AttachDoc = buffbyte;
                    req.AttachDocFileName = ofd.File.Name;
                }
                else if (sender is ApprovalDoc)
                {
                    var approvalDoc = sender as ApprovalDoc;
                    approvalDoc.AttachDoc = buffbyte;
                    approvalDoc.ApprovalDocFileName = ofd.File.Name;
                }
            }
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        protected abstract void RefreshButtonState();

        /// <summary>
        /// 通知属性更改
        /// </summary>
        /// <param name="property">发生更改的属性</param>
        public void NotifyChange(string property)
        {
            RaisePropertyChanged(property);
        }

        #endregion

        #endregion

        #region ViewModel

        #region Property

        #endregion

        #region Command

        #region 保存

        public DelegateCommand<object> SaveCommand { get; private set; }

        private void OnSave(object sender)
        {
            service.SubmitChanges(sm =>
                {
                    if (sm.Error == null)
                    {
                        OnSaveSuccess(sender);
                        RefreshButtonState();
                    }
                    else
                    {
                        OnSaveFail(sender);
                    }
                }, null);
        }

        /// <summary>
        /// 保存成功后执行的操作
        /// </summary>
        /// <param name="sender"></param>
        protected virtual void OnSaveSuccess(object sender)
        {

        }

        /// <summary>
        /// 保存失败后执行的操作
        /// </summary>
        /// <param name="sender"></param>
        protected virtual void OnSaveFail(object sender)
        {

        }

        private bool CanSave(object sender)
        {
            return service.EntityContainer.HasChanges;
        }

        #endregion

        #region 放弃更改

        public DelegateCommand<object> AbortCommand { get; private set; }

        private void OnAbort(object sender)
        {
            service.RejectChanges();
            OnAbortExecuted(sender);
            RefreshButtonState();
        }

        /// <summary>
        /// 放弃更改后执行的操作
        /// </summary>
        /// <param name="sender"></param>
        protected virtual void OnAbortExecuted(object sender)
        {

        }

        private bool CanAbort(object sender)
        {
            return service.EntityContainer.HasChanges;
        }

        #endregion

        #region 添加附件

        public DelegateCommand<object> AttachCommand { get; private set; }

        private static void OnAttach(object sender)
        {
            if (sender is Plan)
            {
                UploadAttachment(sender);
            }
            else if (sender is Request)
            {
                UploadAttachment(sender);
            }
            else if (sender is ApprovalDoc)
            {
                UploadAttachment(sender);
            }
        }

        protected virtual bool CanAttach(object sender)
        {
            return true;
        }

        #endregion

        #region 确认

        /// <summary>
        /// 确认
        /// </summary>
        public DelegateCommand<object> OkCommand { get; private set; }

        private void OnOk(object obj)
        {
            OnOkExecute(obj);
        }

        protected virtual bool CanOk(object obj)
        {
            return true;
        }

        protected virtual void OnOkExecute(object sender)
        {
        }

        #endregion

        #region 取消

        /// <summary>
        /// 取消
        /// </summary>
        public DelegateCommand<object> CancelCommand { get; private set; }

        private void OnCancel(object obj)
        {
            OnCancelExecute(obj);
        }

        protected virtual bool CanCancel(object obj)
        {
            return true;
        }

        protected virtual void OnCancelExecute(object sender)
        {
        }

        #endregion

        #endregion

        #region Method

        public void GridViewSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var addedItem = e.AddedItems.FirstOrDefault();
            if (gridView != null && addedItem != null) gridView.ScrollIntoView(e.AddedItems[0]);
        }

        #endregion

        #endregion

    }
}

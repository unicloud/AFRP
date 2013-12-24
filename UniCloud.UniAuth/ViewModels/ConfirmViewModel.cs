using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.ComponentModel;
using System.Linq.Expressions;
using System;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Commands;
using Telerik.Windows.Controls;
using System.Windows.Controls;

namespace UniCloud.UniAuth.ViewModels
{

    [Export(typeof(ConfirmViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public class ConfirmViewModel : Notification, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ConfirmViewModel()
        {
            this.OkCommand = new DelegateCommand<object>(this.OnOk);
            this.CancelCommand = new DelegateCommand<object>(this.OnCancel);
        }

        #region ViewModel Property

        #region ViewModel 属性 Result

        private bool _result;
        public bool Result
        {
            get { return this._result; }
            private set
            {
                if (this._result != value)
                {
                    this._result = value;
                    this.RaisePropertyChanged(() => this.Result);
                }
            }
        }

        #endregion

        #region ViewModel 属性 ConfirmTitle

        private string _confirmTitle;
        public string ConfirmTitle
        {
            get { return this._confirmTitle; }
            set
            {
                if (this._confirmTitle != value)
                {
                    this._confirmTitle = value;
                    this.RaisePropertyChanged(() => this.ConfirmTitle);
                }
            }
        }

        #endregion

        #region ViewModel 属性 ConfirmContent

        private string _confirmContent;
        public string ConfirmContent
        {
            get { return this._confirmContent; }
            set
            {
                if (this._confirmContent != value)
                {
                    this._confirmContent = value;
                    this.RaisePropertyChanged(() => this.ConfirmContent);
                }
            }
        }

        #endregion

        #endregion

        #region Command

        #region ViewModel 命令 -- 确认

        public DelegateCommand<object> OkCommand { get; private set; }
        private void OnOk(object sender)
        {
            ChildWindow cw = sender as ChildWindow;
            Result = true;
            cw.DialogResult = false;
          
        }

        #endregion

        #region ViewModel 命令 -- 取消

        public DelegateCommand<object> CancelCommand { get; private set; }
        private void OnCancel(object sender)
        {
            ChildWindow cw = sender as ChildWindow;
            Result = false;
            cw.DialogResult = true;
          

        }

        #endregion

        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaisePropertyChanged<T>(Expression<Func<T>> lambda)
        {
            var name = PropertySupport.ExtractPropertyName<T>(lambda);
            OnPropertyChanged(name);
        }


    }

}

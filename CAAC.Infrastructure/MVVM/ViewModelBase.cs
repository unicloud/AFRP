using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.ViewModel;

namespace CAAC.Infrastructure.MVVM
{
    public class ViewModelBase : NotificationObject, INotifyDataErrorInfo
    {
        private Dictionary<string, List<ValidationErrorInfo>> _errors = new Dictionary<string, List<ValidationErrorInfo>>();

        #region INotifyDataErrorInfo Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (!_errors.ContainsKey(propertyName))
                return _errors.Values;
            return _errors[propertyName];
        }

        public bool HasErrors
        {
            get { return this._errors.Count > 0; }
        }

        #endregion

        protected void RemoveErrorFromPropertyAndNotifyErrorChanges(string propertyName, int errorCode)
        {
            if (_errors.ContainsKey(propertyName))
            {
                RemoveErrorFromPropertyIfErrorCodeAlreadyExist(propertyName, errorCode);
                NotifyErrorsChanged(propertyName);
            }
        }

        private void RemoveErrorFromPropertyIfErrorCodeAlreadyExist(string propertyName, int errorCode)
        {
            if (_errors.ContainsKey(propertyName))
            {
                var errorToRemove = _errors[propertyName].SingleOrDefault(error => error.ErrorCode == errorCode);

                if (errorToRemove != null)
                {
                    _errors[propertyName].Remove(errorToRemove); if (_errors[propertyName].Count == 0) _errors.Remove(propertyName);
                }
            }
        }

        protected void AddErrorToPropertyAndNotifyErrorChanges(string propertyName, ValidationErrorInfo errorInfo)
        {
            RemoveErrorFromPropertyIfErrorCodeAlreadyExist(propertyName, errorInfo.ErrorCode); if (!_errors.ContainsKey(propertyName))
                _errors.Add(propertyName, new List<ValidationErrorInfo>());

            _errors[propertyName].Add(errorInfo);

            NotifyErrorsChanged(propertyName);
        }

        private void NotifyErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

    }
}

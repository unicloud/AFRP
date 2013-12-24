using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageOperationLibrary
{
    /// <summary>
    /// 垃圾回收机制
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        private bool _isDisposed;
        protected DisposableObject()
        {

        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            finally
            {
                _isDisposed = true;
            }

        }
        protected abstract void Dispose(bool disposing);


    }
}

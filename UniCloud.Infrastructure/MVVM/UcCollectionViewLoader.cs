using System;
using System.ComponentModel;
using System.ServiceModel.DomainServices.Client;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;

namespace UniCloud.Infrastructure.MVVM
{
    public class UcCollectionViewLoader<TEntity> : CollectionViewLoader where TEntity : Entity
    {
        private readonly Action<Action<ServiceLoadResult<TEntity>>, object> _load;
        private readonly Action<object> _cancelLoad;
        private readonly Action<ServiceLoadResult<TEntity>> _onLoadCompleted;

        private object _currentUserState;

        public UcCollectionViewLoader(Action<Action<ServiceLoadResult<TEntity>>, object> load, Action<object> cancelLoad, Action<ServiceLoadResult<TEntity>> onLoadCompleted)
        {
            if (load == null)
            {
                throw new ArgumentNullException("load");
            }
            if (onLoadCompleted == null)
            {
                throw new ArgumentNullException("onLoadCompleted");
            }

            this._load = load;
            this._cancelLoad = cancelLoad;
            this._onLoadCompleted = onLoadCompleted;
        }

        public override bool CanLoad
        {
            get { return true; }
        }

        private object CurrentUserState
        {
            get
            {
                return this._currentUserState;
            }

            set
            {
                if (this._currentUserState != value)
                {
                    if (this._cancelLoad != null)
                    {
                        this._cancelLoad(this._currentUserState);
                    }
                }

                this._currentUserState = value;
            }
        }

        public override void Load(object userState)
        {
            if (!this.CanLoad)
            {
                throw new InvalidOperationException("Load cannot be called when CanLoad is false");
            }
            this.CurrentUserState = userState;
            this._load(this.OnLoadCompleted, userState);
        }

        private void OnLoadCompleted(ServiceLoadResult<TEntity> result)
        {
            this._onLoadCompleted(result);

            if (this.CurrentUserState == result.UserState)
            {
                this.CurrentUserState = null;
            }

            base.OnLoadCompleted(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using UniCloud.Security.Services;
using System.ComponentModel.DataAnnotations;

namespace UniCloud.Security.Models
{
    public sealed partial class Applications : Entity
    {
        /// <summary>
        /// 用于设置当前项是否可用
        /// </summary>
        public bool IsValid { get; set; }
    }

    public sealed partial class FunctionItem : Entity
    {
        public bool IsClickStatus = false; //IsChecked改变的来源，true表示来源页面的点击，false表示FunctionItem初始化的状态
        private bool reentrancyCheck = false;//控制自先关联
        private bool isAutoCompleted = true;//是否自动完成，如果自动完成，则不继续更新子项
        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool? _isChecked;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (IsClickStatus == true)
                {
                    if (reentrancyCheck)
                        return;
                    _isChecked = value;
                    this.reentrancyCheck = true;
                    this.UpdateCheckState();
                    this.reentrancyCheck = false;
                }
                else
                {
                    _isChecked = value;
                    IsClickStatus = true;
                }
                RaisePropertyChanged("IsChecked");
            }
        }

        #region 属性绑定

        /// <summary>
        /// 功能页面的集合
        /// </summary>
        public IEnumerable<FunctionItem> ViewFunctionItem
        {
            get
            {
                if (this.SubItems != null)
                {
                    return this.SubItems.Where(p => p.IsLeaf == true);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 获取按钮的集合
        /// </summary>
        public IEnumerable<FunctionItem> ButtonFunctionItems
        {
            get
            {
                if (this.SubItems != null)
                {
                    return this.SubItems.Where(p => p.IsButton == true);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 获取菜单集合
        /// </summary>
        public IEnumerable<FunctionItem> MenuFunctionItems
        {
            get
            {
                if (this.SubItems != null)
                {
                    return this.SubItems.Where(p => p.IsLeaf == false && p.IsButton == false);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        /// <summary>
        /// 更新子项的状态
        /// </summary>
        private void UpdateCheckState()
        {
            //更新当前项的状态
            var currentFunctionItem = this.FunctionsInRoles.FirstOrDefault(p => p.Role == StatusData.roles);
            currentFunctionItem.IsValid = this.IsChecked == null || this.IsChecked == true ? true : false;
            // 更新所有子项的状态
            if (this.SubItems.Count != 0)
            {
                this.UpdateChildrenCheckState();
            }
            //获取父亲节点
            if (this.ParentItemID != null)
            {
                //更新父亲项的状态
                var parentFunctionItem = this.Application.FunctionItems.FirstOrDefault(p => p.FunctionItemID == this.ParentItemID);
                parentFunctionItem.isAutoCompleted = true;
                parentFunctionItem.IsChecked = parentFunctionItem.DetermineCheckState();
                parentFunctionItem.isAutoCompleted = false;
            }
        }

        /// <summary>
        /// 更新子项状态
        /// </summary>
        private void UpdateChildrenCheckState()
        {
            if (!isAutoCompleted)
            {
                this.SubItems.SelectMany(p => p.FunctionsInRoles).Where(p => p.Role == StatusData.roles).ToList().ForEach(f =>
                {
                    f.IsValid = this.IsChecked == null || this.IsChecked == true ? true : false;
                    f.FunctionItem.IsChecked = this.IsChecked;
                });
            }
        }

        /// <summary>
        /// 父亲节点可选性
        /// </summary>
        /// <returns></returns>
        public bool? DetermineCheckState()
        {
            bool allChildrenChecked = this.SubItems.SelectMany(p => p.FunctionsInRoles).Where(p => p.Role == StatusData.roles).Count(x => x.FunctionItem.IsChecked == true)
                                     == this.SubItems.SelectMany(p => p.FunctionsInRoles).Where(p => p.Role == StatusData.roles).Count();
            if (allChildrenChecked)
            {
                return true;
            }
            bool allChildrenUnchecked = this.SubItems.SelectMany(p => p.FunctionsInRoles).Where(p => p.Role == StatusData.roles).Count(x => x.FunctionItem.IsChecked == false)
                                     == this.SubItems.SelectMany(p => p.FunctionsInRoles).Where(p => p.Role == StatusData.roles).Count();
            if (allChildrenUnchecked)
            {
                return false;
            }

            return null;
        }

        /// <summary>
        /// 用于设置当前项是否可用
        /// </summary>
        public bool IsValid { get; set; }

    }

    public sealed partial class Roles : Entity
    {
        /// <summary>
        /// 当前角色是否可编辑
        /// </summary>
        public bool IsActive
        {
            get
            {
                IAuthServices _service = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<IAuthServices>();
                return _service.IsExitRole(this);
            }
        }

        /// <summary>
        /// 用于设置当前项是否可用
        /// </summary>
        public bool IsValid { get; set; }
    }

    public sealed partial class Users : Entity
    {
        /// <summary>
        /// 当前用户是否可编辑
        /// </summary>
        public bool IsActive
        {
            get
            {
                IAuthServices _service = Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<IAuthServices>();
                return _service.IsExitUsers(this);
            }
        }
    }
}

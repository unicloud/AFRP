using System.Linq;
using Microsoft.Practices.ServiceLocation;
using UniCloud.Security.Models;
using UniCloud.Security.Services;

namespace UniCloud.AFRP.Operation
{
    /// <summary>
    /// 控制页面按钮是否可用
    /// </summary>
    public  class ViewButtonState
    {

        private static readonly IAuthServices Service = ServiceLocator.Current.GetInstance<IAuthServices>();

        /// <summary>
        /// 获取页面按钮是否可用
        /// </summary>
        /// <param name="viewTag">页面标志</param>
        /// <param name="buttonTag">页面按钮的标志</param>
        /// <returns></returns>
        public static bool GetViewButtonIsValid(string viewTag, string buttonTag)
        {
            //判断当前按钮是否有效
            var isValid = false;

            //获取按钮主键
            int buttonKey;
            //用户是否存在
            var currentUser = Service.EntityContainer.GetEntitySet<Users>().FirstOrDefault(p => p.UserName == StatusData.curUser.UserName);
            if (!string.IsNullOrEmpty(viewTag) && currentUser != null)
            {
                if (StatusData.curApplications != null)
                {
                    buttonKey = StatusData.curApplications.FunctionItems
                   .FirstOrDefault(p => p.Name == viewTag)
                   .SubItems.FirstOrDefault(p => p.Name == buttonTag).FunctionItemID;
                    if (currentUser.UserInRoles.Any() &&
                        currentUser.UserInRoles.SelectMany(p => p.Roles.FunctionsInRoles).Any())
                        isValid =
                            currentUser.UserInRoles
                                .SelectMany(p => p.Roles.FunctionsInRoles).Any(
                                    p => p.FunctionItemID == buttonKey && p.IsValid);

                }
            }

            return isValid;
        }
    }
}

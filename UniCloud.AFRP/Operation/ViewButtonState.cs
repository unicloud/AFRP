using System.Linq;
using Microsoft.Practices.ServiceLocation;
using UniCloud.Security.Models;
using UniCloud.Security.Services;

namespace UniCloud.AFRP.Operation
{
    /// <summary>
    /// ����ҳ�水ť�Ƿ����
    /// </summary>
    public  class ViewButtonState
    {

        private static readonly IAuthServices Service = ServiceLocator.Current.GetInstance<IAuthServices>();

        /// <summary>
        /// ��ȡҳ�水ť�Ƿ����
        /// </summary>
        /// <param name="viewTag">ҳ���־</param>
        /// <param name="buttonTag">ҳ�水ť�ı�־</param>
        /// <returns></returns>
        public static bool GetViewButtonIsValid(string viewTag, string buttonTag)
        {
            //�жϵ�ǰ��ť�Ƿ���Ч
            var isValid = false;

            //��ȡ��ť����
            int buttonKey;
            //�û��Ƿ����
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

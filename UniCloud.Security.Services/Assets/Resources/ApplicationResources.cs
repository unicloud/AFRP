namespace UniCloud.Security.Services
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Browser;

    /// <summary>
    /// 将访问包装为强类型资源类，以便将控件属性绑定到 XAML 中的资源字符串。
    /// </summary>
    public sealed class ApplicationResources
    {
        private static readonly ApplicationStrings applicationStrings = new ApplicationStrings();
        private static readonly ErrorResources errorResources = new ErrorResources();

        /// <summary>
        /// 获取 <see cref="ApplicationStrings"/>。
        /// </summary>
        public ApplicationStrings Strings
        {
            get { return applicationStrings; }
        }

        /// <summary>
        /// 获取 <see cref="ErrorResources"/>。
        /// </summary>
        public ErrorResources Errors
        {
            get { return errorResources; }
        }
    }
}

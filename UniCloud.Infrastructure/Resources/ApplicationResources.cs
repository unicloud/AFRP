namespace UniCloud.Infrastructure
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
        private static readonly AppConfig appConfig = new AppConfig();
        private static readonly CAFMStrings cafmStrings = new CAFMStrings();

        /// <summary>
        /// 获取 <see cref="CAFMStrings"/>。
        /// </summary>
        public AppConfig AppConfig
        {
            get { return appConfig; }
        }

        /// <summary>
        /// 获取 <see cref="CAFMStrings"/>。
        /// </summary>
        public CAFMStrings CafmStrings
        {
            get { return cafmStrings; }
        }

    }
}
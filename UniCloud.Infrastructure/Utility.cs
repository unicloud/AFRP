using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace UniCloud.Infrastructure
{
    /// <summary>
    /// 枚举工具类
    /// </summary>
    public static class EnumUtility
    {
        private static readonly IDictionary<Type, IEnumerable<string>> Names = new Dictionary<Type, IEnumerable<string>>();
        private static readonly IDictionary<Type, IEnumerable<object>> Values = new Dictionary<Type, IEnumerable<object>>();
        private static readonly IDictionary<Type, IDictionary<string, object>> NamesToValues = new Dictionary<Type, IDictionary<string, object>>();
        private static readonly IDictionary<Type, IDictionary<object, string>> ValuesToNames = new Dictionary<Type, IDictionary<object, string>>();

        public static void Initialize(Type type)
        {
            Initialize(type, GetDisplayAttributeName);
        }

        public static void Initialize(Type type, Func<FieldInfo, string> getName)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!type.IsEnum)
            {
                throw new ArgumentException("Type must be an enum.", "type");
            }

            if (getName == null)
            {
                throw new ArgumentNullException("getName");
            }

            if (Names.ContainsKey(type))
            {
                return;
            }

            var tempNames = new List<string>();
            var tempValues = new List<object>();
            var tempNamesToValues = new Dictionary<string, object>();
            var tempValuesToNames = new Dictionary<object, string>();

            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var name = getName(fi);
                var value = fi.GetValue(null);

                tempNames.Add(name);
                tempValues.Add(value);
                tempNamesToValues[name] = value;
                tempValuesToNames[value] = name;
            }

            Names[type] = tempNames;
            Values[type] = tempValues;
            NamesToValues[type] = tempNamesToValues;
            ValuesToNames[type] = tempValuesToNames;
        }

        private static string GetDisplayAttributeName(FieldInfo fi)
        {
            var displayAttribute = (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            return (displayAttribute == null) ? fi.Name : displayAttribute.Name;
        }

        public static IEnumerable<string> GetNames(Type type)
        {
            Initialize(type);
            return Names[type];
        }

        public static IEnumerable<object> GetValues(Type type)
        {
            Initialize(type);
            return Values[type];
        }

        public static string GetName(Type type, object value)
        {
            Initialize(type);
            return ValuesToNames[type][value];
        }

        public static object GetValue(Type type, string name)
        {
            Initialize(type);
            return NamesToValues[type][name];
        }
    }

    /// <summary>
    ///  文件处理工具类
    /// </summary>
    public static class FileOperation
    {

    }
}

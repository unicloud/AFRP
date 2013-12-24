using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CAAC.Infrastructure
{
    /// <summary>
    /// 枚举工具类
    /// </summary>
    public static class EnumUtility
    {
        private static readonly IDictionary<Type, IEnumerable<string>> names = new Dictionary<Type, IEnumerable<string>>();
        private static readonly IDictionary<Type, IEnumerable<object>> values = new Dictionary<Type, IEnumerable<object>>();
        private static readonly IDictionary<Type, IDictionary<string, object>> namesToValues = new Dictionary<Type, IDictionary<string, object>>();
        private static readonly IDictionary<Type, IDictionary<object, string>> valuesToNames = new Dictionary<Type, IDictionary<object, string>>();

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

            if (names.ContainsKey(type))
            {
                return;
            }

            List<string> tempNames = new List<string>();
            List<object> tempValues = new List<object>();
            Dictionary<string, object> tempNamesToValues = new Dictionary<string, object>();
            Dictionary<object, string> tempValuesToNames = new Dictionary<object, string>();

            foreach (FieldInfo fi in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                string name = getName(fi);
                object value = fi.GetValue(null);

                tempNames.Add(name);
                tempValues.Add(value);
                tempNamesToValues[name] = value;
                tempValuesToNames[value] = name;
            }

            names[type] = tempNames;
            values[type] = tempValues;
            namesToValues[type] = tempNamesToValues;
            valuesToNames[type] = tempValuesToNames;
        }

        private static string GetDisplayAttributeName(FieldInfo fi)
        {
            DisplayAttribute displayAttribute = (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            return (displayAttribute == null) ? fi.Name : displayAttribute.Name;
        }

        public static IEnumerable<string> GetNames(Type type)
        {
            Initialize(type);
            return names[type];
        }

        public static IEnumerable<object> GetValues(Type type)
        {
            Initialize(type);
            return values[type];
        }

        public static string GetName(Type type, object value)
        {
            Initialize(type);
            return valuesToNames[type][value];
        }

        public static object GetValue(Type type, string name)
        {
            Initialize(type);
            return namesToValues[type][name];
        }
    }

    /// <summary>
    ///  文件处理工具类
    /// </summary>
    public static class FileOperation
    { 

    }
}

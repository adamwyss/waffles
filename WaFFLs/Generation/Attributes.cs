using System;
using System.Reflection;

namespace WaFFLs.Generation
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TitleAttribute : Attribute
    {
        public string Text { get; set; }

        public static string GetTitleText(object obj)
        {
            Type type = obj.GetType();
            return GetTitleText(type);
        }

        public static string GetTitleText(Type type)
        {
            var title = type.GetCustomAttribute<TitleAttribute>(false);
            if (title != null && !string.IsNullOrWhiteSpace(title.Text))
            {
                return title.Text;
            }

            return type.Name;
        }
    }

    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SummaryAttribute : Attribute
    {
        public string Text { get; set; }

        public static string GetSummaryText(object obj)
        {
            Type type = obj.GetType();
            var summary = type.GetCustomAttribute<SummaryAttribute>(false);
            if (summary != null && !string.IsNullOrWhiteSpace(summary.Text))
            {
                return summary.Text;
            }

            return string.Empty;
        }
    }
}

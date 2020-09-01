using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WaFFLs.Generation
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TitleAttribute : Attribute
    {
        public string Text { get; set; }

        public static string GetTitleText(object obj)
        {
            Type type = obj.GetType();
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

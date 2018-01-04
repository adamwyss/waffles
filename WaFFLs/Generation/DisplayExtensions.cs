using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaFFLs.Generation
{
    public static class DisplayExtensions
    {
        public static string GetDisplayName(this Week week)
        {
            return $"{week.Name} {week.Season.Year}";
        }

        public static string ForDisplay(this Season season)
        {
            return @"{season.Year}";
        }

        public static string GetDisplayName(this Team team, int maxLength = -1)
        {
            string name = team.Name;
            if (maxLength > 0 && name.Length > maxLength)
            {
                name = name.Substring(0, maxLength);
            }

            return name;
        }

        public static string WithCommas(this int value)
        {
            return value.ToString("n0");
        }

        public static string AsWinningPercentage(this double value)
        {
            return value.ToString("n3");
        }
    }
}

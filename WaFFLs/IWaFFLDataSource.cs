using System.IO;
using System.Net;
using System.Text;

namespace WaFFLs
{
    public interface IWaFFLDataSource
    {
        string GetStandingsDataForYear(int year);
    }

    public class OnlineWaFFLDataSource : IWaFFLDataSource
    {
        public const string UrlFormat = "http://thewaffl.net/{0}.php";

        public string GetStandingsDataForYear(int year)
        {
            string url = string.Format(UrlFormat, year);
            if (year == 2018)
            {
                url = string.Format(UrlFormat, "Copied-2017");
            }
            else if (year == 2019)
            {
                url = string.Format(UrlFormat, "Copied-2018");
            }
            else if (year== 2023)
            {
                url = string.Format(UrlFormat, "Copied-2022");
            }

            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }

    public class CachedWaFFLDataSource : IWaFFLDataSource
    {
        public const string PathFormat = @"C:\Source\waffles\WaFFLs\TestData\{0}.txt";

        public string GetStandingsDataForYear(int year)
        {
            string path = string.Format(PathFormat, year);
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public void Cache(int year, string contents)
        {
            string path = string.Format(PathFormat, year);
            File.WriteAllText(path, contents, Encoding.UTF8);
        }
    }

}
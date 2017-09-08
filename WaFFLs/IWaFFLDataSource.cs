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

            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }

    public class CachedWaFFLDataSource : IWaFFLDataSource
    {
        public const string PathFormat = @"C:\Users\adamwy\Documents\visual studio 2010\Projects\WaFFLs\WaFFLs\TestData\{0}.txt";

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
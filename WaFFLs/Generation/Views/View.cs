using System;
using System.IO;

namespace WaFFLs.Generation.Views
{
    public class View
    {
        private static readonly Type _type = typeof(View);

        public static string Get(string name)
        {
            var assembly = _type.Assembly;
            var stream = assembly.GetManifestResourceStream(_type, name);
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

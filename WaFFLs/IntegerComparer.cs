using System.Collections.Generic;

namespace WaFFLs
{
    public class IntegerComparer
    {
        private static readonly IComparer<int> _ascending = new AscendingComparer();
        private static readonly IComparer<int> _descending = new DescendingComparer();

        public static IComparer<int> Ascending
        {
            get { return _ascending; }
        }

        public static IComparer<int> Descending
        {
            get { return _descending; }
        }

        private class AscendingComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x > y)
                {
                    return 1;
                }
                else if (x < y)
                {
                    return -1;
                }

                return 0;
            }
        }

        private class DescendingComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x > y)
                {
                    return -1;
                }
                else if (x < y)
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}
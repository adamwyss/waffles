using System;

namespace WaFFLs
{
    public class Terminal: IDisposable
    {
        private int count;

        public static Terminal Show(string title)
        { 
            return new Terminal(title);  
        }

        private Terminal(string title)
        {
            Console.Write("{0} ", title);
        }

        public void Update(int value)
        {
            Update(value.ToString());
        }

        public void Update(string value)
        {
            Console.Write(value);

            count++;
            if (count % 15 == 12)
            {
                Console.WriteLine();
            }
            else
            {
                Console.Write(", ");
            }
        }

        public void Dispose()
        {
            Console.Write("Done!");
            Console.WriteLine();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaFFLs.Generation.Models
{
    public class StreakRecord
    {
        public int Value { get; set; }

        public Team Team { get; set; }

        public Week From { get; set; }

        public Week To { get; set; }
    }
}

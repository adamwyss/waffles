using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaFFLs.Generation.Models
{
    public class GameRecord
    {
        public string Value { get; set; }

        public Team Team1 { get; set; }

        public Team Team2 { get; set; }

        public Week Game { get; set; }
    }
}

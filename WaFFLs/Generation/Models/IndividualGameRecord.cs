using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaFFLs.Generation.Models
{
    public class IndividualGameRecord
    {
        public object Value { get; set; }

        public Team Team { get; set; }

        public Week Game { get; set; }
    }
}

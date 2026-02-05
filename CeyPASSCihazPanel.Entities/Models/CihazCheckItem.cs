using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class CihazCheckItem
    {
        public int CihazId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public string DisplayText { get; set; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}

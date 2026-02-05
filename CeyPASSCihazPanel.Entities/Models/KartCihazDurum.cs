using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class KartCihazDurum
    {
        public int CihazId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public bool YetkiVarMi { get; set; }
        public bool CihazaEklenmis { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class CihazBilgi
    {
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public string Model { get; set; }
        public string SeriNo { get; set; }
        public string FirmwareVersion { get; set; }
        public string Platform { get; set; }
        public string MACAdres { get; set; }

        public int KullaniciKapasitesi { get; set; }
        public int MevcutKullaniciSayisi { get; set; }

        public int ParmakilziKapasitesi { get; set; }
        public int MevcutParmakilziSayisi { get; set; }

        public int LogKapasitesi { get; set; }
        public int MevcutLogSayisi { get; set; }

        public DateTime? CihazSaati { get; set; }
        public bool BaglantiDurumu { get; set; }
    }
}

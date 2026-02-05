using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class OfflineLog
    {
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public int EnrollNumber { get; set; }
        public int PersonelId { get; set; }
        public string AdSoyad { get; set; }
        public DateTime Tarih { get; set; }
        public int VerifyMode { get; set; }
        public int InOutMode { get; set; }
        public string VerifyModeText { get; set; }
        public string InOutModeText { get; set; }
    }
}

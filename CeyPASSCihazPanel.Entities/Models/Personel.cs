using System;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class Personel
    {
        public string PersonelId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int? FirmaId { get; set; }
        public int? KartNo { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
    }
}

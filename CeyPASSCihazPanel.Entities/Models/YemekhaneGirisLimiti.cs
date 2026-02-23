using System;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class YemekhaneGirisLimiti
    {
        public int Id { get; set; }
        public string PersonelId { get; set; }
        public int? GunlukLimit { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public bool? AktifMi { get; set; }
    }
}

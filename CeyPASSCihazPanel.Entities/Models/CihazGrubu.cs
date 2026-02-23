using System;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class CihazGrubu
    {
        public int Id { get; set; }
        public string GrupAdi { get; set; }
        public int? FirmaId { get; set; }
        public DateTime KayitTarihi { get; set; }
    }
}

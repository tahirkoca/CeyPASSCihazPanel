namespace CeyPASSCihazPanel.Entities.Models
{
    /// <summary>Toplu yükleme işlemi için PuantajsizKartlar satırını temsil eder.</summary>
    public class PuantajsizKartBulk
    {
        public string KartId { get; set; }
        public string KartNo { get; set; }
        public string KartAdi { get; set; }
        public int FirmaId { get; set; }
        public bool AktifMi { get; set; }
        public string CalismaSekli { get; set; }
        public bool ZiyaretciMi { get; set; }
        public bool AracKartiMi { get; set; }
        public bool TaseronCalisanMi { get; set; }
    }
}

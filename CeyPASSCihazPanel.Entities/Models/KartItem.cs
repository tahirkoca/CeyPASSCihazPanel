namespace CeyPASSCihazPanel.Entities.Models
{
    public class KartItem
    {
        public int KartId { get; set; }
        public string KartNo { get; set; }
        public string KartAdi { get; set; }
        public override string ToString() => $"{KartAdi} ({KartId})";
    }
}

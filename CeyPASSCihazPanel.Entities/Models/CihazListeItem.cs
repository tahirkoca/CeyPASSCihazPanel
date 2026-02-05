namespace CeyPASSCihazPanel.Entities.Models
{
    public class CihazListeItem
    {
        public string FirmaAdi { get; set; }
        public string IPAdres { get; set; }
        public string CihazAdi { get; set; }
        public override string ToString()=> $"{CihazAdi} ({IPAdres}) [{FirmaAdi}]";
    }
}

using System;
using zkemkeeper;

namespace CeyPASSCihazPanel.Entities.Models
{
    public class CihazBaglantisi
    {
        public CZKEM Device { get; set; }
        public Terminal Info { get; set; }
        public DateTime SonBaglantiZamani { get; set; }
        public int BaglantiDenemeleri { get; set; }
        public bool Bagli { get; set; }
    }
}

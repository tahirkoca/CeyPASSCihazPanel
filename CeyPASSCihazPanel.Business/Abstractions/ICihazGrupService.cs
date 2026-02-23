using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface ICihazGrupService
    {
        IEnumerable<CihazGrubu> GetGruplar(int? firmaId);
        int EkleGrup(CihazGrubu grup);
        void SilGrup(int id);
        
        IEnumerable<CihazGrupDetay> GetGrupDetaylari(int grupId);
        void KaydetGrupCihazlari(int grupId, IEnumerable<int> cihazIdler);
    }
}

using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface ICihazGrupRepository
    {
        // Grup Ana İşlemleri
        IEnumerable<CihazGrubu> GetGruplar(int? firmaId);
        int EkleGrup(CihazGrubu grup);
        void SilGrup(int id);
        
        // Grup Detay (Bağlı Cihazlar) İşlemleri
        IEnumerable<CihazGrupDetay> GetGrupDetaylari(int grupId);
        void EkleGrupDetaylari(int grupId, IEnumerable<int> cihazIdler);
        void SilGrupDetaylari(int grupId);
    }
}

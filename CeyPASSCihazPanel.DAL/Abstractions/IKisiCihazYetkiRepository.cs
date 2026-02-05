using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IKisiCihazYetkiRepository
    {
        IList<int> GetYetkiliCihazlar(int personelId);
        IList<PersonelCihazDurum> GetPersonelCihazDurumlari(int personelId, int? firmaId);
        bool YetkiKaydet(int personelId, List<int> cihazIdler, int? firmaId);
        bool YetkiSil(int personelId, int cihazId, int? firmaId);
        bool TumYetkileriSil(int personelId, int? firmaId);
    }
}

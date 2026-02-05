using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IPuantajsizKartCihazYetkiRepository
    {
        IList<int> GetYetkiliCihazlar(int kartId);
        IList<KartCihazDurum> GetKartCihazDurumlari(int kartId, int? firmaId);
        bool YetkiKaydet(int kartId, List<int> cihazIdler, int? firmaId);
        bool YetkiSil(int kartId, int cihazId, int? firmaId);
        bool TumYetkileriSil(int kartId, int? firmaId);
    }
}

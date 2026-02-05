using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IAdminLookupService
    {
        IList<Terminal> GetAktifCihazlar(int? firmaId);
        IList<CihazListeItem> GetCihazListeItems(int? firmaId);
        IList<Personel> GetAktifPersoneller(int? firmaId);
        IList<PuantajsizKart> GetAktifPuantajsizKartlar(int? firmaId);
        Personel GetPersonelById(int personelId);
        IList<int> GetPersonelYetkiliCihazlar(int personelId);
        IList<int> GetKartYetkiliCihazlar(int kartId);
        IList<PersonelCihazDurum> GetPersonelCihazDurumlari(int personelId, int? firmaId);
        IList<KartCihazDurum> GetKartCihazDurumlari(int kartId, int? firmaId);
        int? GetCihazIdByIp(string ip);
        bool PersonelYetkiKaydet(int personelId, List<int> cihazIdler, int? firmaId);
        bool KartYetkiKaydet(int kartId, List<int> cihazIdler, int? firmaId);
        bool PersonelYetkiSil(int personelId, int cihazId, int? firmaId);
        bool KartYetkiSil(int kartId, int cihazId, int? firmaId);
        bool PersonelTumYetkileriSil(int personelId, int? firmaId);
        bool KartTumYetkileriSil(int kartId, int? firmaId);
    }
}

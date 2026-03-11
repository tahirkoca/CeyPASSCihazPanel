using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IAdminLookupService
    {
        IList<Terminal> GetAktifCihazlar(int? firmaId);
        IList<CihazListeItem> GetCihazListeItems(int? firmaId);
        IList<Personel> GetAktifPersoneller(int? firmaId);
        IList<PuantajsizKisi> GetAktifPuantajsizKisiler(int? firmaId);
        IList<FirmaSonPuantajsizKisi> GetSonPuantajsizKisiIdleri(int? firmaId);
        Personel GetPersonelById(int personelId);
        IList<int> GetPersonelYetkiliCihazlar(int personelId);
        IList<PersonelCihazDurum> GetPersonelCihazDurumlari(int personelId, int? firmaId);
        int? GetCihazIdByIp(string ip);
        bool PersonelYetkiKaydet(int personelId, List<int> cihazIdler, int? firmaId);
        bool PersonelYetkiSil(int personelId, int cihazId, int? firmaId);
        bool PersonelTumYetkileriSil(int personelId, int? firmaId);
    }
}

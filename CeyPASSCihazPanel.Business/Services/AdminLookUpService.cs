using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Services
{
    public class AdminLookUpService : IAdminLookupService
    {
        private readonly ICihazRepository _cihazRepository;
        private readonly IPersonelRepository _personelRepository;
        private readonly IPuantajsizKartRepository _kartRepository;
        private readonly IKisiCihazYetkiRepository _kisiYetkiRepository;
        private readonly IPuantajsizKartCihazYetkiRepository _kartYetkiRepository;

        public AdminLookUpService(ICihazRepository cihazRepository, IPersonelRepository personelRepository, IPuantajsizKartRepository kartRepository, IKisiCihazYetkiRepository kisiYetkiRepository,IPuantajsizKartCihazYetkiRepository kartYetkiRepository)
        {
            _cihazRepository = cihazRepository;
            _personelRepository = personelRepository;
            _kartRepository = kartRepository;
            _kisiYetkiRepository = kisiYetkiRepository;
            _kartYetkiRepository = kartYetkiRepository;
        }

        public IList<Terminal> GetAktifCihazlar(int? firmaId) => _cihazRepository.GetAktifCihazlar(firmaId);
        public IList<CihazListeItem> GetCihazListeItems(int? firmaId) => _cihazRepository.GetCihazListeItems(firmaId);
        public IList<Personel> GetAktifPersoneller(int? firmaId) => _personelRepository.GetAktifPersoneller(firmaId);
        public IList<PuantajsizKart> GetAktifPuantajsizKartlar(int? firmaId) => _kartRepository.GetAktifKartlar(firmaId);
        public Personel GetPersonelById(int personelId) => _personelRepository.GetById(personelId);
        public IList<int> GetPersonelYetkiliCihazlar(int personelId) => _kisiYetkiRepository.GetYetkiliCihazlar(personelId);
        public IList<int> GetKartYetkiliCihazlar(int kartId) => _kartYetkiRepository.GetYetkiliCihazlar(kartId);
        public IList<PersonelCihazDurum> GetPersonelCihazDurumlari(int personelId, int? firmaId) => _kisiYetkiRepository.GetPersonelCihazDurumlari(personelId, firmaId);
        public IList<KartCihazDurum> GetKartCihazDurumlari(int kartId, int? firmaId) => _kartYetkiRepository.GetKartCihazDurumlari(kartId, firmaId);
        public int? GetCihazIdByIp(string ip) => _cihazRepository.GetCihazIdByIp(ip);
        public bool PersonelYetkiKaydet(int personelId, List<int> cihazIdler, int? firmaId) => _kisiYetkiRepository.YetkiKaydet(personelId, cihazIdler, firmaId);
        public bool KartYetkiKaydet(int kartId, List<int> cihazIdler, int? firmaId) => _kartYetkiRepository.YetkiKaydet(kartId, cihazIdler, firmaId);
        public bool PersonelYetkiSil(int personelId, int cihazId, int? firmaId) => _kisiYetkiRepository.YetkiSil(personelId, cihazId, firmaId);
        public bool KartYetkiSil(int kartId, int cihazId, int? firmaId) => _kartYetkiRepository.YetkiSil(kartId, cihazId, firmaId);
        public bool PersonelTumYetkileriSil(int personelId, int? firmaId) => _kisiYetkiRepository.TumYetkileriSil(personelId, firmaId);
        public bool KartTumYetkileriSil(int kartId, int? firmaId) => _kartYetkiRepository.TumYetkileriSil(kartId, firmaId);
    }
}

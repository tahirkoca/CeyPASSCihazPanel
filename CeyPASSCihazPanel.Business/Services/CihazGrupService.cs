using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Services
{
    public class CihazGrupService : ICihazGrupService
    {
        private readonly ICihazGrupRepository _repo;

        public CihazGrupService(ICihazGrupRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public IEnumerable<CihazGrubu> GetGruplar(int? firmaId)
        {
            return _repo.GetGruplar(firmaId);
        }

        public int EkleGrup(CihazGrubu grup)
        {
            return _repo.EkleGrup(grup);
        }

        public void SilGrup(int id)
        {
            _repo.SilGrup(id);
        }

        public IEnumerable<CihazGrupDetay> GetGrupDetaylari(int grupId)
        {
            return _repo.GetGrupDetaylari(grupId);
        }

        public void KaydetGrupCihazlari(int grupId, IEnumerable<int> cihazIdler)
        {
            _repo.EkleGrupDetaylari(grupId, cihazIdler);
        }
    }
}

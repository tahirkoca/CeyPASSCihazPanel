using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IPersonelRepository
    {
        IList<Personel> GetAktifPersoneller(int? firmaId);
        Personel GetById(int personelId);
    }
}

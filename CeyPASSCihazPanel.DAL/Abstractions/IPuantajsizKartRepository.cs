using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IPuantajsizKartRepository
    {
        IList<PuantajsizKart> GetAktifKartlar(int? firmaId);
    }
}

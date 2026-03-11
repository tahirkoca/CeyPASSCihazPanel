using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IPuantajsizKisiRepository
    {
        IList<PuantajsizKisi> GetAktifKartlar(int? firmaId);
    }
}

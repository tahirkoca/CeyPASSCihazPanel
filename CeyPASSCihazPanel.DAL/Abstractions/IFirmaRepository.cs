using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IFirmaRepository
    {
        IList<FirmaSonPuantajsizKisi> GetSonPuantajsizKisiIdleri(int? firmaId);
    }
}


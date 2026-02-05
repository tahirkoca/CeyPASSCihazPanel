using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface ICihazRepository
    {
        IList<Terminal> GetAktifCihazlar(int? firmaId);
        IList<CihazListeItem> GetCihazListeItems(int? firmaId);
        int? GetCihazIdByIp(string ip);
    }
}

using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IYemekhaneGirisLimitRepository
    {
        (int Basarili, int Hatali) BulkUpsert(IEnumerable<YemekhaneGirisLimiti> rows);
    }
}

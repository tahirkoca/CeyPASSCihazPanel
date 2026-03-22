using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IYemekhaneGirisLimitRepository
    {
        BulkUpsertResult BulkUpsert(IEnumerable<YemekhaneGirisLimiti> rows);
    }
}

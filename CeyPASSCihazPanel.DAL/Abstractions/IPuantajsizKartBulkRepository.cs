using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IPuantajsizKartBulkRepository
    {
        (int Basarili, int Hatali) BulkUpsert(IEnumerable<PuantajsizKartBulk> rows);
    }
}

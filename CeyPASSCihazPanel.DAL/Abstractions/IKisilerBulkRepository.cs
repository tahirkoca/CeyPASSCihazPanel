using System.Collections.Generic;
using CeyPASSCihazPanel.Entities.Models;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IKisilerBulkRepository
    {
        /// <summary>
        /// PersonelId üzerinden UPSERT yapar ve detaylı sonuç döner.
        /// </summary>
        BulkUpsertResult BulkUpsert(IEnumerable<System.Collections.Generic.IDictionary<string, object>> rows);
    }
}

using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IKisilerBulkRepository
    {
        /// <summary>
        /// PersonelId üzerinden UPSERT yapar. (int basarili, int hatali) döner.
        /// </summary>
        (int Basarili, int Hatali) BulkUpsert(IEnumerable<System.Collections.Generic.IDictionary<string, object>> rows);
    }
}

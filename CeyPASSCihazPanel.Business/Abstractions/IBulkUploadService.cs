using System.Data;
using CeyPASSCihazPanel.Entities.Models;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IBulkUploadService
    {
        // Toplu yükleme
        BulkUpsertResult BulkUpsertKisiler(DataTable dt);
        BulkUpsertResult BulkUpsertYemekhane(DataTable dt);

        // Şablon DataTable'ları
        DataTable GetKisiTemplate();
        DataTable GetYemekhaneTemplate();
    }
}

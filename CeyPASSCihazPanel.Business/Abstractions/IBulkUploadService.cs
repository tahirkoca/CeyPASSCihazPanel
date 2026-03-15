using System.Data;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IBulkUploadService
    {
        // Toplu yükleme
        (int Basarili, int Hatali) BulkUpsertKisiler(DataTable dt);
        (int Basarili, int Hatali) BulkUpsertYemekhane(DataTable dt);

        // Şablon DataTable'ları
        DataTable GetKisiTemplate();
        DataTable GetYemekhaneTemplate();
    }
}

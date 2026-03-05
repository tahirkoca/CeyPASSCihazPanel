using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlPuantajsizKartBulkRepository : IPuantajsizKartBulkRepository
    {
        private readonly string _connStr;

        public SqlPuantajsizKartBulkRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public (int Basarili, int Hatali) BulkUpsert(IEnumerable<PuantajsizKartBulk> rows)
        {
            int basarili = 0, hatali = 0;

            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM PuantajsizKartlar WHERE KartId = @KartId)
                BEGIN
                    INSERT INTO PuantajsizKartlar (KartId, KartNo, KartAdi, AktifMi, FirmaId, CalismaSekli, ZiyaretciMi, AracKartiMi, TaseronCalisanMi)
                    VALUES (@KartId, @KartNo, @KartAdi, @AktifMi, @FirmaId, @CalismaSekli, @ZiyaretciMi, @AracKartiMi, @TaseronCalisanMi)
                END
                ELSE
                BEGIN
                    UPDATE PuantajsizKartlar SET
                        KartNo=@KartNo, AktifMi=@AktifMi, FirmaId=@FirmaId,
                        CalismaSekli=@CalismaSekli, ZiyaretciMi=@ZiyaretciMi,
                        AracKartiMi=@AracKartiMi, TaseronCalisanMi=@TaseronCalisanMi
                    WHERE KartId=@KartId
                END";

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var k in rows)
                {
                    if (string.IsNullOrWhiteSpace(k.KartId)) continue;
                    try
                    {
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@KartId", k.KartId);
                            cmd.Parameters.AddWithValue("@KartNo", k.KartNo ?? "");
                            cmd.Parameters.AddWithValue("@KartAdi", k.KartAdi ?? "");
                            cmd.Parameters.AddWithValue("@AktifMi", k.AktifMi);
                            cmd.Parameters.AddWithValue("@FirmaId", k.FirmaId);
                            cmd.Parameters.AddWithValue("@CalismaSekli", k.CalismaSekli ?? "");
                            cmd.Parameters.AddWithValue("@ZiyaretciMi", k.ZiyaretciMi);
                            cmd.Parameters.AddWithValue("@AracKartiMi", k.AracKartiMi);
                            cmd.Parameters.AddWithValue("@TaseronCalisanMi", k.TaseronCalisanMi);
                            cmd.ExecuteNonQuery();
                            basarili++;
                        }
                    }
                    catch { hatali++; }
                }
            }
            return (basarili, hatali);
        }
    }
}

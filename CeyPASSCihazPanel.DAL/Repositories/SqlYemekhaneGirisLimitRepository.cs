using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlYemekhaneGirisLimitRepository : IYemekhaneGirisLimitRepository
    {
        private readonly string _connStr;

        public SqlYemekhaneGirisLimitRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public (int Basarili, int Hatali) BulkUpsert(IEnumerable<YemekhaneGirisLimiti> rows)
        {
            int basarili = 0, hatali = 0;

            const string sql = @"
                UPDATE YemekhaneGirisLimitler SET
                    GunlukLimit = COALESCE(@GunlukLimit, GunlukLimit),
                    KayitTarihi = COALESCE(@KayitTarihi, KayitTarihi),
                    AktifMi     = COALESCE(@AktifMi, AktifMi)
                WHERE PersonelId = @PersonelId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO YemekhaneGirisLimitler (PersonelId, GunlukLimit, KayitTarihi, AktifMi)
                    VALUES (@PersonelId, @GunlukLimit, @KayitTarihi, @AktifMi);
                END";

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var y in rows)
                {
                    if (string.IsNullOrWhiteSpace(y.PersonelId)) continue;
                    try
                    {
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@PersonelId", y.PersonelId);
                            cmd.Parameters.AddWithValue("@GunlukLimit", (object)y.GunlukLimit ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@KayitTarihi", (object)y.KayitTarihi ?? (object)DateTime.Now);
                            cmd.Parameters.AddWithValue("@AktifMi", (object)y.AktifMi ?? (object)true);
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

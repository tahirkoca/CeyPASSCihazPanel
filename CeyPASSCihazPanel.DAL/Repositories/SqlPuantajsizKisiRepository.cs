using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlPuantajsizKisiRepository : IPuantajsizKisiRepository
    {
        private readonly string _connStr;

        public SqlPuantajsizKisiRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<PuantajsizKisi> GetAktifKartlar(int? firmaId)
        {
            var list = new List<PuantajsizKisi>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT
    TRY_CONVERT(int, K.PersonelId) AS KartId,
    K.KartNo,
    LTRIM(RTRIM(COALESCE(K.Ad, ''))) + CASE WHEN NULLIF(LTRIM(RTRIM(COALESCE(K.Soyad, ''))), '') IS NULL THEN '' ELSE ' ' + LTRIM(RTRIM(K.Soyad)) END AS KartAdi
FROM Kisiler K
WHERE K.IstenCikisTarihi IS NULL
  AND ISNULL(K.PuantajYapilirMi, 1) = 0
  AND ( @FirmaId IS NULL OR K.FirmaId = @FirmaId )
  AND TRY_CONVERT(int, K.PersonelId) IS NOT NULL
ORDER BY KartAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new PuantajsizKisi
                            {
                                KartId = Convert.ToInt32(dr["KartId"]),
                                KartNo = dr["KartNo"]?.ToString() ?? "",
                                KartAdi = dr["KartAdi"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}

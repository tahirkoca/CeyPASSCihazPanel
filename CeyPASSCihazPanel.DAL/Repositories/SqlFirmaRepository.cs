using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlFirmaRepository : IFirmaRepository
    {
        private readonly string _connStr;

        public SqlFirmaRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<FirmaSonPuantajsizKisi> GetSonPuantajsizKisiIdleri(int? firmaId)
        {
            var list = new List<FirmaSonPuantajsizKisi>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                var sql = @"
SELECT
    F.FirmaId,
    F.FirmaAdi,
    MAX(TRY_CONVERT(int, K.PersonelId)) AS SonPersonelId
FROM Firmalar F
LEFT JOIN Kisiler K
    ON K.FirmaId = F.FirmaId
   AND K.IstenCikisTarihi IS NULL
   AND ISNULL(K.PuantajYapilirMi, 1) = 0
   AND TRY_CONVERT(int, K.PersonelId) IS NOT NULL
WHERE (@FirmaId IS NULL OR F.FirmaId = @FirmaId)
GROUP BY F.FirmaId, F.FirmaAdi
ORDER BY F.FirmaAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new FirmaSonPuantajsizKisi
                            {
                                FirmaId = dr.GetInt32(0),
                                FirmaAdi = dr.IsDBNull(1) ? "" : dr.GetString(1),
                                SonPersonelId = dr.IsDBNull(2) ? (int?)null : dr.GetInt32(2)
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}


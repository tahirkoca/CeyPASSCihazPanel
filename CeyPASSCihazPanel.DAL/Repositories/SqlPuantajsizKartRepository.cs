using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlPuantajsizKartRepository : IPuantajsizKartRepository
    {
        private readonly string _connStr;

        public SqlPuantajsizKartRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<PuantajsizKart> GetAktifKartlar(int? firmaId)
        {
            var list = new List<PuantajsizKart>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT KartId, KartNo, KartAdi
FROM PuantajsizKartlar
WHERE AktifMi = 1
  AND ( @FirmaId IS NULL OR FirmaId = @FirmaId )
ORDER BY KartAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new PuantajsizKart
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

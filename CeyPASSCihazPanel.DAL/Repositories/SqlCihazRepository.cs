using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlCihazRepository : ICihazRepository
    {
        private readonly string _connStr;

        public SqlCihazRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<Terminal> GetAktifCihazlar(int? firmaId)
        {
            var list = new List<Terminal>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT CihazId, CihazAdi, IPAdres, Port
FROM Cihazlar C
WHERE C.AktifMi = 1
  AND ( @FirmaId IS NULL OR C.FirmaId = @FirmaId )
ORDER BY C.CihazAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Terminal
                            {
                                CihazId = dr.GetInt32(0),
                                CihazAdi = dr.GetString(1),
                                IP = dr.GetString(2),
                                Port = dr.GetInt32(3)
                            });
                        }
                    }
                }
            }

            return list;
        }
        public IList<CihazListeItem> GetCihazListeItems(int? firmaId)
        {
            var list = new List<CihazListeItem>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT F.FirmaAdi, C.IPAdres, C.CihazAdi
FROM Cihazlar C
INNER JOIN Firmalar F ON C.FirmaId = F.FirmaId
WHERE C.AktifMi = 1
  AND ( @FirmaId IS NULL OR C.FirmaId = @FirmaId )
ORDER BY C.CihazAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CihazListeItem
                            {
                                FirmaAdi = dr.GetString(0),
                                IPAdres = dr.GetString(1),
                                CihazAdi = dr.GetString(2)
                            });
                        }
                    }
                }
            }

            return list;
        }
        public int? GetCihazIdByIp(string ip)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = "SELECT CihazId FROM Cihazlar WHERE IPAdres = @IP AND AktifMi = 1";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IP", ip);
                    var result = cmd.ExecuteScalar();
                    return result != null ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }
    }
}

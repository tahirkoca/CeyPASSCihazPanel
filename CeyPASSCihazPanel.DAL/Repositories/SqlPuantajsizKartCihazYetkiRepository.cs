using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlPuantajsizKartCihazYetkiRepository : IPuantajsizKartCihazYetkiRepository
    {
        private readonly string _connStr;

        public SqlPuantajsizKartCihazYetkiRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<int> GetYetkiliCihazlar(int kartId)
        {
            var list = new List<int>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT CihazId
FROM PuantajsizKartCihazYetkileri
WHERE KartId = @KartId 
  AND AktifMi = 1";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KartId", kartId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr.GetInt32(0));
                        }
                    }
                }
            }

            return list;
        }
        public IList<KartCihazDurum> GetKartCihazDurumlari(int kartId, int? firmaId)
        {
            var list = new List<KartCihazDurum>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT 
    C.CihazId,
    C.CihazAdi,
    C.IPAdres,
    CASE WHEN PKCY.CihazId IS NOT NULL THEN 1 ELSE 0 END AS YetkiVarMi
FROM Cihazlar C
LEFT JOIN PuantajsizKartCihazYetkileri PKCY ON C.CihazId = PKCY.CihazId 
    AND PKCY.KartId = @KartId 
    AND PKCY.AktifMi = 1
WHERE C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)
ORDER BY C.CihazAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KartId", kartId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new KartCihazDurum
                            {
                                CihazId = dr.GetInt32(0),
                                CihazAdi = dr.GetString(1),
                                IPAdres = dr.GetString(2),
                                YetkiVarMi = dr.GetInt32(3) == 1,
                                CihazaEklenmis = false // Bu bilgi cihazdan gelecek
                            });
                        }
                    }
                }
            }

            return list;
        }
        public bool YetkiKaydet(int kartId, List<int> cihazIdler, int? firmaId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string checkSql = @"
SELECT COUNT(*) 
FROM PuantajsizKartlar 
WHERE KartId = @KartId 
  AND AktifMi = 1
  AND (@FirmaId IS NULL OR FirmaId = @FirmaId)";

                        using (var cmd = new SqlCommand(checkSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KartId", kartId);
                            cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                            int count = (int)cmd.ExecuteScalar();
                            if (count == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        string deleteSql = @"
UPDATE PKCY
SET PKCY.AktifMi = 0 
FROM PuantajsizKartCihazYetkileri PKCY
INNER JOIN Cihazlar C ON PKCY.CihazId = C.CihazId
WHERE PKCY.KartId = @KartId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                        using (var cmd = new SqlCommand(deleteSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KartId", kartId);
                            cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var cihazId in cihazIdler)
                        {
                            string verifyCihazSql = @"
SELECT COUNT(*) 
FROM Cihazlar 
WHERE CihazId = @CihazId 
  AND AktifMi = 1
  AND (@FirmaId IS NULL OR FirmaId = @FirmaId)";

                            using (var verifyCmd = new SqlCommand(verifyCihazSql, conn, transaction))
                            {
                                verifyCmd.Parameters.AddWithValue("@CihazId", cihazId);
                                verifyCmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                                int cihazCount = (int)verifyCmd.ExecuteScalar();
                                if (cihazCount == 0) continue;
                            }

                            string insertSql = @"
DECLARE @CihazFirmaId INT
SELECT @CihazFirmaId = FirmaId FROM Cihazlar WHERE CihazId = @CihazId

IF EXISTS (SELECT 1 FROM PuantajsizKartCihazYetkileri WHERE KartId = @KartId AND CihazId = @CihazId)
BEGIN
    UPDATE PuantajsizKartCihazYetkileri 
    SET AktifMi = 1,
        FirmaId = @CihazFirmaId
    WHERE KartId = @KartId AND CihazId = @CihazId
END
ELSE
BEGIN
    INSERT INTO PuantajsizKartCihazYetkileri (FirmaId, KartId, CihazId, AktifMi)
    VALUES (@CihazFirmaId, @KartId, @CihazId, 1)
END
";

                            using (var cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@KartId", kartId);
                                cmd.Parameters.AddWithValue("@CihazId", cihazId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                            transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        public bool YetkiSil(int kartId, int cihazId, int? firmaId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
UPDATE PKCY
SET PKCY.AktifMi = 0 
FROM PuantajsizKartCihazYetkileri PKCY
INNER JOIN Cihazlar C ON PKCY.CihazId = C.CihazId
WHERE PKCY.KartId = @KartId 
  AND PKCY.CihazId = @CihazId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KartId", kartId);
                    cmd.Parameters.AddWithValue("@CihazId", cihazId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool TumYetkileriSil(int kartId, int? firmaId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
UPDATE PKCY
SET PKCY.AktifMi = 0 
FROM PuantajsizKartCihazYetkileri PKCY
INNER JOIN Cihazlar C ON PKCY.CihazId = C.CihazId
WHERE PKCY.KartId = @KartId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KartId", kartId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

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
    public class SqlKisiCihazYetkiRepository : IKisiCihazYetkiRepository
    {
        private readonly string _connStr;

        public SqlKisiCihazYetkiRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<int> GetYetkiliCihazlar(int personelId)
        {
            var list = new List<int>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT CihazId
FROM KisiCihazYetkileri
WHERE PersonelId = @PersonelId 
  AND AktifMi = 1";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PersonelId", personelId);

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
        public IList<PersonelCihazDurum> GetPersonelCihazDurumlari(int personelId, int? firmaId)
        {
            var list = new List<PersonelCihazDurum>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT 
    C.CihazId,
    C.CihazAdi,
    C.IPAdres,
    CASE WHEN KCY.CihazId IS NOT NULL THEN 1 ELSE 0 END AS YetkiVarMi
FROM Cihazlar C
LEFT JOIN KisiCihazYetkileri KCY ON C.CihazId = KCY.CihazId 
    AND KCY.PersonelId = @PersonelId 
    AND KCY.AktifMi = 1
WHERE C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)
ORDER BY C.CihazAdi";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PersonelId", personelId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new PersonelCihazDurum
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
        public bool YetkiKaydet(int personelId, List<int> cihazIdler, int? firmaId)
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
FROM Kisiler 
WHERE PersonelId = @PersonelId 
  AND IstenCikisTarihi IS NULL
  AND (@FirmaId IS NULL OR FirmaId = @FirmaId)";

                        using (var cmd = new SqlCommand(checkSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PersonelId", personelId);
                            cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                            int count = (int)cmd.ExecuteScalar();
                            if (count == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        string deleteSql = @"
UPDATE KCY
SET KCY.AktifMi = 0 
FROM KisiCihazYetkileri KCY
INNER JOIN Cihazlar C ON KCY.CihazId = C.CihazId
WHERE KCY.PersonelId = @PersonelId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                        using (var cmd = new SqlCommand(deleteSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PersonelId", personelId);
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

IF EXISTS (SELECT 1 FROM KisiCihazYetkileri WHERE PersonelId = @PersonelId AND CihazId = @CihazId)
BEGIN
    UPDATE KisiCihazYetkileri 
    SET AktifMi = 1,
        FirmaId = @CihazFirmaId
    WHERE PersonelId = @PersonelId AND CihazId = @CihazId
END
ELSE
BEGIN
    INSERT INTO KisiCihazYetkileri (FirmaId, PersonelId, CihazId, AktifMi)
    VALUES (@CihazFirmaId, @PersonelId, @CihazId, 1)
END
";

                            using (var cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PersonelId", personelId);
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
        public bool YetkiSil(int personelId, int cihazId, int? firmaId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
UPDATE KCY
SET KCY.AktifMi = 0 
FROM KisiCihazYetkileri KCY
INNER JOIN Cihazlar C ON KCY.CihazId = C.CihazId
WHERE KCY.PersonelId = @PersonelId 
  AND KCY.CihazId = @CihazId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PersonelId", personelId);
                    cmd.Parameters.AddWithValue("@CihazId", cihazId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool TumYetkileriSil(int personelId, int? firmaId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
UPDATE KCY
SET KCY.AktifMi = 0 
FROM KisiCihazYetkileri KCY
INNER JOIN Cihazlar C ON KCY.CihazId = C.CihazId
WHERE KCY.PersonelId = @PersonelId
  AND C.AktifMi = 1
  AND (@FirmaId IS NULL OR C.FirmaId = @FirmaId)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PersonelId", personelId);
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

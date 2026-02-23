using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlCihazGrupRepository : ICihazGrupRepository
    {
        private readonly string _connStr;

        public SqlCihazGrupRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IEnumerable<CihazGrubu> GetGruplar(int? firmaId)
        {
            var list = new List<CihazGrubu>();
            using (var conn = new SqlConnection(_connStr))
            {
                var query = "SELECT Id, GrupAdi, FirmaId, KayitTarihi FROM CihazGruplari WHERE (@FirmaId IS NULL OR FirmaId = @FirmaId) ORDER BY GrupAdi";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirmaId", firmaId ?? (object)DBNull.Value);
                
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CihazGrubu
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            GrupAdi = reader["GrupAdi"].ToString(),
                            FirmaId = reader["FirmaId"] != DBNull.Value ? Convert.ToInt32(reader["FirmaId"]) : (int?)null,
                            KayitTarihi = Convert.ToDateTime(reader["KayitTarihi"])
                        });
                    }
                }
            }
            return list;
        }

        public int EkleGrup(CihazGrubu grup)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                var query = @"
                    INSERT INTO CihazGruplari (GrupAdi, FirmaId, KayitTarihi) 
                    VALUES (@GrupAdi, @FirmaId, @KayitTarihi);
                    SELECT SCOPE_IDENTITY();";
                
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@GrupAdi", grup.GrupAdi);
                cmd.Parameters.AddWithValue("@FirmaId", grup.FirmaId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@KayitTarihi", grup.KayitTarihi == default ? DateTime.Now : grup.KayitTarihi);
                
                conn.Open();
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public void SilGrup(int id)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                
                // Foreign key ON DELETE CASCADE varsa Detaylar otomatik silinir, 
                // yoksa diye önce detayları silmek en güvenlisi.
                SilGrupDetaylari(id, conn);

                var query = "DELETE FROM CihazGruplari WHERE Id = @Id";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<CihazGrupDetay> GetGrupDetaylari(int grupId)
        {
            var list = new List<CihazGrupDetay>();
            using (var conn = new SqlConnection(_connStr))
            {
                var query = "SELECT Id, GrupId, CihazId FROM CihazGrupDetay WHERE GrupId = @GrupId";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@GrupId", grupId);
                
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CihazGrupDetay
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            GrupId = Convert.ToInt32(reader["GrupId"]),
                            CihazId = Convert.ToInt32(reader["CihazId"])
                        });
                    }
                }
            }
            return list;
        }

        public void EkleGrupDetaylari(int grupId, IEnumerable<int> cihazIdler)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                
                // Önce varsa eski detayları temizle (güncelleme işlemi için de kullanılabilsin diye)
                SilGrupDetaylari(grupId, conn);

                var query = "INSERT INTO CihazGrupDetay (GrupId, CihazId) VALUES (@GrupId, @CihazId)";
                foreach (var cihazId in cihazIdler)
                {
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@GrupId", grupId);
                        cmd.Parameters.AddWithValue("@CihazId", cihazId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void SilGrupDetaylari(int grupId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                SilGrupDetaylari(grupId, conn);
            }
        }

        private void SilGrupDetaylari(int grupId, SqlConnection conn)
        {
            var query = "DELETE FROM CihazGrupDetay WHERE GrupId = @GrupId";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@GrupId", grupId);
            cmd.ExecuteNonQuery();
        }
    }
}

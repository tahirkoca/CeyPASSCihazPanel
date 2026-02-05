using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlPersonelRepository : IPersonelRepository
    {
        private readonly string _connStr;

        public SqlPersonelRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<Personel> GetAktifPersoneller(int? firmaId)
        {
            var list = new List<Personel>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT PersonelId, Ad, Soyad,KartNo
FROM Kisiler K
WHERE K.IstenCikisTarihi IS NULL
  AND ( @FirmaId IS NULL OR K.FirmaId = @FirmaId )
ORDER BY Ad, Soyad";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirmaId", (object)firmaId ?? DBNull.Value);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int? kartNo = null;
                            if (!dr.IsDBNull(dr.GetOrdinal("KartNo")))
                            {
                                var kartNoValue = dr["KartNo"];
                                if (int.TryParse(kartNoValue?.ToString(), out int kartNoInt))
                                {
                                    kartNo = kartNoInt;
                                }
                            }

                            var personel = new Personel
                            {
                                PersonelId = dr["PersonelId"].ToString(),
                                Ad = dr["Ad"]?.ToString(),
                                Soyad = dr["Soyad"]?.ToString(),
                                KartNo = kartNo
                            };

                            list.Add(personel);
                        }
                    }
                }
            }

            return list;
        }

        public Personel GetById(int personelId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT PersonelId, Ad, Soyad,KartNo
FROM Kisiler
WHERE PersonelId = @Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", personelId);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                            return null;

                        int? kartNo = null;
                        if (!dr.IsDBNull(3))
                        {
                            var kartNoValue = dr[3];
                            if (int.TryParse(kartNoValue?.ToString(), out int kartNoInt))
                            {
                                kartNo = kartNoInt;
                            }
                        }

                        return new Personel
                        {
                            PersonelId = dr.GetString(0),
                            Ad = dr.GetString(1),
                            Soyad = dr.GetString(2),
                            KartNo = kartNo
                        };
                    }
                }
            }
        }
    }
}

using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly string _connStr;

        public SqlUserRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public IList<Kullanici> GetAll()
        {
            var list = new List<Kullanici>();

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT FirmaId, RolId, KullaniciAdi, Sifre
FROM [CeyPASS].[dbo].[SenkronizasyonModulKullanicilar]
ORDER BY KullaniciAdi";

                using (var cmd = new SqlCommand(sql, conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new Kullanici
                        {
                            FirmaId = rd.IsDBNull(0) ? (int?)null : rd.GetInt32(0),
                            RolId = rd.IsDBNull(1) ? (int?)null : rd.GetInt32(1),
                            UserName = rd.GetString(2),
                            Password = rd.GetString(3)
                        });
                    }
                }
            }

            return list;
        }
        public Kullanici GetByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string sql = @"
SELECT FirmaId, RolId, KullaniciAdi, Sifre
FROM [CeyPASS].[dbo].[SenkronizasyonModulKullanicilar]
WHERE KullaniciAdi = @UserName";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                            return null;

                        return new Kullanici
                        {
                            FirmaId = rd.IsDBNull(0) ? (int?)null : rd.GetInt32(0),
                            RolId = rd.IsDBNull(1) ? (int?)null : rd.GetInt32(1),
                            UserName = rd.GetString(2),
                            Password = rd.GetString(3)
                        };
                    }
                }
            }
        }
    }
}

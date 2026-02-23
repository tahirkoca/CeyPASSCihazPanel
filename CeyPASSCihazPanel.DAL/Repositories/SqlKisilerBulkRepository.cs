using CeyPASSCihazPanel.DAL.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlKisilerBulkRepository : IKisilerBulkRepository
    {
        private readonly string _connStr;

        public SqlKisilerBulkRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public (int Basarili, int Hatali) BulkUpsert(IEnumerable<IDictionary<string, object>> rows)
        {
            int basarili = 0, hatali = 0;

            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM Kisiler WHERE PersonelId = @PersonelId)
                BEGIN
                    INSERT INTO Kisiler (PersonelId, Ad, Soyad, KartNo, TcKimlikNo, PozisyonId, DogumTarihi,
                        DepartmanId, IseGirisTarihi, IstenCikisTarihi, CalismaStatusu, FirmaId, IsyeriId,
                        CalismaSekli, CepTel, Fotograf, KayitTarihi, Email, PuantajYapilirMi, BolumId)
                    VALUES (@PersonelId, @Ad, @Soyad, @KartNo, @TcKimlikNo, @PozisyonId, @DogumTarihi,
                        @DepartmanId, @IseGirisTarihi, @IstenCikisTarihi, @CalismaStatusu, @FirmaId, @IsyeriId,
                        @CalismaSekli, @CepTel, @Fotograf, @KayitTarihi, @Email, @PuantajYapilirMi, @BolumId)
                END
                ELSE
                BEGIN
                    UPDATE Kisiler SET
                        Ad=@Ad, Soyad=@Soyad, KartNo=@KartNo, TcKimlikNo=@TcKimlikNo, PozisyonId=@PozisyonId,
                        DogumTarihi=@DogumTarihi, DepartmanId=@DepartmanId, IseGirisTarihi=@IseGirisTarihi,
                        IstenCikisTarihi=@IstenCikisTarihi, CalismaStatusu=@CalismaStatusu, FirmaId=@FirmaId,
                        IsyeriId=@IsyeriId, CalismaSekli=@CalismaSekli, CepTel=@CepTel, Fotograf=@Fotograf,
                        Email=@Email, PuantajYapilirMi=@PuantajYapilirMi, BolumId=@BolumId
                    WHERE PersonelId=@PersonelId
                END";

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var row in rows)
                {
                    try
                    {
                        string personelId = GetStr(row, "PersonelId");
                        if (string.IsNullOrWhiteSpace(personelId)) continue;

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@PersonelId", personelId);
                            cmd.Parameters.AddWithValue("@Ad", GetStr(row, "Ad"));
                            cmd.Parameters.AddWithValue("@Soyad", GetStr(row, "Soyad"));
                            cmd.Parameters.AddWithValue("@KartNo", GetNullable(row, "KartNo"));
                            cmd.Parameters.AddWithValue("@TcKimlikNo", GetStr(row, "TcKimlikNo"));
                            cmd.Parameters.AddWithValue("@PozisyonId", GetNullable(row, "PozisyonId"));
                            cmd.Parameters.AddWithValue("@DogumTarihi", GetNullableDate(row, "DogumTarihi"));
                            cmd.Parameters.AddWithValue("@DepartmanId", GetNullable(row, "DepartmanId"));
                            cmd.Parameters.AddWithValue("@IseGirisTarihi", GetNullableDate(row, "IseGirisTarihi"));
                            cmd.Parameters.AddWithValue("@IstenCikisTarihi", GetNullableDate(row, "IstenCikisTarihi"));
                            cmd.Parameters.AddWithValue("@CalismaStatusu", GetNullable(row, "CalismaStatusu"));
                            cmd.Parameters.AddWithValue("@FirmaId", GetNullable(row, "FirmaId") ?? (object)1);
                            cmd.Parameters.AddWithValue("@IsyeriId", GetNullable(row, "IsyeriId"));
                            cmd.Parameters.AddWithValue("@CalismaSekli", GetStr(row, "CalismaSekli"));
                            cmd.Parameters.AddWithValue("@CepTel", GetStr(row, "CepTel"));
                            cmd.Parameters.Add("@Fotograf", SqlDbType.Image).Value = DBNull.Value;
                            cmd.Parameters.AddWithValue("@KayitTarihi", GetNullableDate(row, "KayitTarihi") ?? (object)DateTime.Now);
                            cmd.Parameters.AddWithValue("@Email", GetStr(row, "Email"));
                            cmd.Parameters.AddWithValue("@PuantajYapilirMi", GetBool(row, "PuantajYapilirMi"));
                            cmd.Parameters.AddWithValue("@BolumId", GetNullable(row, "BolumId"));
                            cmd.ExecuteNonQuery();
                            basarili++;
                        }
                    }
                    catch
                    {
                        hatali++;
                    }
                }
            }
            return (basarili, hatali);
        }

        private static string GetStr(IDictionary<string, object> row, string key)
            => row.ContainsKey(key) ? row[key]?.ToString() ?? "" : "";

        private static object GetNullable(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            return string.IsNullOrWhiteSpace(val) ? DBNull.Value : (object)val;
        }

        private static object GetNullableDate(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
            return DateTime.TryParse(val, out var dt) ? (object)dt : DBNull.Value;
        }

        private static object GetBool(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
            return (object)(val == "1" || val.ToLower() == "true");
        }
    }
}

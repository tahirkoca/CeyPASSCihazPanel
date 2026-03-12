using CeyPASSCihazPanel.DAL.Abstractions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

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
            var hataMesajlari = new List<string>();

            // INSERT: Tüm alanlar dahil (Ad, Soyad, Fotograf dahil)
            // UPDATE: Ad, Soyad, Fotograf HARİÇ - bunlar asla güncellenmez
            //         Diğer alanlar COALESCE ile: Excel'de dolu ise güncelle, boş ise mevcut değeri koru
            const string sql = @"
                UPDATE Kisiler SET
                    KartNo       = COALESCE(@KartNo, KartNo),
                    TcKimlikNo   = COALESCE(@TcKimlikNo, TcKimlikNo),
                    PozisyonId   = COALESCE(@PozisyonId, PozisyonId),
                    DogumTarihi  = COALESCE(@DogumTarihi, DogumTarihi),
                    DepartmanId  = COALESCE(@DepartmanId, DepartmanId),
                    IseGirisTarihi   = COALESCE(@IseGirisTarihi, IseGirisTarihi),
                    IstenCikisTarihi = COALESCE(@IstenCikisTarihi, IstenCikisTarihi),
                    CalismaStatusu   = COALESCE(@CalismaStatusu, CalismaStatusu),
                    FirmaId      = COALESCE(@FirmaId, FirmaId),
                    IsyeriId     = COALESCE(@IsyeriId, IsyeriId),
                    CalismaSekli = COALESCE(@CalismaSekli, CalismaSekli),
                    CepTel       = COALESCE(@CepTel, CepTel),
                    KayitTarihi  = COALESCE(@KayitTarihi, KayitTarihi),
                    Email        = COALESCE(@Email, Email),
                    PuantajYapilirMi = COALESCE(@PuantajYapilirMi, PuantajYapilirMi),
                    BolumId      = COALESCE(@BolumId, BolumId),
                    ZiyaretciMi  = COALESCE(@ZiyaretciMi, ZiyaretciMi),
                    AracKartiMi  = COALESCE(@AracKartiMi, AracKartiMi),
                    TaseronCalisanMi = COALESCE(@TaseronCalisanMi, TaseronCalisanMi)
                WHERE PersonelId = @PersonelId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO Kisiler (PersonelId, Ad, Soyad, KartNo, TcKimlikNo, PozisyonId, DogumTarihi,
                        DepartmanId, IseGirisTarihi, IstenCikisTarihi, CalismaStatusu, FirmaId, IsyeriId,
                        CalismaSekli, CepTel, KayitTarihi, Email, PuantajYapilirMi, BolumId, ZiyaretciMi, AracKartiMi, TaseronCalisanMi)
                    VALUES (@PersonelId, @Ad, @Soyad, @KartNo, @TcKimlikNo, @PozisyonId, @DogumTarihi,
                        @DepartmanId, @IseGirisTarihi, @IstenCikisTarihi, @CalismaStatusu, @FirmaId, @IsyeriId,
                        @CalismaSekli, @CepTel, @KayitTarihi, @Email, @PuantajYapilirMi, @BolumId, @ZiyaretciMi, @AracKartiMi, @TaseronCalisanMi);
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
                            // PersonelId: nvarchar(30) NOT NULL
                            cmd.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = personelId;

                            // Ad: nvarchar(100) - sadece INSERT'te kullanılır
                            cmd.Parameters.Add("@Ad", SqlDbType.NVarChar, 100).Value = GetNullableStr(row, "Ad") ?? (object)"";

                            // Soyad: nvarchar(100) - sadece INSERT'te kullanılır
                            cmd.Parameters.Add("@Soyad", SqlDbType.NVarChar, 100).Value = GetNullableStr(row, "Soyad") ?? (object)"";

                            // KartNo: nvarchar(30)
                            cmd.Parameters.Add("@KartNo", SqlDbType.NVarChar, 30).Value = GetNullableStr(row, "KartNo") ?? (object)DBNull.Value;

                            // TcKimlikNo: nvarchar(11)
                            cmd.Parameters.Add("@TcKimlikNo", SqlDbType.NVarChar, 11).Value = GetNullableStr(row, "TcKimlikNo") ?? (object)DBNull.Value;

                            // PozisyonId: int
                            cmd.Parameters.Add("@PozisyonId", SqlDbType.Int).Value = GetNullableInt(row, "PozisyonId");

                            // DogumTarihi: date
                            cmd.Parameters.Add("@DogumTarihi", SqlDbType.Date).Value = GetNullableDate(row, "DogumTarihi");

                            // DepartmanId: int
                            cmd.Parameters.Add("@DepartmanId", SqlDbType.Int).Value = GetNullableInt(row, "DepartmanId");

                            // IseGirisTarihi: date
                            cmd.Parameters.Add("@IseGirisTarihi", SqlDbType.Date).Value = GetNullableDate(row, "IseGirisTarihi");

                            // IstenCikisTarihi: date
                            cmd.Parameters.Add("@IstenCikisTarihi", SqlDbType.Date).Value = GetNullableDate(row, "IstenCikisTarihi");

                            // CalismaStatusu: nvarchar(30)
                            cmd.Parameters.Add("@CalismaStatusu", SqlDbType.NVarChar, 30).Value = GetNullableStr(row, "CalismaStatusu") ?? (object)DBNull.Value;

                            // FirmaId: int (varsayılan 1)
                            cmd.Parameters.Add("@FirmaId", SqlDbType.Int).Value = GetNullableInt(row, "FirmaId") != DBNull.Value ? GetNullableInt(row, "FirmaId") : (object)1;

                            // IsyeriId: int
                            cmd.Parameters.Add("@IsyeriId", SqlDbType.Int).Value = GetNullableInt(row, "IsyeriId");

                            // CalismaSekli: nvarchar(30)
                            cmd.Parameters.Add("@CalismaSekli", SqlDbType.NVarChar, 30).Value = GetNullableStr(row, "CalismaSekli") ?? (object)DBNull.Value;

                            // CepTel: nvarchar(14)
                            cmd.Parameters.Add("@CepTel", SqlDbType.NVarChar, 14).Value = GetNullableStr(row, "CepTel") ?? (object)DBNull.Value;

                            // KayitTarihi: date (varsayılan bugün)
                            var kayitTarihi = GetNullableDate(row, "KayitTarihi");
                            cmd.Parameters.Add("@KayitTarihi", SqlDbType.Date).Value = kayitTarihi != DBNull.Value ? kayitTarihi : (object)DateTime.Now.Date;

                            // Email: nvarchar(50)
                            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = GetNullableStr(row, "Email") ?? (object)DBNull.Value;

                            // PuantajYapilirMi: bit
                            cmd.Parameters.Add("@PuantajYapilirMi", SqlDbType.Bit).Value = GetNullableBit(row, "PuantajYapilirMi");

                            // BolumId: int
                            cmd.Parameters.Add("@BolumId", SqlDbType.Int).Value = GetNullableInt(row, "BolumId");

                            // ZiyaretciMi / AracKartiMi / TaseronCalisanMi: bit
                            cmd.Parameters.Add("@ZiyaretciMi", SqlDbType.Bit).Value = GetNullableBit(row, "ZiyaretciMi");
                            cmd.Parameters.Add("@AracKartiMi", SqlDbType.Bit).Value = GetNullableBit(row, "AracKartiMi");
                            cmd.Parameters.Add("@TaseronCalisanMi", SqlDbType.Bit).Value = GetNullableBit(row, "TaseronCalisanMi");

                            cmd.ExecuteNonQuery();
                            basarili++;
                        }
                    }
                    catch (Exception ex)
                    {
                        hatali++;
                        string pid = GetStr(row, "PersonelId");
                        if (hataMesajlari.Count < 5)
                            hataMesajlari.Add($"PersonelId={pid}: {ex.Message}");
                    }
                }
            }

            if (hataMesajlari.Count > 0)
                throw new Exception($"Toplam {hatali} satırda hata oluştu.\n" + string.Join("\n", hataMesajlari));

            return (basarili, hatali);
        }

        // ── Yardımcı metotlar ──────────────────────────────────────────────────

        private static string GetStr(IDictionary<string, object> row, string key)
            => row.ContainsKey(key) ? row[key]?.ToString() ?? "" : "";

        /// <summary>Boş veya null ise null döner, dolu ise string döner</summary>
        private static string GetNullableStr(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return null;
            var val = row[key]?.ToString();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }

        /// <summary>int parse eder, başarısız veya boş ise DBNull.Value döner</summary>
        private static object GetNullableInt(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
            return int.TryParse(val, out var v) ? (object)v : DBNull.Value;
        }

        /// <summary>DateTime parse eder, başarısız veya boş ise DBNull.Value döner</summary>
        private static object GetNullableDate(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
            return DateTime.TryParse(val, out var dt) ? (object)dt : DBNull.Value;
        }

        /// <summary>bit (bool) parse eder, başarısız veya boş ise DBNull.Value döner</summary>
        private static object GetNullableBit(IDictionary<string, object> row, string key)
        {
            if (!row.ContainsKey(key)) return DBNull.Value;
            var val = row[key]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
            return (object)(val == "1" || val.ToLower() == "true");
        }
    }
}

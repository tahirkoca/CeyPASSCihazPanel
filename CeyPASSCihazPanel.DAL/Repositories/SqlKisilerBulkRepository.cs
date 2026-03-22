using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
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

        public BulkUpsertResult BulkUpsert(IEnumerable<IDictionary<string, object>> rows)
        {
            var result = new BulkUpsertResult();

            // INSERT: Tüm alanlar dahil (Ad, Soyad, Fotograf dahil)
            // UPDATE: Ad, Soyad, Fotograf HARİÇ - bunlar asla güncellenmez
            //         Diğer alanlar COALESCE ile: Excel'de dolu ise güncelle, boş ise mevcut değeri koru
            const string sqlSelect = @"
                SELECT
                    KartNo,
                    TcKimlikNo,
                    PozisyonId,
                    DogumTarihi,
                    DepartmanId,
                    IseGirisTarihi,
                    IstenCikisTarihi,
                    CalismaStatusu,
                    FirmaId,
                    IsyeriId,
                    CalismaSekli,
                    CepTel,
                    KayitTarihi,
                    Email,
                    PuantajYapilirMi,
                    BolumId,
                    ZiyaretciMi,
                    AracKartiMi,
                    TaseronCalisanMi
                FROM Kisiler
                WHERE PersonelId = @PersonelId;";

            const string sqlUpdate = @"
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
                WHERE PersonelId = @PersonelId;";

            const string sqlInsert = @"
                INSERT INTO Kisiler (PersonelId, Ad, Soyad, KartNo, TcKimlikNo, PozisyonId, DogumTarihi,
                    DepartmanId, IseGirisTarihi, IstenCikisTarihi, CalismaStatusu, FirmaId, IsyeriId,
                    CalismaSekli, CepTel, KayitTarihi, Email, PuantajYapilirMi, BolumId, ZiyaretciMi, AracKartiMi, TaseronCalisanMi)
                VALUES (@PersonelId, @Ad, @Soyad, @KartNo, @TcKimlikNo, @PozisyonId, @DogumTarihi,
                    @DepartmanId, @IseGirisTarihi, @IstenCikisTarihi, @CalismaStatusu, @FirmaId, @IsyeriId,
                    @CalismaSekli, @CepTel, @KayitTarihi, @Email, @PuantajYapilirMi, @BolumId, @ZiyaretciMi, @AracKartiMi, @TaseronCalisanMi);";

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var row in rows)
                {
                    try
                    {
                        string personelId = GetStr(row, "PersonelId");
                        if (string.IsNullOrWhiteSpace(personelId))
                        {
                            result.Skipped++;
                            continue;
                        }

                        result.Total++;

                        var ad = GetNullableStr(row, "Ad") ?? "";
                        var soyad = GetNullableStr(row, "Soyad") ?? "";
                        var kartNo = GetNullableStr(row, "KartNo");
                        var tc = GetNullableStr(row, "TcKimlikNo");
                        var pozisyonId = GetNullableInt(row, "PozisyonId");
                        var dogumTarihi = GetNullableDate(row, "DogumTarihi");
                        var departmanId = GetNullableInt(row, "DepartmanId");
                        var iseGiris = GetNullableDate(row, "IseGirisTarihi");
                        var istenCikis = GetNullableDate(row, "IstenCikisTarihi");
                        var calismaStatusu = GetNullableStr(row, "CalismaStatusu");
                        var firmaIdObj = GetNullableInt(row, "FirmaId");
                        var isyeriId = GetNullableInt(row, "IsyeriId");
                        var calismaSekli = GetNullableStr(row, "CalismaSekli");
                        var cepTel = GetNullableStr(row, "CepTel");
                        var kayitTarihiObj = GetNullableDate(row, "KayitTarihi");
                        var email = GetNullableStr(row, "Email");
                        var puantaj = GetNullableBit(row, "PuantajYapilirMi");
                        var bolumId = GetNullableInt(row, "BolumId");
                        var ziyaretci = GetNullableBit(row, "ZiyaretciMi");
                        var aracKarti = GetNullableBit(row, "AracKartiMi");
                        var taseron = GetNullableBit(row, "TaseronCalisanMi");

                        bool exists = false;

                        string dbKartNo = null;
                        string dbTc = null;
                        int? dbPozisyonId = null;
                        DateTime? dbDogum = null;
                        int? dbDepartmanId = null;
                        DateTime? dbIseGiris = null;
                        DateTime? dbIstenCikis = null;
                        string dbCalismaStatusu = null;
                        int? dbFirmaId = null;
                        int? dbIsyeriId = null;
                        string dbCalismaSekli = null;
                        string dbCepTel = null;
                        DateTime? dbKayitTarihi = null;
                        string dbEmail = null;
                        bool? dbPuantaj = null;
                        int? dbBolumId = null;
                        bool? dbZiyaretci = null;
                        bool? dbAracKarti = null;
                        bool? dbTaseron = null;

                        using (var cmdSel = new SqlCommand(sqlSelect, conn))
                        {
                            cmdSel.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = personelId;
                            using (var r = cmdSel.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    exists = true;
                                    dbKartNo = r.IsDBNull(0) ? null : r.GetString(0);
                                    dbTc = r.IsDBNull(1) ? null : r.GetString(1);
                                    dbPozisyonId = r.IsDBNull(2) ? (int?)null : r.GetInt32(2);
                                    dbDogum = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3);
                                    dbDepartmanId = r.IsDBNull(4) ? (int?)null : r.GetInt32(4);
                                    dbIseGiris = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5);
                                    dbIstenCikis = r.IsDBNull(6) ? (DateTime?)null : r.GetDateTime(6);
                                    dbCalismaStatusu = r.IsDBNull(7) ? null : r.GetString(7);
                                    dbFirmaId = r.IsDBNull(8) ? (int?)null : r.GetInt32(8);
                                    dbIsyeriId = r.IsDBNull(9) ? (int?)null : r.GetInt32(9);
                                    dbCalismaSekli = r.IsDBNull(10) ? null : r.GetString(10);
                                    dbCepTel = r.IsDBNull(11) ? null : r.GetString(11);
                                    dbKayitTarihi = r.IsDBNull(12) ? (DateTime?)null : r.GetDateTime(12);
                                    dbEmail = r.IsDBNull(13) ? null : r.GetString(13);
                                    dbPuantaj = r.IsDBNull(14) ? (bool?)null : r.GetBoolean(14);
                                    dbBolumId = r.IsDBNull(15) ? (int?)null : r.GetInt32(15);
                                    dbZiyaretci = r.IsDBNull(16) ? (bool?)null : r.GetBoolean(16);
                                    dbAracKarti = r.IsDBNull(17) ? (bool?)null : r.GetBoolean(17);
                                    dbTaseron = r.IsDBNull(18) ? (bool?)null : r.GetBoolean(18);
                                }
                            }
                        }

                        if (!exists)
                        {
                            using (var cmdIns = new SqlCommand(sqlInsert, conn))
                            {
                                cmdIns.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = personelId;
                                cmdIns.Parameters.Add("@Ad", SqlDbType.NVarChar, 100).Value = ad;
                                cmdIns.Parameters.Add("@Soyad", SqlDbType.NVarChar, 100).Value = soyad;
                                cmdIns.Parameters.Add("@KartNo", SqlDbType.NVarChar, 30).Value = (object)kartNo ?? DBNull.Value;
                                cmdIns.Parameters.Add("@TcKimlikNo", SqlDbType.NVarChar, 11).Value = (object)tc ?? DBNull.Value;
                                cmdIns.Parameters.Add("@PozisyonId", SqlDbType.Int).Value = pozisyonId;
                                cmdIns.Parameters.Add("@DogumTarihi", SqlDbType.Date).Value = dogumTarihi;
                                cmdIns.Parameters.Add("@DepartmanId", SqlDbType.Int).Value = departmanId;
                                cmdIns.Parameters.Add("@IseGirisTarihi", SqlDbType.Date).Value = iseGiris;
                                cmdIns.Parameters.Add("@IstenCikisTarihi", SqlDbType.Date).Value = istenCikis;
                                cmdIns.Parameters.Add("@CalismaStatusu", SqlDbType.NVarChar, 30).Value = (object)calismaStatusu ?? DBNull.Value;
                                cmdIns.Parameters.Add("@FirmaId", SqlDbType.Int).Value = firmaIdObj != DBNull.Value ? firmaIdObj : (object)1;
                                cmdIns.Parameters.Add("@IsyeriId", SqlDbType.Int).Value = isyeriId;
                                cmdIns.Parameters.Add("@CalismaSekli", SqlDbType.NVarChar, 30).Value = (object)calismaSekli ?? DBNull.Value;
                                cmdIns.Parameters.Add("@CepTel", SqlDbType.NVarChar, 14).Value = (object)cepTel ?? DBNull.Value;
                                cmdIns.Parameters.Add("@KayitTarihi", SqlDbType.Date).Value = kayitTarihiObj != DBNull.Value ? kayitTarihiObj : (object)DateTime.Now.Date;
                                cmdIns.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = (object)email ?? DBNull.Value;
                                cmdIns.Parameters.Add("@PuantajYapilirMi", SqlDbType.Bit).Value = puantaj;
                                cmdIns.Parameters.Add("@BolumId", SqlDbType.Int).Value = bolumId;
                                cmdIns.Parameters.Add("@ZiyaretciMi", SqlDbType.Bit).Value = ziyaretci;
                                cmdIns.Parameters.Add("@AracKartiMi", SqlDbType.Bit).Value = aracKarti;
                                cmdIns.Parameters.Add("@TaseronCalisanMi", SqlDbType.Bit).Value = taseron;
                                cmdIns.ExecuteNonQuery();
                            }
                            result.Inserted++;
                            continue;
                        }

                        bool willChange = false;
                        if (kartNo != null && !string.Equals(kartNo, dbKartNo, StringComparison.Ordinal)) willChange = true;
                        if (tc != null && !string.Equals(tc, dbTc, StringComparison.Ordinal)) willChange = true;
                        if (pozisyonId != DBNull.Value && Convert.ToInt32(pozisyonId) != (dbPozisyonId ?? Convert.ToInt32(pozisyonId))) willChange = true;
                        if (dogumTarihi != DBNull.Value)
                        {
                            var newDate = ((DateTime)dogumTarihi).Date;
                            var oldDate = dbDogum?.Date;
                            if (oldDate == null || newDate != oldDate.Value) willChange = true;
                        }
                        if (departmanId != DBNull.Value && Convert.ToInt32(departmanId) != (dbDepartmanId ?? Convert.ToInt32(departmanId))) willChange = true;
                        if (iseGiris != DBNull.Value)
                        {
                            var newDate = ((DateTime)iseGiris).Date;
                            var oldDate = dbIseGiris?.Date;
                            if (oldDate == null || newDate != oldDate.Value) willChange = true;
                        }
                        if (istenCikis != DBNull.Value)
                        {
                            var newDate = ((DateTime)istenCikis).Date;
                            var oldDate = dbIstenCikis?.Date;
                            if (oldDate == null || newDate != oldDate.Value) willChange = true;
                        }
                        if (calismaStatusu != null && !string.Equals(calismaStatusu, dbCalismaStatusu, StringComparison.Ordinal)) willChange = true;
                        if (firmaIdObj != DBNull.Value && Convert.ToInt32(firmaIdObj) != (dbFirmaId ?? Convert.ToInt32(firmaIdObj))) willChange = true;
                        if (isyeriId != DBNull.Value && Convert.ToInt32(isyeriId) != (dbIsyeriId ?? Convert.ToInt32(isyeriId))) willChange = true;
                        if (calismaSekli != null && !string.Equals(calismaSekli, dbCalismaSekli, StringComparison.Ordinal)) willChange = true;
                        if (cepTel != null && !string.Equals(cepTel, dbCepTel, StringComparison.Ordinal)) willChange = true;
                        if (kayitTarihiObj != DBNull.Value)
                        {
                            var newDate = ((DateTime)kayitTarihiObj).Date;
                            var oldDate = dbKayitTarihi?.Date;
                            if (oldDate == null || newDate != oldDate.Value) willChange = true;
                        }
                        if (email != null && !string.Equals(email, dbEmail, StringComparison.OrdinalIgnoreCase)) willChange = true;
                        if (puantaj != DBNull.Value && Convert.ToBoolean(puantaj) != (dbPuantaj ?? Convert.ToBoolean(puantaj))) willChange = true;
                        if (bolumId != DBNull.Value && Convert.ToInt32(bolumId) != (dbBolumId ?? Convert.ToInt32(bolumId))) willChange = true;
                        if (ziyaretci != DBNull.Value && Convert.ToBoolean(ziyaretci) != (dbZiyaretci ?? Convert.ToBoolean(ziyaretci))) willChange = true;
                        if (aracKarti != DBNull.Value && Convert.ToBoolean(aracKarti) != (dbAracKarti ?? Convert.ToBoolean(aracKarti))) willChange = true;
                        if (taseron != DBNull.Value && Convert.ToBoolean(taseron) != (dbTaseron ?? Convert.ToBoolean(taseron))) willChange = true;

                        if (!willChange)
                        {
                            result.NoChange++;

                            bool hasAnyBlankUpdatableField =
                                kartNo == null ||
                                tc == null ||
                                pozisyonId == DBNull.Value ||
                                dogumTarihi == DBNull.Value ||
                                departmanId == DBNull.Value ||
                                iseGiris == DBNull.Value ||
                                istenCikis == DBNull.Value ||
                                calismaStatusu == null ||
                                firmaIdObj == DBNull.Value ||
                                isyeriId == DBNull.Value ||
                                calismaSekli == null ||
                                cepTel == null ||
                                kayitTarihiObj == DBNull.Value ||
                                email == null ||
                                puantaj == DBNull.Value ||
                                bolumId == DBNull.Value ||
                                ziyaretci == DBNull.Value ||
                                aracKarti == DBNull.Value ||
                                taseron == DBNull.Value;

                            if (hasAnyBlankUpdatableField) result.BlankNoOp++;
                            else result.SameData++;
                            continue;
                        }

                        using (var cmdUpd = new SqlCommand(sqlUpdate, conn))
                        {
                            cmdUpd.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = personelId;
                            cmdUpd.Parameters.Add("@KartNo", SqlDbType.NVarChar, 30).Value = (object)kartNo ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@TcKimlikNo", SqlDbType.NVarChar, 11).Value = (object)tc ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@PozisyonId", SqlDbType.Int).Value = pozisyonId;
                            cmdUpd.Parameters.Add("@DogumTarihi", SqlDbType.Date).Value = dogumTarihi;
                            cmdUpd.Parameters.Add("@DepartmanId", SqlDbType.Int).Value = departmanId;
                            cmdUpd.Parameters.Add("@IseGirisTarihi", SqlDbType.Date).Value = iseGiris;
                            cmdUpd.Parameters.Add("@IstenCikisTarihi", SqlDbType.Date).Value = istenCikis;
                            cmdUpd.Parameters.Add("@CalismaStatusu", SqlDbType.NVarChar, 30).Value = (object)calismaStatusu ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@FirmaId", SqlDbType.Int).Value = firmaIdObj;
                            cmdUpd.Parameters.Add("@IsyeriId", SqlDbType.Int).Value = isyeriId;
                            cmdUpd.Parameters.Add("@CalismaSekli", SqlDbType.NVarChar, 30).Value = (object)calismaSekli ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@CepTel", SqlDbType.NVarChar, 14).Value = (object)cepTel ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@KayitTarihi", SqlDbType.Date).Value = kayitTarihiObj;
                            cmdUpd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = (object)email ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@PuantajYapilirMi", SqlDbType.Bit).Value = puantaj;
                            cmdUpd.Parameters.Add("@BolumId", SqlDbType.Int).Value = bolumId;
                            cmdUpd.Parameters.Add("@ZiyaretciMi", SqlDbType.Bit).Value = ziyaretci;
                            cmdUpd.Parameters.Add("@AracKartiMi", SqlDbType.Bit).Value = aracKarti;
                            cmdUpd.Parameters.Add("@TaseronCalisanMi", SqlDbType.Bit).Value = taseron;
                            cmdUpd.ExecuteNonQuery();
                        }

                        result.Updated++;
                    }
                    catch (Exception ex)
                    {
                        string pid = GetStr(row, "PersonelId");
                        result.Failed++;
                        if (result.ErrorSamples.Count < 5)
                            result.ErrorSamples.Add($"PersonelId={pid}: {ex.Message}");
                    }
                }
            }
            return result;
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

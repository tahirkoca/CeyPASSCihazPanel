using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASSCihazPanel.Business.Services
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly IKisilerBulkRepository _kisiRepo;
        private readonly IYemekhaneGirisLimitRepository _yemekhaneRepo;

        public BulkUploadService(
            IKisilerBulkRepository kisiRepo,
            IYemekhaneGirisLimitRepository yemekhaneRepo)
        {
            _kisiRepo = kisiRepo;
            _yemekhaneRepo = yemekhaneRepo;
        }

        // ── Kisiler ─────────────────────────────────────────────────────────────
        public BulkUpsertResult BulkUpsertKisiler(DataTable dt)
        {
            var rows = new List<IDictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                    dict[col.ColumnName] = dr[col];
                rows.Add(dict);
            }
            return _kisiRepo.BulkUpsert(rows);
        }

        // ── YemekhaneGirisLimitler ───────────────────────────────────────────────
        public BulkUpsertResult BulkUpsertYemekhane(DataTable dt)
        {
            var list = new List<YemekhaneGirisLimiti>();
            foreach (DataRow dr in dt.Rows)
            {
                string pid = GetStr(dr, "PersonelId");
                if (string.IsNullOrWhiteSpace(pid)) continue;
                list.Add(new YemekhaneGirisLimiti
                {
                    PersonelId  = pid,
                    GunlukLimit = GetNullableInt(dr, "GunlukLimit"),
                    KayitTarihi = GetNullableDate(dr, "KayitTarihi"),
                    AktifMi     = GetNullableBool(dr, "AktifMi"),
                });
            }
            return _yemekhaneRepo.BulkUpsert(list);
        }

        // ── Şablon DataTable'ları ────────────────────────────────────────────────
        public DataTable GetKisiTemplate()
        {
            var dt = new DataTable();
            dt.Columns.Add("PersonelId", typeof(string));
            dt.Columns.Add("Ad", typeof(string));
            dt.Columns.Add("Soyad", typeof(string));
            dt.Columns.Add("KartNo", typeof(string));
            dt.Columns.Add("TcKimlikNo", typeof(string));
            dt.Columns.Add("PozisyonId", typeof(int));
            dt.Columns.Add("DogumTarihi", typeof(DateTime));
            dt.Columns.Add("DepartmanId", typeof(int));
            dt.Columns.Add("IseGirisTarihi", typeof(DateTime));
            dt.Columns.Add("IstenCikisTarihi", typeof(DateTime));
            dt.Columns.Add("CalismaStatusu", typeof(string));
            dt.Columns.Add("FirmaId", typeof(int));
            dt.Columns.Add("IsyeriId", typeof(int));
            dt.Columns.Add("CalismaSekli", typeof(string));
            dt.Columns.Add("CepTel", typeof(string));
            dt.Columns.Add("Fotograf", typeof(string));
            dt.Columns.Add("KayitTarihi", typeof(DateTime));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("PuantajYapilirMi", typeof(int));
            dt.Columns.Add("BolumId", typeof(int));
            dt.Columns.Add("ZiyaretciMi", typeof(int));
            dt.Columns.Add("AracKartiMi", typeof(int));
            dt.Columns.Add("TaseronCalisanMi", typeof(int));

            var row = dt.NewRow();
            row["PersonelId"]     = "10001";
            row["Ad"]             = "ÖRNEK";
            row["Soyad"]          = "KİŞİ";
            row["KartNo"]         = "12345678";
            row["TcKimlikNo"]     = "12345678901";
            row["PozisyonId"]     = 1;
            row["DogumTarihi"]    = new DateTime(1990, 5, 1);
            row["DepartmanId"]    = 1;
            row["IseGirisTarihi"] = new DateTime(2020, 5, 1);
            row["FirmaId"]        = 1;
            row["IsyeriId"]       = 1;
            row["CepTel"]         = "0555-555-55-55";
            row["KayitTarihi"]    = new DateTime(2020, 5, 1);
            row["CalismaSekli"]   = "9";
            row["Email"]          = "ornek@firma.com";
            row["PuantajYapilirMi"] = 1;
            row["BolumId"]        = 1;
            row["ZiyaretciMi"]    = 0;
            row["AracKartiMi"]    = 0;
            row["TaseronCalisanMi"]= 0;
            dt.Rows.Add(row);
            return dt;
        }

        public DataTable GetYemekhaneTemplate()
        {
            var dt = new DataTable();
            dt.Columns.Add("PersonelId", typeof(string));
            dt.Columns.Add("GunlukLimit", typeof(int));
            dt.Columns.Add("KayitTarihi", typeof(DateTime));
            dt.Columns.Add("AktifMi", typeof(int));

            var row = dt.NewRow();
            row["PersonelId"]  = "10001";
            row["GunlukLimit"] = 1;
            row["KayitTarihi"] = new DateTime(2025, 11, 24);
            row["AktifMi"]     = 1;
            dt.Rows.Add(row);
            return dt;
        }

        // ── Yardımcı parse metotları ─────────────────────────────────────────────
        private static string GetStr(DataRow dr, string col)
            => dr.Table.Columns.Contains(col) ? dr[col]?.ToString() ?? "" : "";

        private static int GetInt(DataRow dr, string col, int def)
        {
            if (!dr.Table.Columns.Contains(col)) return def;
            return int.TryParse(dr[col]?.ToString(), out var v) ? v : def;
        }

        private static int? GetNullableInt(DataRow dr, string col)
        {
            if (!dr.Table.Columns.Contains(col)) return null;
            return int.TryParse(dr[col]?.ToString(), out var v) ? v : (int?)null;
        }

        private static bool GetBool(DataRow dr, string col, bool def)
        {
            if (!dr.Table.Columns.Contains(col)) return def;
            var val = dr[col]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return def;
            return val == "1" || val.ToLower() == "true";
        }

        private static bool? GetNullableBool(DataRow dr, string col)
        {
            if (!dr.Table.Columns.Contains(col)) return null;
            var val = dr[col]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) return null;
            return val == "1" || val.ToLower() == "true";
        }

        private static DateTime? GetNullableDate(DataRow dr, string col)
        {
            if (!dr.Table.Columns.Contains(col)) return null;
            var val = dr[col]?.ToString();
            return DateTime.TryParse(val, out var dt) ? dt : (DateTime?)null;
        }
    }
}

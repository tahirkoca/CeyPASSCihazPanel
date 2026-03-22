using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CeyPASSCihazPanel.DAL.Repositories
{
    public class SqlYemekhaneGirisLimitRepository : IYemekhaneGirisLimitRepository
    {
        private readonly string _connStr;

        public SqlYemekhaneGirisLimitRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["CeyPASS"].ConnectionString;
        }

        public BulkUpsertResult BulkUpsert(IEnumerable<YemekhaneGirisLimiti> rows)
        {
            var result = new BulkUpsertResult();

            const string sqlSelect = @"
                SELECT GunlukLimit, KayitTarihi, AktifMi
                FROM YemekhaneGirisLimitler
                WHERE PersonelId = @PersonelId;";

            const string sqlInsert = @"
                INSERT INTO YemekhaneGirisLimitler (PersonelId, GunlukLimit, KayitTarihi, AktifMi)
                VALUES (@PersonelId, @GunlukLimit, @KayitTarihi, @AktifMi);";

            const string sqlUpdate = @"
                UPDATE YemekhaneGirisLimitler SET
                    GunlukLimit = COALESCE(@GunlukLimit, GunlukLimit),
                    KayitTarihi = COALESCE(@KayitTarihi, KayitTarihi),
                    AktifMi     = COALESCE(@AktifMi, AktifMi)
                WHERE PersonelId = @PersonelId;";

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var y in rows)
                {
                    if (string.IsNullOrWhiteSpace(y.PersonelId))
                    {
                        result.Skipped++;
                        continue;
                    }

                    result.Total++;
                    try
                    {
                        int? dbGunlukLimit = null;
                        DateTime? dbKayitTarihi = null;
                        bool? dbAktifMi = null;
                        bool exists = false;

                        using (var cmdSel = new SqlCommand(sqlSelect, conn))
                        {
                            cmdSel.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = y.PersonelId;
                            using (var r = cmdSel.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    exists = true;
                                    dbGunlukLimit = r.IsDBNull(0) ? (int?)null : r.GetInt32(0);
                                    dbKayitTarihi = r.IsDBNull(1) ? (DateTime?)null : r.GetDateTime(1);
                                    dbAktifMi = r.IsDBNull(2) ? (bool?)null : r.GetBoolean(2);
                                }
                            }
                        }

                        if (!exists)
                        {
                            using (var cmdIns = new SqlCommand(sqlInsert, conn))
                            {
                                cmdIns.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = y.PersonelId;
                                cmdIns.Parameters.Add("@GunlukLimit", SqlDbType.Int).Value = (object)y.GunlukLimit ?? DBNull.Value;
                                cmdIns.Parameters.Add("@KayitTarihi", SqlDbType.DateTime).Value = (object)y.KayitTarihi ?? (object)DateTime.Now;
                                cmdIns.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = (object)y.AktifMi ?? (object)true;
                                cmdIns.ExecuteNonQuery();
                            }
                            result.Inserted++;
                            continue;
                        }

                        bool willChange = false;
                        if (y.GunlukLimit.HasValue && y.GunlukLimit.Value != (dbGunlukLimit ?? y.GunlukLimit.Value)) willChange = true;
                        if (y.KayitTarihi.HasValue)
                        {
                            var dbDate = dbKayitTarihi?.Date;
                            if (dbDate == null || y.KayitTarihi.Value.Date != dbDate.Value) willChange = true;
                        }
                        if (y.AktifMi.HasValue && y.AktifMi.Value != (dbAktifMi ?? y.AktifMi.Value)) willChange = true;

                        if (!willChange)
                        {
                            result.NoChange++;
                            bool hasAnyBlankUpdatableField =
                                !y.GunlukLimit.HasValue ||
                                !y.KayitTarihi.HasValue ||
                                !y.AktifMi.HasValue;

                            if (hasAnyBlankUpdatableField) result.BlankNoOp++;
                            else result.SameData++;
                            continue;
                        }

                        using (var cmdUpd = new SqlCommand(sqlUpdate, conn))
                        {
                            cmdUpd.Parameters.Add("@PersonelId", SqlDbType.NVarChar, 30).Value = y.PersonelId;
                            cmdUpd.Parameters.Add("@GunlukLimit", SqlDbType.Int).Value = (object)y.GunlukLimit ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@KayitTarihi", SqlDbType.DateTime).Value = (object)y.KayitTarihi ?? DBNull.Value;
                            cmdUpd.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = (object)y.AktifMi ?? DBNull.Value;
                            cmdUpd.ExecuteNonQuery();
                        }
                        result.Updated++;
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        if (result.ErrorSamples.Count < 5)
                            result.ErrorSamples.Add($"PersonelId={y.PersonelId}: {ex.Message}");
                    }
                }
            }
            return result;
        }
    }
}

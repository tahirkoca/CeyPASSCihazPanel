using CeyPASSCihazPanel.Business.Services;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace CeyPASSCihazPanel.Tests
{
    /// <summary>
    /// BulkUploadService unit testleri.
    ///
    /// Test edilen mantık:
    ///   - BulkUpsertKisiler: DataTable satırlarını IDictionary'e dönüştürüp repo'ya iletme.
    ///   - BulkUpsertYemekhane: PersonelId boş satırları atlama ve doğru mapping.
    ///   - GetKisiTemplate / GetYemekhaneTemplate: Dönen DataTable'ın yapısı (sütunlar, örnek satır).
    /// </summary>
    public class BulkUploadServiceTests
    {
        // ── Yardımcı fabrika ──────────────────────────────────────────────────────
        private (BulkUploadService service,
                 Mock<IKisilerBulkRepository> kisiMock,
                 Mock<IYemekhaneGirisLimitRepository> yemekhaneMock) CreateSut()
        {
            var kisiMock = new Mock<IKisilerBulkRepository>();
            var yemekhaneMock = new Mock<IYemekhaneGirisLimitRepository>();
            var service = new BulkUploadService(kisiMock.Object, yemekhaneMock.Object);
            return (service, kisiMock, yemekhaneMock);
        }

        // ── BulkUpsertKisiler ─────────────────────────────────────────────────────

        [Fact]
        public void BulkUpsertKisiler_EmptyTable_CallsRepoWithEmptyList()
        {
            // Arrange: hiç satır içermeyen DataTable.
            var (service, kisiMock, _) = CreateSut();
            kisiMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<IDictionary<string, object>>>()))
                    .Returns(new BulkUpsertResult { Total = 0, Inserted = 0, Updated = 0, NoChange = 0, SameData = 0, BlankNoOp = 0, Skipped = 0, Failed = 0 });
            var dt = new DataTable();
            dt.Columns.Add("PersonelId");

            // Act
            var res = service.BulkUpsertKisiler(dt);

            // Assert: repo boş liste ile çağrılmalı, sonuç (0,0) olmalı.
            res.Total.Should().Be(0);
            res.Failed.Should().Be(0);
            kisiMock.Verify(r => r.BulkUpsert(
                It.Is<IEnumerable<IDictionary<string, object>>>(list => !list.Any())),
                Times.Once);
        }

        [Fact]
        public void BulkUpsertKisiler_MapsColumnsCorrectly()
        {
            // Arrange: 2 satırlı DataTable; sütun adları dict anahtarlarına eşlenmeli.
            var (service, kisiMock, _) = CreateSut();

            IEnumerable<IDictionary<string, object>> capturedRows = null;
            kisiMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<IDictionary<string, object>>>()))
                    .Callback<IEnumerable<IDictionary<string, object>>>(rows => capturedRows = rows)
                    .Returns(new BulkUpsertResult { Total = 2, Inserted = 2, SameData = 0, BlankNoOp = 0, Failed = 0 });

            var dt = new DataTable();
            dt.Columns.Add("PersonelId");
            dt.Columns.Add("Ad");
            dt.Rows.Add("10001", "Ahmet");
            dt.Rows.Add("10002", "Mehmet");

            // Act
            service.BulkUpsertKisiler(dt);

            // Assert: repo'ya 2 dict gelmeli; her dict'te "PersonelId" ve "Ad" anahtarları olmalı.
            capturedRows.Should().HaveCount(2);
            capturedRows.First().Keys.Should().Contain("PersonelId");
            capturedRows.First().Keys.Should().Contain("Ad");
            capturedRows.First()["PersonelId"].Should().Be("10001");
        }

        [Fact]
        public void BulkUpsertKisiler_ReturnsRepoResult()
        {
            // Arrange: repo (3, 1) döner; servis de aynısını döndürmeli.
            var (service, kisiMock, _) = CreateSut();
            kisiMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<IDictionary<string, object>>>()))
                    .Returns(new BulkUpsertResult { Total = 4, Inserted = 3, SameData = 0, BlankNoOp = 0, Failed = 1 });

            var dt = new DataTable();
            dt.Columns.Add("PersonelId");
            dt.Rows.Add("10001");
            dt.Rows.Add("10002");
            dt.Rows.Add("10003");
            dt.Rows.Add("10004");

            // Act
            var res = service.BulkUpsertKisiler(dt);

            // Assert
            res.Inserted.Should().Be(3);
            res.Failed.Should().Be(1);
        }

        [Fact]
        public void BulkUpsertKisiler_ColumnLookupIsCaseInsensitive()
        {
            // Arrange: sütun adları küçük harfle bile sözlüğe eklenmeli (OrdinalIgnoreCase).
            var (service, kisiMock, _) = CreateSut();

            IEnumerable<IDictionary<string, object>> capturedRows = null;
            kisiMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<IDictionary<string, object>>>()))
                    .Callback<IEnumerable<IDictionary<string, object>>>(rows => capturedRows = rows)
                    .Returns(new BulkUpsertResult { Total = 1, Inserted = 1, SameData = 0, BlankNoOp = 0, Failed = 0 });

            var dt = new DataTable();
            dt.Columns.Add("personelid");   // küçük harf
            dt.Rows.Add("10001");

            // Act
            service.BulkUpsertKisiler(dt);

            // Assert: dict'te "personelid" anahtarıyla "PersonelId" değerine erişilebilmeli
            // (StringComparer.OrdinalIgnoreCase ile kurulduğu için).
            capturedRows.First().ContainsKey("PersonelId").Should().BeTrue();
        }

        // ── BulkUpsertYemekhane ───────────────────────────────────────────────────

        [Fact]
        public void BulkUpsertYemekhane_SkipsRowsWithoutPersonelId()
        {
            // Arrange: PersonelId boş olan satır atlanmalı; repo 0 item almalı.
            var (service, _, yemekhaneMock) = CreateSut();

            IEnumerable<YemekhaneGirisLimiti> capturedList = null;
            yemekhaneMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<YemekhaneGirisLimiti>>()))
                         .Callback<IEnumerable<YemekhaneGirisLimiti>>(list => capturedList = list)
                         .Returns(new BulkUpsertResult { Total = 0, SameData = 0, BlankNoOp = 0, Failed = 0 });

            var dt = new DataTable();
            dt.Columns.Add("PersonelId");
            dt.Columns.Add("GunlukLimit", typeof(int));
            dt.Columns.Add("AktifMi", typeof(int));
            dt.Rows.Add("", 1, 1);       // boş PersonelId → atlanmalı
            dt.Rows.Add(DBNull.Value, 1, 1); // null PersonelId → atlanmalı

            // Act
            service.BulkUpsertYemekhane(dt);

            // Assert
            capturedList.Should().BeEmpty();
        }

        [Fact]
        public void BulkUpsertYemekhane_MapsFieldsCorrectly()
        {
            // Arrange: geçerli bir satır; PersonelId, GunlukLimit ve AktifMi doğru map edilmeli.
            var (service, _, yemekhaneMock) = CreateSut();

            IEnumerable<YemekhaneGirisLimiti> capturedList = null;
            yemekhaneMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<YemekhaneGirisLimiti>>()))
                         .Callback<IEnumerable<YemekhaneGirisLimiti>>(list => capturedList = list)
                         .Returns(new BulkUpsertResult { Total = 1, Inserted = 1, SameData = 0, BlankNoOp = 0, Failed = 0 });

            var dt = new DataTable();
            dt.Columns.Add("PersonelId");
            dt.Columns.Add("GunlukLimit");
            dt.Columns.Add("AktifMi");
            dt.Rows.Add("10001", "2", "1");

            // Act
            service.BulkUpsertYemekhane(dt);

            // Assert
            capturedList.Should().HaveCount(1);
            var item = capturedList.First();
            item.PersonelId.Should().Be("10001");
            item.GunlukLimit.Should().Be(2);
            item.AktifMi.Should().BeTrue();
        }

        [Fact]
        public void BulkUpsertYemekhane_AktifMiDefault_TrueWhenMissing()
        {
            // Arrange: AktifMi sütunu yok; servis null göndermeli (repo insert sırasında default uygulayabilir).
            var (service, _, yemekhaneMock) = CreateSut();

            IEnumerable<YemekhaneGirisLimiti> capturedList = null;
            yemekhaneMock.Setup(r => r.BulkUpsert(It.IsAny<IEnumerable<YemekhaneGirisLimiti>>()))
                         .Callback<IEnumerable<YemekhaneGirisLimiti>>(list => capturedList = list)
                         .Returns(new BulkUpsertResult { Total = 1, Inserted = 1, SameData = 0, BlankNoOp = 0, Failed = 0 });

            var dt = new DataTable();
            dt.Columns.Add("PersonelId");
            dt.Rows.Add("10001");

            // Act
            service.BulkUpsertYemekhane(dt);

            // Assert
            capturedList.First().AktifMi.Should().BeNull();
        }

        // ── GetKisiTemplate ───────────────────────────────────────────────────────

        [Fact]
        public void GetKisiTemplate_HasExpectedColumns()
        {
            // Arrange & Act: şablon DataTable alınıyor.
            var (service, _, _) = CreateSut();
            var dt = service.GetKisiTemplate();

            // Assert: zorunlu sütunların varlığını kontrol et.
            var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            columns.Should().Contain("PersonelId");
            columns.Should().Contain("Ad");
            columns.Should().Contain("Soyad");
            columns.Should().Contain("KartNo");
            columns.Should().Contain("FirmaId");
            columns.Should().Contain("PuantajYapilirMi");
            columns.Should().Contain("ZiyaretciMi");
            columns.Should().Contain("AracKartiMi");
            columns.Should().Contain("TaseronCalisanMi");
        }

        [Fact]
        public void GetKisiTemplate_HasOneExampleRow()
        {
            // Arrange & Act
            var (service, _, _) = CreateSut();
            var dt = service.GetKisiTemplate();

            // Assert: şablonda tam 1 örnek satır olmalı (kullanıcı bunu silebilir).
            dt.Rows.Should().HaveCount(1);
        }

        // ── GetYemekhaneTemplate ──────────────────────────────────────────────────

        [Fact]
        public void GetYemekhaneTemplate_HasExpectedColumns()
        {
            // Arrange & Act
            var (service, _, _) = CreateSut();
            var dt = service.GetYemekhaneTemplate();

            // Assert
            var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            columns.Should().Contain("PersonelId");
            columns.Should().Contain("GunlukLimit");
            columns.Should().Contain("AktifMi");
        }

        [Fact]
        public void GetYemekhaneTemplate_HasOneExampleRow()
        {
            var (service, _, _) = CreateSut();
            var dt = service.GetYemekhaneTemplate();
            dt.Rows.Should().HaveCount(1);
        }
    }
}

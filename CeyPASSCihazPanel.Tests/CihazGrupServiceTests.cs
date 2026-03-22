using CeyPASSCihazPanel.Business.Services;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CeyPASSCihazPanel.Tests
{
    /// <summary>
    /// CihazGrupService unit testleri.
    ///
    /// Servis tamamen ICihazGrupRepository'ye delege eder; bu nedenle testlerin
    /// odak noktaları:
    ///   1. Constructor'ın null kontrolü yapıp yapmadığı.
    ///   2. Her metodun repo'ya doğru parametrelerle iletip iletmediği.
    ///   3. Repo'dan dönen değerin olduğu gibi geri döndürülüp döndürülmediği.
    /// </summary>
    public class CihazGrupServiceTests
    {
        // ── Yardımcı fabrika ──────────────────────────────────────────────────────
        private (CihazGrupService service, Mock<ICihazGrupRepository> repoMock) CreateSut()
        {
            var mock = new Mock<ICihazGrupRepository>();
            var service = new CihazGrupService(mock.Object);
            return (service, mock);
        }

        // ── Constructor testleri ──────────────────────────────────────────────────

        [Fact]
        public void Constructor_NullRepo_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            // Servis null repo ile oluşturulmaya çalışılınca ArgumentNullException fırlatmalı.
            Action act = () => new CihazGrupService(null);
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("repo");
        }

        // ── GetGruplar ────────────────────────────────────────────────────────────

        [Fact]
        public void GetGruplar_DelegatesToRepo_ReturnsSameList()
        {
            // Arrange
            var (service, repoMock) = CreateSut();
            var beklenen = new List<CihazGrubu>
            {
                new CihazGrubu { Id = 1, GrupAdi = "Giriş" },
                new CihazGrubu { Id = 2, GrupAdi = "Çıkış" }
            };
            repoMock.Setup(r => r.GetGruplar(It.IsAny<int?>())).Returns(beklenen);

            // Act
            var sonuc = service.GetGruplar(firmaId: 1);

            // Assert: aynı liste referansı dönmeli.
            sonuc.Should().BeEquivalentTo(beklenen);
            repoMock.Verify(r => r.GetGruplar(1), Times.Once);
        }

        [Fact]
        public void GetGruplar_NullFirmaId_PassesNullToRepo()
        {
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetGruplar(null)).Returns(new List<CihazGrubu>());

            service.GetGruplar(null);

            repoMock.Verify(r => r.GetGruplar(null), Times.Once);
        }

        // ── EkleGrup ──────────────────────────────────────────────────────────────

        [Fact]
        public void EkleGrup_DelegatesToRepo_ReturnsNewId()
        {
            // Arrange: repo yeni gruba 42 id atıyor.
            var (service, repoMock) = CreateSut();
            var yeniGrup = new CihazGrubu { GrupAdi = "Yeni Grup", FirmaId = 1 };
            repoMock.Setup(r => r.EkleGrup(yeniGrup)).Returns(42);

            // Act
            int id = service.EkleGrup(yeniGrup);

            // Assert
            id.Should().Be(42);
            repoMock.Verify(r => r.EkleGrup(yeniGrup), Times.Once);
        }

        // ── SilGrup ───────────────────────────────────────────────────────────────

        [Fact]
        public void SilGrup_DelegatesToRepo()
        {
            // Arrange
            var (service, repoMock) = CreateSut();

            // Act
            service.SilGrup(5);

            // Assert: repo'nun SilGrup(5) metodu tam 1 kez çağrılmış olmalı.
            repoMock.Verify(r => r.SilGrup(5), Times.Once);
        }

        [Fact]
        public void SilGrup_DoesNotCallOtherRepoMethods()
        {
            // Sadece SilGrup çağrılıyor; başka repo metotları tetiklenmemeli.
            var (service, repoMock) = CreateSut();
            service.SilGrup(99);

            repoMock.Verify(r => r.GetGruplar(It.IsAny<int?>()), Times.Never);
            repoMock.Verify(r => r.EkleGrup(It.IsAny<CihazGrubu>()), Times.Never);
        }

        // ── GetGrupDetaylari ──────────────────────────────────────────────────────

        [Fact]
        public void GetGrupDetaylari_DelegatesToRepo()
        {
            // Arrange
            var (service, repoMock) = CreateSut();
            var beklenen = new List<CihazGrupDetay>
            {
                new CihazGrupDetay { Id = 1, GrupId = 3, CihazId = 10 }
            };
            repoMock.Setup(r => r.GetGrupDetaylari(3)).Returns(beklenen);

            // Act
            var sonuc = service.GetGrupDetaylari(3);

            // Assert
            sonuc.Should().BeEquivalentTo(beklenen);
        }

        // ── KaydetGrupCihazlari ───────────────────────────────────────────────────

        [Fact]
        public void KaydetGrupCihazlari_CallsEkleGrupDetaylari_WithCorrectParams()
        {
            // Arrange: 3 cihaz id listesi gönderiliyor.
            var (service, repoMock) = CreateSut();
            var cihazIdler = new List<int> { 10, 20, 30 };

            // Act
            service.KaydetGrupCihazlari(grupId: 7, cihazIdler);

            // Assert: repo'nun EkleGrupDetaylari'nın grupId=7 ve doğru liste ile çağrıldığını doğrula.
            repoMock.Verify(r => r.EkleGrupDetaylari(
                7,
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(cihazIdler))),
                Times.Once);
        }

        [Fact]
        public void KaydetGrupCihazlari_EmptyList_StillCallsRepo()
        {
            // Boş liste ile bile repo çağrılmalı (silme/güncelleme senaryosu olabilir).
            var (service, repoMock) = CreateSut();

            service.KaydetGrupCihazlari(grupId: 1, new List<int>());

            repoMock.Verify(r => r.EkleGrupDetaylari(1, It.IsAny<IEnumerable<int>>()), Times.Once);
        }
    }
}

using CeyPASSCihazPanel.Business.Services;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CeyPASSCihazPanel.Tests
{
    /// <summary>
    /// AdminLookUpService unit testleri.
    ///
    /// Servis tüm metodlarını ilgili repository arayüzlerine delege eder.
    /// Bu testler:
    ///   - Her repo çağrısının doğru parametreyle yapıldığını,
    ///   - Repo'dan dönen sonucun olduğu gibi servisten çıktığını doğrular.
    /// </summary>
    public class AdminLookUpServiceTests
    {
        // ── Mock'lar ve fabrika ───────────────────────────────────────────────────
        private Mock<ICihazRepository>          _cihazMock;
        private Mock<IPersonelRepository>       _personelMock;
        private Mock<IPuantajsizKisiRepository> _kartMock;
        private Mock<IKisiCihazYetkiRepository> _yetkiMock;
        private Mock<IFirmaRepository>          _firmaMock;

        private AdminLookUpService CreateSut()
        {
            _cihazMock    = new Mock<ICihazRepository>();
            _personelMock = new Mock<IPersonelRepository>();
            _kartMock     = new Mock<IPuantajsizKisiRepository>();
            _yetkiMock    = new Mock<IKisiCihazYetkiRepository>();
            _firmaMock    = new Mock<IFirmaRepository>();

            return new AdminLookUpService(
                _cihazMock.Object,
                _personelMock.Object,
                _kartMock.Object,
                _yetkiMock.Object,
                _firmaMock.Object);
        }

        // ── Personel ──────────────────────────────────────────────────────────────

        [Fact]
        public void GetAktifPersoneller_DelegatesToRepo_ReturnsSameList()
        {
            // Arrange
            var service = CreateSut();
            var beklenen = new List<Personel>
            {
                new Personel { PersonelId = "10001", Ad = "Ali", Soyad = "Veli" }
            };
            _personelMock.Setup(r => r.GetAktifPersoneller(1)).Returns(beklenen);

            // Act
            var sonuc = service.GetAktifPersoneller(firmaId: 1);

            // Assert
            sonuc.Should().BeEquivalentTo(beklenen);
            _personelMock.Verify(r => r.GetAktifPersoneller(1), Times.Once);
        }

        [Fact]
        public void GetPersonelById_DelegatesToRepo()
        {
            // Arrange
            var service = CreateSut();
            var personel = new Personel { PersonelId = "10001", Ad = "Ayse" };
            _personelMock.Setup(r => r.GetById(10001)).Returns(personel);

            // Act
            var sonuc = service.GetPersonelById(10001);

            // Assert
            sonuc.Should().Be(personel);
        }

        [Fact]
        public void GetPersonelById_NotFound_ReturnsNull()
        {
            var service = CreateSut();
            _personelMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Personel)null);

            var sonuc = service.GetPersonelById(99999);

            sonuc.Should().BeNull();
        }

        // ── Cihaz ─────────────────────────────────────────────────────────────────

        [Fact]
        public void GetAktifCihazlar_DelegatesToRepo()
        {
            var service = CreateSut();
            var beklenen = new List<Terminal>
            {
                new Terminal { CihazId = 1, CihazAdi = "Turnike A", IP = "192.168.0.201" }
            };
            _cihazMock.Setup(r => r.GetAktifCihazlar(1)).Returns(beklenen);

            var sonuc = service.GetAktifCihazlar(1);

            sonuc.Should().BeEquivalentTo(beklenen);
        }

        [Fact]
        public void GetCihazIdByIp_ReturnsNullWhenNotFound()
        {
            // Arrange: bilinmeyen IP için repo null döner.
            var service = CreateSut();
            _cihazMock.Setup(r => r.GetCihazIdByIp("10.0.0.99")).Returns((int?)null);

            // Act
            var result = service.GetCihazIdByIp("10.0.0.99");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCihazIdByIp_KnownIp_ReturnsId()
        {
            var service = CreateSut();
            _cihazMock.Setup(r => r.GetCihazIdByIp("192.168.0.201")).Returns(5);

            var result = service.GetCihazIdByIp("192.168.0.201");

            result.Should().Be(5);
        }

        // ── Yetki işlemleri ───────────────────────────────────────────────────────

        [Fact]
        public void GetPersonelYetkiliCihazlar_DelegatesToRepo()
        {
            var service = CreateSut();
            var beklenen = new List<int> { 1, 3, 7 };
            _yetkiMock.Setup(r => r.GetYetkiliCihazlar(10001)).Returns(beklenen);

            var sonuc = service.GetPersonelYetkiliCihazlar(10001);

            sonuc.Should().BeEquivalentTo(beklenen);
        }

        [Fact]
        public void PersonelYetkiKaydet_DelegatesToRepo_ReturnsTrue()
        {
            // Arrange: yetki başarıyla kaydedildi (true).
            var service = CreateSut();
            var cihazIdler = new List<int> { 1, 2 };
            _yetkiMock.Setup(r => r.YetkiKaydet(10001, cihazIdler, 1)).Returns(true);

            // Act
            var sonuc = service.PersonelYetkiKaydet(10001, cihazIdler, 1);

            // Assert
            sonuc.Should().BeTrue();
            _yetkiMock.Verify(r => r.YetkiKaydet(10001, cihazIdler, 1), Times.Once);
        }

        [Fact]
        public void PersonelYetkiSil_DelegatesToRepo_ReturnsTrue()
        {
            var service = CreateSut();
            _yetkiMock.Setup(r => r.YetkiSil(10001, 3, 1)).Returns(true);

            var sonuc = service.PersonelYetkiSil(10001, 3, 1);

            sonuc.Should().BeTrue();
            _yetkiMock.Verify(r => r.YetkiSil(10001, 3, 1), Times.Once);
        }

        [Fact]
        public void PersonelYetkiSil_DelegatesToRepo_ReturnsFalseOnFailure()
        {
            // Repo başarısız döner.
            var service = CreateSut();
            _yetkiMock.Setup(r => r.YetkiSil(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                      .Returns(false);

            var sonuc = service.PersonelYetkiSil(10001, 99, 1);

            sonuc.Should().BeFalse();
        }

        [Fact]
        public void PersonelTumYetkileriSil_DelegatesToRepo()
        {
            var service = CreateSut();
            _yetkiMock.Setup(r => r.TumYetkileriSil(10001, 1)).Returns(true);

            var sonuc = service.PersonelTumYetkileriSil(10001, 1);

            sonuc.Should().BeTrue();
            _yetkiMock.Verify(r => r.TumYetkileriSil(10001, 1), Times.Once);
        }

        // ── Cihaz durumları ───────────────────────────────────────────────────────

        [Fact]
        public void GetPersonelCihazDurumlari_DelegatesToRepo()
        {
            var service = CreateSut();
            var beklenen = new List<PersonelCihazDurum>
            {
                new PersonelCihazDurum { CihazId = 1, YetkiVarMi = true }
            };
            _yetkiMock.Setup(r => r.GetPersonelCihazDurumlari(10001, 1)).Returns(beklenen);

            var sonuc = service.GetPersonelCihazDurumlari(10001, 1);

            sonuc.Should().BeEquivalentTo(beklenen);
        }
    }
}

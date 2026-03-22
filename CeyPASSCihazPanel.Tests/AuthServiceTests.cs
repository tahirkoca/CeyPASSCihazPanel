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
    /// AuthService unit testleri.
    ///
    /// Her test Arrange – Act – Assert deseniyle yazılmıştır:
    ///   Arrange : Mock nesneler hazırlanır, giriş verileri belirlenir.
    ///   Act     : Test edilecek metot çağrılır.
    ///   Assert  : Dönen sonuç FluentAssertions ile doğrulanır.
    ///
    /// IUserRepository bir mock ile taklit edildiği için gerçek DB bağlantısı gerekmez.
    /// </summary>
    public class AuthServiceTests
    {
        // ── Yardımcı fabrika ──────────────────────────────────────────────────────
        private (AuthService service, Mock<IUserRepository> repoMock) CreateSut()
        {
            var mock = new Mock<IUserRepository>();
            var service = new AuthService(mock.Object);
            return (service, mock);
        }

        // ── Login testleri ────────────────────────────────────────────────────────

        [Fact]
        public void Login_EmptyUserName_ReturnsFailed()
        {
            // Arrange
            var (service, _) = CreateSut();

            // Act
            var result = service.Login("", "sifre123");

            // Assert
            // Kullanıcı adı boş verilince servis validation yapıp hata dönmeli.
            result.Basarili.Should().BeFalse();
            result.Mesaj.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Login_WhitespaceUserName_ReturnsFailed()
        {
            var (service, _) = CreateSut();
            var result = service.Login("   ", "sifre123");
            result.Basarili.Should().BeFalse();
        }

        [Fact]
        public void Login_EmptyPassword_ReturnsFailed()
        {
            // Arrange
            var (service, _) = CreateSut();

            // Act
            var result = service.Login("admin", "");

            // Assert
            result.Basarili.Should().BeFalse();
            result.Mesaj.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Login_UserNotFound_ReturnsFailed()
        {
            // Arrange: repository, istenen kullanıcı için null döner (bulunamadı senaryosu).
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetByUserName("admin")).Returns((Kullanici)null);

            // Act
            var result = service.Login("admin", "sifre123");

            // Assert
            result.Basarili.Should().BeFalse();
            result.Kullanici.Should().BeNull();
        }

        [Fact]
        public void Login_WrongPassword_ReturnsFailed()
        {
            // Arrange: kullanıcı bulunuyor fakat şifre yanlış.
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetByUserName("admin"))
                    .Returns(new Kullanici { UserName = "admin", Password = "dogruSifre" });

            // Act
            var result = service.Login("admin", "yanlisSifre");

            // Assert
            result.Basarili.Should().BeFalse();
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange: doğru kullanıcı adı ve şifre.
            var (service, repoMock) = CreateSut();
            var kullanici = new Kullanici { UserName = "admin", Password = "Pdks1234", FirmaId = 1 };
            repoMock.Setup(r => r.GetByUserName("admin")).Returns(kullanici);

            // Act
            var result = service.Login("admin", "Pdks1234");

            // Assert
            result.Basarili.Should().BeTrue();
            result.Kullanici.Should().NotBeNull();
            result.Kullanici.UserName.Should().Be("admin");
        }

        [Fact]
        public void Login_TrimsUserName_BeforeLookup()
        {
            // Arrange: kullanıcı adının başında/sonunda boşluk var;
            // servis trim() yapıp "admin" olarak aramalı.
            var (service, repoMock) = CreateSut();
            var kullanici = new Kullanici { UserName = "admin", Password = "sifre" };
            repoMock.Setup(r => r.GetByUserName("admin")).Returns(kullanici);

            // Act
            var result = service.Login("  admin  ", "sifre");

            // Assert: trim yapıldığında kullanıcı bulunacak ve giriş başarılı olacak.
            result.Basarili.Should().BeTrue();
            // Repository'nin "admin" (trim edilmiş) ile tam 1 kez çağrıldığını doğrula.
            repoMock.Verify(r => r.GetByUserName("admin"), Times.Once);
        }

        [Fact]
        public void Login_CaseSensitive_WrongCasePassword_ReturnsFailed()
        {
            // Arrange: şifre büyük/küçük harf duyarlı olmalı.
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetByUserName("admin"))
                    .Returns(new Kullanici { UserName = "admin", Password = "Pdks1234" });

            // Act: şifrenin tamamı büyük harf veriliyor.
            var result = service.Login("admin", "PDKS1234");

            // Assert
            result.Basarili.Should().BeFalse();
        }

        // ── GetAllUserNames testleri ───────────────────────────────────────────────

        [Fact]
        public void GetAllUserNames_ReturnsAllUserNames()
        {
            // Arrange: repository 2 kullanıcı döner.
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetAll()).Returns(new List<Kullanici>
            {
                new Kullanici { UserName = "admin" },
                new Kullanici { UserName = "operator" }
            });

            // Act
            var names = service.GetAllUserNames();

            // Assert: 2 isim geldi ve içerik doğru.
            names.Should().HaveCount(2);
            names.Should().Contain("admin");
            names.Should().Contain("operator");
        }

        [Fact]
        public void GetAllUserNames_EmptyRepo_ReturnsEmptyList()
        {
            // Arrange
            var (service, repoMock) = CreateSut();
            repoMock.Setup(r => r.GetAll()).Returns(new List<Kullanici>());

            // Act
            var names = service.GetAllUserNames();

            // Assert
            names.Should().BeEmpty();
        }
    }
}

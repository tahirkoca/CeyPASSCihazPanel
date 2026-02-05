using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASSCihazPanel.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public LoginResult Login(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                return new LoginResult
                {
                    Basarili = false,
                    Mesaj = "Kullanıcı adı ve şifre zorunludur."
                };
            }

            var user = _userRepository.GetByUserName(userName.Trim());

            if (user == null || !string.Equals(user.Password, password, StringComparison.Ordinal))
            {
                return new LoginResult
                {
                    Basarili = false,
                    Mesaj = "Hatalı kullanıcı adı veya şifre!"
                };
            }

            return new LoginResult
            {
                Basarili = true,
                Mesaj = "Giriş başarılı.",
                Kullanici = user
            };
        }
        public IList<string> GetAllUserNames()
        {
            return _userRepository
                .GetAll()
                .Select(u => u.UserName)
                .ToList();
        }
    }
}

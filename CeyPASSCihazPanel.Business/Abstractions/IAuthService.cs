using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IAuthService
    {
        LoginResult Login(string userName, string password);
        IList<string> GetAllUserNames();
    }
}

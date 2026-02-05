using CeyPASSCihazPanel.Entities.Models;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.DAL.Abstractions
{
    public interface IUserRepository
    {
        IList<Kullanici> GetAll();
        Kullanici GetByUserName(string userName);
    }
}

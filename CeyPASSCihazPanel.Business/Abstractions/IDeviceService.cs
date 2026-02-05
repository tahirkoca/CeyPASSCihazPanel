using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;

namespace CeyPASSCihazPanel.Business.Abstractions
{
    public interface IDeviceService : IDisposable
    {
        void Start(int? firmaId, Action<string> logCallback);
        bool TryGetConnection(string ip, out CihazBaglantisi baglanti);
        IReadOnlyDictionary<string, CihazBaglantisi> GetAllConnections();
        List<OfflineLog> GetOfflineData(string ipAdres, DateTime baslangic, DateTime bitis);
        bool ClearOfflineData(string ipAdres);
        bool ClearAllLogs(string ipAdres);
        bool ClearAllUsers(string ipAdres);
        bool SynchronizeTime(string ipAdres);
        CihazBilgi GetDeviceInfo(string ipAdres);
        bool RestartDevice(string ipAdres);
        bool PowerOffDevice(string ipAdres);
    }
}

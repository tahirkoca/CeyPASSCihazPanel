using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.DAL.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using zkemkeeper;

namespace CeyPASSCihazPanel.Business.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ICihazRepository _cihazRepository;

        private readonly Dictionary<string, CihazBaglantisi> _cihazlar = new Dictionary<string, CihazBaglantisi>();
        private readonly object _lock = new object();
        private Timer _connectionCheckTimer;
        private Action<string> _log;
        private int? _firmaId;

        public DeviceService(ICihazRepository cihazRepository)
        {
            _cihazRepository = cihazRepository;
        }

        public void Start(int? firmaId, Action<string> logCallback)
        {
            _firmaId = firmaId;
            _log = logCallback ?? (_ => { });

            var cihazListesi = _cihazRepository.GetAktifCihazlar(firmaId);

            foreach (var cihazBilgi in cihazListesi)
            {
                ThreadPool.QueueUserWorkItem(_ => CihazaBaglan(cihazBilgi));
            }

            _connectionCheckTimer = new Timer(BaglantilariKontrolEt, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        public bool TryGetConnection(string ip, out CihazBaglantisi baglanti)
        {
            lock (_lock)
            {
                return _cihazlar.TryGetValue(ip, out baglanti);
            }
        }
        public IReadOnlyDictionary<string, CihazBaglantisi> GetAllConnections()
        {
            lock (_lock)
            {
                return new Dictionary<string, CihazBaglantisi>(_cihazlar);
            }
        }
        private void CihazaBaglan(Terminal cihazBilgi)
        {
            try
            {
                _log?.Invoke($"Bağlanılıyor: {cihazBilgi.CihazAdi} ({cihazBilgi.IP})...");

                CZKEM zkem = new CZKEM();
                zkem.SetCommPassword(0);

                if (zkem.Connect_Net(cihazBilgi.IP, cihazBilgi.Port))
                {
                    int machineNumber = 1;

                    if (zkem.RegEvent(machineNumber, 65535))
                    {
                        zkem.EnableDevice(machineNumber, true);

                        var baglanti = new CihazBaglantisi
                        {
                            Device = zkem,
                            Info = cihazBilgi,
                            SonBaglantiZamani = DateTime.Now,
                            BaglantiDenemeleri = 0,
                            Bagli = true
                        };

                        lock (_lock)
                        {
                            _cihazlar[cihazBilgi.IP] = baglanti;
                        }

                        _log?.Invoke($"✅ Bağlandı: {cihazBilgi.CihazAdi}");
                    }
                    else
                    {
                        _log?.Invoke($"❌ Event kaydı başarısız: {cihazBilgi.CihazAdi}");
                        zkem.Disconnect();
                    }
                }
                else
                {
                    int errorCode = 0;
                    zkem.GetLastError(ref errorCode);
                    _log?.Invoke($"❌ Bağlantı hatası: {cihazBilgi.CihazAdi} - Hata kodu: {errorCode}");
                }
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Kritik hata: {cihazBilgi.CihazAdi} - {ex.Message}");
            }
        }
        private void BaglantilariKontrolEt(object state)
        {
            List<Terminal> yenidenBaglanacaklar = new List<Terminal>();

            lock (_lock)
            {
                foreach (var kvp in _cihazlar.ToList())
                {
                    var baglanti = kvp.Value;

                    try
                    {
                        int dwMachineNumber = 1;
                        int dwYear = 0, dwMonth = 0, dwDay = 0, dwHour = 0, dwMinute = 0, dwSecond = 0;

                        if (!baglanti.Device.GetDeviceTime(dwMachineNumber, ref dwYear, ref dwMonth,
                                                            ref dwDay, ref dwHour, ref dwMinute, ref dwSecond))
                        {
                            baglanti.Bagli = false;
                            baglanti.BaglantiDenemeleri++;

                            if (baglanti.BaglantiDenemeleri <= 10)
                            {
                                yenidenBaglanacaklar.Add(baglanti.Info);
                            }
                            else
                            {
                                _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - 10 deneme sonrası bağlanamadı!");
                            }
                        }
                        else
                        {
                            baglanti.SonBaglantiZamani = DateTime.Now;
                            baglanti.BaglantiDenemeleri = 0;
                            baglanti.Bagli = true;
                        }
                    }
                    catch
                    {
                        baglanti.Bagli = false;
                        yenidenBaglanacaklar.Add(baglanti.Info);
                    }
                }
            }

            foreach (var cihaz in yenidenBaglanacaklar)
            {
                _log?.Invoke($"🔄 Yeniden bağlanılıyor: {cihaz.CihazAdi}");
                YenidenBaglan(cihaz);
            }
        }
        private void YenidenBaglan(Terminal cihazBilgi)
        {
            try
            {
                lock (_lock)
                {
                    if (_cihazlar.ContainsKey(cihazBilgi.IP))
                    {
                        var eskiBaglanti = _cihazlar[cihazBilgi.IP];
                        try
                        {
                            eskiBaglanti.Device.Disconnect();
                        }
                        catch { }

                        _cihazlar.Remove(cihazBilgi.IP);
                    }
                }

                Thread.Sleep(1000);
                CihazaBaglan(cihazBilgi);
            }
            catch (Exception ex)
            {
                _log?.Invoke($"Yeniden bağlantı hatası: {cihazBilgi.CihazAdi} - {ex.Message}");
            }
        }
        public void Dispose()
        {
            _connectionCheckTimer?.Dispose();

            lock (_lock)
            {
                foreach (var kvp in _cihazlar)
                {
                    try
                    {
                        kvp.Value.Device.Disconnect();
                    }
                    catch { }
                }
                _cihazlar.Clear();
            }
        }
        public List<OfflineLog> GetOfflineData(string ipAdres, DateTime baslangic, DateTime bitis)
        {
            var loglar = new List<OfflineLog>();

            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return loglar;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"📥 {baglanti.Info.CihazAdi} cihazından offline veri okunuyor...");

                cihaz.EnableDevice(makineNo, false);

                if (!cihaz.ReadGeneralLogData(makineNo))
                {
                    cihaz.EnableDevice(makineNo, true);
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Log verisi okunamadı.");
                    return loglar;
                }

                string enrollNumber = "";
                int verifyMode = 0;
                int inOutMode = 0;
                int yil = 0, ay = 0, gun = 0, saat = 0, dakika = 0, saniye = 0;
                int workCode = 0;

                //while (cihaz.SSR_GetGeneralLogData(makineNo, out enrollNumber, out verifyMode, out inOutMode,
                //    out yil, out ay, out gun, out saat, out dakika, out saniye, ref workCode))
                //{
                //    try
                //    {
                //        var tarih = new DateTime(yil, ay, gun, saat, dakika, saniye);

                //        // Tarih filtresi
                //        if (tarih < baslangic || tarih > bitis)
                //            continue;

                //        // Kullanıcı bilgisini al
                //        string adSoyad = "";
                //        string sifre = "";
                //        int yetki = 0;
                //        bool aktif = false;

                //        if (int.TryParse(enrollNumber, out int userId))
                //        {
                //            cihaz.GetUserInfo(makineNo, userId, ref adSoyad, ref sifre, ref yetki, ref aktif);
                //        }

                //        loglar.Add(new OfflineLog
                //        {
                //            CihazAdi = baglanti.Info.CihazAdi,
                //            IPAdres = ipAdres,
                //            EnrollNumber = int.TryParse(enrollNumber, out int en) ? en : 0,
                //            AdSoyad = string.IsNullOrWhiteSpace(adSoyad) ? $"Bilinmeyen ({enrollNumber})" : adSoyad,
                //            Tarih = tarih,
                //            VerifyMode = verifyMode,
                //            InOutMode = inOutMode,
                //            VerifyModeText = GetVerifyModeText(verifyMode),
                //            InOutModeText = GetInOutModeText(inOutMode)
                //        });
                //    }
                //    catch (Exception ex)
                //    {
                //        _log?.Invoke($"⚠️ Log kaydı atlandı: {ex.Message}");
                //    }
                //}
                while (cihaz.SSR_GetGeneralLogData(makineNo, out enrollNumber, out verifyMode, out inOutMode,
    out yil, out ay, out gun, out saat, out dakika, out saniye, ref workCode))
                {
                    try
                    {
                        var tarih = new DateTime(yil, ay, gun, saat, dakika, saniye);

                        // Tarih filtresi
                        if (tarih < baslangic || tarih > bitis)
                            continue;

                        // Kullanıcı bilgisini al
                        string adSoyad = "";
                        string sifre = "";
                        int yetki = 0;
                        bool aktif = false;

                        // 👇 WorkCode'u personel ID olarak kullan
                        int personelId = workCode > 0 ? workCode : (int.TryParse(enrollNumber, out int en) ? en : 0);

                        if (personelId > 0)
                        {
                            cihaz.GetUserInfo(makineNo, personelId, ref adSoyad, ref sifre, ref yetki, ref aktif);
                        }

                        loglar.Add(new OfflineLog
                        {
                            CihazAdi = baglanti.Info.CihazAdi,
                            IPAdres = ipAdres,
                            EnrollNumber = int.TryParse(enrollNumber, out int enrollNo) ? enrollNo : 0,
                            PersonelId = personelId,
                            AdSoyad = string.IsNullOrWhiteSpace(adSoyad) ? $"Bilinmeyen ({personelId})" : adSoyad,
                            Tarih = tarih,
                            VerifyMode = verifyMode,
                            InOutMode = inOutMode,
                            VerifyModeText = GetVerifyModeText(verifyMode),
                            InOutModeText = GetInOutModeText(inOutMode)
                        });
                    }
                    catch (Exception ex)
                    {
                        _log?.Invoke($"⚠️ Log kaydı atlandı: {ex.Message}");
                    }
                }

                cihaz.EnableDevice(makineNo, true);
                _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - {loglar.Count} kayıt okundu.");
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Offline veri çekme hatası ({ipAdres}): {ex.Message}");
            }

            return loglar;
        }
        public bool ClearOfflineData(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"🗑️ {baglanti.Info.CihazAdi} cihazındaki veriler temizleniyor...");

                cihaz.EnableDevice(makineNo, false);
                bool sonuc = cihaz.ClearGLog(makineNo);
                cihaz.EnableDevice(makineNo, true);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Offline veriler temizlendi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Veri temizleme başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Offline veri temizleme hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
        private string GetVerifyModeText(int mode)
        {
            switch (mode)
            {
                case 0: return "Şifre";
                case 1: return "Parmak İzi";
                case 2: return "Kart";
                case 3: return "Şifre+Parmak";
                case 4: return "Parmak+Şifre";
                case 5: return "Parmak+Kart";
                case 15: return "Yüz Tanıma";
                default: return $"Bilinmeyen ({mode})";
            }
        }
        private string GetInOutModeText(int mode)
        {
            switch (mode)
            {
                case 0: return "Giriş";
                case 1: return "Çıkış";
                case 2: return "Mola Başlangıç";
                case 3: return "Mola Bitiş";
                case 4: return "Mesai Başlangıç";
                case 5: return "Mesai Bitiş";
                default: return $"Diğer ({mode})";
            }
        }
        public bool ClearAllLogs(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"🗑️ {baglanti.Info.CihazAdi} - Tüm loglar temizleniyor...");

                cihaz.EnableDevice(makineNo, false);
                bool sonuc = cihaz.ClearGLog(makineNo);
                cihaz.EnableDevice(makineNo, true);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Tüm loglar temizlendi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Log temizleme başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Log temizleme hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
        public bool ClearAllUsers(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"👥 {baglanti.Info.CihazAdi} - Tüm kullanıcılar temizleniyor...");

                cihaz.EnableDevice(makineNo, false);
                bool sonuc = cihaz.ClearData(makineNo, 5); // 5 = Kullanıcı bilgileri
                cihaz.EnableDevice(makineNo, true);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Tüm kullanıcılar temizlendi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Kullanıcı temizleme başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Kullanıcı temizleme hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
        public bool SynchronizeTime(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"⏰ {baglanti.Info.CihazAdi} - Saat senkronize ediliyor...");

                cihaz.EnableDevice(makineNo, false);
                bool sonuc = cihaz.SetDeviceTime(makineNo);
                cihaz.EnableDevice(makineNo, true);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Saat senkronize edildi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Saat senkronizasyonu başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Saat senkronizasyon hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
        public CihazBilgi GetDeviceInfo(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return null;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"📊 {baglanti.Info.CihazAdi} - Cihaz bilgileri okunuyor...");

                var bilgi = new CihazBilgi
                {
                    CihazAdi = baglanti.Info.CihazAdi,
                    IPAdres = ipAdres,
                    BaglantiDurumu = baglanti.Bagli
                };

                // Firmware version
                string firmwareVersion = "";
                if (cihaz.GetFirmwareVersion(makineNo, ref firmwareVersion))
                {
                    bilgi.FirmwareVersion = firmwareVersion;
                }

                // Serial number
                string serialNumber = "";
                if (cihaz.GetSerialNumber(makineNo, out serialNumber))
                {
                    bilgi.SeriNo = serialNumber;
                }

                // Platform
                string platform = "";
                if (cihaz.GetPlatform(makineNo, ref platform))
                {
                    bilgi.Platform = platform;
                }

                // MAC Address
                string macAddress = "";
                if (cihaz.GetDeviceMAC(makineNo, ref macAddress))
                {
                    bilgi.MACAdres = macAddress;
                }

                // Kullanıcı sayısı
                int userCount = 0;
                if (cihaz.GetDeviceStatus(makineNo, 2, ref userCount))
                {
                    bilgi.MevcutKullaniciSayisi = userCount;
                }

                // Kullanıcı kapasitesi
                int userCapacity = 0;
                if (cihaz.GetDeviceStatus(makineNo, 8, ref userCapacity))
                {
                    bilgi.KullaniciKapasitesi = userCapacity;
                }

                // Log sayısı
                int logCount = 0;
                if (cihaz.GetDeviceStatus(makineNo, 6, ref logCount))
                {
                    bilgi.MevcutLogSayisi = logCount;
                }

                // Log kapasitesi
                int logCapacity = 0;
                if (cihaz.GetDeviceStatus(makineNo, 9, ref logCapacity))
                {
                    bilgi.LogKapasitesi = logCapacity;
                }

                // Parmak izi sayısı
                int fpCount = 0;
                if (cihaz.GetDeviceStatus(makineNo, 3, ref fpCount))
                {
                    bilgi.MevcutParmakilziSayisi = fpCount;
                }

                // Parmak izi kapasitesi
                int fpCapacity = 0;
                if (cihaz.GetDeviceStatus(makineNo, 7, ref fpCapacity))
                {
                    bilgi.ParmakilziKapasitesi = fpCapacity;
                }

                // Cihaz saati
                int yil = 0, ay = 0, gun = 0, saat = 0, dakika = 0, saniye = 0;
                if (cihaz.GetDeviceTime(makineNo, ref yil, ref ay, ref gun, ref saat, ref dakika, ref saniye))
                {
                    bilgi.CihazSaati = new DateTime(yil, ay, gun, saat, dakika, saniye);
                }

                _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Cihaz bilgileri okundu.");

                return bilgi;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Cihaz bilgisi okuma hatası ({ipAdres}): {ex.Message}");
                return null;
            }
        }
        public bool RestartDevice(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"🔄 {baglanti.Info.CihazAdi} - Cihaz yeniden başlatılıyor...");

                bool sonuc = cihaz.RestartDevice(makineNo);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Yeniden başlatma komutu gönderildi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Yeniden başlatma başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Cihaz restart hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
        public bool PowerOffDevice(string ipAdres)
        {
            if (!TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
            {
                _log?.Invoke($"❌ Cihaz bağlantısı yok: {ipAdres}");
                return false;
            }

            try
            {
                var cihaz = baglanti.Device;
                const int makineNo = 1;

                _log?.Invoke($"⚡ {baglanti.Info.CihazAdi} - Cihaz kapatılıyor...");

                bool sonuc = cihaz.PowerOffDevice(makineNo);

                if (sonuc)
                    _log?.Invoke($"✅ {baglanti.Info.CihazAdi} - Kapatma komutu gönderildi.");
                else
                    _log?.Invoke($"⚠️ {baglanti.Info.CihazAdi} - Kapatma başarısız.");

                return sonuc;
            }
            catch (Exception ex)
            {
                _log?.Invoke($"❌ Cihaz kapatma hatası ({ipAdres}): {ex.Message}");
                return false;
            }
        }
    }
}

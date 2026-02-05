using CeyPASSCihazPanel.Business.Abstractions;
using CeyPASSCihazPanel.Entities.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CeyPASSCihazPanel.UI
{
    public partial class anaForm : Form
    {
        private readonly IAdminLookupService _lookupService;
        private readonly IDeviceService _deviceService;
        private string _kullaniciAdi;
        private int? _firmaId;
        private List<Personel> _tumPersoneller;
        private List<PuantajsizKart> _tumKartlar;
        private List<Terminal> _tumCihazlar;

        public anaForm(IAdminLookupService lookupService, IDeviceService deviceService)
        {
            _lookupService = lookupService;
            _deviceService = deviceService;
            InitializeComponent();
        }

        public void SetContext(string kullaniciAdi, int? firmaId)
        {
            _kullaniciAdi = kullaniciAdi;
            _firmaId = firmaId;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                chkKartModu.CheckedChanged += (s, ea) =>
                {
                    ComboListesiniModaGoreYukle();
                    idBox.Clear();
                    kartBox.Clear();
                    dgvCihazDurum.DataSource = null;
                };

                _deviceService.Start(_firmaId, LogYaz);
                ComboListesiniModaGoreYukle();
                CihazlariListele();
                InitializeDataGridView();

                this.WindowState = FormWindowState.Maximized;

                dgvCihazDurum.RowTemplate.Height = 32;
                dgvCihazDurum.ColumnHeadersHeight = 40;
                dgvCihazDurum.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
                dgvCihazDurum.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
                dgvCihazDurum.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                dgvCihazDurum.DefaultCellStyle.Padding = new Padding(5);

                foreach (DataGridViewColumn col in dgvCihazDurum.Columns)
                {
                    if (col is DataGridViewCheckBoxColumn)
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }

                cihazSecimListesi.Font = new Font("Segoe UI", 10F);
                cihazSecimListesi.ItemHeight = 24;

                logList.Font = new Font("Consolas", 10F);
                logList.ItemHeight = 20;

                cihazUserListBox.Font = new Font("Consolas", 10F);
                cihazUserListBox.ItemHeight = 20;

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                this.Text = $"CeyPASS Cihaz Paneli - v{version} - {_kullaniciAdi}";

                YetkiYonetimiTabBaslat();
                OfflineVeriTabBaslat();
                CihazYonetimiTabBaslat();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Form yükleme hatası:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ComboListesiniModaGoreYukle()
        {
            if (chkKartModu.Checked)
                PuantajsizKartlariComboyaYukle();
            else
                PersonelleriYukle();
        }
        private void PuantajsizKartlariComboyaYukle()
        {
            personelComboBox.Items.Clear();

            var kartlar = _lookupService.GetAktifPuantajsizKartlar(_firmaId);

            foreach (var k in kartlar)
            {
                personelComboBox.Items.Add(new KartItem
                {
                    KartId = k.KartId,
                    KartNo = k.KartNo ?? "",
                    KartAdi = k.KartAdi ?? ""
                });
            }

            if (personelComboBox.Items.Count > 0)
                personelComboBox.SelectedIndex = 0;

            AdjustComboDropDownWidth(personelComboBox);
        }
        private void LogYaz(string mesaj)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => LogYaz(mesaj)));
                return;
            }

            string logMesaj = $"[{DateTime.Now:HH:mm:ss}] {mesaj}";
            logList.Items.Insert(0, logMesaj);

            if (logList.Items.Count > 500)
            {
                logList.Items.RemoveAt(logList.Items.Count - 1);
            }
        }
        private void PersonelleriYukle()
        {
            personelComboBox.Items.Clear();

            var personeller = _lookupService.GetAktifPersoneller(_firmaId);

            foreach (var p in personeller)
            {
                string sicil = p.PersonelId.ToString();
                string ad = p.Ad ?? "";
                string soyad = p.Soyad ?? "";
                personelComboBox.Items.Add($"{ad} {soyad} ({sicil})");
            }

            AdjustComboDropDownWidth(personelComboBox);
        }
        private void CihazlariListele()
        {
            cihazSecimListesi.Items.Clear();

            var list = _lookupService.GetCihazListeItems(_firmaId);

            foreach (var item in list)
            {
                cihazSecimListesi.Items.Add(item.ToString());
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _deviceService.Dispose();
            Application.Exit();
        }
        private void btnTanimla_Click(object sender, EventArgs e)
        {
            // KART MODU
            if (chkKartModu.Checked)
            {
                if (!(personelComboBox.SelectedItem is KartItem k))
                {
                    MessageBox.Show("Kart Modu: Lütfen listeden bir kart seçin.");
                    return;
                }

                int kartId = k.KartId;
                string kartAdi = k.KartAdi;
                string kartNo = string.IsNullOrWhiteSpace(kartBox.Text) ? (k.KartNo ?? "") : kartBox.Text.Trim();

                // DB'den yetkili cihazları al
                var yetkiliCihazIdListesi = _lookupService.GetKartYetkiliCihazlar(kartId);

                List<string> hedefIpListesi = new List<string>();

                if (yetkiliCihazIdListesi.Count > 0)
                {
                    // DB'de yetki varsa sadece yetkili cihazlara ekle
                    var tumCihazListesi = _lookupService.GetAktifCihazlar(_firmaId);
                    hedefIpListesi = tumCihazListesi
                        .Where(c => yetkiliCihazIdListesi.Contains(c.CihazId))
                        .Select(c => c.IP)
                        .ToList();

                    LogYaz($"Kart {kartAdi} için DB'de {yetkiliCihazIdListesi.Count} yetkili cihaz bulundu.");
                }
                else
                {
                    // DB'de yetki yoksa panel'den seçilenlere ekle
                    if (cihazSecimListesi.CheckedItems.Count == 0)
                    {
                        MessageBox.Show("DB'de yetki tanımı yok. Lütfen cihaz seçim listesinden en az bir cihaz seçin!");
                        return;
                    }

                    foreach (var seciliItem in cihazSecimListesi.CheckedItems)
                    {
                        var ipAdresi = ParseIp(seciliItem);
                        if (!string.IsNullOrEmpty(ipAdresi))
                            hedefIpListesi.Add(ipAdresi);
                    }

                    LogYaz($"Kart {kartAdi} için DB'de yetki yok, panel seçimi kullanılıyor ({hedefIpListesi.Count} cihaz).");
                }

                int basariliSayac = 0;

                foreach (var hedefIp in hedefIpListesi)
                {
                    if (!_deviceService.TryGetConnection(hedefIp, out var cihazBaglanti) || !cihazBaglanti.Bagli)
                    {
                        LogYaz($"Cihaz bağlantısı yok: {hedefIp}");
                        continue;
                    }

                    try
                    {
                        var zkemCihaz = cihazBaglanti.Device;
                        const int makineNo = 1;

                        // Zaten ekliyse tekrar ekleme
                        string mevcutIsim = "", mevcutSifre = "", mevcutKart = "";
                        int mevcutYetki = 0;
                        bool mevcutAktif = false;

                        if (zkemCihaz.GetUserInfo(makineNo, kartId, ref mevcutIsim, ref mevcutSifre, ref mevcutYetki, ref mevcutAktif))
                        {
                            LogYaz($"⚠️ Kart {kartAdi} zaten {cihazBaglanti.Info.CihazAdi} cihazında mevcut, atlanıyor.");
                            continue;
                        }

                        zkemCihaz.EnableDevice(makineNo, false);
                        zkemCihaz.SSR_DeleteEnrollData(makineNo, kartId.ToString(), 12);
                        zkemCihaz.SetStrCardNumber(kartNo);

                        if (zkemCihaz.SetUserInfo(makineNo, kartId, kartAdi, "", 0, true))
                        {
                            zkemCihaz.RefreshData(makineNo);
                            basariliSayac++;
                            LogYaz($"✅ {kartAdi} -> {cihazBaglanti.Info.CihazAdi} tanımlandı.");
                        }

                        zkemCihaz.EnableDevice(makineNo, true);
                    }
                    catch (Exception ex)
                    {
                        LogYaz($"Kart tanımlama hatası ({hedefIp}): {ex.Message}");
                    }
                }

                MessageBox.Show(
                    basariliSayac > 0
                    ? $"✅ {kartAdi} (KartNo:{kartNo}) {basariliSayac} cihaza tanımlandı. (UserID:{kartId})"
                    : "❌ Tanımlama başarısız.");

                // Grid'i güncelle
                //Task.Run(async () => await KartCihazDurumunuGoster(kartId));
                this.BeginInvoke(new Action(() =>
                {
                    KartCihazDurumunuGoster(kartId);
                }));
                return;
            }

            // PERSONEL MODU
            if (personelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir personel seçin.");
                return;
            }
            if (string.IsNullOrWhiteSpace(kartBox.Text) || string.IsNullOrWhiteSpace(idBox.Text))
            {
                MessageBox.Show("Kart No ve Personel ID boş olamaz!");
                return;
            }

            int personelKartNo = Convert.ToInt32(kartBox.Text.Trim());
            int personelCihazID = Convert.ToInt32(idBox.Text.Trim());

            string personelSecim = personelComboBox.SelectedItem.ToString();
            string personelSicilNo = personelSecim.Substring(personelSecim.LastIndexOf('(') + 1).Trim(')');

            if (!int.TryParse(personelSicilNo, out int seciliPersonelId))
            {
                MessageBox.Show("Geçersiz personel seçimi.");
                return;
            }

            var secilenPersonel = _lookupService.GetPersonelById(seciliPersonelId);
            if (secilenPersonel == null)
            {
                MessageBox.Show("Personel bulunamadı.");
                return;
            }

            string personelAd = secilenPersonel.Ad ?? "";
            string personelSoyad = secilenPersonel.Soyad ?? "";

            // DB'den yetkili cihazları al
            var personelYetkiliCihazIdler = _lookupService.GetPersonelYetkiliCihazlar(seciliPersonelId);

            List<string> personelHedefIpListesi = new List<string>();

            if (personelYetkiliCihazIdler.Count > 0)
            {
                // DB'de yetki varsa sadece yetkili cihazlara ekle
                var personelTumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                personelHedefIpListesi = personelTumCihazlar
                    .Where(c => personelYetkiliCihazIdler.Contains(c.CihazId))
                    .Select(c => c.IP)
                    .ToList();

                LogYaz($"{personelAd} {personelSoyad} için DB'de {personelYetkiliCihazIdler.Count} yetkili cihaz bulundu.");
            }
            else
            {
                // DB'de yetki yoksa panel'den seçilenlere ekle
                if (cihazSecimListesi.CheckedItems.Count == 0)
                {
                    MessageBox.Show("DB'de yetki tanımı yok. Lütfen cihaz seçim listesinden en az bir cihaz seçin!");
                    return;
                }

                foreach (var personelSeciliItem in cihazSecimListesi.CheckedItems)
                {
                    var personelIpAdres = ParseIp(personelSeciliItem);
                    if (!string.IsNullOrEmpty(personelIpAdres))
                        personelHedefIpListesi.Add(personelIpAdres);
                }

                LogYaz($"{personelAd} {personelSoyad} için DB'de yetki yok, panel seçimi kullanılıyor ({personelHedefIpListesi.Count} cihaz).");
            }

            int personelBasariliSayisi = 0;

            foreach (var personelHedefIp in personelHedefIpListesi)
            {
                if (!_deviceService.TryGetConnection(personelHedefIp, out var personelBaglanti) || !personelBaglanti.Bagli)
                {
                    LogYaz($"Cihaz bağlantısı yok: {personelHedefIp}");
                    continue;
                }

                try
                {
                    var personelCihaz = personelBaglanti.Device;
                    const int personelCihazNo = 1;

                    // Zaten ekliyse tekrar ekleme
                    string personelMevcutIsim = "", personelMevcutSifre = "", personelMevcutKart = "";
                    int personelMevcutYetki = 0;
                    bool personelMevcutAktif = false;

                    if (personelCihaz.GetUserInfo(personelCihazNo, personelCihazID, ref personelMevcutIsim, ref personelMevcutSifre, ref personelMevcutYetki, ref personelMevcutAktif))
                    {
                        LogYaz($"⚠️ {personelAd} {personelSoyad} zaten {personelBaglanti.Info.CihazAdi} cihazında mevcut, atlanıyor.");
                        continue;
                    }

                    personelCihaz.EnableDevice(personelCihazNo, false);
                    personelCihaz.SSR_DeleteEnrollData(personelCihazNo, personelCihazID.ToString(), 12);
                    personelCihaz.SetStrCardNumber(personelKartNo.ToString());

                    bool personelTanimlandi = personelCihaz.SetUserInfo(personelCihazNo, personelCihazID, $"{personelAd} {personelSoyad}", "", 0, true);
                    if (personelTanimlandi)
                    {
                        personelCihaz.RefreshData(personelCihazNo);
                        personelBasariliSayisi++;
                        LogYaz($"✅ {personelAd} {personelSoyad} -> {personelBaglanti.Info.CihazAdi} tanımlandı.");
                    }

                    personelCihaz.EnableDevice(personelCihazNo, true);
                }
                catch (Exception ex)
                {
                    LogYaz($"Tanımlama hatası ({personelHedefIp}): {ex.Message}");
                }
            }

            MessageBox.Show(personelBasariliSayisi > 0
                ? $"✅ {personelAd} {personelSoyad} [{personelSicilNo}] {personelBasariliSayisi} cihaza tanımlandı. (CihazID:{personelCihazID}, KartNo:{personelKartNo})"
                : "❌ Tanımlama başarısız.");

            // Grid'i güncelle
            //Task.Run(async () => await PersonelCihazDurumunuGoster(seciliPersonelId));
            this.BeginInvoke(new Action(() =>
            {
                PersonelCihazDurumunuGoster(seciliPersonelId);
            }));
            return;
        }
        private void btnKisiSil_Click(object sender, EventArgs e)
        {
            // KART MODU
            if (chkKartModu.Checked)
            {
                if (!(personelComboBox.SelectedItem is KartItem k))
                {
                    MessageBox.Show("Kart Modu: Lütfen listeden bir kart seçin.");
                    return;
                }

                int silKartId = k.KartId;
                string silKartAdi = k.KartAdi;

                // DB'den yetkili cihazları al
                var silYetkiliCihazIdler = _lookupService.GetKartYetkiliCihazlar(silKartId);

                List<string> silHedefIpList = new List<string>();

                if (silYetkiliCihazIdler.Count > 0)
                {
                    var silTumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                    silHedefIpList = silTumCihazlar
                        .Where(c => silYetkiliCihazIdler.Contains(c.CihazId))
                        .Select(c => c.IP)
                        .ToList();
                }
                else
                {
                    if (cihazSecimListesi.CheckedItems.Count == 0)
                    {
                        MessageBox.Show("DB'de yetki tanımı yok. Lütfen cihaz seçim listesinden en az bir cihaz seçin!");
                        return;
                    }

                    foreach (var silItem in cihazSecimListesi.CheckedItems)
                    {
                        var silIp = ParseIp(silItem);
                        if (!string.IsNullOrEmpty(silIp))
                            silHedefIpList.Add(silIp);
                    }
                }

                int silBasarili = 0;

                foreach (var silIpAdres in silHedefIpList)
                {
                    if (!_deviceService.TryGetConnection(silIpAdres, out var silBaglanti) || !silBaglanti.Bagli)
                    {
                        LogYaz($"Cihaz bağlantısı yok: {silIpAdres}");
                        continue;
                    }

                    try
                    {
                        var silCihaz = silBaglanti.Device;
                        const int silCihazNo = 1;

                        silCihaz.EnableDevice(silCihazNo, false);

                        if (silCihaz.DeleteEnrollData(silCihazNo, silKartId, 1, 12))
                        {
                            silCihaz.RefreshData(silCihazNo);
                            silBasarili++;
                            LogYaz($"🗑 {silKartAdi} -> {silBaglanti.Info.CihazAdi} silindi.");
                        }

                        silCihaz.EnableDevice(silCihazNo, true);
                    }
                    catch (Exception ex)
                    {
                        LogYaz($"Kart silme hatası ({silIpAdres}): {ex.Message}");
                    }
                }

                MessageBox.Show(silBasarili > 0 ? $"🗑 {silKartAdi} (UserID:{silKartId}) {silBasarili} cihazdan silindi." : "❌ Silme başarısız.");

                //Task.Run(async () => await KartCihazDurumunuGoster(silKartId));
                this.BeginInvoke(new Action(() =>
                {
                    KartCihazDurumunuGoster(silKartId);
                }));
                return;
            }

            // PERSONEL MODU
            string silmePersonelCihazID = idBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(silmePersonelCihazID))
            {
                MessageBox.Show("Lütfen silinecek Cihaz ID'yi girin.");
                return;
            }

            if (!int.TryParse(silmePersonelCihazID, out int silmePersonelId))
            {
                MessageBox.Show("Geçersiz Personel ID.");
                return;
            }

            var silmeYetkiliCihazIdler = _lookupService.GetPersonelYetkiliCihazlar(silmePersonelId);

            List<string> silmeHedefIpList = new List<string>();

            if (silmeYetkiliCihazIdler.Count > 0)
            {
                var silmeTumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                silmeHedefIpList = silmeTumCihazlar
                    .Where(c => silmeYetkiliCihazIdler.Contains(c.CihazId))
                    .Select(c => c.IP)
                    .ToList();
            }
            else
            {
                if (cihazSecimListesi.CheckedItems.Count == 0)
                {
                    MessageBox.Show("DB'de yetki tanımı yok. Lütfen cihaz seçim listesinden en az bir cihaz seçin!");
                    return;
                }

                foreach (var silmeItem in cihazSecimListesi.CheckedItems)
                {
                    var silmeIp = ParseIp(silmeItem);
                    if (!string.IsNullOrEmpty(silmeIp))
                        silmeHedefIpList.Add(silmeIp);
                }
            }

            int silmeBasariliSayisi = 0;

            foreach (var silmeIpAdres in silmeHedefIpList)
            {
                if (!_deviceService.TryGetConnection(silmeIpAdres, out var silmeBaglanti) || !silmeBaglanti.Bagli)
                {
                    LogYaz($"Cihaz bağlantısı yok: {silmeIpAdres}");
                    continue;
                }

                try
                {
                    var silmeCihaz = silmeBaglanti.Device;
                    const int silmeCihazNo = 1;

                    silmeCihaz.EnableDevice(silmeCihazNo, false);

                    bool silmeSonuc = silmeCihaz.DeleteEnrollData(silmeCihazNo, silmePersonelId, 1, 12);
                    if (silmeSonuc)
                    {
                        silmeCihaz.RefreshData(silmeCihazNo);
                        silmeBasariliSayisi++;
                        LogYaz($"🗑 Personel ID {silmePersonelId} -> {silmeBaglanti.Info.CihazAdi} silindi.");
                    }

                    silmeCihaz.EnableDevice(silmeCihazNo, true);
                }
                catch (Exception ex)
                {
                    LogYaz($"Silme hatası ({silmeIpAdres}): {ex.Message}");
                }
            }

            MessageBox.Show(
                silmeBasariliSayisi > 0
                    ? $"🗑 Kullanıcı ID {silmePersonelCihazID} {silmeBasariliSayisi} cihazdan silindi."
                    : "❌ Silme başarısız.");

            //Task.Run(async () => await PersonelCihazDurumunuGoster(silmePersonelId));
            this.BeginInvoke(new Action(() =>
            {
                PersonelCihazDurumunuGoster(silmePersonelId);
            }));
        }
        private void btnTopluTanimla_Click(object sender, EventArgs e)
        {
            // KART MODU
            if (chkKartModu.Checked)
            {
                var kartlar = _lookupService.GetAktifPuantajsizKartlar(_firmaId).ToList();

                if (kartlar.Count == 0)
                {
                    MessageBox.Show("Aktif kart bulunamadı.");
                    return;
                }

                // Panel'den seçilen cihazları al (DB'de yetki olmayanlar için kullanılacak)
                var panelSecilenIpList = new List<string>();
                foreach (var item in cihazSecimListesi.CheckedItems)
                {
                    var ip = ParseIp(item);
                    if (!string.IsNullOrEmpty(ip))
                        panelSecilenIpList.Add(ip);
                }

                int toplamBasarili = 0;
                int toplamAtlanan = 0;

                foreach (var kart in kartlar)
                {
                    // Her kart için ayrı ayrı yetki kontrolü
                    var yetkiliCihazIdler = _lookupService.GetKartYetkiliCihazlar(kart.KartId);

                    List<string> hedefIpList = new List<string>();

                    if (yetkiliCihazIdler.Count > 0)
                    {
                        // DB'de yetki varsa sadece yetkili cihazlara ekle
                        var tumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                        hedefIpList = tumCihazlar
                            .Where(c => yetkiliCihazIdler.Contains(c.CihazId))
                            .Select(c => c.IP)
                            .ToList();

                        LogYaz($"Toplu: {kart.KartAdi} için DB'de {yetkiliCihazIdler.Count} yetkili cihaz.");
                    }
                    else
                    {
                        // DB'de yetki yoksa panel seçimini kullan
                        if (panelSecilenIpList.Count == 0)
                        {
                            LogYaz($"⚠️ Toplu: {kart.KartAdi} için DB'de yetki yok ve panel seçimi yok, atlanıyor.");
                            toplamAtlanan++;
                            continue;
                        }

                        hedefIpList = panelSecilenIpList;
                        LogYaz($"Toplu: {kart.KartAdi} için panel seçimi kullanılıyor ({hedefIpList.Count} cihaz).");
                    }

                    // Bu kartı hedef cihazlara ekle
                    foreach (var ip in hedefIpList)
                    {
                        if (!_deviceService.TryGetConnection(ip, out var baglanti) || !baglanti.Bagli)
                            continue;

                        try
                        {
                            var cihaz = baglanti.Device;
                            const int cihazNo = 1;

                            // Zaten varsa atla
                            string mevcutIsim = "", mevcutSifre = "", mevcutKart = "";
                            int mevcutYetki = 0;
                            bool mevcutAktif = false;

                            if (cihaz.GetUserInfo(cihazNo, kart.KartId, ref mevcutIsim, ref mevcutSifre, ref mevcutYetki, ref mevcutAktif))
                                continue;

                            cihaz.EnableDevice(cihazNo, false);
                            cihaz.SSR_DeleteEnrollData(cihazNo, kart.KartId.ToString(), 12);
                            cihaz.SetStrCardNumber(kart.KartNo ?? "");

                            if (cihaz.SetUserInfo(cihazNo, kart.KartId, kart.KartAdi ?? "", "", 0, true))
                            {
                                cihaz.RefreshData(cihazNo);
                                toplamBasarili++;
                                LogYaz($"✅ Toplu: {kart.KartAdi} -> {baglanti.Info.CihazAdi}");
                            }

                            cihaz.EnableDevice(cihazNo, true);
                        }
                        catch (Exception ex)
                        {
                            LogYaz($"❌ Toplu kart tanımlama hatası ({ip} - {kart.KartAdi}): {ex.Message}");
                        }
                    }
                }

                MessageBox.Show($"Toplu Tanımlama Tamamlandı!\n✅ Başarılı: {toplamBasarili}\n⚠️ Atlanan: {toplamAtlanan}");
                return;
            }

            // PERSONEL MODU
            var personeller = _lookupService
                .GetAktifPersoneller(_firmaId)
                .Where(p => p.KartNo.HasValue)
                .Select(p => new
                {
                    PersonelIdInt = int.Parse(p.PersonelId),
                    SicilNo = p.PersonelId,
                    AdSoyad = (p.Ad ?? "") + " " + (p.Soyad ?? ""),
                    KartNo = p.KartNo.Value
                })
                .ToList();

            if (personeller.Count == 0)
            {
                MessageBox.Show("Aktif personel bulunamadı.");
                return;
            }

            // Panel'den seçilen cihazları al
            var personelPanelSecilenIpList = new List<string>();
            foreach (var item in cihazSecimListesi.CheckedItems)
            {
                var ip = ParseIp(item);
                if (!string.IsNullOrEmpty(ip))
                    personelPanelSecilenIpList.Add(ip);
            }

            int personelToplamBasarili = 0;
            int personelToplamAtlanan = 0;

            foreach (var personel in personeller)
            {
                // Her personel için ayrı ayrı yetki kontrolü
                var yetkiliCihazIdler = _lookupService.GetPersonelYetkiliCihazlar(personel.PersonelIdInt);

                List<string> hedefIpList = new List<string>();

                if (yetkiliCihazIdler.Count > 0)
                {
                    // DB'de yetki varsa sadece yetkili cihazlara ekle
                    var tumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                    hedefIpList = tumCihazlar
                        .Where(c => yetkiliCihazIdler.Contains(c.CihazId))
                        .Select(c => c.IP)
                        .ToList();

                    LogYaz($"Toplu: {personel.AdSoyad} için DB'de {yetkiliCihazIdler.Count} yetkili cihaz.");
                }
                else
                {
                    // DB'de yetki yoksa panel seçimini kullan
                    if (personelPanelSecilenIpList.Count == 0)
                    {
                        LogYaz($"⚠️ Toplu: {personel.AdSoyad} için DB'de yetki yok ve panel seçimi yok, atlanıyor.");
                        personelToplamAtlanan++;
                        continue;
                    }

                    hedefIpList = personelPanelSecilenIpList;
                    LogYaz($"Toplu: {personel.AdSoyad} için panel seçimi kullanılıyor ({hedefIpList.Count} cihaz).");
                }

                // Bu personeli hedef cihazlara ekle
                foreach (var ip in hedefIpList)
                {
                    if (!_deviceService.TryGetConnection(ip, out var baglanti) || !baglanti.Bagli)
                        continue;

                    try
                    {
                        var cihaz = baglanti.Device;
                        const int cihazNo = 1;

                        // Zaten varsa atla
                        string mevcutIsim = "", mevcutSifre = "", mevcutKart = "";
                        int mevcutYetki = 0;
                        bool mevcutAktif = false;

                        if (cihaz.GetUserInfo(cihazNo, personel.PersonelIdInt, ref mevcutIsim, ref mevcutSifre, ref mevcutYetki, ref mevcutAktif))
                            continue;

                        cihaz.EnableDevice(cihazNo, false);
                        cihaz.SSR_DeleteEnrollData(cihazNo, personel.SicilNo, 12);
                        cihaz.SetStrCardNumber(personel.KartNo.ToString());

                        if (cihaz.SetUserInfo(cihazNo, personel.PersonelIdInt, personel.AdSoyad, "", 0, true))
                        {
                            cihaz.RefreshData(cihazNo);
                            personelToplamBasarili++;
                            LogYaz($"✅ Toplu: {personel.AdSoyad} -> {baglanti.Info.CihazAdi}");
                        }

                        cihaz.EnableDevice(cihazNo, true);
                    }
                    catch (Exception ex)
                    {
                        LogYaz($"❌ Toplu personel tanımlama hatası ({ip} - {personel.AdSoyad}): {ex.Message}");
                    }
                }
            }

            MessageBox.Show($"Toplu Tanımlama Tamamlandı!\n✅ Başarılı: {personelToplamBasarili}\n⚠️ Atlanan: {personelToplamAtlanan}");
        }
        private void btnTopluSilme_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "Toplu silme işlemi yapılacak. Emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult != DialogResult.Yes)
                return;

            // KART MODU
            if (chkKartModu.Checked)
            {
                var kartlar = _lookupService.GetAktifPuantajsizKartlar(_firmaId).ToList();

                if (kartlar.Count == 0)
                {
                    MessageBox.Show("Aktif kart bulunamadı.");
                    return;
                }

                // Panel'den seçilen cihazları al
                var panelSecilenIpList = new List<string>();
                foreach (var item in cihazSecimListesi.CheckedItems)
                {
                    var ip = ParseIp(item);
                    if (!string.IsNullOrEmpty(ip))
                        panelSecilenIpList.Add(ip);
                }

                int toplamSilinen = 0;
                int toplamAtlanan = 0;

                foreach (var kart in kartlar)
                {
                    // Her kart için ayrı ayrı yetki kontrolü
                    var yetkiliCihazIdler = _lookupService.GetKartYetkiliCihazlar(kart.KartId);

                    List<string> hedefIpList = new List<string>();

                    if (yetkiliCihazIdler.Count > 0)
                    {
                        var tumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                        hedefIpList = tumCihazlar
                            .Where(c => yetkiliCihazIdler.Contains(c.CihazId))
                            .Select(c => c.IP)
                            .ToList();

                        LogYaz($"Toplu Silme: {kart.KartAdi} için DB'de {yetkiliCihazIdler.Count} yetkili cihaz.");
                    }
                    else
                    {
                        if (panelSecilenIpList.Count == 0)
                        {
                            LogYaz($"⚠️ Toplu Silme: {kart.KartAdi} için DB'de yetki yok ve panel seçimi yok, atlanıyor.");
                            toplamAtlanan++;
                            continue;
                        }

                        hedefIpList = panelSecilenIpList;
                        LogYaz($"Toplu Silme: {kart.KartAdi} için panel seçimi kullanılıyor ({hedefIpList.Count} cihaz).");
                    }

                    foreach (var ip in hedefIpList)
                    {
                        if (!_deviceService.TryGetConnection(ip, out var baglanti) || !baglanti.Bagli)
                            continue;

                        try
                        {
                            var cihaz = baglanti.Device;
                            const int cihazNo = 1;

                            cihaz.EnableDevice(cihazNo, false);

                            if (cihaz.DeleteEnrollData(cihazNo, kart.KartId, 1, 12))
                            {
                                cihaz.RefreshData(cihazNo);
                                toplamSilinen++;
                                LogYaz($"🗑 Toplu Silme: {kart.KartAdi} <- {baglanti.Info.CihazAdi}");
                            }

                            cihaz.EnableDevice(cihazNo, true);
                        }
                        catch (Exception ex)
                        {
                            LogYaz($"❌ Toplu kart silme hatası ({ip} - {kart.KartAdi}): {ex.Message}");
                        }
                    }
                }

                MessageBox.Show($"Toplu Silme Tamamlandı!\n🗑 Silinen: {toplamSilinen}\n⚠️ Atlanan: {toplamAtlanan}");
                return;
            }

            // PERSONEL MODU
            var personeller = _lookupService
                .GetAktifPersoneller(_firmaId)
                .Select(p => new
                {
                    PersonelIdInt = int.Parse(p.PersonelId),
                    AdSoyad = (p.Ad ?? "") + " " + (p.Soyad ?? "")
                })
                .ToList();

            if (personeller.Count == 0)
            {
                MessageBox.Show("Aktif personel bulunamadı.");
                return;
            }

            var personelPanelSecilenIpList = new List<string>();
            foreach (var item in cihazSecimListesi.CheckedItems)
            {
                var ip = ParseIp(item);
                if (!string.IsNullOrEmpty(ip))
                    personelPanelSecilenIpList.Add(ip);
            }

            int personelToplamSilinen = 0;
            int personelToplamAtlanan = 0;

            foreach (var personel in personeller)
            {
                var yetkiliCihazIdler = _lookupService.GetPersonelYetkiliCihazlar(personel.PersonelIdInt);

                List<string> hedefIpList = new List<string>();

                if (yetkiliCihazIdler.Count > 0)
                {
                    var tumCihazlar = _lookupService.GetAktifCihazlar(_firmaId);
                    hedefIpList = tumCihazlar
                        .Where(c => yetkiliCihazIdler.Contains(c.CihazId))
                        .Select(c => c.IP)
                        .ToList();

                    LogYaz($"Toplu Silme: {personel.AdSoyad} için DB'de {yetkiliCihazIdler.Count} yetkili cihaz.");
                }
                else
                {
                    if (personelPanelSecilenIpList.Count == 0)
                    {
                        LogYaz($"⚠️ Toplu Silme: {personel.AdSoyad} için DB'de yetki yok ve panel seçimi yok, atlanıyor.");
                        personelToplamAtlanan++;
                        continue;
                    }

                    hedefIpList = personelPanelSecilenIpList;
                    LogYaz($"Toplu Silme: {personel.AdSoyad} için panel seçimi kullanılıyor ({hedefIpList.Count} cihaz).");
                }

                foreach (var ip in hedefIpList)
                {
                    if (!_deviceService.TryGetConnection(ip, out var baglanti) || !baglanti.Bagli)
                        continue;

                    try
                    {
                        var cihaz = baglanti.Device;
                        const int cihazNo = 1;

                        cihaz.EnableDevice(cihazNo, false);

                        if (cihaz.DeleteEnrollData(cihazNo, personel.PersonelIdInt, 1, 12))
                        {
                            cihaz.RefreshData(cihazNo);
                            personelToplamSilinen++;
                            LogYaz($"🗑 Toplu Silme: {personel.AdSoyad} <- {baglanti.Info.CihazAdi}");
                        }

                        cihaz.EnableDevice(cihazNo, true);
                    }
                    catch (Exception ex)
                    {
                        LogYaz($"❌ Toplu personel silme hatası ({ip} - {personel.AdSoyad}): {ex.Message}");
                    }
                }
            }

            MessageBox.Show($"Toplu Silme Tamamlandı!\n🗑 Silinen: {personelToplamSilinen}\n⚠️ Atlanan: {personelToplamAtlanan}");
        }
        private bool TryParsePersonFromComboRow(string row, out string personelId)
        {
            personelId = null;
            if (string.IsNullOrWhiteSpace(row)) return false;

            int open = row.LastIndexOf('(');
            int close = row.LastIndexOf(')');
            if (open < 0 || close < 0 || close <= open) return false;

            personelId = row.Substring(open + 1, close - open - 1).Trim();
            return personelId.Length > 0;
        }
        private void AdjustComboDropDownWidth(ComboBox combo)
        {
            int width = combo.DropDownWidth;
            using (Graphics g = combo.CreateGraphics())
            {
                foreach (var item in combo.Items)
                {
                    string text = combo.GetItemText(item) ?? "";
                    SizeF sz = g.MeasureString(text, combo.Font);

                    int w = (int)sz.Width + SystemInformation.VerticalScrollBarWidth + 20;
                    if (w > width) width = w;
                }
            }

            int max = Screen.FromControl(combo).WorkingArea.Width - 40;
            combo.DropDownWidth = Math.Min(width, max);
        }
        private async void personelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (personelComboBox.SelectedItem == null) return;

            if (chkKartModu.Checked)
            {
                if (personelComboBox.SelectedItem is KartItem k)
                {
                    idBox.Text = k.KartId.ToString();
                    kartBox.Text = k.KartNo ?? "";
                    await KartCihazDurumunuGoster(k.KartId);
                }
                else
                {
                    idBox.Clear();
                    kartBox.Clear();
                    dgvCihazDurum.DataSource = null;
                }
                return;
            }

            if (TryParsePersonFromComboRow(personelComboBox.SelectedItem.ToString(), out string personelIdStr)
                && int.TryParse(personelIdStr, out int personelId))
            {
                idBox.Text = personelIdStr;

                var personel = _lookupService.GetPersonelById(personelId);
                if (personel != null && personel.KartNo.HasValue)
                    kartBox.Text = personel.KartNo.Value.ToString();
                else
                    kartBox.Clear();

                await PersonelCihazDurumunuGoster(personelId);
            }
            else
            {
                idBox.Clear();
                kartBox.Clear();
                dgvCihazDurum.DataSource = null;
            }
        }
        private void InitializeDataGridView()
        {
            dgvCihazDurum.AutoGenerateColumns = false;
            dgvCihazDurum.Columns.Clear();

            dgvCihazDurum.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CihazAdi",
                HeaderText = "Cihaz Adı",
                Name = "CihazAdi",
                ReadOnly = true,
                FillWeight = 25
            });

            dgvCihazDurum.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IPAdres",
                HeaderText = "IP Adres",
                Name = "IPAdres",
                ReadOnly = true,
                FillWeight = 25
            });

            dgvCihazDurum.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "YetkiVarMi",
                HeaderText = "DB Yetki",
                Name = "YetkiVarMi",
                ReadOnly = true,
                FillWeight = 25
            });

            dgvCihazDurum.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "CihazaEklenmis",
                HeaderText = "Cihazda Var",
                Name = "CihazaEklenmis",
                ReadOnly = true,
                FillWeight = 25
            });

            dgvCihazDurum.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvCihazDurum.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCihazDurum.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvCihazDurum.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCihazDurum.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCihazDurum.EnableHeadersVisualStyles = false;
        }
        private async Task PersonelCihazDurumunuGoster(int personelId)
        {
            try
            {
                var durumListesi = _lookupService.GetPersonelCihazDurumlari(personelId, _firmaId);

                // Cihazdan kontrol et
                foreach (var durum in durumListesi)
                {
                    durum.CihazaEklenmis = await CihazaEklenmisMi(durum.IPAdres, personelId);
                }

                dgvCihazDurum.DataSource = new BindingSource { DataSource = durumListesi };
            }
            catch (Exception ex)
            {
                LogYaz($"Personel cihaz durumu gösterme hatası: {ex.Message}");
            }
        }
        private async Task KartCihazDurumunuGoster(int kartId)
        {
            try
            {
                var kartDurumListesi = _lookupService.GetKartCihazDurumlari(kartId, _firmaId);

                // Cihazdan kontrol et
                foreach (var durum in kartDurumListesi)
                {
                    durum.CihazaEklenmis = await CihazaEklenmisMi(durum.IPAdres, kartId);
                }

                dgvCihazDurum.DataSource = new BindingSource { DataSource = kartDurumListesi };
            }
            catch (Exception ex)
            {
                LogYaz($"Kart cihaz durumu gösterme hatası: {ex.Message}");
            }
        }
        private Task<bool> CihazaEklenmisMi(string ipAdresi, int kullaniciId)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!_deviceService.TryGetConnection(ipAdresi, out var baglanti) || !baglanti.Bagli)
                        return false;

                    var cihaz = baglanti.Device;
                    const int makineNumarasi = 1;

                    string isim = "", sifre = "", kartNumarasi = "";
                    int yetki = 0;
                    bool aktif = false;

                    return cihaz.GetUserInfo(makineNumarasi, kullaniciId, ref isim, ref sifre, ref yetki, ref aktif);
                }
                catch
                {
                    return false;
                }
            });
        }
        private void btnCihazKullanicilariGetir_Click(object sender, EventArgs e)
        {
            cihazUserListBox.Items.Clear();

            var seciliIpListesi = new List<string>();
            foreach (var item in cihazSecimListesi.CheckedItems)
            {
                var ipAdres = ParseIp(item);
                if (!string.IsNullOrEmpty(ipAdres)) seciliIpListesi.Add(ipAdres);
            }

            if (seciliIpListesi.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!");
                return;
            }

            foreach (var ipAdres in seciliIpListesi)
            {
                if (!_deviceService.TryGetConnection(ipAdres, out var baglanti) || !baglanti.Bagli)
                {
                    LogYaz($"Cihaz bağlantısı yok: {ipAdres}");
                    continue;
                }

                try
                {
                    var cihaz = baglanti.Device;
                    const int cihazNo = 1;

                    cihaz.EnableDevice(cihazNo, false);

                    if (!cihaz.ReadAllUserID(cihazNo))
                    {
                        cihaz.EnableDevice(cihazNo, true);
                        LogYaz($"Kullanıcı listesi okunamadı: {baglanti.Info.CihazAdi} ({ipAdres})");
                        continue;
                    }

                    int kullaniciId = 0;
                    string kullaniciAdi = "", kullaniciSifre = "";
                    int kullaniciYetki = 0;
                    bool kullaniciAktif = false;

                    while (cihaz.GetAllUserInfo(cihazNo, ref kullaniciId, ref kullaniciAdi, ref kullaniciSifre, ref kullaniciYetki, ref kullaniciAktif))
                    {
                        string kartNumarasi = "";
                        try { cihaz.GetStrCardNumber(out kartNumarasi); } catch { kartNumarasi = ""; }

                        cihazUserListBox.Items.Add(
                            $"[{baglanti.Info.CihazAdi}] ID:{kullaniciId} | Ad:{kullaniciAdi} | Kart:{kartNumarasi} | Aktif:{kullaniciAktif}"
                        );
                    }

                    cihaz.EnableDevice(cihazNo, true);
                }
                catch (Exception ex)
                {
                    LogYaz($"Kullanıcı listesi hatası ({ipAdres}): {ex.Message}");
                }
            }
        }
        private string ParseIp(object cihazItem)
        {
            if (cihazItem == null) return null;
            string metin = cihazItem.ToString();

            var esleme = Regex.Match(metin, @"\b\d{1,3}(?:\.\d{1,3}){3}\b");
            if (esleme.Success) return esleme.Value;

            int parantezAc = metin.IndexOf('(');
            int parantezKapa = metin.IndexOf(')', parantezAc + 1);
            if (parantezAc >= 0 && parantezKapa > parantezAc)
                return metin.Substring(parantezAc + 1, parantezKapa - parantezAc - 1).Trim();

            return null;
        }
        private void YetkiYonetimiTabBaslat()
        {
            try
            {
                _tumCihazlar = _lookupService.GetAktifCihazlar(_firmaId).ToList();
                YetkiPersonelKartListesiniYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yetki yönetimi başlatma hatası: {ex.Message}", "Hata",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void chkYetkiKartModu_CheckedChanged(object sender, EventArgs e)
        {
            YetkiPersonelKartListesiniYukle();
            clbYetkiCihazlar.Items.Clear();
            txtYetkiArama.Text = "🔍 Ara...";
            txtYetkiArama.ForeColor = Color.Gray;
        }
        private void YetkiPersonelKartListesiniYukle()
        {
            lstYetkiPersonelKart.Items.Clear();

            try
            {
                if (chkYetkiKartModu.Checked)
                {
                    _tumKartlar = _lookupService.GetAktifPuantajsizKartlar(_firmaId).ToList();

                    foreach (var kart in _tumKartlar)
                    {
                        lstYetkiPersonelKart.Items.Add(new KartItem
                        {
                            KartId = kart.KartId,
                            KartNo = kart.KartNo ?? "",
                            KartAdi = kart.KartAdi ?? ""
                        });
                    }
                }
                else
                {
                    _tumPersoneller = _lookupService.GetAktifPersoneller(_firmaId).ToList();

                    foreach (var p in _tumPersoneller)
                    {
                        lstYetkiPersonelKart.Items.Add(new PersonelItem
                        {
                            PersonelId = Convert.ToInt32(p.PersonelId),
                            Ad = p.Ad ?? "",
                            Soyad = p.Soyad ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Liste yükleme hatası: {ex.Message}", "Hata",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void txtYetkiArama_Enter(object sender, EventArgs e)
        {
            if (txtYetkiArama.Text == "🔍 Ara..." && txtYetkiArama.ForeColor == Color.Gray)
            {
                txtYetkiArama.Text = "";
                txtYetkiArama.ForeColor = Color.Black;
            }
        }
        private void txtYetkiArama_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYetkiArama.Text))
            {
                txtYetkiArama.Text = "🔍 Ara...";
                txtYetkiArama.ForeColor = Color.Gray;
            }
        }
        private void txtYetkiArama_TextChanged(object sender, EventArgs e)
        {
            if (txtYetkiArama.ForeColor == Color.Gray) return;

            string aramaMetni = txtYetkiArama.Text.ToLower().Trim();

            lstYetkiPersonelKart.Items.Clear();

            try
            {
                if (chkYetkiKartModu.Checked)
                {
                    var filtreliKartlar = _tumKartlar
                        .Where(k => (k.KartAdi ?? "").ToLower().Contains(aramaMetni) ||
                                    (k.KartNo ?? "").ToLower().Contains(aramaMetni))
                        .ToList();

                    foreach (var kart in filtreliKartlar)
                    {
                        lstYetkiPersonelKart.Items.Add(new KartItem
                        {
                            KartId = kart.KartId,
                            KartNo = kart.KartNo ?? "",
                            KartAdi = kart.KartAdi ?? ""
                        });
                    }
                }
                else
                {
                    var filtreliPersoneller = _tumPersoneller
                        .Where(p => (p.Ad ?? "").ToLower().Contains(aramaMetni) ||
                                    (p.Soyad ?? "").ToLower().Contains(aramaMetni) ||
                                    p.PersonelId.ToString().Contains(aramaMetni))
                        .ToList();

                    foreach (var p in filtreliPersoneller)
                    {
                        lstYetkiPersonelKart.Items.Add(new PersonelItem
                        {
                            PersonelId = Convert.ToInt32(p.PersonelId),
                            Ad = p.Ad ?? "",
                            Soyad = p.Soyad ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Arama hatası: {ex.Message}", "Hata",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void lstYetkiPersonelKart_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstYetkiPersonelKart.SelectedItem == null) return;

            clbYetkiCihazlar.Items.Clear();

            try
            {
                if (chkYetkiKartModu.Checked)
                {
                    if (!(lstYetkiPersonelKart.SelectedItem is KartItem kart)) return;

                    var yetkiliCihazIdler = _lookupService.GetKartYetkiliCihazlar(kart.KartId);

                    foreach (var cihaz in _tumCihazlar)
                    {
                        bool yetkili = yetkiliCihazIdler.Contains(cihaz.CihazId);
                        clbYetkiCihazlar.Items.Add(new CihazCheckItem
                        {
                            CihazId = cihaz.CihazId,
                            CihazAdi = cihaz.CihazAdi,
                            IPAdres = cihaz.IP,
                            DisplayText = $"{cihaz.CihazAdi} ({cihaz.IP})"
                        }, yetkili);
                    }
                }
                else
                {
                    if (!(lstYetkiPersonelKart.SelectedItem is PersonelItem personelItem)) return;

                    var yetkiliCihazIdler = _lookupService.GetPersonelYetkiliCihazlar(personelItem.PersonelId);

                    foreach (var cihaz in _tumCihazlar)
                    {
                        bool yetkili = yetkiliCihazIdler.Contains(cihaz.CihazId);
                        clbYetkiCihazlar.Items.Add(new CihazCheckItem
                        {
                            CihazId = cihaz.CihazId,
                            CihazAdi = cihaz.CihazAdi,
                            IPAdres = cihaz.IP,
                            DisplayText = $"{cihaz.CihazAdi} ({cihaz.IP})"
                        }, yetkili);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yetki bilgileri yükleme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnYetkiTumunuSec_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbYetkiCihazlar.Items.Count; i++)
            {
                clbYetkiCihazlar.SetItemChecked(i, true);
            }
        }
        private void btnYetkiTumunuKaldir_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbYetkiCihazlar.Items.Count; i++)
            {
                clbYetkiCihazlar.SetItemChecked(i, false);
            }
        }
        private void btnYetkiKaydet_Click(object sender, EventArgs e)
        {
            if (lstYetkiPersonelKart.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir personel veya kart seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var secilenCihazIdler = new List<int>();
                for (int i = 0; i < clbYetkiCihazlar.Items.Count; i++)
                {
                    if (clbYetkiCihazlar.GetItemChecked(i))
                    {
                        if (clbYetkiCihazlar.Items[i] is CihazCheckItem item)
                        {
                            secilenCihazIdler.Add(item.CihazId);
                        }
                    }
                }

                bool basarili = false;
                string mesaj = "";

                if (chkYetkiKartModu.Checked)
                {
                    if (!(lstYetkiPersonelKart.SelectedItem is KartItem kart)) return;

                    basarili = _lookupService.KartYetkiKaydet(kart.KartId, secilenCihazIdler, _firmaId);
                    mesaj = basarili
                        ? $"✅ {kart.KartAdi} için {secilenCihazIdler.Count} cihaz yetkisi kaydedildi."
                        : "❌ Yetki kaydedilemedi!";
                }
                else
                {
                    if (!(lstYetkiPersonelKart.SelectedItem is PersonelItem personelItem)) return;

                    basarili = _lookupService.PersonelYetkiKaydet(personelItem.PersonelId, secilenCihazIdler, _firmaId);
                    mesaj = basarili
                        ? $"✅ {personelItem.Ad} {personelItem.Soyad} için {secilenCihazIdler.Count} cihaz yetkisi kaydedildi."
                        : "❌ Yetki kaydedilemedi!";
                }

                MessageBox.Show(mesaj, basarili ? "Başarılı" : "Hata",
                    MessageBoxButtons.OK, basarili ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yetki kaydetme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnYetkiIptal_Click(object sender, EventArgs e)
        {
            lstYetkiPersonelKart.SelectedIndex = -1;
            clbYetkiCihazlar.Items.Clear();
        }
        private async void btnOfflineVeriCek_Click(object sender, EventArgs e)
        {
            // Seçili cihaz kontrolü
            var secilenCihazlar = new List<CihazCheckItem>();
            for (int i = 0; i < clbOfflineCihazlar.Items.Count; i++)
            {
                if (clbOfflineCihazlar.GetItemChecked(i))
                {
                    if (clbOfflineCihazlar.Items[i] is CihazCheckItem item)
                    {
                        secilenCihazlar.Add(item);
                    }
                }
            }

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tarih kontrolü
            if (dtpOfflineBaslangic.Value > dtpOfflineBitis.Value)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnOfflineVeriCek.Enabled = false;
                btnOfflineVeriCek.Text = "⏳ VERİ ÇEKİLİYOR...";

                var tumLoglar = new List<OfflineLog>();

                foreach (var cihaz in secilenCihazlar)
                {
                    LogYaz($"📥 {cihaz.CihazAdi} cihazından veri çekiliyor...");

                    var loglar = await Task.Run(() =>
                        _deviceService.GetOfflineData(cihaz.IPAdres, dtpOfflineBaslangic.Value, dtpOfflineBitis.Value)
                    );

                    tumLoglar.AddRange(loglar);
                }

                // Tarihe göre sırala (yeniden eskiye)
                tumLoglar = tumLoglar.OrderByDescending(x => x.Tarih).ToList();

                // Grid'e yükle
                dgvOfflineVeriler.DataSource = new BindingSource { DataSource = tumLoglar };

                LogYaz($"✅ Toplam {tumLoglar.Count} kayıt getirildi.");

                // 👇 KÜÇÜK İYİLEŞTİRME: 0 kayıt için farklı mesaj
                if (tumLoglar.Count == 0)
                {
                    MessageBox.Show(
                        $"⚠️ Seçilen tarih aralığında kayıt bulunamadı.\n\n" +
                        $"📅 {dtpOfflineBaslangic.Value:dd.MM.yyyy HH:mm} - {dtpOfflineBitis.Value:dd.MM.yyyy HH:mm}\n" +
                        $"🖥️ {secilenCihazlar.Count} cihaz kontrol edildi.\n\n" +
                        $"💡 Listener servisiniz çalışıyor olabilir veya\n" +
                        $"   bu tarihte giriş/çıkış yapılmamış olabilir.",
                        "Kayıt Yok",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"✅ Toplam {tumLoglar.Count} kayıt getirildi.\n\n" +
                        $"📊 {secilenCihazlar.Count} cihazdan veri çekildi.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogYaz($"❌ Offline veri çekme hatası: {ex.Message}");
                MessageBox.Show($"Offline veri çekme hatası:\n\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnOfflineVeriCek.Enabled = true;
                btnOfflineVeriCek.Text = "📥 OFFLINE VERİ ÇEK";
            }
        }
        private void btnOfflineExport_Click(object sender, EventArgs e)
        {
            if (dgvOfflineVeriler.Rows.Count == 0)
            {
                MessageBox.Show("Excel'e aktarılacak veri yok!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Dosyası|*.csv";
                    saveDialog.Title = "Offline Verileri Kaydet";
                    saveDialog.FileName = $"OfflineVeriler_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                        {
                            // Header
                            writer.WriteLine("Cihaz Adı;IP Adres;Personel ID;Ad Soyad;Tarih/Saat;Doğrulama;Durum");

                            // Data
                            foreach (DataGridViewRow row in dgvOfflineVeriler.Rows)
                            {
                                if (row.DataBoundItem is OfflineLog log)
                                {
                                    writer.WriteLine($"{log.CihazAdi};{log.IPAdres};{log.PersonelId};{log.AdSoyad};" +
                                        $"{log.Tarih:dd.MM.yyyy HH:mm:ss};{log.VerifyModeText};{log.InOutModeText}");
                                }
                            }
                        }

                        LogYaz($"✅ Excel dosyası kaydedildi: {saveDialog.FileName}");
                        MessageBox.Show($"✅ Excel dosyası başarıyla kaydedildi!\n\n{saveDialog.FileName}",
                            "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Dosyayı aç
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                LogYaz($"❌ Excel aktarma hatası: {ex.Message}");
                MessageBox.Show($"Excel aktarma hatası:\n\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnOfflineTemizle_Click(object sender, EventArgs e)
        {
            if (dgvOfflineVeriler.Rows.Count == 0)
            {
                MessageBox.Show("Temizlenecek veri yok!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Grid'deki {dgvOfflineVeriler.Rows.Count} kayıt temizlenecek. Emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                dgvOfflineVeriler.DataSource = null;
                LogYaz("🗑️ Offline veri listesi temizlendi.");
            }
        }
        private void OfflineVeriTabBaslat()
        {
            try
            {
                // Cihazları listele
                clbOfflineCihazlar.Items.Clear();
                var cihazlar = _lookupService.GetAktifCihazlar(_firmaId);

                foreach (var cihaz in cihazlar)
                {
                    clbOfflineCihazlar.Items.Add(new CihazCheckItem
                    {
                        CihazId = cihaz.CihazId,
                        CihazAdi = cihaz.CihazAdi,
                        IPAdres = cihaz.IP,
                        DisplayText = $"{cihaz.CihazAdi} ({cihaz.IP})"
                    });
                }

                // Tarih picker'ları ayarla
                dtpOfflineBitis.Value = DateTime.Now;
                dtpOfflineBaslangic.Value = DateTime.Now.AddDays(-7); // Son 7 gün

                // DataGridView'i hazırla
                InitializeOfflineDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Offline veri tab başlatma hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeOfflineDataGridView()
        {
            dgvOfflineVeriler.AutoGenerateColumns = false;
            dgvOfflineVeriler.Columns.Clear();

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CihazAdi",
                HeaderText = "Cihaz Adı",
                Name = "CihazAdi",
                ReadOnly = true,
                FillWeight = 15
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IPAdres",
                HeaderText = "IP Adres",
                Name = "IPAdres",
                ReadOnly = true,
                FillWeight = 12
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PersonelId",
                HeaderText = "Sicil No",
                Name = "PersonelId",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AdSoyad",
                HeaderText = "Ad Soyad",
                Name = "AdSoyad",
                ReadOnly = true,
                FillWeight = 20
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Tarih",
                HeaderText = "Tarih/Saat",
                Name = "Tarih",
                ReadOnly = true,
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm:ss" }
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VerifyModeText",
                HeaderText = "Doğrulama",
                Name = "VerifyModeText",
                ReadOnly = true,
                FillWeight = 14
            });

            dgvOfflineVeriler.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InOutModeText",
                HeaderText = "Durum",
                Name = "InOutModeText",
                ReadOnly = true,
                FillWeight = 14
            });

            // Görünüm ayarları
            dgvOfflineVeriler.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvOfflineVeriler.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvOfflineVeriler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvOfflineVeriler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOfflineVeriler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOfflineVeriler.EnableHeadersVisualStyles = false;
            dgvOfflineVeriler.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvOfflineVeriler.RowTemplate.Height = 32;
        }
        private void CihazYonetimiTabBaslat()
        {
            try
            {
                // Cihazları listele
                clbCihazYonetimCihazlar.Items.Clear();
                var cihazlar = _lookupService.GetAktifCihazlar(_firmaId);

                foreach (var cihaz in cihazlar)
                {
                    clbCihazYonetimCihazlar.Items.Add(new CihazCheckItem
                    {
                        CihazId = cihaz.CihazId,
                        CihazAdi = cihaz.CihazAdi,
                        IPAdres = cihaz.IP,
                        DisplayText = $"{cihaz.CihazAdi} ({cihaz.IP})"
                    });
                }

                // DataGridView'i hazırla
                InitializeCihazBilgileriDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cihaz yönetimi tab başlatma hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeCihazBilgileriDataGridView()
        {
            dgvCihazBilgileri.AutoGenerateColumns = false;
            dgvCihazBilgileri.Columns.Clear();

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CihazAdi",
                HeaderText = "Cihaz Adı",
                Name = "CihazAdi",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IPAdres",
                HeaderText = "IP Adres",
                Name = "IPAdres",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FirmwareVersion",
                HeaderText = "Firmware",
                Name = "FirmwareVersion",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SeriNo",
                HeaderText = "Seri No",
                Name = "SeriNo",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Platform",
                HeaderText = "Platform",
                Name = "Platform",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MevcutKullaniciSayisi",
                HeaderText = "Kullanıcı",
                Name = "MevcutKullaniciSayisi",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "KullaniciKapasitesi",
                HeaderText = "Kullanıcı Kapasite",
                Name = "KullaniciKapasitesi",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MevcutLogSayisi",
                HeaderText = "Log Sayısı",
                Name = "MevcutLogSayisi",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LogKapasitesi",
                HeaderText = "Log Kapasite",
                Name = "LogKapasitesi",
                ReadOnly = true,
                FillWeight = 10
            });

            dgvCihazBilgileri.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CihazSaati",
                HeaderText = "Cihaz Saati",
                Name = "CihazSaati",
                ReadOnly = true,
                FillWeight = 10,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm:ss" }
            });

            // Görünüm ayarları
            dgvCihazBilgileri.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvCihazBilgileri.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCihazBilgileri.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvCihazBilgileri.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCihazBilgileri.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvCihazBilgileri.EnableHeadersVisualStyles = false;
            dgvCihazBilgileri.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }
        private async void btnSaatSenkronize_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Seçilen {secilenCihazlar.Count} cihazın saati senkronize edilecek. Devam edilsin mi?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No) return;

            try
            {
                btnSaatSenkronize.Enabled = false;
                btnSaatSenkronize.Text = "⏳ İŞLEM YAPILIYOR...";

                int basarili = 0, basarisiz = 0;

                foreach (var cihaz in secilenCihazlar)
                {
                    bool sonuc = await Task.Run(() => _deviceService.SynchronizeTime(cihaz.IPAdres));
                    if (sonuc) basarili++; else basarisiz++;
                }

                MessageBox.Show(
                    $"✅ Başarılı: {basarili}\n❌ Başarısız: {basarisiz}",
                    "İşlem Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSaatSenkronize.Enabled = true;
                btnSaatSenkronize.Text = "⏰ SAAT SENKRONİZE ET";
            }
        }
        private async void btnCihazRestart_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ DİKKAT!\n\nSeçilen {secilenCihazlar.Count} cihaz yeniden başlatılacak!\n\nDevam edilsin mi?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No) return;

            try
            {
                btnCihazRestart.Enabled = false;
                btnCihazRestart.Text = "⏳ İŞLEM YAPILIYOR...";

                int basarili = 0, basarisiz = 0;

                foreach (var cihaz in secilenCihazlar)
                {
                    bool sonuc = await Task.Run(() => _deviceService.RestartDevice(cihaz.IPAdres));
                    if (sonuc) basarili++; else basarisiz++;
                }

                MessageBox.Show(
                    $"✅ Başarılı: {basarili}\n❌ Başarısız: {basarisiz}\n\n⏳ Cihazların yeniden başlaması birkaç dakika sürebilir.",
                    "İşlem Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCihazRestart.Enabled = true;
                btnCihazRestart.Text = "🔄 CİHAZI YENİDEN BAŞLAT";
            }
        }
        private async void btnCihazKapat_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ ÇOK DİKKAT!\n\nSeçilen {secilenCihazlar.Count} cihaz KAPATILACAK!\n\nBu işlem geri alınamaz!\n\nDevam edilsin mi?",
                "UYARI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop
            );

            if (result == DialogResult.No) return;

            try
            {
                btnCihazKapat.Enabled = false;
                btnCihazKapat.Text = "⏳ İŞLEM YAPILIYOR...";

                int basarili = 0, basarisiz = 0;

                foreach (var cihaz in secilenCihazlar)
                {
                    bool sonuc = await Task.Run(() => _deviceService.PowerOffDevice(cihaz.IPAdres));
                    if (sonuc) basarili++; else basarisiz++;
                }

                MessageBox.Show(
                    $"✅ Başarılı: {basarili}\n❌ Başarısız: {basarisiz}",
                    "İşlem Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCihazKapat.Enabled = true;
                btnCihazKapat.Text = "⚡ CİHAZI KAPAT";
            }
        }
        private async void btnTumLoglariSil_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ DİKKAT!\n\nSeçilen {secilenCihazlar.Count} cihazın TÜM LOGLARI silinecek!\n\nBu işlem geri alınamaz!\n\nDevam edilsin mi?",
                "UYARI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No) return;

            try
            {
                btnTumLoglariSil.Enabled = false;
                btnTumLoglariSil.Text = "⏳ İŞLEM YAPILIYOR...";

                int basarili = 0, basarisiz = 0;

                foreach (var cihaz in secilenCihazlar)
                {
                    bool sonuc = await Task.Run(() => _deviceService.ClearAllLogs(cihaz.IPAdres));
                    if (sonuc) basarili++; else basarisiz++;
                }

                MessageBox.Show(
                    $"✅ Başarılı: {basarili}\n❌ Başarısız: {basarisiz}",
                    "İşlem Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTumLoglariSil.Enabled = true;
                btnTumLoglariSil.Text = "🗑️ TÜM LOGLARI SİL";
            }
        }
        private async void btnTumKullanicilariSil_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ ÇOK DİKKAT!\n\nSeçilen {secilenCihazlar.Count} cihazın TÜM KULLANICILARI silinecek!\n\nBu işlem geri alınamaz!\n\nDevam edilsin mi?",
                "UYARI",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop
            );

            if (result == DialogResult.No) return;

            try
            {
                btnTumKullanicilariSil.Enabled = false;
                btnTumKullanicilariSil.Text = "⏳ İŞLEM YAPILIYOR...";

                int basarili = 0, basarisiz = 0;

                foreach (var cihaz in secilenCihazlar)
                {
                    bool sonuc = await Task.Run(() => _deviceService.ClearAllUsers(cihaz.IPAdres));
                    if (sonuc) basarili++; else basarisiz++;
                }

                MessageBox.Show(
                    $"✅ Başarılı: {basarili}\n❌ Başarısız: {basarisiz}",
                    "İşlem Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTumKullanicilariSil.Enabled = true;
                btnTumKullanicilariSil.Text = "👥 TÜM KULLANICILARI SİL";
            }
        }
        private async void btnCihazBilgileriYenile_Click(object sender, EventArgs e)
        {
            var secilenCihazlar = GetSecilenCihazlar(clbCihazYonetimCihazlar);

            if (secilenCihazlar.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir cihaz seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnCihazBilgileriYenile.Enabled = false;
                btnCihazBilgileriYenile.Text = "⏳ BİLGİLER OKUNUYOR...";

                var cihazBilgileri = new List<CihazBilgi>();

                foreach (var cihaz in secilenCihazlar)
                {
                    var bilgi = await Task.Run(() => _deviceService.GetDeviceInfo(cihaz.IPAdres));
                    if (bilgi != null)
                    {
                        cihazBilgileri.Add(bilgi);
                    }
                }

                dgvCihazBilgileri.DataSource = new BindingSource { DataSource = cihazBilgileri };

                LogYaz($"✅ {cihazBilgileri.Count} cihaz bilgisi okundu.");
            }
            catch (Exception ex)
            {
                LogYaz($"❌ Cihaz bilgisi okuma hatası: {ex.Message}");
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCihazBilgileriYenile.Enabled = true;
                btnCihazBilgileriYenile.Text = "🔄 CİHAZ BİLGİLERİNİ YENİLE";
            }
        }
        private List<CihazCheckItem> GetSecilenCihazlar(CheckedListBox listBox)
        {
            var secilenler = new List<CihazCheckItem>();
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                if (listBox.GetItemChecked(i))
                {
                    if (listBox.Items[i] is CihazCheckItem item)
                    {
                        secilenler.Add(item);
                    }
                }
            }
            return secilenler;
        }
    }
}
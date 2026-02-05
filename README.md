# CeyPASS Cihaz Paneli

[ ��🇷 Türkçe ](#türkçe) | [ 🇬🇧 English ](#english)

---

<a name="türkçe"></a>
## 🇹🇷 Türkçe

### Proje Hakkında
CeyPASS Cihaz Paneli, ZKTeco marka biyometrik cihazların (parmak izi okuyucu, yüz tanıma terminalleri) merkezi olarak yönetilmesini sağlayan bir Windows Forms uygulamasıdır. Uygulama, personel ve kart bazlı erişim kontrolü, cihaz yönetimi, offline veri toplama ve yetkilendirme işlemlerini gerçekleştirir.

### Özellikler

#### 🔐 Kullanıcı Yönetimi
- Güvenli kullanıcı girişi
- Firma bazlı yetkilendirme
- Kullanıcı oturum yönetimi

#### 👥 Personel ve Kart Yönetimi
- Aktif personel listesi görüntüleme
- Puantajsız kart yönetimi
- Personel ve kart bazlı cihaz yetkilendirme
- Toplu tanımlama ve silme işlemleri

#### 🖥️ Cihaz Yönetimi
- ZKTeco cihazlarına TCP/IP üzerinden bağlantı
- Cihaz durumu izleme (bağlı/bağlı değil)
- Cihaz bilgilerini görüntüleme (model, seri no, firmware, MAC adresi)
- Kullanıcı ve log kapasitesi takibi
- Saat senkronizasyonu
- Cihaz yeniden başlatma ve kapatma
- Tüm logları ve kullanıcıları silme

#### 📊 Offline Veri Yönetimi
- Cihazlardan offline veri çekme
- Giriş/çıkış kayıtlarını görüntüleme
- Excel formatında veri dışa aktarma
- Offline veri temizleme

#### 🔄 Yetkilendirme Sistemi
- Personel bazlı cihaz yetkilendirme
- Kart bazlı cihaz yetkilendirme
- Veritabanı ile senkronize yetki yönetimi
- Toplu yetki ekleme/çıkarma

#### 🔄 Otomatik Güncelleme
- Uygulama başlangıcında otomatik güncelleme kontrolü
- Zorunlu güncelleme desteği
- HTTP üzerinden güncelleme paketi indirme

### Teknoloji Stack

#### Framework ve Dil
- **.NET Framework 4.7.2**
- **C# (Windows Forms)**

#### Mimari Katmanlar
Proje, katmanlı mimari (Layered Architecture) prensiplerine göre tasarlanmıştır:

1. **CeyPASSCihazPanel.UI** - Kullanıcı Arayüzü Katmanı
2. **CeyPASSCihazPanel.Business** - İş Mantığı Katmanı
3. **CeyPASSCihazPanel.DAL** - Veri Erişim Katmanı
4. **CeyPASSCihazPanel.Entities** - Varlık Katmanı

### Kurulum ve Çalıştırma

> [!ÖNEMLİ]
> **Yapılandırma Gereklidir**: Bu proje güvenli bir yapılandırma kullanır. Uygulamayı çalıştırmadan önce `App.config` dosyasını ayarlamanız gerekir.

1.  **Projeyi indirin**:
    ```bash
    git clone https://github.com/tahirkoca/CeyPASSCihazPanel.git
    ```

2.  **Yapılandırma Dosyasını Ayarlayın**:
    - `CeyPASSCihazPanel.UI` klasörüne gidin.
    - **`App.config.example`** dosyasını bulun.
    - Adını **`App.config`** olarak değiştirin.
    - Yeni `App.config` dosyasını açın ve connection string içerisindeki şifre alanını güncelleyin:
    ```xml
    <connectionStrings>
        <add name="CeyPASS"
             connectionString="Server=...;Database=CeyPASS;User Id=sa;Password=SIFRENIZI_BURAYA_YAZIN;" />
    </connectionStrings>
    ```

3.  **Visual Studio Kurulumu**:
    - `CeyPASSCihazPanel.sln` dosyasını açın.
    - NuGet paketlerini geri yükleyin.
    - `zkemkeeper` COM bileşenini kaydedin (gerekirse).
    - Projeyi derleyin ve çalıştırın (F5).

### Lisans
Bu proje özel mülkiyettir ve telif hakkı koruması altındadır.

### İletişim
**Geliştirici:** Tahir Koca

---

<a name="english"></a>
## 🇬🇧 English

### About the Project
CeyPASS Device Panel is a Windows Forms application designed for the centralized management of ZKTeco biometric devices (fingerprint readers, face recognition terminals). The application handles personnel and card-based access control, device management, offline data collection, and authorization processes.

### Features

#### 🔐 User Management
- Secure user login
- Company-based authorization
- User session management

#### 👥 Personnel and Card Management
- View active personnel list
- Manage cards without attendance (puantaj)
- Personnel and card-based device authorization
- Bulk definition and deletion operations

#### 🖥️ Device Management
- Connection to ZKTeco devices via TCP/IP
- Device status monitoring (connected/disconnected)
- View device information (model, serial no, firmware, MAC address)
- User and log capacity tracking
- Time synchronization
- Reboot and power off devices
- Delete all logs and users

#### 📊 Offline Data Management
- Retrieve offline data from devices
- View entry/exit records
- Export data to Excel format
- Clear offline data

#### 🔄 Authorization System
- Personnel-based device authorization
- Card-based device authorization
- Database-synchronized authorization management
- Bulk authorization addition/removal

#### 🔄 Auto Update
- Automatic update check at startup
- Mandatory update support
- Download update packages via HTTP

### Technology Stack

#### Framework and Language
- **.NET Framework 4.7.2**
- **C# (Windows Forms)**

#### Architectural Layers
The project is designed according to Layered Architecture principles:

1. **CeyPASSCihazPanel.UI** - User Information Layer
2. **CeyPASSCihazPanel.Business** - Business Logic Layer
3. **CeyPASSCihazPanel.DAL** - Data Access Layer
4. **CeyPASSCihazPanel.Entities** - Entity Layer

### Installation and Setup

> [!IMPORTANT]
> **Configuration Required**: This project uses a secure configuration setup. You must configure the `App.config` file before running the application.

1.  **Clone the project**:
    ```bash
    git clone https://github.com/tahirkoca/CeyPASSCihazPanel.git
    ```

2.  **Setup Configuration File**:
    - Navigate to the `CeyPASSCihazPanel.UI` directory.
    - Find the file named **`App.config.example`**.
    - Rename it to **`App.config`**.
    - Open `App.config` and update the connection string with your SQL Server password:
    ```xml
    <connectionStrings>
        <add name="CeyPASS"
             connectionString="Server=...;Database=CeyPASS;User Id=sa;Password=YOUR_PASSWORD_HERE;" />
    </connectionStrings>
    ```

3.  **Open in Visual Studio**:
    - Open `CeyPASSCihazPanel.sln`.
    - Restore NuGet packages.
    - Register `zkemkeeper` COM component (if not automatically handled).
    - Build and Run (F5).

---

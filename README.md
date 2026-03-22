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
- **.NET 8 (Windows)**
- **C# (Windows Forms)**

#### Mimari Katmanlar
Proje, katmanlı mimari (Layered Architecture) prensiplerine göre tasarlanmıştır:

1. **CeyPASSCihazPanel.UI** - Kullanıcı Arayüzü Katmanı
2. **CeyPASSCihazPanel.Business** - İş Mantığı Katmanı
3. **CeyPASSCihazPanel.DAL** - Veri Erişim Katmanı
4. **CeyPASSCihazPanel.Entities** - Varlık Katmanı

### CI/CD (Sürekli Entegrasyon ve Dağıtım)

Bu depo, GitHub Actions tabanlı bir CI/CD hattına sahiptir:

- Ana dala (`main`) yapılan push veya manuel tetikleme sonrasında Windows Forms uygulaması için otomatik build çalıştırılır.
- Derleme çıktısı güncelleme paketi (zip + update.xml) olarak üretilir ve dağıtım klasörüne kopyalanır.
- Uygulama içi otomatik güncelleme (AutoUpdater.NET) bu paketler üzerinden kullanıcılara yeni sürümleri sunar.

### Testler

Proje, **xUnit** tabanlı bir birim test paketi içermektedir. Testler `CeyPASSCihazPanel.Tests` projesinde yer alır.

#### Test Bağımlılıkları

| Paket | Sürüm |
|---|---|
| xunit | 2.9.2 |
| Moq | 4.20.72 |
| FluentAssertions | 6.12.2 |

#### Test Kapsamı

| Test Dosyası | Kapsadığı Alan |
|---|---|
| `AuthServiceTests.cs` | Kullanıcı girişi doğrulama, boş/hatalı kimlik bilgileri, büyük/küçük harf duyarlılığı |
| `AdminLookUpServiceTests.cs` | Personel, cihaz ve yetki CRUD işlemlerinin delegasyonu |
| `BulkUploadServiceTests.cs` | Toplu kişi/yemekhane yükleme, şablon kolon yapısı kontrolü |
| `CihazGrupServiceTests.cs` | Cihaz grubu CRUD, null guard ve cihaz-grup ilişkilendirme |
| `MiniExcelTests.cs` | XLSX dosyası üretimi, ZIP yapısı, XML kaçış karakterleri, üzerine yazma davranışı |

#### Testleri Çalıştırma

**Komut satırından (dotnet CLI):**
```bash
dotnet test CeyPASSCihazPanel.Tests/CeyPASSCihazPanel.Tests.csproj
```

**Visual Studio'da:**
- Çözüm (`CeyPASSCihazPanel.sln`) açıldığında `CeyPASSCihazPanel.Tests` projesi otomatik olarak Test Explorer'a yüklenir.
- **Test** → **Tüm Testleri Çalıştır** seçeneği ile tüm testler çalıştırılabilir.

> **Not:** Mevcut CI/CD pipeline yalnızca derleme ve dağıtım işlemlerini kapsamaktadır; `dotnet test` adımı henüz eklenmemiştir.

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
             connectionString="Server=...;Database=CeyPASS;User Id=ID_BURAYA_YAZIN;Password=SIFRENIZI_BURAYA_YAZIN;Encrypt=False;TrustServerCertificate=True;" />
    </connectionStrings>
    ```
    
    > Not: `Microsoft.Data.SqlClient` varsayılan olarak şifrelemeyi açtığı için, kurum içi/self-signed sertifikalı SQL Server'larda yukarıdaki `Encrypt=False;TrustServerCertificate=True;` ayarı gerekebilir.

3.  **ZKTeco COM bileşeni (zkemkeeper)** – cihazlara bağlanmak için zorunludur:
    - ZKTeco SDK veya cihaz CD'sinden **zkemkeeper.dll** dosyasını alıp `CeyPASSCihazPanel.UI\Libs` klasörüne kopyalayın.
    - Proje **registration-free COM** kullanır: derleme sonrası DLL otomatik olarak uygulama çıktı klasörüne kopyalanır; **regsvr32 ile kayıt gerekmez**.
    - **"Modül yüklenemedi" / "Belirtilen yordam bulunamadı"** hatası alırsanız, DLL genelde eski Visual C++ çalışma zamanına bağımlıdır. **Microsoft Visual C++ Redistributable (x86)** yükleyin: [VC++ 2015-2022 x86](https://aka.ms/vs/17/release/vc_redist.x86.exe) ve gerekirse [VC++ 2010 x86](https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-6A6860F8D4F6/vcredist_x86.exe). Yükleme sonrası uygulamayı yeniden başlatın.
    - İsterseniz **RegisterZkemkeeper.bat** ile de COM kaydı yapabilirsiniz (yönetici gerekir); registration-free ile kayıt şart değildir.

4.  **Visual Studio / Derleme**:
    - `CeyPASSCihazPanel.sln` dosyasını açın.
    - NuGet paketlerini geri yükleyin.
    - Projeyi derleyin ve çalıştırın (F5).

5.  **Dağıtım (Başka PC'ye EXE kopyalama)**:
    - En güvenlisi `publish` çıktısını komple kopyalamaktır (exe + tüm dll/manifest/config dosyaları).
    - Framework-dependent çalıştırırsanız hedef PC'de **.NET 8 Desktop Runtime (x86)** yüklü olmalı.
    - .NET kurdurmadan çalıştırmak için self-contained yayın alabilirsiniz:
      ```bash
      dotnet publish CeyPASSCihazPanel.UI/CeyPASSCihazPanel.UI.csproj -c Release -r win-x86 --self-contained true
      ```
      Çıktı klasörünü komple kopyalayın.

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
- **.NET 8 (Windows)**
- **C# (Windows Forms)**

#### Architectural Layers
The project is designed according to Layered Architecture principles:

1. **CeyPASSCihazPanel.UI** - User Information Layer
2. **CeyPASSCihazPanel.Business** - Business Logic Layer
3. **CeyPASSCihazPanel.DAL** - Data Access Layer
4. **CeyPASSCihazPanel.Entities** - Entity Layer

### CI/CD (Continuous Integration and Deployment)

This repository has a CI/CD pipeline based on GitHub Actions:

- After each push to `main` or manual trigger, an automatic build runs for the Windows Forms application.
- The build output is produced as an update package (zip + update.xml) and copied to the deploy folder.
- The in-app auto-updater (AutoUpdater.NET) delivers new versions to users from these packages.

### Tests

The project includes a unit test suite based on **xUnit**, located in the `CeyPASSCihazPanel.Tests` project.

#### Test Dependencies

| Package | Version |
|---|---|
| xunit | 2.9.2 |
| Moq | 4.20.72 |
| FluentAssertions | 6.12.2 |

#### Test Coverage

| Test File | Area Covered |
|---|---|
| `AuthServiceTests.cs` | User login validation, empty/invalid credentials, case-sensitive password checks |
| `AdminLookUpServiceTests.cs` | Personnel, device, and authorization CRUD delegation |
| `BulkUploadServiceTests.cs` | Bulk personnel/cafeteria upload, template column structure validation |
| `CihazGrupServiceTests.cs` | Device group CRUD, null guard, and device-group association |
| `MiniExcelTests.cs` | XLSX file generation, ZIP structure, XML escaping, overwrite behavior |

#### Running the Tests

**Via command line (dotnet CLI):**
```bash
dotnet test CeyPASSCihazPanel.Tests/CeyPASSCihazPanel.Tests.csproj
```

**In Visual Studio:**
- When the solution (`CeyPASSCihazPanel.sln`) is opened, the `CeyPASSCihazPanel.Tests` project is automatically loaded into Test Explorer.
- Use **Test** → **Run All Tests** to execute the full test suite.

> **Note:** The current CI/CD pipeline only covers build and deployment steps; a `dotnet test` step has not been added yet.

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
             connectionString="Server=...;Database=CeyPASS;User Id=YOUR_ID_HERE;Password=YOUR_PASSWORD_HERE;Encrypt=False;TrustServerCertificate=True;" />
    </connectionStrings>
    ```

    > Note: `Microsoft.Data.SqlClient` enables encryption by default. For on-prem/self-signed SQL Server setups, `Encrypt=False;TrustServerCertificate=True;` may be required.

3.  **ZKTeco COM component (zkemkeeper)** (required to connect devices):
    - Copy **`zkemkeeper.dll`** from the ZKTeco SDK / device media into `CeyPASSCihazPanel.UI\Libs`.
    - This project uses **registration-free COM**: the DLL is copied to the output folder after build; **no `regsvr32` is required**.
    - If you see “module could not be loaded” / “procedure entry point not found”, install **Microsoft Visual C++ Redistributable (x86)**: [VC++ 2015-2022 x86](https://aka.ms/vs/17/release/vc_redist.x86.exe) (and if needed [VC++ 2010 x86](https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-6A6860F8D4F6/vcredist_x86.exe)).

4.  **Open in Visual Studio**:
    - Open `CeyPASSCihazPanel.sln`.
    - Restore NuGet packages.
    - Build and Run (F5).

5.  **Deploy (copy to another PC)**:
    - Prefer copying the full `publish` output (exe + all dlls/manifests/config).
    - If framework-dependent, the target PC must have **.NET 8 Desktop Runtime (x86)** installed.
    - To avoid installing .NET on the target PC, publish self-contained:
      ```bash
      dotnet publish CeyPASSCihazPanel.UI/CeyPASSCihazPanel.UI.csproj -c Release -r win-x86 --self-contained true
      ```
      Copy the whole output directory.

---

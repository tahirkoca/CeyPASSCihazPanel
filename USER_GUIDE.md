# CeyPASS Cihaz Paneli - Kullanıcı Kılavuzu

## İçindekiler
1. [Giriş](#giriş)
2. [Sistem Gereksinimleri](#sistem-gereksinimleri)
3. [İlk Kurulum](#ilk-kurulum)
4. [Uygulamaya Giriş](#uygulamaya-giriş)
5. [Ana Ekran Tanıtımı](#ana-ekran-tanıtımı)
6. [Personel ve Kart Tanımlama](#personel-ve-kart-tanımlama)
7. [Toplu İşlemler](#toplu-işlemler)
8. [Yetki Yönetimi](#yetki-yönetimi)
9. [Offline Veri İşlemleri](#offline-veri-işlemleri)
10. [Cihaz Yönetimi](#cihaz-yönetimi)
11. [Sık Karşılaşılan Sorunlar](#sık-karşılaşılan-sorunlar)

---

## Giriş

CeyPASS Cihaz Paneli, biyometrik cihazlarınızı (parmak izi okuyucu, yüz tanıma terminalleri) merkezi olarak yönetmenizi sağlayan bir masaüstü uygulamasıdır. Bu uygulama ile:

✅ Personel ve kartları cihazlara tanımlayabilirsiniz  
✅ Toplu tanımlama ve silme işlemleri yapabilirsiniz  
✅ Cihaz yetkilerini yönetebilirsiniz  
✅ Cihazlardan giriş-çıkış verilerini çekebilirsiniz  
✅ Cihazları uzaktan yönetebilirsiniz  

---

## Sistem Gereksinimleri

### Minimum Gereksinimler
- **İşletim Sistemi**: Windows 10 veya üzeri
- **İşlemci**: Intel Core i3 veya eşdeğeri
- **RAM**: 4 GB
- **Disk Alanı**: 500 MB boş alan
- **.NET Framework**: 4.7.2 veya üzeri
- **Ağ**: Cihazlarla aynı ağda olmalısınız

### Önerilen Gereksinimler
- **İşletim Sistemi**: Windows 10/11 (64-bit)
- **İşlemci**: Intel Core i5 veya üzeri
- **RAM**: 8 GB
- **Ağ**: Gigabit Ethernet

---

## İlk Kurulum

### Adım 1: Kurulum Dosyasını Çalıştırın

1. `CeyPASSCihazPanel_Setup.msi` dosyasını çift tıklayın
2. Kurulum sihirbazını takip edin
3. Lisans sözleşmesini kabul edin
4. Kurulum klasörünü seçin (varsayılan: `C:\Program Files\CeyPASSCihazPanel`)
5. "Kur" butonuna tıklayın

### Adım 2: İlk Çalıştırma

1. Masaüstündeki "CeyPASS Cihaz Paneli" kısayoluna çift tıklayın
2. Uygulama otomatik güncelleme kontrolü yapacaktır
3. Güncelleme varsa otomatik olarak indirilip kurulacaktır

> ⚠️ **Önemli**: İlk çalıştırmada internet bağlantısı gereklidir.

---

## Uygulamaya Giriş

### Giriş Ekranı

![Giriş Ekranı](images/login_screen.png)

1. **Kullanıcı Adı**: Size verilen kullanıcı adınızı girin
2. **Şifre**: Şifrenizi girin
3. **Firma**: Firma seçin (opsiyonel - admin kullanıcılar için)
4. **Giriş**: Giriş yapmak için tıklayın

### Giriş Yapma

```
Kullanıcı Adı: admin
Şifre: ********
Firma: [Tüm Firmalar] veya [Firma Adı]
```

> 💡 **İpucu**: Firma seçmezseniz tüm firmaların verilerini görebilirsiniz (yetkiniz varsa).

### Şifremi Unuttum

Şifrenizi unuttuysanız, sistem yöneticinizle iletişime geçin.

---

## Ana Ekran Tanıtımı

Giriş yaptıktan sonra ana ekran açılır. Ana ekran 6 sekmeden oluşur:

### 1️⃣ Personel/Kart Tanımlama
Tekil personel veya kart tanımlama işlemleri

### 2️⃣ Toplu Tanımlama
Birden fazla personeli aynı anda tanımlama

### 3️⃣ Toplu Silme
Birden fazla personeli aynı anda silme

### 4️⃣ Yetki Yönetimi
Cihaz bazlı yetki atama ve kaldırma

### 5️⃣ Offline Veri
Cihazlardan giriş-çıkış verilerini çekme

### 6️⃣ Cihaz Yönetimi
Cihaz bilgileri ve kontrol işlemleri

---

## Personel ve Kart Tanımlama

### Personel Tanımlama

#### Adım 1: Personel Seçimi

1. **"Personel/Kart"** sekmesine tıklayın
2. **"Personel Modu"** seçeneğinin işaretli olduğundan emin olun
3. Arama kutusuna personel adı yazarak arama yapabilirsiniz
4. Listeden personeli seçin

![Personel Seçimi](images/personel_secimi.png)

#### Adım 2: Cihaz Seçimi

Personeli seçtikten sonra, sağ taraftaki cihaz listesi görünür:

- ✅ **Yeşil işaret**: Personel bu cihazda tanımlı
- ❌ **Kırmızı çarpı**: Personel bu cihazda tanımlı değil
- 🔵 **Mavi nokta**: Cihaz bağlı
- 🔴 **Kırmızı nokta**: Cihaz bağlı değil

**Cihaz seçmek için**:
1. Tanımlamak istediğiniz cihazları işaretleyin
2. Birden fazla cihaz seçebilirsiniz

#### Adım 3: Tanımlama

1. **"Tanımla"** butonuna tıklayın
2. İşlem başlar ve her cihaz için durum gösterilir
3. İşlem tamamlandığında sonuç mesajı görüntülenir

```
✅ Başarılı: 3 cihaza eklendi
❌ Hatalı: 1 cihaza eklenemedi (Bağlantı hatası)
```

> ⚠️ **Dikkat**: Tanımlama işlemi sırasında cihazların açık ve ağa bağlı olması gerekir.

### Kart Tanımlama

Puantajsız kart tanımlama işlemi personel tanımlama ile aynıdır:

1. **"Kart Modu"** seçeneğini işaretleyin
2. Listeden kartı seçin
3. Cihazları işaretleyin
4. **"Tanımla"** butonuna tıklayın

---

## Personel/Kart Silme

### Tekil Silme

1. **"Personel/Kart"** sekmesinde personeli/kartı seçin
2. Silmek istediğiniz cihazları işaretleyin
3. **"Kişi Sil"** butonuna tıklayın
4. Onay mesajını kabul edin

> ⚠️ **Uyarı**: Silme işlemi geri alınamaz!

---

## Toplu İşlemler

### Toplu Tanımlama

Birden fazla personeli aynı anda tanımlamak için:

#### Adım 1: Toplu Tanımlama Sekmesi

1. **"Toplu Tanımlama"** sekmesine tıklayın
2. **"Personel Modu"** veya **"Kart Modu"** seçin

#### Adım 2: Personel Seçimi

**Yöntem 1: Listeden Seçim**
1. Sol taraftaki listeden personelleri tek tek işaretleyin
2. Birden fazla personel seçebilirsiniz

**Yöntem 2: Tümünü Seç**
1. **"Tümünü Seç"** butonuna tıklayın
2. Tüm personeller seçilir

**Yöntem 3: Arama ile Seçim**
1. Arama kutusuna kriter girin
2. Filtrelenen listeden seçim yapın

#### Adım 3: Cihaz Seçimi

1. Sağ taraftaki cihaz listesinden cihazları işaretleyin
2. **"Tümünü Seç"** ile tüm cihazları seçebilirsiniz

#### Adım 4: Tanımlama

1. **"Toplu Tanımla"** butonuna tıklayın
2. Onay mesajını kabul edin
3. İşlem başlar ve ilerleme çubuğu gösterilir
4. Her personel ve cihaz için durum log'lanır

```
[10:30:15] Ahmet Yılmaz - Giriş Kapısı: BAŞARILI
[10:30:16] Ahmet Yılmaz - Çıkış Kapısı: BAŞARILI
[10:30:17] Mehmet Demir - Giriş Kapısı: BAŞARILI
...
```

> 💡 **İpucu**: Toplu işlemler uzun sürebilir. İşlem sırasında uygulamayı kapatmayın.

### Toplu Silme

Toplu silme işlemi toplu tanımlama ile aynı mantıkta çalışır:

1. **"Toplu Silme"** sekmesine tıklayın
2. Personelleri seçin
3. Cihazları seçin
4. **"Toplu Sil"** butonuna tıklayın
5. Onay mesajını kabul edin

---

## Yetki Yönetimi

Yetki yönetimi sekmesi, personel/kartların hangi cihazlara erişim yetkisi olduğunu yönetmenizi sağlar.

### Yetki Görüntüleme

1. **"Yetki Yönetimi"** sekmesine tıklayın
2. Sol listeden personel/kart seçin
3. Sağ tarafta cihaz yetkileri görüntülenir:
   - ✅ İşaretli: Yetkili
   - ☐ İşaretsiz: Yetkili değil

### Yetki Ekleme

1. Personel/kart seçin
2. Yetki vermek istediğiniz cihazları işaretleyin
3. **"Kaydet"** butonuna tıklayın

### Yetki Kaldırma

1. Personel/kart seçin
2. Yetkisini kaldırmak istediğiniz cihazların işaretini kaldırın
3. **"Kaydet"** butonuna tıklayın

### Toplu Yetki İşlemleri

- **"Tümünü Seç"**: Tüm cihazlara yetki verir
- **"Tümünü Kaldır"**: Tüm cihazlardan yetkiyi kaldırır

> 💡 **Not**: Yetki değişiklikleri sadece veritabanında yapılır. Cihazlara tanımlama yapmaz.

---

## Offline Veri İşlemleri

Offline veri, cihazların hafızasında saklanan giriş-çıkış kayıtlarıdır.

### Veri Çekme

#### Adım 1: Cihaz Seçimi

1. **"Offline Veri"** sekmesine tıklayın
2. Sol taraftaki cihaz listesinden veri çekmek istediğiniz cihazları işaretleyin
3. **"Tümünü Seç"** ile tüm cihazları seçebilirsiniz

#### Adım 2: Veri Çekme

1. **"Veri Çek"** butonuna tıklayın
2. İşlem başlar ve her cihaz için durum gösterilir
3. Veriler tabloda görüntülenir

![Offline Veri](images/offline_data.png)

### Tablo Sütunları

| Sütun | Açıklama |
|-------|----------|
| **Cihaz Adı** | Kaydın geldiği cihaz |
| **Personel ID** | Personel numarası |
| **Ad Soyad** | Personel adı |
| **Tarih** | Giriş/çıkış tarihi ve saati |
| **Doğrulama Tipi** | Parmak izi, yüz tanıma, şifre vb. |
| **Giriş/Çıkış** | Giriş, çıkış, ara çıkış vb. |

### Excel'e Aktarma

1. Veri çekme işleminden sonra
2. **"Excel'e Aktar"** butonuna tıklayın
3. Dosya kaydetme konumunu seçin
4. Excel dosyası oluşturulur

> 💡 **İpucu**: Excel dosyası `.xlsx` formatındadır ve Microsoft Excel veya LibreOffice ile açılabilir.

### Verileri Temizleme

Tablodaki verileri temizlemek için:
1. **"Temizle"** butonuna tıklayın
2. Tablo boşaltılır

> ⚠️ **Dikkat**: Bu işlem sadece ekrandaki tabloyu temizler, cihazlardaki verileri silmez.

---

## Cihaz Yönetimi

Cihaz yönetimi sekmesi, cihazlarınızın durumunu görüntülemenizi ve kontrol etmenizi sağlar.

### Cihaz Bilgileri Görüntüleme

1. **"Cihaz Yönetimi"** sekmesine tıklayın
2. **"Bilgileri Yenile"** butonuna tıklayın
3. Tüm cihazların bilgileri tabloda görüntülenir

### Tablo Sütunları

| Sütun | Açıklama |
|-------|----------|
| **Cihaz Adı** | Cihazın adı |
| **IP Adresi** | Cihazın IP adresi |
| **Model** | Cihaz modeli |
| **Seri No** | Seri numarası |
| **Firmware** | Firmware versiyonu |
| **MAC Adresi** | MAC adresi |
| **Kullanıcı** | Mevcut/Kapasite |
| **Parmak İzi** | Mevcut/Kapasite |
| **Log** | Mevcut/Kapasite |
| **Cihaz Saati** | Cihazın sistem saati |
| **Bağlantı** | Bağlı/Bağlı Değil |

### Cihaz İşlemleri

Tüm işlemler için önce cihazları seçmeniz gerekir:
1. Tablodaki checkbox'ları işaretleyin
2. İstediğiniz işlemi seçin

#### Saat Senkronizasyonu

1. Cihazları seçin
2. **"Saat Senkronize Et"** butonuna tıklayın
3. Cihazların saati bilgisayarınızın saatine göre ayarlanır

> 💡 **İpucu**: Düzenli olarak saat senkronizasyonu yapmanız önerilir.

#### Cihazı Yeniden Başlatma

1. Cihazları seçin
2. **"Cihazı Yeniden Başlat"** butonuna tıklayın
3. Onay mesajını kabul edin
4. Cihazlar yeniden başlar

> ⚠️ **Uyarı**: Yeniden başlatma sırasında cihaz kullanılamaz (yaklaşık 1-2 dakika).

#### Cihazı Kapatma

1. Cihazları seçin
2. **"Cihazı Kapat"** butonuna tıklayın
3. Onay mesajını kabul edin
4. Cihazlar kapanır

> ⚠️ **Uyarı**: Kapatılan cihazı açmak için fiziksel olarak güç tuşuna basmanız gerekir.

#### Tüm Logları Silme

1. Cihazları seçin
2. **"Tüm Logları Sil"** butonuna tıklayın
3. Onay mesajını kabul edin
4. Cihazlardaki tüm giriş-çıkış kayıtları silinir

> ⚠️ **UYARI**: Bu işlem geri alınamaz! Logları silmeden önce mutlaka yedek alın.

#### Tüm Kullanıcıları Silme

1. Cihazları seçin
2. **"Tüm Kullanıcıları Sil"** butonuna tıklayın
3. Onay mesajını kabul edin
4. Cihazlardaki tüm kullanıcılar ve parmak izi verileri silinir

> ⚠️ **UYARI**: Bu işlem geri alınamaz! Kullanıcıları silmeden önce mutlaka yedek alın.

---

## Sık Karşılaşılan Sorunlar

### Cihaza Bağlanamıyorum

**Belirtiler**: Cihaz listesinde cihazlar kırmızı nokta ile gösteriliyor

**Çözümler**:
1. ✅ Cihazın açık olduğundan emin olun
2. ✅ Cihazın ağa bağlı olduğunu kontrol edin
3. ✅ IP adresini ping atarak test edin:
   ```
   Windows Komut İstemi'nde: ping 192.168.1.100
   ```
4. ✅ Firewall'un cihaz portunu (4370) engellememesini sağlayın
5. ✅ Cihazın IP adresinin doğru olduğunu kontrol edin

### Personel Tanımlanamıyor

**Belirtiler**: "Tanımla" butonuna bastığımda hata alıyorum

**Çözümler**:
1. ✅ Cihaza bağlantı olduğundan emin olun
2. ✅ Personelin kart numarasının olduğunu kontrol edin
3. ✅ Cihazın kullanıcı kapasitesi dolmamış olmalı
4. ✅ Personelin zaten cihazda olup olmadığını kontrol edin

### Offline Veri Çekilemiyor

**Belirtiler**: "Veri Çek" butonuna bastığımda veri gelmiyor

**Çözümler**:
1. ✅ Cihazda kayıt olduğundan emin olun
2. ✅ Cihaz bağlantısını kontrol edin
3. ✅ Cihazın log kapasitesini kontrol edin
4. ✅ Cihazı yeniden başlatmayı deneyin

### Excel'e Aktarma Çalışmıyor

**Belirtiler**: "Excel'e Aktar" butonuna bastığımda hata alıyorum

**Çözümler**:
1. ✅ Önce veri çekme işlemi yapın
2. ✅ Tabloda veri olduğundan emin olun
3. ✅ Kaydetmek istediğiniz klasöre yazma izniniz olduğunu kontrol edin
4. ✅ Aynı isimde açık bir Excel dosyası varsa kapatın

### Uygulama Açılmıyor

**Belirtiler**: Uygulamayı çalıştırdığımda hata veriyor

**Çözümler**:
1. ✅ .NET Framework 4.7.2'nin kurulu olduğundan emin olun
2. ✅ Uygulamayı yönetici olarak çalıştırmayı deneyin
3. ✅ Antivirüs programını geçici olarak devre dışı bırakın
4. ✅ Uygulamayı yeniden kurun

### Veritabanı Bağlantı Hatası

**Belirtiler**: "SQL Server'a bağlanılamadı" hatası

**Çözümler**:
1. ✅ SQL Server servisinin çalıştığından emin olun
2. ✅ Ağ bağlantınızı kontrol edin
3. ✅ Sistem yöneticinizle iletişime geçin

---

## İpuçları ve Püf Noktaları

### ⚡ Hızlı İşlemler

1. **Arama Kullanın**: Uzun listelerde arama kutusunu kullanarak hızlıca bulun
2. **Toplu İşlemler**: Birden fazla personel için toplu tanımlama kullanın
3. **Tümünü Seç**: Tüm cihazları seçmek için "Tümünü Seç" butonunu kullanın

### 🔒 Güvenlik

1. **Şifrenizi Paylaşmayın**: Kullanıcı bilgilerinizi kimseyle paylaşmayın
2. **Oturumu Kapatın**: İşiniz bittiğinde uygulamayı kapatın
3. **Yedek Alın**: Önemli işlemlerden önce veri yedeği alın

### 📊 Raporlama

1. **Düzenli Veri Çekme**: Offline verileri düzenli olarak çekin
2. **Excel Raporları**: Verileri Excel'e aktararak analiz yapın
3. **Log Takibi**: İşlem loglarını takip edin

### 🔧 Bakım

1. **Saat Senkronizasyonu**: Haftada bir cihaz saatlerini senkronize edin
2. **Log Temizliği**: Ayda bir cihaz loglarını temizleyin (yedek aldıktan sonra)
3. **Güncelleme**: Uygulama güncellemelerini takip edin

---

## Klavye Kısayolları

| Kısayol | İşlev |
|---------|-------|
| `Ctrl + F` | Arama kutusuna odaklan |
| `Ctrl + A` | Tümünü seç |
| `Ctrl + S` | Kaydet |
| `F5` | Yenile |
| `Esc` | İptal |

---

## Destek ve İletişim

### Teknik Destek

Sorunlarınız için:
1. Önce bu kılavuzu kontrol edin
2. Sistem yöneticinizle iletişime geçin
3. Hata mesajının ekran görüntüsünü alın

### Eğitim Talebi

Uygulama kullanımı hakkında eğitim almak için sistem yöneticinizle iletişime geçin.

---

## Sözlük

| Terim | Açıklama |
|-------|----------|
| **Biyometrik Cihaz** | Parmak izi, yüz tanıma gibi biyolojik özellikleri kullanan cihaz |
| **Offline Veri** | Cihazın hafızasında saklanan giriş-çıkış kayıtları |
| **Terminal** | Giriş-çıkış kontrolü yapan cihaz |
| **Yetkilendirme** | Personelin hangi cihazlara erişebileceğinin belirlenmesi |
| **Tanımlama** | Personelin cihaza kaydedilmesi |
| **Firmware** | Cihazın işletim sistemi yazılımı |
| **IP Adresi** | Cihazın ağdaki benzersiz adresi |

---

## Ekler

### Ek A: Doğrulama Tipleri

- **Şifre**: Sadece şifre ile giriş
- **Parmak İzi**: Parmak izi okutma
- **Yüz Tanıma**: Yüz tanıma ile giriş
- **Kart**: RFID kart ile giriş
- **Şifre + Parmak İzi**: Çift doğrulama

### Ek B: Giriş/Çıkış Modları

- **Giriş (0)**: Normal giriş
- **Çıkış (1)**: Normal çıkış
- **Ara Çıkış (2)**: Öğle arası vb.
- **Ara Giriş (3)**: Ara dönüşü
- **Mesai Başlangıcı (4)**: Vardiya başlangıcı
- **Mesai Bitişi (5)**: Vardiya bitişi

---

**Kılavuz Versiyonu:** 1.0  
**Son Güncelleme:** 26 Aralık 2024  
**Hazırlayan:** CeyPASS Ekibi

---

> 📖 Bu kılavuz, CeyPASS Cihaz Paneli uygulamasının tüm özelliklerini kapsamaktadır. Sorularınız için lütfen sistem yöneticinizle iletişime geçin.

# Yönetici Sosyal Bağ ve Çalışan Gelişmeleri Portalı

Yöneticilerin, kendi alt organizasyonlarındaki (N-1 ve N-2) çalışanların önemli gelişmelerini tek ekrandan takip etmesini sağlayan bir portal. Doğum günleri, kıdem yıl dönümleri, aile ve yaşam olayları ile akademik başarılar tek panelde toplanır; yöneticiye her ay otomatik bir özet e-postası gönderilir.

## Canlı demo

https://tuanapektas.github.io/Sosyal-Olaylar-Portali/

Tüm ekranlar tek bir `index.html` dosyasındadır: gösterge paneli, çalışan detay sayfası (listeden isme tıklanınca açılır) ve aylık yönetici e-postası önizlemesi. Dosya bilgisayarda çift tıklanarak da açılabilir.

## Özellikler

- Birime, kategoriye, duruma, kıdem yılına ve cinsiyete göre dağılım grafikleri
- Departman / birim, olay / kategori, kıdem, durum ve tarih aralığı filtreleri; aktif filtreler üstte görünür ve ✖ ile kaldırılır
- Grafiklerdeki bir çubuğa veya dilime tıklayarak filtreleme; tüm grafikler ve liste anında güncellenir
- Kategori seçildiğinde kategori içi olay kırılımı
- Olay listesinde arama ve sıralama
- Çalışan detayında kurum içi, eğitim, kişisel ve aile bilgileri ile kilometre taşları
- Çalışanın paylaşıma kapattığı olaylar hiçbir ekranda ve e-postada gösterilmez

## C# uygulaması

`Program.cs` ve `MonthlyMailBuilder.cs` dosyalarından oluşur. Aktif yöneticinin organizasyonundaki son 1 ay ve önümüzdeki 1 aya ait olayları listeler ve her yönetici için aylık e-postayı HTML olarak üretir. Veriyi `employees.json` dosyasından okur.

Deponun ana dizininde çalıştırın:

```bash
dotnet run
```

.NET 8 SDK gerekir. Üretilen e-postalar `output/mails/` klasörüne kaydedilir.

## Dosyalar

| Dosya | Açıklama |
|---|---|
| `index.html` | Web arayüzü: gösterge paneli, çalışan detay ve aylık mail ekranları |
| `Program.cs` | C# ana program: veri okuma, LINQ ile olay filtreleme, mail üretimi |
| `MonthlyMailBuilder.cs` | Aylık yönetici e-postasının HTML şablonu |
| `SocialDashboard.csproj` | .NET proje dosyası |
| `employees.json` | 100 kişilik test verisi |

## İş kuralları

- **Kapsam:** Yönetici yalnızca doğrudan bağlı (N-1) ve müdürleri üzerinden bağlı (N-2) çalışanları görür.
- **Kategoriler:** Akademik & Gelişim, Aile & Yaşam, Kurumsal Kıdem, Doğum Günü.
- **Durum:** Geçmiş, Bugün, Yaklaşıyor.
- **Tarih filtresi:** Varsayılan aralık bugünden 1 ay geri ve 1 ay ileridir; başlangıç en fazla 1 yıl geriye gidebilir.
- **Aylık e-posta:** Her ayın 1'inde 09:00'da gönderilir ve gönderim tarihinden 1 ay geriye, 1 ay ileriye bakar.

## Teknolojiler

C#, .NET 8, LINQ, System.Text.Json, HTML, CSS, JavaScript

> Veriler 100 kişilik test verisidir ve gerçek kişilerle ilgisi yoktur. Ekranlarda referans tarih 23.09.2026 olarak kullanılır.

AlbionOnline - Statistics Analysis

## This txt file has been translated by DemonHarion
===================
[![Github All Releases](https://img.shields.io/github/v/release/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases)
[![Github All Releases](https://img.shields.io/github/downloads/Triky313/AlbionOnline-StatisticsAnalysis/total.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases) 
[![CodeFactor](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/badge/main)](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/overview/main)
[![Build + Tests](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/build-and-unit-tests.yml/badge.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/dotnet-desktop.yml)
![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/Triky313/AlbionOnline-StatisticsAnalysis/latest?color=AF3B7F)
[![GitHub issues](https://img.shields.io/github/issues/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues)
[![Discord](https://img.shields.io/discord/772406813438115891?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat)](https://discord.gg/Wv5RWehbrU)
[![Donate](https://img.shields.io/badge/paypal-donate-1e477a)](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)
[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3DTriky313%26type%3Dpatrons&style=flat)](https://patreon.com/Triky313)

Bir loot kaydedici, hasar ölçer, zindan izleyici, zindan giriş zamanlayıcısı, işçilik hesap makinesi, harita geçmişi ve oyuncu bilgileri ile Market verilerini kolayca okumak için bir araçtır.

<p align="center" align='right'>
  <img src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" data-canonical-src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" width="400" height="400" />
</p>

## Başlangıç için

### Önkoşullar ve Kurulum
- **Windows 10** veya sonraki bir sürüme ihtiyacınız var
- **.NET 6.0 Desktop Runtime** (v6.0.5 veya üstü) [buradan] kurmanız gerekmektedir (https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.5-windows-x64-installer) (To start the tool)
- **Npcap** Free Edition'ı (v1.6 veya üstü) yükleyin [buradan](https://npcap.com/#download) (Oyun takibi için)

**İstatistik Analiz Aracını İndirin**
- [**İndirin**](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases/download/v5.17.3/StatisticsAnalysis-AlbionOnline-v5.17.3-x64.zip)
`.zip` dosyasını açın ve çift tıklamayla `StatisticsAnalysisTool.exe`yi başlatın. `.exe` dosyasını göremeyebilirsiniz. Endişelenmeyin, genellikle simgeli dosyadır.

![tool_dir](https://user-images.githubusercontent.com/14247773/170473306-4dcc629e-384e-41b2-ada8-657cabe1b472.png)


## Buna Programa İzin Veriliyor mu?
https://forum.albiononline.com/index.php/Thread/124819-Regarding-3rd-Party-Software-and-Network-Traffic-aka-do-not-cheat-Update-16-45-U/

- [x] Yalnızca monitörler.
- [x] Oyun istemcimizi değiştirmez.
- [x] Oyuncunun görüş alanında olmayan oyuncuları izlemez.
- [x] uygulama üstünde widget yok.

## AYARLAR

### ÖĞE KAYNAK LİSTESİ
Öğe listesi eskiyse, kendiniz değiştirebilirsiniz. Bunun için "ÖĞE KAYNAĞI LİSTESİNİ" URL'sini değiştirmeniz yeterlidir.

Başka bir iyi kaynak ise https://github.com/broderickhyman/ao-bin-dumps

Veya dosyaları oyundan kendiniz çıkarırsınız. Daha fazla bilgi burada bulunabilir: https://github.com/broderickhyman/ao-id-extractor


## SSS
### Hangi işletim sistemi desteklenir?
✅ Windows 10 ve sonrası

❌ Windows XP, Vista, 7 ve 8 desteklenmez!

❌ Linux şu anda desteklenmiyor!

### Aracı ExitLag veya VPN ile kullanabilir miyim?
Maalesef **ExitLag desteklenmemektedir** ancak aracın iyi çalıştığı başka VPN hizmetleri de vardır. Bunun için geliştiricilerden herhangi bir destek yok. Onaylamak çok zaman alabilirdi.

### İnternetimin ne kadar hızlı olması gerekiyor?
En az 1M/bit (256KB/sn) indirme hızına sahip bir internet bağlantısı olması gerekmetedir.

### Araç, ItemList.json veya Item.json dosyasını indiremez, ne yapmalı?
Ekran görüntüsündeki gibi bir buton belirir ve dosyaların tekrar tekrar otomatik olarak indirilmesi çalışmazsa, aşağıdakiler yapılabilir:

![tekrar-indirmeyi-dene-butonu](https://user-images.githubusercontent.com/14247773/170475039-3739e5cd-5d02-41bf-a77d-f58290de75a3.png)

Bu dosyayı İndirin https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json ve ItemList.json olarak yeniden adlandırın.
ve
bu dosyayı indir https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/items.json 
Ardından her iki dosyayı da araçlar klasörüne atın.

Ardından aracı yeniden başlatın ve her şey çalışmalıdır.


## BİLGİ

### Fiyat bilgisi nereden geliyor?
bu projeden: [https://www.albion-online-data.com/](https://www.albion-online-data.com/)

### Daha güncel fiyatlar istiyorum!
İndirin [Albion online data client](https://www.albion-online-data.com/) ve marketleri tarayın.


## KATKI

### Sorun mu soru mu?
Bir sorun oluşturun [Buradan](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues).

### Dil çevirisi

#### Nasıl Çalışır
Kendi dilinizi ekleyin, sadece 'Diller' dizininde bir dosya oluşturmanız veya uyarlamanız gerekir
Dosya her zaman dil koduna sahip olmalıdır.
Örnek:
```
en-US.xml
```

Dil kodlarını burada bulabilirsiniz: https://www.andiamo.co.uk/resources/iso-language-codes/

#### Dosya Yaratma
en-US dosyasını kopyalayın ve her şeyi değiştirin, ardından tüm UYGULAMA KODLARI kullanılabilir olmalıdır.
```
<translation name = "EVERY_3_DAYS">HERE YOUR TEXT</translation>
```

## Bu web sitesi
[triky313.github.io/AlbionOnline-StatisticsAnalysis](https://triky313.github.io/AlbionOnline-StatisticsAnalysis/)

## BAĞIŞLAR
Bu proje Haziran 2019'dan beri var. Neredeyse her hafta bu projeye +10 saat koyuyorum ve onu seviyorum. Bu yüzden çoğu zaman Albion Online oynamaya zamanım olmuyor. Bu yüzden beni mutlu etmek ve bu projeye destek olmak istiyorsanız, birkaç parça bağışta bulunmanız veya Patreon'da bağış yapmanız yeterli.

[Patreon - Triky313](https://www.patreon.com/triky313)

<img src="https://user-images.githubusercontent.com/14247773/166248069-3211a206-b475-4e83-860b-e5c51b9554bf.png" data-canonical-src="https://www.patreon.com/triky313" width="40" height="40" />

[PayPal - Triky313](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)

<img src="https://user-images.githubusercontent.com/14247773/201472890-33a0ed70-7ef8-4804-aa84-46f0a84f3168.png" width="100" height="100" />

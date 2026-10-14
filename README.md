<div align="center">

# 🛡️ Sentinel
### Distributed Health & Circuit Breaker Hub

**.NET 10 · Polly v8 · SignalR · Docker Compose**

Mikroservislerin sağlık durumlarını gerçek zamanlı izleyen, hata senaryolarını simüle eden ve dayanıklılık mekanizmalarını görselleştiren dağıtık izleme platformu.

</div>

---

## 📌 Proje Hakkında

**Sentinel**, mikroservis mimarilerinde hedef servislerin sağlık durumlarını (Health Checks) periyodik olarak izlemek, hata senaryolarını yönetmek ve sistem dayanıklılığını gerçek zamanlı olarak gözlemlemek amacıyla geliştirilmiş dağıtık bir izleme merkezidir.

**.NET 10**, **Polly v8**, **ASP.NET Core SignalR** ve **Docker Compose** teknolojileriyle geliştirilen proje; servis izleme, hata yönetimi ve devre kesici (Circuit Breaker) mekanizmalarını tek bir dashboard üzerinden takip etmeyi sağlar.

Sentinel, yalnızca servislerin erişilebilirliğini kontrol etmekle kalmaz; aynı zamanda gecikme sürelerini ölçer, geçici hatalara karşı yeniden deneme stratejileri uygular ve hata senaryolarında sistemin davranışını gözlemlemeye olanak tanır.

### 🎯 Temel Hedefler

- Mikroservislerin sağlık durumlarını merkezi olarak izlemek.
- Servis yanıt sürelerini ve hata durumlarını takip etmek.
- Polly v8 ile Timeout, Retry ve Circuit Breaker stratejilerini uygulamak.
- SignalR üzerinden gerçek zamanlı dashboard güncellemeleri sağlamak.
- Hata enjeksiyonu ile farklı arıza senaryolarını simüle etmek.
- Docker Compose ile birbirinden bağımsız servisleri birlikte çalıştırmak.

## 📸 Demo ve Ekran Görüntüleri
<img width="832" height="676" alt="resim" src="https://github.com/user-attachments/assets/df85fa4b-c189-4f78-bada-123544d0d47c" />

## 🏗️ Mimari Genel Bakış

Sentinel, birbirinden bağımsız çalışan üç temel bileşenden oluşur:

```text
                         ┌───────────────────────────┐
                         │     Tarayıcı İstemcileri  │
                         └─────────────┬─────────────┘
                                       │
                              WebSocket / SignalR
                                       │
                                       ▼
                        ┌───────────────────────────┐
                        │       Sentinel.Api        │
                        │        Dashboard          │
                        │         Port: 5000        │
                        └─────────────▲─────────────┘
                                      │
                               HTTP Raporlama
                                      │
                        ┌─────────────┴─────────────┐
                        │                           │
                        │                           │
                        ▼                           ▼
             ┌────────────────────┐     ┌────────────────────┐
             │   Sentinel.Worker  │     │   Dummy.Service    │
             │                    │────>│                    │
             │  BackgroundService │ HTTP│ Target Microservice│
             │  Polly Resilience  │ Ping│ Fault / Slow Mode  │
             └────────────────────┘     └────────────────────┘
```

### Bileşenler

| Bileşen | Sorumluluk |
|---|---|
| `Sentinel.Api` | Merkezi dashboard'u sunar, sağlık raporlarını alır ve SignalR üzerinden istemcilere iletir. |
| `Sentinel.Worker` | Hedef servisleri periyodik olarak kontrol eder, Polly stratejilerini uygular ve sonuçları API'ye gönderir. |
| `Sentinel.DummyService` | İzleme mekanizmalarını test etmek için kullanılan, hata ve gecikme senaryoları simüle edilebilen örnek mikroservistir. |

## 🚀 Öne Çıkan Özellikler

### 1. Polly v8 ile Dayanıklılık Yönetimi

`Sentinel.Worker`, hedef servislere gönderdiği HTTP isteklerinde Polly v8 tabanlı dayanıklılık stratejilerinden yararlanır.

- **Timeout Strategy:** Belirlenen süre içerisinde yanıt alınamayan isteklerin zaman aşımına uğramasını sağlar.
- **Retry Strategy:** Geçici hatalarda isteklerin belirlenen politikaya göre yeniden denenmesini sağlar.
- **Circuit Breaker:** Belirli koşullar altında art arda veya belirli bir oranda başarısız olan isteklere karşı devreyi açarak hedef servise gönderilen çağrıları sınırlar.
- **Resilience Pipeline:** Birden fazla dayanıklılık stratejisinin belirlenen sırayla birlikte uygulanmasına olanak tanır.

Bu mekanizmalar sayesinde servis kesintileri, geçici ağ problemleri ve uzun yanıt süreleri gibi senaryolar kontrollü şekilde test edilebilir.

### 2. Bağımsız Arka Plan İzleme Ajanı

`Sentinel.Worker`, ASP.NET Core `BackgroundService` altyapısıyla çalışan bağımsız bir izleme servisidir.

Temel sorumlulukları:

- Hedef servisleri belirlenen aralıklarla kontrol etmek.
- HTTP yanıtlarını ve istek sürelerini takip etmek.
- Başarılı ve başarısız kontrolleri değerlendirmek.
- Polly dayanıklılık stratejilerini uygulamak.
- İzleme sonuçlarını merkezi API'ye raporlamak.

Worker'ın API ve hedef servisten bağımsız çalışması, izleme mekanizmasının uygulama mimarisinde ayrı bir sorumluluk olarak konumlandırılmasını sağlar.

### 3. SignalR ile Gerçek Zamanlı Dashboard

`Sentinel.Api`, worker tarafından gönderilen sağlık raporlarını ASP.NET Core SignalR üzerinden bağlı istemcilere iletir.

Dashboard üzerinde:

- Servislerin güncel sağlık durumları görüntülenir.
- Yanıt süreleri takip edilir.
- Hata ve gecikme senaryoları gözlemlenir.
- Circuit Breaker durumundaki değişiklikler izlenir.

Bu yapı, kullanıcıların sayfayı sürekli yenilemesine gerek kalmadan güncel izleme verilerini takip etmesine olanak tanır.

### 4. İnteraktif Hata Enjeksiyonu

`Sentinel.DummyService`, izleme ve dayanıklılık mekanizmalarını test etmek için kontrol edilebilir arıza senaryoları sunar.

Örnek yönetim uç noktaları:

- `/admin/toggle-fault`: Hata modunu etkinleştirir veya devre dışı bırakır.
- `/admin/toggle-slow`: Gecikme modunu etkinleştirir veya devre dışı bırakır.

Bu uç noktalar aracılığıyla hedef servisin davranışı değiştirilerek Sentinel'in hata algılama, yanıt süresi ölçümü ve Circuit Breaker davranışı gözlemlenebilir.

> **Not:** Hata enjeksiyonu uç noktaları test amaçlıdır. Üretim ortamında kullanılmaları hâlinde kimlik doğrulama ve yetkilendirme ile korunmaları gerekir.

## 🛠️ Teknoloji Yığını

| Teknoloji | Kullanım Amacı |
|---|---|
| **.NET 10** | Uygulama altyapısı ve çalışma zamanı |
| **ASP.NET Core Web API** | Merkezi API ve dashboard sunucusu |
| **ASP.NET Core SignalR** | Gerçek zamanlı istemci iletişimi |
| **Polly v8** | Timeout, Retry ve Circuit Breaker stratejileri |
| **BackgroundService** | Periyodik sağlık kontrolleri ve arka plan işlemleri |
| **HttpClient** | Hedef servislere HTTP istekleri gönderilmesi |
| **HTML5** | Dashboard arayüzünün oluşturulması |
| **JavaScript** | İstemci tarafındaki etkileşimler ve dinamik güncellemeler |
| **Docker** | Uygulamaların konteynerleştirilmesi |
| **Docker Compose** | Çoklu konteyner ortamının yönetilmesi |

## 📂 Proje Yapısı

```text
Sentinel/
├── src/
│   ├── Sentinel.Api/
│   │   ├── Hubs/
│   │   ├── wwwroot/
│   │   │   └── index.html
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── Sentinel.Api.csproj
│   │
│   ├── Sentinel.Worker/
│   │   ├── Worker.cs
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── Sentinel.Worker.csproj
│   │
│   └── Sentinel.DummyService/
│       ├── Program.cs
│       ├── Dockerfile
│       └── Sentinel.DummyService.csproj
│
├── docker-compose.yml
└── README.md
```

## ⚡ Hızlı Başlangıç

### Gereksinimler

Projeyi çalıştırmak için aşağıdaki araçların kurulu olması gerekir:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

Docker Compose, Docker Desktop ile birlikte kullanılabilir.

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/ufukgulec/Sentinel.git
cd Sentinel
```

### 2. Servisleri Başlatın

`docker-compose.yml` dosyasının bulunduğu proje kök dizininde aşağıdaki komutu çalıştırın:

```bash
docker compose up --build -d
```

Bu komut, Docker Compose yapılandırmasında tanımlanan servisleri derler ve arka planda başlatır.

### 3. Çalışan Servisleri Kontrol Edin

```bash
docker compose ps
```

Konteyner günlüklerini görüntülemek için:

```bash
docker compose logs -f
```

Yalnızca worker günlüklerini incelemek için:

```bash
docker compose logs -f sentinel-worker
```

> Servis adlarını kendi `docker-compose.yml` dosyanızdaki tanımlarla eşleştirin.

### 4. Dashboard'u Açın

Tarayıcınızdan aşağıdaki adresi ziyaret edin:

**[http://localhost:5000](http://localhost:5000)**

Dashboard üzerinden hedef servisin sağlık durumunu izleyebilir ve hata senaryolarını test edebilirsiniz.

## 🎛️ Test ve Yönetim Senaryoları

Sentinel'in dayanıklılık mekanizmalarını dashboard üzerinden farklı senaryolarla test edebilirsiniz.

### 💥 Senaryo 1: Servis Hatası

1. Dashboard'u açın.
2. **Arıza Modu** butonuyla hedef serviste hata modunu etkinleştirin.
3. Worker'ın gerçekleştirdiği sağlık kontrollerini takip edin.
4. Başarısız istekleri, hata durumunu ve Circuit Breaker davranışını gözlemleyin.
5. Arıza modunu kapatarak servisin normal davranışına dönmesini sağlayın.

**Beklenen sonuç:** Sağlık kontrolleri başarısız olduğunda hata durumu dashboard'a yansır. Circuit Breaker'ın açılması, tanımlanan Polly yapılandırmasına ve hata koşullarına bağlıdır.

### 🐢 Senaryo 2: Yavaş Yanıt

1. Dashboard üzerinden **Yavaş Mod** özelliğini etkinleştirin.
2. Hedef servisin yanıt sürelerindeki artışı gözlemleyin.
3. Timeout stratejisinin devreye girip girmediğini kontrol edin.
4. Oluşan hata ve gecikme kayıtlarını inceleyin.
5. Yavaş Mod'u devre dışı bırakarak normal yanıt sürelerine dönün.

**Beklenen sonuç:** Hedef servisin yanıt süresi artar. Gecikmenin yapılandırılmış timeout sınırını aşması durumunda istek zaman aşımına uğrayabilir.

### 🔄 Senaryo 3: Devre Kesicinin Yeniden Açılması

1. Hedef serviste hata modunu etkinleştirin.
2. Circuit Breaker'ın açılması için gereken başarısızlık koşullarının oluşmasını bekleyin.
3. Devre açıkken yapılan çağrıların davranışını gözlemleyin.
4. Hedef servisi tekrar çalışır duruma getirin.
5. Devrenin yarı açık duruma geçişini ve başarılı denemeler sonrasında yeniden kapatılmasını inceleyin.

**Beklenen sonuç:** Circuit Breaker, yapılandırılmış eşiklere ve bekleme süresine göre açılır. Yeniden deneme aşamasında başarılı sonuçlar alındığında normal çağrı akışı tekrar başlayabilir.

## 🧠 Teknik Kazanımlar

Sentinel, aşağıdaki yazılım geliştirme ve sistem tasarımı konularında pratik yapmayı amaçlar:

- **Microservices Architecture:** Bağımsız servislerin birlikte çalışması.
- **Resilience Engineering:** Hata toleransı ve dayanıklılık stratejileri.
- **Circuit Breaker Pattern:** Arızalı servislerin çağrı trafiğinin sınırlandırılması.
- **Retry & Timeout Patterns:** Geçici hataların ve uzun süren isteklerin yönetimi.
- **Background Processing:** ASP.NET Core arka plan servisleriyle periyodik görevlerin yürütülmesi.
- **Real-Time Communication:** SignalR ile anlık veri aktarımı.
- **Fault Injection:** Kontrollü hata ve gecikme senaryolarının oluşturulması.
- **Containerization:** Docker ile servislerin izole ortamlarda çalıştırılması.
- **Distributed System Observability:** Servis sağlığı ve yanıt sürelerinin merkezi olarak izlenmesi.

## 🔮 Geliştirme Fikirleri

Projenin ilerleyen sürümlerinde aşağıdaki özellikler değerlendirilebilir:

- Birden fazla hedef mikroservisin dinamik olarak tanımlanması.
- Sağlık kontrolü aralıklarının ve timeout sürelerinin yönetilebilmesi.
- Geçmiş sağlık kontrollerinin veritabanında saklanması.
- Yanıt sürelerinin geçmişe dönük grafiklerle gösterilmesi.
- Servis bazında hata oranı ve kullanılabilirlik metriklerinin hesaplanması.
- E-posta veya diğer kanallar üzerinden arıza bildirimleri gönderilmesi.
- Dashboard için kimlik doğrulama ve rol tabanlı yetkilendirme.
- OpenTelemetry entegrasyonu ile dağıtık izlenebilirlik.
- Prometheus ve Grafana entegrasyonuyla metriklerin dış sistemlere aktarılması.
- Birden fazla worker örneğiyle dağıtık izleme kapasitesinin artırılması.


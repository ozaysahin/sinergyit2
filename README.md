# Mikroservis Log Yönetim Sistemi

Distributed logging sistemi - ASP.NET Core mikroservisleri için Serilog, RabbitMQ ve Elasticsearch kullanarak merkezi log toplama ve analiz çözümü.

## 📋 Proje Hakkında

Bu proje, iki ayrı mikroservisin loglarını merkezi bir yapıda toplayan ve analiz eden bir sistemdir. Her servis kendi loglarını hem yerel dosyalara hem de RabbitMQ üzerinden Elasticsearch'e gönderir.

### Mimari
```
ServiceA ──┐
           ├──> RabbitMQ (Topic Exchange) ──> LogConsumer ──> Elasticsearch ──> Kibana
ServiceB ──┘                                                        │
                                                                    ├─> project-servicea-logs
                                                                    ├─> project-serviceb-logs
                                                                    └─> project-microservices-logs
```

### Özellikler

- ✅ İki bağımsız mikroservis (ServiceA, ServiceB)
- ✅ Serilog ile yapılandırılmış loglama
- ✅ RabbitMQ Topic Exchange ile routing
- ✅ Elasticsearch'te 3 farklı index (servis bazlı + ortak)
- ✅ Kibana ile görselleştirme
- ✅ Dosya bazlı yedek loglama

## 🛠️ Kullanılan Teknolojiler

- .NET 8.0
- ASP.NET Core Web API
- Serilog
- RabbitMQ (Topic Exchange)
- Elasticsearch 8.11.0
- Kibana 8.11.0
- Docker
- NEST (Elasticsearch .NET Client)

## 📦 Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) veya [VS Code](https://code.visualstudio.com/)
- Windows 10/11, macOS, veya Linux

## 🚀 Kurulum

### 1. Repository'yi Klonlayın
```bash
git clone https://github.com/kullaniciadi/mikroservis-log-sistemi.git
cd mikroservis-log-sistemi
```

### 2. Docker Servislerini Başlatın

#### RabbitMQ
```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

**Management UI:** http://localhost:15672  
**Kullanıcı Adı:** guest  
**Şifre:** guest

#### Elasticsearch
```bash
docker run -d --name elasticsearch \
  -p 19200:9200 \
  -p 19300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.11.0
```

**URL:** http://localhost:19200

#### Kibana
```bash
docker run -d --name kibana \
  -p 15601:5601 \
  -e "ELASTICSEARCH_HOSTS=http://host.docker.internal:19200" \
  docker.elastic.co/kibana/kibana:8.11.0
```

**URL:** http://localhost:15601  
⚠️ Kibana'nın başlaması 2-3 dakika sürebilir.

### 3. Projeleri Çalıştırın

#### Visual Studio ile:
1. Solution'ı açın
2. Solution'a sağ tık → **Properties**
3. **Multiple startup projects** seçin
4. Şunları **Start** olarak işaretleyin:
   - `ServiceA.API`
   - `ServiceB.API`
   - `LogConsumer`
5. **F5** ile başlatın

#### CLI ile:
```bash
# Terminal 1
cd ServiceA.API
dotnet run

# Terminal 2
cd ServiceB.API
dotnet run

# Terminal 3
cd LogConsumer
dotnet run
```

## 🧪 Test Etme

### API Endpointleri

**ServiceA:**
```bash
curl http://localhost:5001/api/test
```

**ServiceB:**
```bash
curl http://localhost:5002/api/test
```

### Elasticsearch Sorgulama

**ServiceA logları:**
```bash
curl http://localhost:19200/project-servicea-logs/_search?pretty
```

**ServiceB logları:**
```bash
curl http://localhost:19200/project-serviceb-logs/_search?pretty
```

**Ortak log havuzu:**
```bash
curl http://localhost:19200/project-microservices-logs/_search?pretty
```

### Kibana'da Görüntüleme

1. http://localhost:15601 adresine gidin
2. Sol menüden **Analytics** → **Discover**
3. **Create data view** tıklayın
4. **Index pattern:** `project-*`
5. **Timestamp field:** `timestamp`
6. **Save**

**Filtreleme örnekleri:**
- `service: servicea` → Sadece ServiceA logları
- `service: serviceb` → Sadece ServiceB logları
- `level: Warning` → Sadece uyarı logları

## 📊 RabbitMQ Yapısı

| Bileşen | Değer |
|---------|-------|
| Exchange | `logs-exchange` |
| Exchange Type | `topic` |
| Routing Keys | `servicea`, `serviceb` |
| Queues | `servicea-logs-queue`, `serviceb-logs-queue` |

RabbitMQ Management Panel: http://localhost:15672

## 📁 Proje Yapısı
```
mikroservis-log-sistemi/
│
├── ServiceA.API/
│   ├── Controllers/
│   │   └── TestController.cs
│   ├── Helpers/
│   │   └── RabbitMQLogger.cs
│   ├── logs/                    # Log dosyaları
│   └── Program.cs
│
├── ServiceB.API/
│   ├── Controllers/
│   │   └── TestController.cs
│   ├── Helpers/
│   │   └── RabbitMQLogger.cs
│   ├── logs/                    # Log dosyaları
│   └── Program.cs
│
├── LogConsumer/
│   └── Program.cs               # RabbitMQ consumer + Elasticsearch writer
│
└── README.md
```

## 🔍 Loglama Akışı

1. **ServiceA/B** bir endpoint'e istek gelir
2. **Serilog** logu 3 yere yazar:
   - Console (anlık görüntüleme)
   - Dosya (`logs/` klasörü)
   - RabbitMQ (`logs-exchange`)
3. **RabbitMQ** mesajı routing key'e göre ilgili kuyruğa yönlendirir
4. **LogConsumer** kuyruklardan mesajları okur
5. **Elasticsearch**'e 3 index'e yazar:
   - Servis özel index (`project-servicea-logs`)
   - Ortak index (`project-microservices-logs`)
6. **Kibana** üzerinden görselleştirme ve analiz

## 🐛 Sorun Giderme

### Port Çakışması
Eğer 9200, 5672 gibi portlar kullanımdaysa, Docker komutlarında `-p` parametrelerini değiştirin:
```bash
# Örnek: 19200 yerine 20200
docker run -d --name elasticsearch -p 20200:9200 ...
```

### RabbitMQ Bağlantı Hatası
```bash
docker ps                          # Container çalışıyor mu?
docker logs rabbitmq               # Hata logları
docker restart rabbitmq            # Yeniden başlat
```

### Elasticsearch Erişim Hatası
```bash
curl http://localhost:19200        # Çalışıyor mu kontrol et
docker logs elasticsearch          # Logları incele
```

### LogConsumer Mesaj Almıyor
1. RabbitMQ Management'ta queue'ları kontrol edin
2. ServiceA/B API'lerinde `RabbitMQLogger` inject edilmiş mi?
3. `Program.cs`'de `AddSingleton` eklenmiş mi?

## 📝 Notlar

- Docker container'ları her sistem yeniden başlatıldığında tekrar başlatılmalıdır
- Elasticsearch indeksleri ilk log yazıldığında otomatik oluşur
- Log dosyaları günlük olarak döner (rolling)
- RabbitMQ mesajları durable olarak işaretlenmiştir

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Commit edin (`git commit -m 'Yeni özellik eklendi'`)
4. Push edin (`git push origin feature/YeniOzellik`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 👤 İletişim

Projeyle ilgili sorularınız için issue açabilirsiniz.

---

⭐ Projeyi beğendiyseniz yıldız vermeyi unutmayın!

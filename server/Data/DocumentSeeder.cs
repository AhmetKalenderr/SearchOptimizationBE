using Microsoft.EntityFrameworkCore;
using SearchOptimizationBE.Models;
using SearchOptimizationBE.Services;

namespace SearchOptimizationBE.Data;

public static class DocumentSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Documents.AnyAsync()) return;

        var seedRecords = BuildSeedRecords().ToList();

        foreach (var record in seedRecords)
        {
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                Title = record.Title,
                Content = record.Content,
                DocumentTypeId = record.DocumentTypeId,
                UploadedById = record.UploadedById,
                UploadedAt = record.UploadedAt,
                FileSizeKb = record.Content.Length / 50 + 1
            };
            db.Documents.Add(doc);

            db.DocumentContentHashes.Add(new DocumentContentHash
            {
                DocumentId = doc.Id,
                ContentSha256 = ContentHasher.Sha256(doc.Title, doc.Content),
                NormalizedTitle = TurkishNormalizer.Normalize(doc.Title)
            });

            foreach (var token in BuildTokens(doc))
            {
                db.DocumentTokens.Add(token);
            }
        }

        await db.SaveChangesAsync();
    }

    private static IEnumerable<DocumentToken> BuildTokens(Document doc)
    {
        return BuildFieldTokens(doc.Id, doc.Title, TokenField.Title)
            .Concat(BuildFieldTokens(doc.Id, doc.Content, TokenField.Content));
    }

    private static IEnumerable<DocumentToken> BuildFieldTokens(Guid documentId, string text, byte field)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in Tokenizer.Tokenize(text))
        {
            counts[token] = counts.GetValueOrDefault(token) + 1;
        }
        foreach (var (token, freq) in counts)
        {
            yield return new DocumentToken
            {
                DocumentId = documentId,
                Token = token,
                Field = field,
                Frequency = freq
            };
        }
    }

    private record SeedRecord(string Title, string Content, int DocumentTypeId, int UploadedById, DateTime UploadedAt);

    private static IEnumerable<SeedRecord> BuildSeedRecords()
    {
        var d = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        // --- Sözleşmeler (Type 1) ---
        yield return new SeedRecord(
            "Tedarik Sözleşmesi - ABC Lojistik Ltd. Şti.",
            "İşbu tedarik sözleşmesi, şirketimiz ile ABC Lojistik Ltd. Şti. arasında 2026 yılı için kara taşımacılığı hizmetlerinin sağlanması amacıyla imzalanmıştır. Yıllık tahmini hacim 1200 sevkiyattır. Ödeme vadesi 60 gündür.",
            1, 1, d.AddDays(2));

        yield return new SeedRecord(
            "Hizmet Alım Sözleşmesi - Mavi Bilişim",
            "Şirketimiz ile Mavi Bilişim arasında yazılım bakım ve destek hizmetleri kapsamında 12 aylık sözleşme imzalanmıştır. SLA yanıt süresi 4 saat olarak belirlenmiştir.",
            1, 1, d.AddDays(5));

        yield return new SeedRecord(
            "Gayrimenkul Kira Sözleşmesi - Levent Ofis",
            "Levent merkez ofis kullanımı için 3 yıllık kira sözleşmesi imzalandı. Aylık kira bedeli güncellenmiş, kefil olarak şirket genel müdürlüğü gösterildi.",
            1, 6, d.AddDays(8));

        yield return new SeedRecord(
            "Danışmanlık Sözleşmesi - Hukuk Bürosu",
            "Yıllık genel hukuki danışmanlık hizmetleri için Yıldız Hukuk Bürosu ile sözleşme imzalanmıştır. Aylık 20 saat danışmanlık dahildir.",
            1, 1, d.AddDays(12));

        yield return new SeedRecord(
            "Personel İstihdam Sözleşmesi - Yazılım Geliştirici",
            "Yazılım geliştirici pozisyonu için belirsiz süreli iş sözleşmesi imzalandı. Deneme süresi 2 ay olarak belirlendi. Pozisyon uzaktan çalışmaya uygundur.",
            1, 5, d.AddDays(15));

        yield return new SeedRecord(
            "Bakım Onarım Sözleşmesi - Klima Tesisat",
            "Genel müdürlük binası klima ve havalandırma sistemleri için yıllık bakım sözleşmesi yapılmıştır. 4 dönem periyodik bakım planlanmıştır.",
            1, 6, d.AddDays(20));

        yield return new SeedRecord(
            "Sigorta Poliçesi Sözleşmesi - Filo Araçları",
            "Şirket filo araçları için kasko ve trafik sigortası sözleşmesi yenilendi. Toplam 32 araç kapsamında. Hasarsızlık indirimi %15 uygulandı.",
            1, 3, d.AddDays(25));

        // --- Teklifler (Type 2) ---
        yield return new SeedRecord(
            "Yazılım Geliştirme Teklifi - Müşteri Portal Projesi",
            "Sayın Yetkili, müşteri portal projesi için talep ettiğiniz teklif aşağıdadır. Geliştirme süresi 6 ay, ekip 4 kişi, toplam adam-ay maliyeti 24 olarak hesaplanmıştır.",
            2, 8, d.AddDays(3));

        yield return new SeedRecord(
            "Eğitim Hizmetleri Teklifi - Liderlik Programı",
            "Üst düzey yöneticilere yönelik 5 günlük liderlik gelişim programı için teklif. Her bir oturum 6 saat, katılımcı başına maliyet ayrı kalemlere bölünmüştür.",
            2, 5, d.AddDays(7));

        yield return new SeedRecord(
            "Ofis Mobilyaları Tedarik Teklifi",
            "Yeni şube ofisi için talep edilen mobilya tedariği. Toplam 45 çalışma istasyonu, 12 toplantı odası kapsamı dahilindedir. Teslim süresi sözleşme imzalanmasından itibaren 8 haftadır.",
            2, 6, d.AddDays(11));

        yield return new SeedRecord(
            "IT Danışmanlık Teklifi - Bulut Geçiş Projesi",
            "Mevcut on-premise sistemlerin Azure platformuna taşınması için danışmanlık teklifi. Faz 1 (analiz ve plan) 6 hafta, Faz 2 (uygulama) 16 hafta sürecektir.",
            2, 8, d.AddDays(14));

        yield return new SeedRecord(
            "Tasarım Hizmetleri Teklifi - Marka Kimliği",
            "Yeni ürün lansmanı için marka kimliği tasarım çalışması teklifimiz. Logo, kurumsal kimlik kılavuzu, web sitesi mockup'larını kapsar.",
            2, 7, d.AddDays(18));

        yield return new SeedRecord(
            "Pazarlama Kampanyası Teklifi - Q2 Lansman",
            "İkinci çeyrek lansmanı için dijital pazarlama kampanyası planı ve bütçe önerisi. Sosyal medya, arama motoru ve içerik pazarlama kanalları dahildir.",
            2, 7, d.AddDays(22));

        // --- Faturalar (Type 3) ---
        yield return new SeedRecord(
            "Fatura - Mart 2026 - ABC Lojistik",
            "Fatura No: 2026/0312\nMüşteri: ABC Lojistik Ltd. Şti.\nDönem: Mart 2026\nHizmet: Aylık taşımacılık hizmetleri\nTutar: KDV dahil belirlenen tutar üzerinden faturalanmıştır.",
            3, 3, d.AddDays(60));

        yield return new SeedRecord(
            "Fatura - Şubat 2026 - Mavi Bilişim",
            "Fatura No: 2026/0218\nTedarikçi: Mavi Bilişim\nDönem: Şubat 2026\nHizmet: Yazılım bakım ve destek\nÖdeme vadesi: 30 gün.",
            3, 3, d.AddDays(45));

        yield return new SeedRecord(
            "Fatura - Ocak 2026 - Mavi Bilişim",
            "Fatura No: 2026/0119\nTedarikçi: Mavi Bilişim\nDönem: Ocak 2026\nHizmet: Yazılım bakım ve destek\nÖdeme vadesi: 30 gün.",
            3, 3, d.AddDays(15));

        yield return new SeedRecord(
            "Fatura - Elektrik Tüketimi Mart 2026",
            "Fatura No: ELK-2026-03-7821\nTedarikçi: Enerji A.Ş.\nDönem: Mart 2026\nKullanım: 12.450 kWh\nDüzenli ödeme talimatı verilmiştir.",
            3, 3, d.AddDays(62));

        yield return new SeedRecord(
            "Fatura - Internet Hizmeti Şubat 2026",
            "Fatura No: NET-2026-02-1149\nTedarikçi: Telekom Hizmetleri\nDönem: Şubat 2026\nHizmet: Kurumsal fiber internet\nÖdeme: otomatik ödeme.",
            3, 3, d.AddDays(48));

        yield return new SeedRecord(
            "Fatura - Ofis Temizlik Hizmeti Mart 2026",
            "Fatura No: TMZ-2026-03-0455\nTedarikçi: Pırıl Temizlik\nDönem: Mart 2026\nHizmet: Genel müdürlük ofis temizliği\nNot: Mesai dışı temizlik dahildir.",
            3, 3, d.AddDays(63));

        yield return new SeedRecord(
            "Fatura - Yazılım Lisans Yenileme",
            "Fatura No: LIC-2026-1208\nTedarikçi: Global Software Inc.\nKalem: Geliştirme araçları yıllık lisans yenileme\nLisans sayısı: 24 kullanıcı.",
            3, 8, d.AddDays(35));

        yield return new SeedRecord(
            "Fatura - Eğitim Hizmetleri Şubat 2026",
            "Fatura No: EGT-2026-02-0231\nTedarikçi: Akademi Eğitim Kurumu\nDönem: Şubat 2026\nKonu: Liderlik programı 1. modül.",
            3, 5, d.AddDays(50));

        yield return new SeedRecord(
            "Fatura - Mobilya Tedariği Şube Ofisi",
            "Fatura No: MBL-2026-0102\nTedarikçi: Mobilya Tedarik A.Ş.\nKalem: 45 çalışma istasyonu, 12 toplantı odası mobilya seti\nTeslimat tamamlandı.",
            3, 6, d.AddDays(70));

        // --- Diğer (Type 4) ---
        yield return new SeedRecord(
            "Q1 2026 Bütçe Raporu",
            "İlk çeyrek bütçe gerçekleşmeleri ve sapma analizi. Operasyonel giderler beklenen seviyenin %3 altında, gelir tahminleri %5 üzerinde gerçekleşmiştir.",
            4, 3, d.AddDays(85));

        yield return new SeedRecord(
            "Yıllık Personel Değerlendirme Raporu 2025",
            "2025 yılı personel performans değerlendirme süreci özeti. Toplam 312 çalışan değerlendirildi. İlerleme planları ve geri bildirimler özetlenmiştir.",
            4, 5, d.AddDays(20));

        yield return new SeedRecord(
            "Yönetim Kurulu Toplantı Notları - Mart 2026",
            "Toplantı tarihi: 15.03.2026. Gündem: Q1 sonuçları, yeni şube açılış planı, dijital dönüşüm yatırımları, IK politika güncellemeleri.",
            4, 1, d.AddDays(73));

        yield return new SeedRecord(
            "Bilgi Güvenliği Politikası v3",
            "Şirket bilgi güvenliği politikası üçüncü versiyon güncellendi. Parola politikası, MFA zorunluluğu, veri sınıflandırma tabloları ekleneek geçerlilik tarihi belirlendi.",
            4, 8, d.AddDays(40));

        yield return new SeedRecord(
            "Hukuki Risk Değerlendirme Raporu",
            "Devam eden davaların durumu, beklenen sonuçlar ve karşılık tutarları özetlenmiştir. Dış hukuk ekibi ile aylık koordinasyon devam etmektedir.",
            4, 1, d.AddDays(55));

        // --- Edge case: tam duplicate (aynı kişi tekrar yüklemiş) ---
        yield return new SeedRecord(
            "Fatura - Mart 2026 - ABC Lojistik",
            "Fatura No: 2026/0312\nMüşteri: ABC Lojistik Ltd. Şti.\nDönem: Mart 2026\nHizmet: Aylık taşımacılık hizmetleri\nTutar: KDV dahil belirlenen tutar üzerinden faturalanmıştır.",
            3, 4, d.AddDays(75));

        yield return new SeedRecord(
            "Fatura - Şubat 2026 - Mavi Bilişim",
            "Fatura No: 2026/0218\nTedarikçi: Mavi Bilişim\nDönem: Şubat 2026\nHizmet: Yazılım bakım ve destek\nÖdeme vadesi: 30 gün.",
            3, 8, d.AddDays(58));

        // --- Edge case: yakın başlık duplicate (case farkı + ek karakter) ---
        yield return new SeedRecord(
            "Tedarik Sözleşmesi - ABC Lojistik LTD. ŞTİ.",
            "Yeniden gözden geçirilmiş tedarik sözleşmesi metni. Hizmet kapsamı genişletildi, yıllık hacim tahmini 1500 sevkiyat olarak güncellendi.",
            1, 2, d.AddDays(34));

        yield return new SeedRecord(
            "Q1 2026 Bütçe Raporu (Revize)",
            "Gözden geçirilmiş bütçe raporu. Mart ayı kapanışı sonrasında ek revizyonlar eklenmiştir.",
            4, 3, d.AddDays(90));
    }
}

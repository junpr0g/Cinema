using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using System.IO;
using Cinema.Models.DatabaseClasses;

namespace Cinema.Services
{
    public class TicketService
    {
        public static string secretKey = "";
    public record TicketData(
        int Id,
        string Movie,
        int Place,
        int Row,
        DateTime Date,
        bool IsVip,
        string ViewerName
        );

    public byte[] Generate(int ticketId, string movie, int place, int row, DateTime date, bool isVip, string ViewerName)
    {
        // 1. Подготовка данных
        var data = new TicketData(
            Id: ticketId,
            Movie: movie,
            Place: place,
            Row: row,
            Date: date,
            IsVip: isVip,
            ViewerName: ViewerName
        );

        // 2. Сериализация и шифрование
        string json = JsonSerializer.Serialize(data);
        string encryptedData = EncryptString(json, secretKey);

        // 3. Тема оформления
        var theme = new
        {
            PageBg = isVip ? "#0B0F1A" : "#F3F6FA",
            CardBg = isVip ? "#111827" : "#FFFFFF",
            Accent = isVip ? "#D4AF37" : "#2563EB",
            MainText = isVip ? "#FFFFFF" : "#1F2937",
            SecondaryText = isVip ? "#9CA3AF" : "#6B7280",
            Border = isVip ? "#D4AF37" : "#E5E7EB",
            Line = isVip ? "#374151" : "#E5E7EB"
        };

        byte[] qrCodeImage = GenerateQrCode(encryptedData);

        // 4. Генерация PDF в память (не сохраняем физический файл)
        using var ms = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(theme.PageBg);
                page.DefaultTextStyle(x => x.FontSize(14).FontFamily(Fonts.Verdana).FontColor(theme.MainText));

                page.Content().AlignCenter().Column(col =>
                {
                    col.Spacing(25);
                    col.Item().Background(theme.CardBg).Border(1).BorderColor(theme.Border).Padding(30).Column(card =>
                    {
                        card.Spacing(18);

                        // Header
                        card.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Билет № {data.Id}").FontSize(26).SemiBold().FontColor(theme.Accent);
                            if (isVip) row.ConstantItem(80).Background("#1F2937").PaddingVertical(5).Border(1).BorderColor(theme.Accent).AlignCenter().Text("VIP").FontSize(16).SemiBold().FontColor(theme.Accent);
                        });

                        card.Item().LineHorizontal(1).LineColor(theme.Line);

                        // Movie Title
                        card.Item().PaddingTop(10).AlignCenter().Column(c => {
                            c.Item().Text($"\"{data.Movie}\"").FontSize(28).SemiBold().AlignCenter();
                            c.Item().Text("Космическое путешествие начинается").FontSize(12).FontColor(theme.SecondaryText).AlignCenter();
                        });

                        card.Item().LineHorizontal(1).LineColor(theme.Line);

                        // Grid
                        card.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(l => {
                                l.Item().Text($"Место: {data.Place}").FontColor(theme.Accent);
                                l.Item().Text($"Ряд: {data.Row}").FontColor(theme.Accent);
                                l.Item().Text("Кинотеатр: CIL");
                            });
                            row.RelativeItem().Column(r => {
                                r.Item().Text("Дата: ").FontColor(theme.SecondaryText);
                                r.Item().Text(data.Date.ToString("dd.MM.yyyy HH:mm")).SemiBold();
                                r.Item().Text($"Владелец: {data.ViewerName}").FontColor(theme.SecondaryText);
                            });
                        });

                        card.Item().LineHorizontal(1).LineColor(theme.Line);

                        // QR
                        card.Item().PaddingTop(25).AlignCenter().Column(qr =>
                        {
                            qr.Item().Background(Colors.White).Padding(10).Width(160).Height(160).Image(qrCodeImage);
                            qr.Item().PaddingTop(5).Text("Зашифрованный электронный контроль").FontSize(8).FontColor(theme.SecondaryText).Italic();
                        });
                    });
                });
            });
        }).GeneratePdf(ms);

        return ms.ToArray();
    }

    private byte[] GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        // Используем уровень коррекции M или Q, так как зашифрованная строка длиннее обычной ссылки
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(10);
    }

    // Метод шифрования AES
    private string EncryptString(string plainText, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv; // В продакшене лучше использовать рандомный IV и прикреплять его к строке

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    array = memoryStream.ToArray();
                }
            }
        }
        return Convert.ToBase64String(array);
    }
}
}

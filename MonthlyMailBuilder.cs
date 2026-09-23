using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace SocialDashboard
{
    public class MonthlyMail
    {
        public string Subject { get; set; }
        public string Html { get; set; }
        public int RowCount { get; set; }
    }

    public static class MonthlyMailBuilder
    {
        static readonly string[] Months = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                                            "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

        public static MonthlyMail Build(List<Employee> employees, string recipientName, string recipientTitle, DateTime sendDate)
        {
            DateTime from = sendDate.Date.AddMonths(-1);
            DateTime to = sendDate.Date.AddMonths(1);

            var rows = employees
                .Where(e => e.ManagerName == recipientName || e.DirectorName == recipientName)
                .SelectMany(e => e.Events.Where(ev => ev.IsShared).Select(ev => new { Emp = e, Ev = ev }))
                .Where(x => x.Ev.EventDate.Date >= from && x.Ev.EventDate.Date <= to)
                .OrderBy(x => x.Ev.EventDate.Date)
                .ThenBy(x => x.Emp.FullName, StringComparer.Ordinal)
                .ToList();

            int past = rows.Count(x => x.Ev.EventDate.Date < sendDate.Date);
            string subject = $"Aylık Çalışan Gelişmeleri Özeti | {Months[sendDate.Month - 1]} {sendDate.Year}";

            const string td = "padding:10px 8px;border-bottom:1px solid #E2E4E7;font-size:13px;color:#26282B;";
            const string th = "padding:10px 8px;background:#A6192E;color:#FFFFFF;font-size:12.5px;font-weight:600;text-align:left;";

            var body = new StringBuilder();
            if (rows.Count == 0)
            {
                body.Append($"<tr><td colspan=\"6\" style=\"{td}text-align:center;color:#7D828A;\">Bu dönemde organizasyonunuzda paylaşılmış bir gelişme bulunmuyor.</td></tr>");
            }
            foreach (var r in rows)
            {
                bool upcoming = r.Ev.EventDate.Date >= sendDate.Date;
                string timeStyle = upcoming ? "color:#A6192E;font-weight:600;" : "color:#7D828A;";
                body.Append("<tr>")
                    .Append($"<td style=\"{td}font-weight:600;\">{H(r.Emp.FullName)}</td>")
                    .Append($"<td style=\"{td}\">{H(r.Emp.Department)}</td>")
                    .Append($"<td style=\"{td}\">{H(r.Ev.Category)}</td>")
                    .Append($"<td style=\"{td}\">{H(r.Ev.EventType)}</td>")
                    .Append($"<td style=\"{td}white-space:nowrap;\">{r.Ev.EventDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}</td>")
                    .Append($"<td style=\"{td}white-space:nowrap;{timeStyle}\">{RelativeTime(r.Ev.EventDate, sendDate)}</td>")
                    .Append("</tr>");
            }

            string html = $@"<!DOCTYPE html>
<html lang=""tr""><head><meta charset=""UTF-8""><title>{H(subject)}</title></head>
<body style=""margin:0;padding:24px;background:#EFF0F2;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:900px;margin:0 auto;background:#FFFFFF;border:1px solid #E2E4E7;font-family:'IBM Plex Sans',Segoe UI,Arial,sans-serif;"">
  <tr><td style=""background:#A6192E;padding:18px 28px;color:#FFFFFF;font-size:17px;font-weight:700;"">Aylık Çalışan Gelişmeleri Özeti</td></tr>
  <tr><td style=""padding:26px 28px 8px;font-size:14px;line-height:1.6;color:#26282B;"">
    <p style=""margin:0 0 14px;font-weight:600;"">Merhaba {H(recipientName)},</p>
    <p style=""margin:0 0 12px;"">Aşağıda organizasyonunuzdaki çalışanlara ait, belirlenen zaman penceresinde gerçekleşen veya yaklaşan önemli gelişmeler toplu olarak listelenmiştir. Bu e-posta bilgilendirme amaçlıdır; herhangi bir aksiyon alınması beklenmez.</p>
    <p style=""margin:0 0 12px;"">Raporlama kapsamı: son 1 ayda gerçekleşen olaylar ve önümüzdeki 1 ayda gerçekleşmesi beklenen olaylar. Bu dönemde <b>{past}</b> gerçekleşen, <b>{rows.Count - past}</b> yaklaşan gelişme var.</p>
  </td></tr>
  <tr><td style=""padding:10px 28px 4px;font-size:12.5px;font-weight:700;color:#4A4D52;"">Organizasyonunuzdaki gelişmeler</td></tr>
  <tr><td style=""padding:6px 28px 24px;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;"">
      <tr><th style=""{th}"">Çalışan</th><th style=""{th}"">Birim</th><th style=""{th}"">Kategori</th><th style=""{th}"">Olay / Gelişme</th><th style=""{th}"">Tarih</th><th style=""{th}"">Zaman</th></tr>
      {body}
    </table>
  </td></tr>
  <tr><td style=""padding:14px 28px 24px;font-size:11.5px;color:#A3A7AE;text-align:center;font-style:italic;"">Bu e-posta sistem tarafından otomatik olarak oluşturulmuştur. Lütfen yanıtlamayınız.</td></tr>
</table>
</body></html>";

            return new MonthlyMail { Subject = subject, Html = html, RowCount = rows.Count };
        }

        public static string RelativeTime(DateTime eventDate, DateTime reference)
        {
            int diff = (eventDate.Date - reference.Date).Days;
            int a = Math.Abs(diff);
            if (diff == 0) return "Bugün";
            if (diff < 0)
                return a == 1 ? "Dün" : a < 7 ? $"{a} gün önce" : a < 14 ? "1 hafta önce" : a < 28 ? $"{a / 7} hafta önce" : "Geçen ay";
            return a == 1 ? "Yarın" : a < 7 ? $"{a} gün sonra" : a < 14 ? "Gelecek hafta" : a < 28 ? $"{a / 7} hafta sonra" : "Gelecek ay";
        }

        public static string Slug(string name)
        {
            var map = new Dictionary<char, string> { ['ç'] = "c", ['ğ'] = "g", ['ı'] = "i", ['ö'] = "o", ['ş'] = "s", ['ü'] = "u", ['İ'] = "i", [' '] = "-" };
            var sb = new StringBuilder();
            foreach (char c in name.ToLowerInvariant())
                sb.Append(map.TryGetValue(c, out var r) ? r : char.IsLetterOrDigit(c) ? c.ToString() : "");
            return sb.ToString();
        }

        static string H(string s) => WebUtility.HtmlEncode(s ?? "");
    }
}

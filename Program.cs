using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SocialDashboard
{
    public static class Categories
    {
        public const string FamilyLife = "Aile & Yaşam";
        public const string AcademicDev = "Akademik & Gelişim";
        public const string Corporate = "Kurumsal Kıdem";
        public const string Birthday = "Doğum Günü";
    }

    public class SocialEvent
    {
        public string EventType { get; set; }
        public string Category { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; }
        public bool IsShared { get; set; }

        public string TimePosition
        {
            get
            {
                if (EventDate.Date < DateTime.Now.Date) return "Geçmiş";
                else if (EventDate.Date > DateTime.Now.Date) return "Yaklaşıyor";
                else return "Bugün";
            }
        }
    }

    public class Employee
    {
        public int RegistrationNumber { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string ManagerName { get; set; }
        public List<SocialEvent> Events { get; set; } = new List<SocialEvent>();

        public string Gender { get; set; }
        public string Title { get; set; }
        public string ManagerTitle { get; set; }
        public string DirectorName { get; set; }
        public string DirectorTitle { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime UnitStartDate { get; set; }
        public DateTime BirthDate { get; set; }
        public string Bachelor { get; set; }
        public string Master { get; set; }
        public string Certificates { get; set; }
        public string Language { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseInfo { get; set; }
        public string ChildrenInfo { get; set; }
        public string EmergencyPhone { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            string dataPath = args.Length > 0 ? args[0] : FindDataFile("employees.json");
            string pythonJsonData = File.ReadAllText(dataPath, Encoding.UTF8);

            List<Employee> employees = JsonSerializer.Deserialize<List<Employee>>(pythonJsonData);

            DateTime today = DateTime.Now;
            string activeManager = "Mehmet Öztürk";

            Console.WriteLine($"=== YÖNETİCİ EKRANI: {activeManager} ===");
            Console.WriteLine($"Sistemde toplam {employees.Count} çalışan verisi başarıyla yüklendi!\n");

            var dashboardEvents = employees
                .Where(emp => emp.ManagerName == activeManager || emp.DirectorName == activeManager)
                .SelectMany(emp => emp.Events
                    .Where(ev => ev.IsShared)
                    .Select(ev => new {
                        EmployeeName = emp.FullName,
                        Department = emp.Department,
                        EventDetail = ev
                    }))
                .Where(x => x.EventDetail.EventDate.Date >= today.AddMonths(-1).Date &&
                            x.EventDetail.EventDate.Date <= today.AddMonths(1).Date)
                .OrderBy(x => x.EventDetail.EventDate)
                .ToList();

            if (dashboardEvents.Any())
            {
                foreach (var item in dashboardEvents)
                {
                    Console.WriteLine($"Tarih: {item.EventDetail.EventDate.ToShortDateString()} [{item.EventDetail.TimePosition}]");
                    Console.WriteLine($"Kişi: {item.EmployeeName} ({item.Department})");
                    Console.WriteLine($"Gelişme: {item.EventDetail.EventType} ({item.EventDetail.Category})");
                    Console.WriteLine("--------------------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("Kriterlere uygun yaklaşan bir olay bulunmamaktadır.");
            }

            DateTime sendDate = new DateTime(today.Year, today.Month, 1, 9, 0, 0);
            string outDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(dataPath)), "output", "mails");
            Directory.CreateDirectory(outDir);

            var recipients = employees
                .Select(e => (Name: e.ManagerName, Title: e.ManagerTitle))
                .Concat(employees.Select(e => (Name: e.DirectorName, Title: e.DirectorTitle)))
                .Where(r => !string.IsNullOrEmpty(r.Name))
                .Distinct()
                .ToList();

            Console.WriteLine($"\n=== AYLIK MAİLLER ({sendDate:dd.MM.yyyy HH:mm}) ===");
            foreach (var r in recipients)
            {
                MonthlyMail mail = MonthlyMailBuilder.Build(employees, r.Name, r.Title, sendDate);
                string file = Path.Combine(outDir, $"{sendDate:yyyy-MM}_{MonthlyMailBuilder.Slug(r.Name)}.html");
                File.WriteAllText(file, mail.Html, Encoding.UTF8);
                Console.WriteLine($"{r.Name}: {mail.RowCount} gelişme -> {Path.GetFullPath(file)}");
            }
        }

        static string FindDataFile(string relativePath)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, relativePath);
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new FileNotFoundException($"{relativePath} bulunamadı.");
        }
    }
}

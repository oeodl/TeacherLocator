using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    class TeacherResult
    {
        public string Name { get; set; }
        public string Sprechstunde { get; set; }
        public string Telefon { get; set; }
        public string Url { get; set; }
    }

    static async Task Main()
    {
        Console.WriteLine("=== HTL Salzburg Raum-Suche ===");
        Console.Write("Bitte gib die Raumnummer ein (z. B. C 114 oder c-114): ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return;

        // Wir bereiten den Raumcode für die URL vor (z. B. "c-114")
        string targetRoomCode = input.ToLower().Replace(" ", "-").Trim();
        if(!targetRoomCode.Contains("-") && targetRoomCode.Length >= 4)
        {
             targetRoomCode = targetRoomCode.Insert(1, "-"); 
        }

        using HttpClient client = new HttpClient();
        string mainUrl = "https://www.htl-salzburg.ac.at/lehrerinnen.html";
        
        try
        {
            Console.WriteLine($"\nLade Daten für Raum {targetRoomCode.ToUpper().Replace("-", " ")}...");
            string mainHtml = await client.GetStringAsync(mainUrl);

            // 1. SCHRITT: Der korrigierte Regex für die Hauptseite (beachtet das <span>-Tag!)
            string linkPattern = $@"href\s*=\s*[""'](?<Link>[^""']*?lehrerinnen(?:-details)?/[^""']*?-{targetRoomCode}\.html)[""'][^>]*><span[^>]*>(?<Name>.*?)</span></a>";
            MatchCollection linkMatches = Regex.Matches(mainHtml, linkPattern, RegexOptions.IgnoreCase);

            var teachersToProcess = new Dictionary<string, string>();
            foreach (Match match in linkMatches)
            {
                string link = match.Groups["Link"].Value;
                string name = match.Groups["Name"].Value.Trim(); 
                
                if (!link.Contains("mailto") && !teachersToProcess.ContainsKey(link))
                {
                    string absoluteUrl = link.StartsWith("http") ? link : "https://www.htl-salzburg.ac.at" + (link.StartsWith("/") ? "" : "/") + link;
                    teachersToProcess[absoluteUrl] = name;
                }
            }

            if (teachersToProcess.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Keine Lehrkräfte für diesen Raum auf der Hauptseite gefunden.");
                Console.ResetColor();
                return;
            }

            var results = new ConcurrentBag<TeacherResult>();
            var tasks = new List<Task>();

            // 2. SCHRITT: Unterseiten parallel abrufen
            foreach (var kvp in teachersToProcess)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try 
                    {
                        string html = await client.GetStringAsync(kvp.Key);
                        
                        // Regex für Sprechstunde
                        string sPattern = @"<div[^>]*class=""label"">Sprechstunde:</div>\s*<div[^>]*class=""value"">\s*<span[^>]*class=""text"">(?<Zeit>.*?)</span>";
                        Match sMatch = Regex.Match(html, sPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        string sprechstunde = sMatch.Success ? sMatch.Groups["Zeit"].Value.Trim() : "Keine Angabe";
                        
                        if (sprechstunde == "-" || string.IsNullOrWhiteSpace(sprechstunde)) sprechstunde = "Keine Angabe";

                        // Regex für Telefon 
                        string tPattern = @"<div[^>]*class=""label"">Tel(?:-DW)?:</div>\s*<div[^>]*class=""value"">\s*<span[^>]*class=""text"">(?<Tel>.*?)</span>";
                        Match tMatch = Regex.Match(html, tPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        string telefon = tMatch.Success ? tMatch.Groups["Tel"].Value.Trim() : "Keine Angabe";

                        // html decode the name (so that &#40; becomes '(' )
                        string decodedName = System.Net.WebUtility.HtmlDecode(kvp.Value);

                        results.Add(new TeacherResult { Name = decodedName, Sprechstunde = sprechstunde, Telefon = telefon, Url = kvp.Key });
                    }
                    catch { /* Fehler einzelner Seitenaufrufe ignorieren */ }
                }));
            }

            await Task.WhenAll(tasks);

            // 3. SCHRITT: Ausgabe in der Konsole
            foreach (var res in results)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"- {res.Name}");
                Console.ResetColor();
                Console.WriteLine($"  Sprechstunde: {res.Sprechstunde}");
                Console.WriteLine($"  Telefon:      {res.Telefon}");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nEin Fehler ist aufgetreten: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nDrücke eine beliebige Taste zum Beenden...");
        Console.ReadKey();
    }
}
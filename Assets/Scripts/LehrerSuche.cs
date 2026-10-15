using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

// Unity-Version des Konsolenprogramms (Program.cs): UnityWebRequest + Coroutine statt HttpClient + async
public static class LehrerSuche
{
    private const string BaseUrl = "https://www.htl-salzburg.ac.at";

    // Die Hauptseite wird nur einmal geladen und dann für alle Räume wiederverwendet
    private static string mainHtml;

    // Liefert den fertigen Anzeigetext, oder null wenn die Website nicht erreichbar ist bzw. nichts gefunden wurde
    public static IEnumerator LadeLehrer(string raumEingabe, Action<string> fertig)
    {
        // Raumcode vorbereiten (z. B. "B210" -> "b-210")
        string raumCode = raumEingabe.ToLower().Replace(" ", "-").Trim();
        if (!raumCode.Contains("-") && raumCode.Length >= 4)
        {
            raumCode = raumCode.Insert(1, "-");
        }

        // 1. SCHRITT: Hauptseite laden und Links zu den Lehrkräften suchen
        if (mainHtml == null)
        {
            yield return Lade(BaseUrl + "/lehrerinnen.html", html => mainHtml = html);
        }
        if (mainHtml == null)
        {
            fertig(null);
            yield break;
        }

        string linkPattern = $@"href\s*=\s*[""'](?<Link>[^""']*?lehrerinnen(?:-details)?/[^""']*?-{raumCode}\.html)[""'][^>]*><span[^>]*>(?<Name>.*?)</span></a>";
        var teachersToProcess = new Dictionary<string, string>();
        foreach (Match match in Regex.Matches(mainHtml, linkPattern, RegexOptions.IgnoreCase))
        {
            string link = match.Groups["Link"].Value;
            if (link.Contains("mailto")) continue;

            string absoluteUrl = link.StartsWith("http") ? link : BaseUrl + (link.StartsWith("/") ? "" : "/") + link;
            teachersToProcess[absoluteUrl] = match.Groups["Name"].Value.Trim();
        }

        if (teachersToProcess.Count == 0)
        {
            fertig(null);
            yield break;
        }

        // 2. SCHRITT: Unterseiten nacheinander abrufen
        var text = new StringBuilder();
        foreach (var kvp in teachersToProcess)
        {
            string html = null;
            yield return Lade(kvp.Key, h => html = h);

            text.AppendLine($"<b>{WebUtility.HtmlDecode(kvp.Value)}</b>");
            if (html != null)
            {
                text.AppendLine("Sprechstunde: " + Finde(html, @"<div[^>]*class=""label"">Sprechstunde:</div>\s*<div[^>]*class=""value"">\s*<span[^>]*class=""text"">(?<Wert>.*?)</span>"));
                text.AppendLine("Telefon: " + Finde(html, @"<div[^>]*class=""label"">Tel(?:-DW)?:</div>\s*<div[^>]*class=""value"">\s*<span[^>]*class=""text"">(?<Wert>.*?)</span>"));
            }
            text.AppendLine();
        }

        // 3. SCHRITT: Ergebnis zurückgeben (statt Console.WriteLine)
        fertig(text.ToString().TrimEnd());
    }

    private static IEnumerator Lade(string url, Action<string> ergebnis)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            ergebnis(request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null);
        }
    }

    private static string Finde(string html, string pattern)
    {
        Match match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        string wert = match.Success ? match.Groups["Wert"].Value.Trim() : "";
        return (wert == "" || wert == "-") ? "Keine Angabe" : wert;
    }
}

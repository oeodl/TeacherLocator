using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

// Lädt beim Spielstart für jede Tür die Lehrer von der Website und schreibt sie in DoorInfo.lehrerNamen.
// DoorInfo zeigt diesen Text dann wie gewohnt an, DoorInfo selbst muss dafür nicht geändert werden.
// Verwendung: leeres GameObject in der Szene anlegen und dieses Skript draufziehen.
public class LehrerDatenLader : MonoBehaviour
{
    private IEnumerator Start()
    {
        foreach (DoorInfo tuer in FindObjectsByType<DoorInfo>(FindObjectsSortMode.None))
        {
            // Raumnummer aus dem Objektnamen lesen: "TürB210" -> "B210"
            Match match = Regex.Match(tuer.name, @"([A-Za-z])[\s_-]*(\d+)$");
            if (!match.Success) continue;

            string raum = match.Groups[1].Value.ToUpper() + match.Groups[2].Value;
            string vorher = tuer.lehrerNamen; // im Inspector eingetragener Text als Fallback
            string ladeText = $"Raum {raum}\n\nLade Lehrer...";
            tuer.lehrerNamen = ladeText;

            // Räume nacheinander laden, damit die Hauptseite nur einmal heruntergeladen wird
            yield return LehrerSuche.LadeLehrer(raum, text =>
            {
                if (text != null)
                    tuer.lehrerNamen = $"Raum {raum}\n\n{text}";
                else
                    tuer.lehrerNamen = string.IsNullOrWhiteSpace(vorher) ? $"Raum {raum}\n\nKeine Lehrkräfte gefunden" : vorher;

                // Steht der Spieler gerade vor dieser Tür, das offene Schild sofort aktualisieren
                if (tuer.infoText != null && tuer.infoText.text == ladeText)
                    tuer.infoText.text = tuer.lehrerNamen;
            });
        }
    }
}

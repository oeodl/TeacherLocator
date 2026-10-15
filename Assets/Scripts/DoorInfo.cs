using UnityEngine; // Grundfunktionen von Unity (MonoBehaviour, GameObject, Collider2D, ...)
using TMPro; // Wird für das TextMeshPro UI benötigt

// Zeigt ein Info-Schild mit den Lehrkräften an, sobald der Spieler vor einer Tür steht.
// Das Skript liegt auf jeder Tür (z. B. "TürB210"). Jede Tür hat einen BoxCollider2D mit
// aktiviertem "Is Trigger": Der Spieler kann durchlaufen, aber Unity meldet das Betreten und Verlassen.
public class DoorInfo : MonoBehaviour
{
    // [TextArea] macht im Inspector ein mehrzeiliges Textfeld daraus.
    // Beim Spielstart überschreibt das Skript LehrerDatenLader diesen Text mit den Daten von der Website.
    [TextArea]
    public string lehrerNamen; // Hier trägst du im Inspector temporär die Lehrer ein

    // Diese beiden Felder werden im Inspector verknüpft (Objekt aus der Hierarchy hineinziehen).
    // Alle Türen verwenden dasselbe Panel und denselben Text.
    public GameObject infoSchild; // Das Panel-Hintergrundbild
    public TextMeshProUGUI infoText; // Das Textfeld für die Namen

    // Wird von Unity automatisch aufgerufen, wenn ein anderer Collider den Trigger der Tür betritt.
    // "collision" ist der Collider, der hineingelaufen ist.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Wenn der Spieler in die Zone der Tür läuft
        // (nur Objekte mit dem Tag "Player" sollen das Schild öffnen)
        if (collision.CompareTag("Player"))
        {
            infoText.text = lehrerNamen;   // Text dieser Tür ins Schild schreiben
            infoSchild.SetActive(true);    // Schild einblenden
        }
    }

    // Wird von Unity automatisch aufgerufen, wenn der Collider den Trigger wieder verlässt.
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Wenn der Spieler die Zone wieder verlässt
        if (collision.CompareTag("Player"))
        {
            infoSchild.SetActive(false);   // Schild ausblenden
        }
    }
}

using UnityEngine;
using TMPro; // Wird für das TextMeshPro UI benötigt

public class DoorInfo : MonoBehaviour
{
    [TextArea] 
    public string lehrerNamen; // Hier trägst du im Inspector temporär die Lehrer ein
    
    public GameObject infoSchild; // Das Panel-Hintergrundbild
    public TextMeshProUGUI infoText; // Das Textfeld für die Namen

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Wenn der Spieler in die Zone der Tür läuft
        if (collision.CompareTag("Player"))
        {
            infoText.text = lehrerNamen;
            infoSchild.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Wenn der Spieler die Zone wieder verlässt
        if (collision.CompareTag("Player"))
        {
            infoSchild.SetActive(false);
        }
    }
}
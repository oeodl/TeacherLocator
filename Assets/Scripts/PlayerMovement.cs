using UnityEngine; // Grundfunktionen von Unity (MonoBehaviour, Input, Rigidbody2D, ...)

// Steuert den Spieler (roter Punkt) mit den Tasten W, A, S, D.
// Das Skript liegt auf dem Player-Objekt. Dieses braucht zusätzlich einen Rigidbody2D
// mit Gravity Scale = 0, damit der Punkt in der Draufsicht nicht nach unten "fällt".
public class PlayerMovement : MonoBehaviour
{
    // Geschwindigkeit in Unity-Einheiten pro Sekunde.
    // "public" bedeutet: Der Wert kann im Inspector verändert werden, ohne den Code anzufassen.
    public float speed = 5f;

    // Verweis auf den Rigidbody2D (die Physik-Komponente) des Spielers.
    // Über ihn wird der Spieler bewegt, damit er mit den Wänden (Collidern) zusammenstößt.
    private Rigidbody2D rb;

    // Speichert die gewünschte Bewegungsrichtung, z. B. (1, 0) = nach rechts, (0, 1) = nach oben.
    private Vector2 movement;

    // Start() wird von Unity einmal beim Spielstart aufgerufen.
    void Start()
    {
        // Holt sich den physikalischen Körper des Spielers
        rb = GetComponent<Rigidbody2D>();
    }

    // Update() wird von Unity in jedem Frame aufgerufen (also viele Male pro Sekunde).
    // Hier wird nur die Tastatur abgefragt, bewegt wird erst in FixedUpdate().
    void Update()
    {
        // Setzt die Bewegung jeden Frame auf 0 zurück
        // (wenn keine Taste gedrückt ist, bleibt der Spieler stehen)
        movement = Vector2.zero;

        // Prüft explizit die WASD-Tasten ab
        // Input.GetKey liefert true, solange die Taste gedrückt gehalten wird.
        if (Input.GetKey(KeyCode.W))
        {
            movement.y = 1;  // nach oben
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement.y = -1; // nach unten
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement.x = -1; // nach links
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement.x = 1;  // nach rechts
        }
    }

    // FixedUpdate() wird in festen Zeitabständen aufgerufen (Standard: 50-mal pro Sekunde).
    // Physik-Bewegungen gehören hierher, damit sie auf jedem Computer gleich schnell ablaufen.
    void FixedUpdate()
    {
        // Bewegt den Spieler basierend auf der Eingabe:
        // - movement.normalized: Richtung mit Länge 1, damit man schräg (z. B. W + D)
        //   nicht schneller ist als gerade aus
        // - * speed: wie weit pro Sekunde
        // - * Time.fixedDeltaTime: Zeit seit dem letzten FixedUpdate, rechnet "pro Sekunde" auf "pro Aufruf" um
        // MovePosition bewegt über die Physik, deshalb bleibt der Spieler an Wänden hängen.
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}

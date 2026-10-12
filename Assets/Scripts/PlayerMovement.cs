using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        // Holt sich den physikalischen Körper des Spielers
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Setzt die Bewegung jeden Frame auf 0 zurück
        movement = Vector2.zero;

        // Prüft explizit die WASD-Tasten ab
        if (Input.GetKey(KeyCode.W))
        {
            movement.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement.x = 1;
        }
    }

    void FixedUpdate()
    {
        // Bewegt den Spieler basierend auf der Eingabe
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}
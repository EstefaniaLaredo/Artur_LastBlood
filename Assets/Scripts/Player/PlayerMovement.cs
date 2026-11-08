using UnityEngine;

// Control de movimiento en 8 direcciones con teclado (WASD / Flechas) y rotación hacia la dirección de avance para Artur Jones.
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.up;

    public Vector2 UltimaDireccion => ultimaDireccion;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        // Si hay una conversación en pantalla, detener al personaje por completo
        if (DialogueUI.EstaEnDialogo)
        {
            movimiento = Vector2.zero;
            return;
        }

        movimiento = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            movimiento.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            movimiento.x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            movimiento.y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            movimiento.y += 1f;

        movimiento = movimiento.normalized;

        // Si Artur se está moviendo, orientar su mirada hacia la dirección de movimiento (W, A, S, D y diagonales)
        if (movimiento.sqrMagnitude > 0.001f)
        {
            ultimaDireccion = movimiento;
            float angulo = Mathf.Atan2(ultimaDireccion.y, ultimaDireccion.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }

    private void FixedUpdate()
    {
        if (DialogueUI.EstaEnDialogo)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}


using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Vector2 movimiento;

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
        // Se leen teclas directas para funcionar tanto con Input Manager como
        // con el New Input System sin configuración manual de ejes.
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
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}

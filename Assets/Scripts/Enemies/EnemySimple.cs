using UnityEngine;

// Enemigo con IA básica (vigilancia, detección, persecución y ataque), vida visible y retroalimentación.
public class EnemySimple : MonoBehaviour
{
    public enum EstadoEnemigo { Vigilancia, Persecucion, Ataque, Muerto }

    [SerializeField] private float velocidad = 2.4f;
    [SerializeField] private float rangoDeteccion = 6.8f;
    [SerializeField] private float distanciaAtaque = 1.15f;
    [SerializeField] private int vidaInicial = 3;
    [SerializeField] private int dañoAtaque = 1;
    [SerializeField] private float tiempoEntreAtaques = 1.0f;
    [SerializeField] private string tipoEnemigo = "Guardia";

    private EstadoEnemigo estadoActual = EstadoEnemigo.Vigilancia;
    private Transform jugador;
    private Health salud;
    private Rigidbody2D rb;
    private float siguienteAtaque;
    private Vector3 posicionInicial;

    public EstadoEnemigo Estado => estadoActual;
    public string Tipo => tipoEnemigo;

    private void Awake()
    {
        posicionInicial = transform.position;

        salud = GetComponent<Health>();
        if (salud == null)
            salud = gameObject.AddComponent<Health>();
        salud.Configurar(vidaInicial);

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Crear la barra de vida flotante sobre la cabeza
        EnemyHealthBar.CrearBarra(gameObject);
    }

    public void Configurar(int vida, float vel, int daño, string nombreTipo = "Guardia")
    {
        vidaInicial = vida;
        velocidad = vel;
        dañoAtaque = daño;
        tipoEnemigo = nombreTipo;

        if (salud == null)
            salud = GetComponent<Health>();
        if (salud != null)
            salud.Configurar(vidaInicial);
    }

    private void Start()
    {
        BuscarJugador();
    }

    private void BuscarJugador()
    {
        PlayerMovement jugadorMov = FindAnyObjectByType<PlayerMovement>();
        if (jugadorMov != null)
            jugador = jugadorMov.transform;
    }

    private void Update()
    {
        if (salud == null || !salud.EstaVivo || estadoActual == EstadoEnemigo.Muerto)
            return;

        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        // Si hay diálogo activo en pantalla, el enemigo se queda quieto
        if (DialogueUI.EstaEnDialogo)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case EstadoEnemigo.Vigilancia:
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoEnemigo.Persecucion;
                }
                break;

            case EstadoEnemigo.Persecucion:
                if (distanciaAlJugador > rangoDeteccion * 1.4f)
                {
                    // Si se aleja mucho, vuelve a vigilancia
                    estadoActual = EstadoEnemigo.Vigilancia;
                }
                else if (distanciaAlJugador <= distanciaAtaque)
                {
                    estadoActual = EstadoEnemigo.Ataque;
                }
                else
                {
                    Vector2 direccion = ((Vector2)jugador.position - rb.position).normalized;
                    if (direccion.sqrMagnitude > 0.001f)
                    {
                        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - 90f;
                        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
                    }
                    rb.MovePosition(rb.position + direccion * velocidad * Time.deltaTime);
                }
                break;

            case EstadoEnemigo.Ataque:
                if (distanciaAlJugador > distanciaAtaque * 1.3f)
                {
                    estadoActual = EstadoEnemigo.Persecucion;
                }
                else if (Time.time >= siguienteAtaque)
                {
                    siguienteAtaque = Time.time + tiempoEntreAtaques;
                    EjecutarAtaque();
                }
                break;
        }
    }

    private void EjecutarAtaque()
    {
        if (jugador == null)
            return;

        Health saludJugador = jugador.GetComponent<Health>();
        if (saludJugador != null)
        {
            saludJugador.RecibirDaño(dañoAtaque, transform.position);
        }
    }

    public void RecibirDaño(int daño, Vector2? puntoOrigen = null)
    {
        if (salud != null)
            salud.RecibirDaño(daño, puntoOrigen);

        // Si nos atacan, entramos en persecución inmediatamente
        if (estadoActual == EstadoEnemigo.Vigilancia)
            estadoActual = EstadoEnemigo.Persecucion;
    }

    public void Morir()
    {
        estadoActual = EstadoEnemigo.Muerto;

        // Notificar a la progresión de Fase 1 si existe
        MissionProgression progresionFase1 = FindAnyObjectByType<MissionProgression>();
        if (progresionFase1 != null)
            progresionFase1.NotificarEnemigoDerrotado();

        // Notificar a la progresión de Fase 2 si existe
        Phase2MissionManager progresionFase2 = FindAnyObjectByType<Phase2MissionManager>();
        if (progresionFase2 != null)
            progresionFase2.NotificarEnemigoDerrotado();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}

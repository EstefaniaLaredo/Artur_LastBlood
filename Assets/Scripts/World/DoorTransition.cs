using UnityEngine;

// Puerta interactiva con tecla E que conecta los escenarios y adapta los límites de la cámara.
public class DoorTransition : MonoBehaviour, IInteractable
{
    [Header("Destino")]
    [SerializeField] private string nombreDestino = "Almacén";
    [SerializeField] private Transform puntoDestino;
    [SerializeField] private LocationID lugarDestino;
    [SerializeField] private Vector2 limitesCamaraMin;
    [SerializeField] private Vector2 limitesCamaraMax;

    [Header("Bloqueo")]
    [SerializeField] private bool bloqueada = false;
    [SerializeField] private string razonBloqueo = "La puerta está cerrada.";

    public bool EstaBloqueada => bloqueada;
    public LocationID LugarDestino => lugarDestino;

    public void Configurar(
        string destino,
        Transform punto,
        LocationID lugar,
        Vector2 camMin,
        Vector2 camMax,
        bool estaBloqueada = false,
        string motivoBloqueo = "")
    {
        nombreDestino = destino;
        puntoDestino = punto;
        lugarDestino = lugar;
        limitesCamaraMin = camMin;
        limitesCamaraMax = camMax;
        bloqueada = estaBloqueada;
        razonBloqueo = motivoBloqueo;
    }

    public void Desbloquear()
    {
        bloqueada = false;
        Debug.Log($"[PUERTA] Desbloqueada puerta hacia: {nombreDestino}");
    }

    public void Bloquear(string motivo)
    {
        bloqueada = true;
        razonBloqueo = motivo;
    }

    // Implementación de IInteractable
    public string ObtenerTextoInteraccion()
    {
        if (bloqueada)
            return $"[E] Puerta cerrada: {razonBloqueo}";

        return $"[E] Ir a {nombreDestino}";
    }

    public bool PuedeInteractuar()
    {
        return gameObject.activeInHierarchy;
    }

    public void Interactuar()
    {
        if (bloqueada)
        {
            Debug.Log($"Puerta bloqueada: {razonBloqueo}");
            return;
        }

        if (puntoDestino == null)
        {
            Debug.LogWarning("No se asignó punto de destino a la puerta.");
            return;
        }

        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        if (jugador != null)
        {
            // Reposicionar a Artur
            jugador.transform.position = puntoDestino.position;
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = puntoDestino.position;
                rb.linearVelocity = Vector2.zero;
            }

            // Actualizar límites de la cámara para el nuevo escenario
            CameraFollow camara = FindFirstObjectByType<CameraFollow>();
            if (camara != null)
            {
                camara.EstablecerLimites(limitesCamaraMin, limitesCamaraMax);
                camara.SeguirInmediatamente();
            }

            // Notificar estado del lugar
            if (LocationStateManager.Instancia != null)
            {
                if (LocationStateManager.Instancia.ObtenerEstado(lugarDestino) == LocationInvestigationState.NoInvestigado)
                {
                    LocationStateManager.Instancia.EstablecerEstado(lugarDestino, LocationInvestigationState.InvestigadoParcialmente);
                }
            }

            // Notificar al gestor de misiones
            Phase2MissionManager manager = FindFirstObjectByType<Phase2MissionManager>();
            if (manager != null)
            {
                manager.NotificarCambioLugar(lugarDestino);
            }
        }
    }
}


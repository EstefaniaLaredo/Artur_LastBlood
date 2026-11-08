using UnityEngine;

public class MissionProgression : MonoBehaviour
{
    [SerializeField] private EvidenceInteractable pistaRegreso;
    private ObjectiveHUD hud;
    private Vector3 oficina;
    private bool tienePrimeraPista;
    private bool enemigoDerrotado;
    private bool regresoCompletado;

    public void ConfigurarPistaRegreso(EvidenceInteractable nuevaPista)
    {
        pistaRegreso = nuevaPista;
    }

    private void Start()
    {
        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        if (jugador != null)
            oficina = jugador.transform.position;
        hud = FindFirstObjectByType<ObjectiveHUD>();
        ActualizarObjetivo("Investiga el documento que dejó Mila.");
    }

    private void Update()
    {
        if (!tienePrimeraPista || !enemigoDerrotado || regresoCompletado)
            return;

        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        if (jugador != null && Vector2.Distance(jugador.transform.position, oficina) < 1.8f)
        {
            regresoCompletado = true;
            if (pistaRegreso != null)
                pistaRegreso.gameObject.SetActive(true);
            ActualizarObjetivo("Regresa a la oficina y examina la nueva evidencia.");
        }
    }

    public void NotificarPista(string titulo)
    {
        if (!tienePrimeraPista)
        {
            tienePrimeraPista = true;
            ActualizarObjetivo("Derrota al guardia que protege la salida.");
        }
    }

    public void NotificarEnemigoDerrotado()
    {
        enemigoDerrotado = true;
        ActualizarObjetivo("Regresa a la oficina para comprobar si apareció algo nuevo.");
    }

    private void ActualizarObjetivo(string texto)
    {
        if (hud != null)
            hud.EstablecerObjetivo(texto);
        EvidenceJournal diario = FindFirstObjectByType<EvidenceJournal>();
        if (diario != null)
            diario.EstablecerObjetivo(texto);
    }
}

using UnityEngine;

// Representa una pista o documento interactuable en el escenario.
public class EvidenceInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string titulo = "Una pista sobre Mila";
    [SerializeField] private string descripcion = "Un documento relacionado con la muerte de Mila Jones.";
    [SerializeField] private string lugar = "Oficina de Artur";
    [SerializeField] private string fecha = "Noche del 14 de octubre";

    private bool recogida = false;

    public string Titulo => titulo;
    public string Descripcion => descripcion;
    public string Lugar => lugar;
    public string Fecha => fecha;
    public bool Recogida => recogida;

    public void Configurar(string nuevoTitulo, string nuevaDescripcion)
    {
        titulo = nuevoTitulo;
        descripcion = nuevaDescripcion;
    }

    public void ConfigurarCompleto(string nuevoTitulo, string nuevaDescripcion, string nuevoLugar, string nuevaFecha)
    {
        titulo = nuevoTitulo;
        descripcion = nuevaDescripcion;
        lugar = nuevoLugar;
        fecha = nuevaFecha;
    }

    // Interfaz IInteractable
    public string ObtenerTextoInteraccion()
    {
        return "[E] Examinar: " + titulo;
    }

    public bool PuedeInteractuar()
    {
        return !recogida && gameObject.activeInHierarchy;
    }

    public void Interactuar()
    {
        if (recogida)
            return;

        recogida = true;

        EvidenceJournal diario = FindFirstObjectByType<EvidenceJournal>();
        if (diario != null)
            diario.AgregarPista(titulo, descripcion, lugar, fecha);

        // Notificación para la Fase 1 si existe
        MissionProgression progresion = FindFirstObjectByType<MissionProgression>();
        if (progresion != null)
            progresion.NotificarPista(titulo);

        // Notificación para la Fase 2 si existe
        Phase2MissionManager misionFase2 = FindFirstObjectByType<Phase2MissionManager>();
        if (misionFase2 != null)
            misionFase2.NotificarPistaRecogida(titulo);

        Debug.Log("PISTA ENCONTRADA: " + titulo);
        gameObject.SetActive(false);
    }
}

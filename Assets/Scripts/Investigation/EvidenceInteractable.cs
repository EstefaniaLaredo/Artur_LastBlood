
using UnityEngine;

public class EvidenceInteractable : MonoBehaviour
{
    [SerializeField] private string titulo = "Una pista sobre Mila";
    [SerializeField] private string descripcion = "Un documento relacionado con la muerte de Mila Jones.";
    [SerializeField] private string lugar = "Oficina de Artur";
    [SerializeField] private string fecha = "Noche del 14 de octubre";

    private bool recogida = false;

    public void Interactuar()
    {
        if (recogida)
            return;

        recogida = true;

        EvidenceJournal diario =
            FindFirstObjectByType<EvidenceJournal>();

        if (diario != null)
            diario.AgregarPista(titulo, descripcion, lugar, fecha);

        Debug.Log("PISTA ENCONTRADA: " + titulo);
        gameObject.SetActive(false);
    }
}

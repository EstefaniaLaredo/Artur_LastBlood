
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float radioInteraccion = 1.35f;
    private InteractionPrompt prompt;

    private void Awake()
    {
        prompt = FindFirstObjectByType<InteractionPrompt>();
    }

    private void Update()
    {
        EvidenceInteractable cercana = ObtenerPistaCercana();
        if (prompt != null)
            prompt.Mostrar(cercana != null);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (cercana != null)
                cercana.Interactuar();
        }
    }

    private EvidenceInteractable ObtenerPistaCercana()
    {
        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, radioInteraccion);
        EvidenceInteractable mejor = null;
        float distancia = float.MaxValue;
        foreach (Collider2D objeto in objetos)
        {
            EvidenceInteractable pista = objeto.GetComponent<EvidenceInteractable>();
            if (pista == null || !pista.isActiveAndEnabled)
                continue;
            float nuevaDistancia = (objeto.transform.position - transform.position).sqrMagnitude;
            if (nuevaDistancia < distancia)
            {
                distancia = nuevaDistancia;
                mejor = pista;
            }
        }
        return mejor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}

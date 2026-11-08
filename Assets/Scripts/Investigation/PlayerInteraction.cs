using UnityEngine;

// Detecta objetos y personajes interactuables cercanos y ejecuta su acción con la tecla E.
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float radioInteraccion = 1.4f;
    private InteractionPrompt prompt;

    private void Awake()
    {
        prompt = FindFirstObjectByType<InteractionPrompt>();
    }

    private void Update()
    {
        // Si el diario o un diálogo están abiertos, no interactuar con el entorno
        if (DialogueUI.EstaEnDialogo)
        {
            if (prompt != null)
                prompt.Mostrar(false);
            return;
        }

        IInteractable interactuable = ObtenerInteractuableCercano();

        if (prompt != null)
        {
            if (interactuable != null)
                prompt.Mostrar(true, interactuable.ObtenerTextoInteraccion());
            else
                prompt.Mostrar(false);
        }

        if (Input.GetKeyDown(KeyCode.E) && interactuable != null)
        {
            interactuable.Interactuar();
        }
    }

    private IInteractable ObtenerInteractuableCercano()
    {
        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, radioInteraccion);
        IInteractable mejor = null;
        float distanciaMinima = float.MaxValue;

        foreach (Collider2D objeto in objetos)
        {
            IInteractable interactuable = objeto.GetComponent<IInteractable>();
            if (interactuable == null || !interactuable.PuedeInteractuar())
                continue;

            float distanciaActual = (objeto.transform.position - transform.position).sqrMagnitude;
            if (distanciaActual < distanciaMinima)
            {
                distanciaMinima = distanciaActual;
                mejor = interactuable;
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

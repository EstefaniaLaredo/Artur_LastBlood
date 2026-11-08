using System;
using UnityEngine;

// Personaje no jugable con diálogo interactuable reutilizable.
public class DialogueNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string nombrePersonaje = "Informante";
    [SerializeField] private string rol = "Contacto";
    [SerializeField] private DialogueLine[] dialogoPrincipal;
    [SerializeField] private DialogueLine[] dialogoPosterior;

    private bool haHablado = false;
    public event Action OnDialogoCompletado;

    public string Nombre => nombrePersonaje;
    public string Rol => rol;

    public void Configurar(string nombre, string descripcionRol, DialogueLine[] inicial, DialogueLine[] posterior = null)
    {
        nombrePersonaje = nombre;
        rol = descripcionRol;
        dialogoPrincipal = inicial;
        dialogoPosterior = posterior ?? inicial;
    }

    public string ObtenerTextoInteraccion()
    {
        return $"[E] Hablar con {nombrePersonaje} ({rol})";
    }

    public bool PuedeInteractuar()
    {
        return !DialogueUI.EstaEnDialogo && gameObject.activeInHierarchy;
    }

    public void Interactuar()
    {
        if (DialogueUI.Instancia == null)
        {
            Debug.LogWarning("No se encontró DialogueUI en la escena.");
            return;
        }

        DialogueLine[] lineasParaUsar = haHablado ? dialogoPosterior : dialogoPrincipal;

        DialogueUI.Instancia.IniciarDialogo(lineasParaUsar, () =>
        {
            if (!haHablado)
            {
                haHablado = true;
                OnDialogoCompletado?.Invoke();

                // Notificar a la progresión de Fase 2 si existe
                Phase2MissionManager manager = FindFirstObjectByType<Phase2MissionManager>();
                if (manager != null)
                {
                    manager.NotificarDialogoNPC(nombrePersonaje);
                }
            }
        });
    }
}


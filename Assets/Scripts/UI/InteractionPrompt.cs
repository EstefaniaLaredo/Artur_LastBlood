using TMPro;
using UnityEngine;

// Muestra el indicador interactivo en el centro de la pantalla cuando Artur se acerca a un objeto o personaje.
public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text texto;
    [SerializeField] private GameObject fondo;

    public void Configurar(TMP_Text nuevoTexto, GameObject nuevoFondo = null)
    {
        texto = nuevoTexto;
        fondo = nuevoFondo;
        Mostrar(false);
    }

    // Muestra u oculta el indicador con el texto correspondiente
    public void Mostrar(bool visible, string mensaje = "[E] Examinar")
    {
        if (texto != null)
        {
            texto.text = mensaje;
            texto.gameObject.SetActive(visible);
        }

        if (fondo != null)
        {
            fondo.SetActive(visible);
        }
    }
}

using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text texto;

    public void Configurar(TMP_Text nuevoTexto)
    {
        texto = nuevoTexto;
        Mostrar(false);
    }

    public void Mostrar(bool visible)
    {
        if (texto != null)
            texto.gameObject.SetActive(visible);
    }
}

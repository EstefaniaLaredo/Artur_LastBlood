using TMPro;
using UnityEngine;

public class ObjectiveHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text objetivoText;
    [SerializeField] private TMP_Text ayudaText;

    public void Configurar(TMP_Text objetivo, TMP_Text ayuda)
    {
        objetivoText = objetivo;
        ayudaText = ayuda;
        EstablecerObjetivo("Regresa a tu oficina y busca una respuesta.");
    }

    public void EstablecerObjetivo(string objetivo)
    {
        if (objetivoText != null)
            objetivoText.text = "OBJETIVO\n" + objetivo;
        if (ayudaText != null)
            ayudaText.text = "WASD mover   E examinar   J diario   ESC pausa";
    }
}

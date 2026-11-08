using TMPro;
using UnityEngine;

// Muestra la barra de salud de Artur en la esquina superior de la interfaz.
public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text textoSalud;
    private Health saludJugador;

    public void Configurar(TMP_Text nuevoTexto)
    {
        textoSalud = nuevoTexto;
        ConectarConJugador();
    }

    private void Start()
    {
        ConectarConJugador();
    }

    private void ConectarConJugador()
    {
        if (saludJugador != null)
            return;

        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        if (jugador != null)
        {
            saludJugador = jugador.GetComponent<Health>();
            if (saludJugador != null)
            {
                saludJugador.OnVidaCambiada += ActualizarTexto;
                ActualizarTexto(saludJugador.VidaActual, saludJugador.VidaMaxima);
            }
        }
    }

    private void OnDestroy()
    {
        if (saludJugador != null)
            saludJugador.OnVidaCambiada -= ActualizarTexto;
    }

    private void ActualizarTexto(int actual, int maximo)
    {
        if (textoSalud == null)
            return;

        string bloques = "";
        for (int i = 0; i < maximo; i++)
        {
            if (i < actual)
                bloques += "<color=#E53935>■</color> ";
            else
                bloques += "<color=#555555>□</color> ";
        }

        textoSalud.text = $"ARTUR  {bloques} ({actual}/{maximo})";
    }
}


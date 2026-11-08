using System;
using UnityEngine;

// Estructura de una línea de diálogo con personaje y texto.
[Serializable]
public class DialogueLine
{
    public string personaje;
    [TextArea(2, 4)] public string texto;

    public DialogueLine(string personaje, string texto)
    {
        this.personaje = personaje;
        this.texto = texto;
    }
}


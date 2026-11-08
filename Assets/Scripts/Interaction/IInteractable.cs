using UnityEngine;

// Interfaz universal para objetos interactuables del juego (pistas, puertas, personajes).
public interface IInteractable
{
    // Devuelve el texto para mostrar en el HUD, por ejemplo: "[E] Examinar pista" o "[E] Hablar con Informante"
    string ObtenerTextoInteraccion();

    // Ejecuta la lógica de la interacción
    void Interactuar();

    // Determina si actualmente se puede interactuar con el objeto
    bool PuedeInteractuar();
}


using UnityEngine;

// Inicializador de tiempo de ejecución para la Fase 2: enlaza jugador y cámara en la calle de la ciudad.
public class Phase2Runtime : MonoBehaviour
{
    private void Awake()
    {
        PlayerMovement movimiento = FindAnyObjectByType<PlayerMovement>();
        CameraFollow camara = FindAnyObjectByType<CameraFollow>();

        if (movimiento != null && camara != null)
        {
            // Límites iniciales correspondientes a la Calle Principal de la ciudad (permite ver las fachadas norte)
            camara.Configurar(movimiento.transform, new Vector2(-36f, -38f), new Vector2(36f, -10f));
            camara.SeguirInmediatamente();
        }
    }
}

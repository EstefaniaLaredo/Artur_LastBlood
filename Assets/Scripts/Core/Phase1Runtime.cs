using UnityEngine;

public class Phase1Runtime : MonoBehaviour
{
    private void Awake()
    {
        PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
        CameraFollow camara = FindFirstObjectByType<CameraFollow>();
        if (movimiento != null && camara != null)
            camara.Configurar(movimiento.transform, new Vector2(-20f, -12f), new Vector2(20f, 12f));

        EvidenceJournal diario = FindFirstObjectByType<EvidenceJournal>();
        ObjectiveHUD hud = FindFirstObjectByType<ObjectiveHUD>();
        if (diario != null)
            diario.EstablecerObjetivo("Regresa a tu oficina y busca una respuesta.");
        if (hud != null)
            hud.EstablecerObjetivo("Regresa a tu oficina y busca una respuesta.");
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

// Administra el estado de investigación de los escenarios del juego y actualiza el diario.
public class LocationStateManager : MonoBehaviour
{
    public static LocationStateManager Instancia { get; private set; }

    private readonly Dictionary<LocationID, LocationInvestigationState> estados =
        new Dictionary<LocationID, LocationInvestigationState>();

    public event Action<LocationID, LocationInvestigationState> OnEstadoLugarCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        // Estados iniciales del mundo
        estados[LocationID.Calle] = LocationInvestigationState.InvestigadoParcialmente;
        estados[LocationID.Oficina] = LocationInvestigationState.InvestigadoParcialmente;
        estados[LocationID.Almacen] = LocationInvestigationState.NoInvestigado;
        estados[LocationID.ClubNocturno] = LocationInvestigationState.NoInvestigado;
    }

    public void EstablecerEstado(LocationID lugar, LocationInvestigationState nuevoEstado)
    {
        estados[lugar] = nuevoEstado;
        Debug.Log($"[MUNDO] Estado de {ObtenerNombreLugar(lugar)} cambiado a: {ObtenerNombreEstado(nuevoEstado)}");
        OnEstadoLugarCambiado?.Invoke(lugar, nuevoEstado);

        // Actualizar el diario automáticamente si existe
        EvidenceJournal diario = FindAnyObjectByType<EvidenceJournal>();
        if (diario != null)
            diario.RefrescarVista();
    }

    public LocationInvestigationState ObtenerEstado(LocationID lugar)
    {
        if (estados.TryGetValue(lugar, out LocationInvestigationState estado))
            return estado;
        return LocationInvestigationState.NoInvestigado;
    }

    public static string ObtenerNombreLugar(LocationID lugar)
    {
        switch (lugar)
        {
            case LocationID.Calle: return "Calles de la Ciudad (Distrito Central)";
            case LocationID.Oficina: return "Oficina de Artur (Base)";
            case LocationID.Almacen: return "Almacén Industrial";
            case LocationID.ClubNocturno: return "Club Nocturno Vortex";
            default: return lugar.ToString();
        }
    }

    public static string ObtenerNombreEstado(LocationInvestigationState estado)
    {
        switch (estado)
        {
            case LocationInvestigationState.NoInvestigado:
                return "<color=#888888>No investigado</color>";
            case LocationInvestigationState.InvestigadoParcialmente:
                return "<color=#FFB74D>Investigado parcialmente</color>";
            case LocationInvestigationState.NuevaInformacion:
                return "<color=#4FC3F7>¡Nueva información disponible!</color>";
            case LocationInvestigationState.InvestigacionCompletada:
                return "<color=#81C784>Investigación completada</color>";
            default:
                return estado.ToString();
        }
    }

    public string GenerarTextoResumenLugares()
    {
        string texto = "<b><color=#4FC3F7>ESTADO DE LOS ESCENARIOS</color></b>\n";
        foreach (LocationID lugar in (LocationID[])Enum.GetValues(typeof(LocationID)))
        {
            texto += $"• {ObtenerNombreLugar(lugar)}: {ObtenerNombreEstado(ObtenerEstado(lugar))}\n";
        }
        return texto;
    }
}

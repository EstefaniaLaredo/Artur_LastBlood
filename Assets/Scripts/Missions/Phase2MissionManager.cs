using UnityEngine;

// Coordina la progresión narrativa y los objetivos encadenados de la ciudad y los 3 escenarios.
public class Phase2MissionManager : MonoBehaviour
{
    public enum EtapaMision
    {
        Paso1_InicioCalle,
        Paso2_InvestigarOficina,
        Paso3_SalirCalleHaciaAlmacen,
        Paso4_HablarInformante,
        Paso5_InvestigarAlmacen,
        Paso6_IrClubNocturno,
        Paso7_HablarValeria,
        Paso8_RegresarOficina,
        Paso9_InvestigarSecretoOficina,
        Fase2_Completada
    }

    [Header("Estado Actual")]
    [SerializeField] private EtapaMision etapaActual = EtapaMision.Paso1_InicioCalle;

    [Header("Referencias de Puertas")]
    [SerializeField] private DoorTransition puertaCalleAOficina;
    [SerializeField] private DoorTransition puertaCalleAAlmacen;
    [SerializeField] private DoorTransition puertaCalleAClub;

    [Header("Referencias de Pistas")]
    [SerializeField] private EvidenceInteractable pistaOficina;
    [SerializeField] private EvidenceInteractable pistaAlmacen;
    [SerializeField] private EvidenceInteractable pistaSecretaOficina;
    [SerializeField] private GameObject habitacionSecretaVisual;

    private ObjectiveHUD hud;
    private EvidenceJournal diario;

    public EtapaMision Etapa => etapaActual;

    public void Configurar(
        DoorTransition puertaOficina,
        DoorTransition puertaAlmacen,
        DoorTransition puertaClub,
        EvidenceInteractable pistaOfi,
        EvidenceInteractable pistaAlm,
        EvidenceInteractable pistaSec,
        GameObject secretoVisual = null)
    {
        puertaCalleAOficina = puertaOficina;
        puertaCalleAAlmacen = puertaAlmacen;
        puertaCalleAClub = puertaClub;
        pistaOficina = pistaOfi;
        pistaAlmacen = pistaAlm;
        pistaSecretaOficina = pistaSec;
        habitacionSecretaVisual = secretoVisual;

        if (pistaSecretaOficina != null)
            pistaSecretaOficina.gameObject.SetActive(false);
        if (habitacionSecretaVisual != null)
            habitacionSecretaVisual.SetActive(false);
    }

    private void Start()
    {
        hud = FindAnyObjectByType<ObjectiveHUD>();
        diario = FindAnyObjectByType<EvidenceJournal>();

        AvanzarEtapa(EtapaMision.Paso1_InicioCalle);
    }

    public void AvanzarEtapa(EtapaMision nuevaEtapa)
    {
        etapaActual = nuevaEtapa;
        string textoObjetivo = "";

        switch (etapaActual)
        {
            case EtapaMision.Paso1_InicioCalle:
                textoObjetivo = "Estás en la calle. Dirígete a la entrada de tu oficina (acera oeste) para revisar el caso.";
                if (puertaCalleAAlmacen != null)
                    puertaCalleAAlmacen.Bloquear("Zona restringida por la Organización. Necesitas una pista primero.");
                if (puertaCalleAClub != null)
                    puertaCalleAClub.Bloquear("Entrada exclusiva VIP. El portero no te dejará pasar sin credencial.");
                break;

            case EtapaMision.Paso2_InvestigarOficina:
                textoObjetivo = "Entraste a tu oficina. Investiga tu despacho y examina el informe confidencial sobre Mila.";
                break;

            case EtapaMision.Paso3_SalirCalleHaciaAlmacen:
                textoObjetivo = "La pista apunta al muelle. Sal a la calle y entra al Almacén Industrial.";
                if (puertaCalleAAlmacen != null)
                    puertaCalleAAlmacen.Desbloquear();
                break;

            case EtapaMision.Paso4_HablarInformante:
                textoObjetivo = "Encuentra y habla con 'El Cuervo' (Informante) en los pasillos del almacén.";
                break;

            case EtapaMision.Paso5_InvestigarAlmacen:
                textoObjetivo = "Neutraliza a los guardias del almacén y abre el contenedor con el Pase VIP.";
                break;

            case EtapaMision.Paso6_IrClubNocturno:
                textoObjetivo = "Has obtenido el Pase VIP. Sal a la calle y dirígete a la entrada del Club Nocturno Vortex.";
                if (puertaCalleAClub != null)
                    puertaCalleAClub.Desbloquear();
                break;

            case EtapaMision.Paso7_HablarValeria:
                textoObjetivo = "Entraste al club. Localiza a Valeria (Antiguo contacto) en el reservado VIP.";
                break;

            case EtapaMision.Paso8_RegresarOficina:
                textoObjetivo = "Valeria reveló que buscaban algo en tu oficina. Regresa por la calle a tu base.";
                if (LocationStateManager.Instancia != null)
                    LocationStateManager.Instancia.EstablecerEstado(LocationID.Oficina, LocationInvestigationState.NuevaInformacion);

                if (pistaSecretaOficina != null)
                    pistaSecretaOficina.gameObject.SetActive(true);
                if (habitacionSecretaVisual != null)
                    habitacionSecretaVisual.SetActive(true);
                break;

            case EtapaMision.Paso9_InvestigarSecretoOficina:
                textoObjetivo = "Examina el compartimento secreto descubierto en la pared de tu despacho.";
                break;

            case EtapaMision.Fase2_Completada:
                textoObjetivo = "FASE 2 COMPLETADA: Has recorrido la ciudad, desarticulado a los vigías y recuperado la carta de Mila.";
                if (LocationStateManager.Instancia != null)
                {
                    LocationStateManager.Instancia.EstablecerEstado(LocationID.Calle, LocationInvestigationState.InvestigacionCompletada);
                    LocationStateManager.Instancia.EstablecerEstado(LocationID.Oficina, LocationInvestigationState.InvestigacionCompletada);
                    LocationStateManager.Instancia.EstablecerEstado(LocationID.Almacen, LocationInvestigationState.InvestigacionCompletada);
                    LocationStateManager.Instancia.EstablecerEstado(LocationID.ClubNocturno, LocationInvestigationState.InvestigacionCompletada);
                }
                break;
        }

        ActualizarTextoObjetivo(textoObjetivo);
    }

    public void NotificarPistaRecogida(string tituloPista)
    {
        Debug.Log($"[MISION] Pista recogida: {tituloPista} durante etapa {etapaActual}");

        if (etapaActual == EtapaMision.Paso1_InicioCalle || etapaActual == EtapaMision.Paso2_InvestigarOficina)
        {
            if (tituloPista.Contains("Informe"))
            {
                AvanzarEtapa(EtapaMision.Paso3_SalirCalleHaciaAlmacen);
            }
        }
        else if (etapaActual == EtapaMision.Paso5_InvestigarAlmacen)
        {
            if (tituloPista.Contains("Pase VIP") || tituloPista.Contains("Manifiesto"))
            {
                AvanzarEtapa(EtapaMision.Paso6_IrClubNocturno);
            }
        }
        else if (etapaActual == EtapaMision.Paso8_RegresarOficina || etapaActual == EtapaMision.Paso9_InvestigarSecretoOficina)
        {
            if (tituloPista.Contains("carta de Mila"))
            {
                AvanzarEtapa(EtapaMision.Fase2_Completada);
            }
        }
    }

    public void NotificarDialogoNPC(string nombrePersonaje)
    {
        Debug.Log($"[MISION] Diálogo completado con {nombrePersonaje} en etapa {etapaActual}");

        if (etapaActual == EtapaMision.Paso4_HablarInformante && nombrePersonaje.Contains("Cuervo"))
        {
            if (diario != null)
                diario.AgregarContacto("El Cuervo: Informante del bajo mundo. Afirmó que el envío de armas contenía pases de la Organización.");
            AvanzarEtapa(EtapaMision.Paso5_InvestigarAlmacen);
        }
        else if (etapaActual == EtapaMision.Paso7_HablarValeria && nombrePersonaje.Contains("Valeria"))
        {
            if (diario != null)
            {
                diario.AgregarContacto("Valeria: Antigua socia. Confirmó que la Organización no buscaba a Mila, sino los archivos que Artur ocultó en su propia oficina.");
                diario.AgregarPista("Grabación del Club: El Encargo", "Valeria entregó la grabación que prueba que un traidor ordenó registrar la oficina de Artur.", "Club Nocturno Vortex", "Noche del 14 de octubre");
            }

            AvanzarEtapa(EtapaMision.Paso8_RegresarOficina);
        }
    }

    public void NotificarEnemigoDerrotado()
    {
        Debug.Log($"[MISION] Guardia derrotado en etapa {etapaActual}");
        if (etapaActual == EtapaMision.Paso5_InvestigarAlmacen)
        {
            ActualizarTextoObjetivo("Guardia neutralizado. Ahora registra el contenedor de contrabando al fondo.");
        }
    }

    public void NotificarCambioLugar(LocationID nuevoLugar)
    {
        Debug.Log($"[MISION] Artur entró a {nuevoLugar} en etapa {etapaActual}");

        if (nuevoLugar == LocationID.Oficina && etapaActual == EtapaMision.Paso1_InicioCalle)
        {
            AvanzarEtapa(EtapaMision.Paso2_InvestigarOficina);
        }
        else if (nuevoLugar == LocationID.Almacen && etapaActual == EtapaMision.Paso3_SalirCalleHaciaAlmacen)
        {
            AvanzarEtapa(EtapaMision.Paso4_HablarInformante);
        }
        else if (nuevoLugar == LocationID.ClubNocturno && etapaActual == EtapaMision.Paso6_IrClubNocturno)
        {
            AvanzarEtapa(EtapaMision.Paso7_HablarValeria);
        }
        else if (nuevoLugar == LocationID.Oficina && etapaActual == EtapaMision.Paso8_RegresarOficina)
        {
            AvanzarEtapa(EtapaMision.Paso9_InvestigarSecretoOficina);
        }
    }

    private void ActualizarTextoObjetivo(string texto)
    {
        if (hud != null)
            hud.EstablecerObjetivo(texto);
        if (diario != null)
            diario.EstablecerObjetivo(texto);
    }
}

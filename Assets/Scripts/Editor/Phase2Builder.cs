using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builder automático para la Fase 2 corregida:
// 1. Alineación directa de pasos peatonales con las puertas de entrada.
// 2. Eliminación de farolas distorsionadas (los dos conos alargados de la calle).
// 3. Pasillos interiores 100% diáfanos sin muros bloqueantes.
// 4. Todas las pistas y puertas con isTrigger = true (cero barreras invisibles).
// 5. Paredes con color claro visible de contraste para no confundirlas con el suelo.
// 6. Enemigos con 4 a 6 de vida para combates fluidos estilo juego de acción.
public static class Phase2Builder
{
    private const string RootName = "ARTUR_Fase2_Generado";
    private static readonly Vector2 DesplazamientoAlmacen = new Vector2(0f, 40f);
    private static readonly Vector2 DesplazamientoClub = new Vector2(0f, 80f);

    // Separa los edificios en el espacio (antes se solapaban). Las puertas teletransportan, así que da igual la distancia.
    private static void MoverEdificio(Transform root, string nombre, Vector2 desplazamiento)
    {
        Transform edificio = root.Find(nombre);
        if (edificio != null)
            edificio.position += new Vector3(desplazamiento.x, desplazamiento.y, 0f);
    }

    [MenuItem("ARTUR/Construir Fase 2")]
    public static void Construir()
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Limpieza previa para garantizar idempotencia
        GameObject rootViejo = GameObject.Find(RootName);
        if (rootViejo != null)
            Object.DestroyImmediate(rootViejo);

        GameObject root = new GameObject(RootName);
        root.AddComponent<Phase2Runtime>();

        // Gestores centrales
        LocationStateManager stateManager = root.AddComponent<LocationStateManager>();
        Phase2MissionManager missionManager = root.AddComponent<Phase2MissionManager>();

        // Jugador inicia en la Calle directamente frente al paso de peatones de su oficina
        Vector3 spawnCalle = new Vector3(-20f, -27f, 0f);
        GameObject jugador = CrearJugador(root.transform, spawnCalle);
        CrearCamara(root.transform, jugador.transform);

        // UI completa con CanvasScaler responsive y Diario centrado sin desbordamientos
        CrearInterfaz(root.transform);

        // 1. EL HUB URBANO: La Calle Principal
        DoorTransition puertaCalleAOficina;
        DoorTransition puertaCalleAAlmacen;
        DoorTransition puertaCalleAClub;
        ConstruirCallePrincipal(root.transform, out puertaCalleAOficina, out puertaCalleAAlmacen, out puertaCalleAClub);

        // 2. EDIFICIO 1: Oficina de Artur (Acceso directo diáfano al despacho y pista en el suelo)
        EvidenceInteractable pistaOficina;
        EvidenceInteractable pistaSecreta;
        GameObject cajonSecretoVisual;
        DoorTransition puertaOficinaACalle;
        ConstruirOficina(root.transform, out pistaOficina, out pistaSecreta, out cajonSecretoVisual, out puertaOficinaACalle);

        // 3. EDIFICIO 2: Almacén Industrial (Pasillos anchos, 4 guardias, El Cuervo y contenedor VIP)
        EvidenceInteractable pistaAlmacen;
        DoorTransition puertaAlmacenACalle;
        DialogueNPC informante;
        ConstruirAlmacen(root.transform, out pistaAlmacen, out puertaAlmacenACalle, out informante);

        // 4. EDIFICIO 3: Club Nocturno Vortex (Pista neón, barra y reservado con Valeria)
        DoorTransition puertaClubACalle;
        DialogueNPC valeria;
        ConstruirClubNocturno(root.transform, out puertaClubACalle, out valeria);

        MoverEdificio(root.transform, "Edificio_Almacen_Industrial", DesplazamientoAlmacen);
        MoverEdificio(root.transform, "Edificio_Club_Nocturno", DesplazamientoClub);
        // Conectar puertas bidireccionales
        ConectarPuertas(
            puertaCalleAOficina, puertaOficinaACalle,
            puertaCalleAAlmacen, puertaAlmacenACalle,
            puertaCalleAClub, puertaClubACalle
        );

        // Configurar gestor de misiones
        missionManager.Configurar(
            puertaCalleAOficina,
            puertaCalleAAlmacen,
            puertaCalleAClub,
            pistaOficina,
            pistaAlmacen,
            pistaSecreta,
            cajonSecretoVisual
        );

        EditorSceneManager.SaveScene(escena, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("ARTUR: Fase 2 (Calles despejadas y arquitectura sin barreras) construida con éxito.");
    }

    private static GameObject CrearJugador(Transform parent, Vector3 posicionInicial)
    {
        GameObject jugador = new GameObject("Artur");
        jugador.transform.SetParent(parent);
        jugador.transform.position = posicionInicial;

        Rigidbody2D rb = jugador.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = jugador.AddComponent<CircleCollider2D>();
        collider.radius = .42f;

        SolidSprite sprite = jugador.AddComponent<SolidSprite>();
        // Azul oscuro de la gabardina de Artur
        sprite.Configurar(new Color(.12f, .32f, .52f), new Vector2(1.15f, 1.15f), true, 10);

        jugador.AddComponent<PlayerMovement>();
        jugador.AddComponent<PlayerInteraction>();
        jugador.AddComponent<PlayerCombat>();
        Health salud = jugador.AddComponent<Health>();
        salud.Configurar(3);

        return jugador;
    }

    private static void CrearCamara(Transform parent, Transform jugador)
    {
        GameObject camaraObj = new GameObject("Camara_Principal");
        camaraObj.transform.SetParent(parent);
        camaraObj.transform.position = new Vector3(jugador.position.x, jugador.position.y, -10f);

        Camera cam = camaraObj.AddComponent<Camera>();
        camaraObj.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.015f, .018f, .025f);

        CameraFollow seguimiento = camaraObj.AddComponent<CameraFollow>();
        // Límites amplios de la Calle para ver tanto las aceras como las fachadas norte
        seguimiento.Configurar(jugador, new Vector2(-36f, -38f), new Vector2(36f, -10f));
        camaraObj.AddComponent<AudioListener>();
    }

    // =========================================================================
    // 1. EL HUB URBANO: LA CALLE PRINCIPAL (x: -36 a 36, y: -38 a -14)
    // =========================================================================
    private static void ConstruirCallePrincipal(
        Transform parent,
        out DoorTransition puertaAOficina,
        out DoorTransition puertaAAlmacen,
        out DoorTransition puertaAClub)
    {
        GameObject calle = new GameObject("Escenario_CallePrincipal");
        calle.transform.SetParent(parent);

        // Calzada central de asfalto oscuro
        GameObject asfalto = new GameObject("Asfalto_Calle");
        asfalto.transform.SetParent(calle.transform);
        asfalto.transform.position = new Vector3(0f, -27f, 0f);
        SolidSprite spriteAsfalto = asfalto.AddComponent<SolidSprite>();
        spriteAsfalto.Configurar(new Color(.045f, .05f, .06f), new Vector2(74f, 22f), false, -10);

        // Acera Norte
        GameObject aceraNorte = new GameObject("Acera_Norte");
        aceraNorte.transform.SetParent(calle.transform);
        aceraNorte.transform.position = new Vector3(0f, -18f, 0f);
        SolidSprite spriteAceraNorte = aceraNorte.AddComponent<SolidSprite>();
        spriteAceraNorte.Configurar(new Color(.12f, .13f, .16f), new Vector2(74f, 4f), false, -9);

        // Acera Sur
        GameObject aceraSur = new GameObject("Acera_Sur");
        aceraSur.transform.SetParent(calle.transform);
        aceraSur.transform.position = new Vector3(0f, -36f, 0f);
        SolidSprite spriteAceraSur = aceraSur.AddComponent<SolidSprite>();
        spriteAceraSur.Configurar(new Color(.12f, .13f, .16f), new Vector2(74f, 4f), false, -9);

        // Muros perimetrales
        CrearMuro(calle.transform, "Muro_Calle_Sur", new Vector2(0f, -38.2f), new Vector2(74f, .6f));
        CrearMuro(calle.transform, "Muro_Calle_Este", new Vector2(37.2f, -27f), new Vector2(.6f, 23f));
        CrearMuro(calle.transform, "Muro_Calle_Oeste", new Vector2(-37.2f, -27f), new Vector2(.6f, 23f));

        // Fachada Norte con huecos de 5m de paso libre exactamente frente a cada puerta
        // Puerta Oficina: x = -20 (Hueco de -22.5 a -17.5)
        CrearMuro(calle.transform, "Fachada_1", new Vector2(-29.5f, -16f), new Vector2(14f, .6f));
        // Puerta Almacén: x = 2 (Hueco de -0.5 a 4.5)
        CrearMuro(calle.transform, "Fachada_2", new Vector2(-9f, -16f), new Vector2(17f, .6f));
        // Puerta Club: x = 24 (Hueco de 21.5 a 26.5)
        CrearMuro(calle.transform, "Fachada_3", new Vector2(13f, -16f), new Vector2(17f, .6f));
        CrearMuro(calle.transform, "Fachada_4", new Vector2(31.5f, -16f), new Vector2(10f, .6f));

        // Pasos de peatones alineados exactamente con cada una de las 3 puertas
        CrearPasoPeatones(calle.transform, new Vector2(-20f, -25f)); // Lleva directo a la Oficina
        CrearPasoPeatones(calle.transform, new Vector2(2f, -25f));   // Lleva directo al Almacén
        CrearPasoPeatones(calle.transform, new Vector2(24f, -25f));  // Lleva directo al Club
        for (int i = -7; i <= 7; i++) if (i != -4 && i != 0 && i != 5) CrearDecor(calle.transform, "Linea_Carril_" + i, new Vector2(i * 4.6f, -28.5f), new Vector2(2.4f, 0.22f), new Color(0.90f, 0.72f, 0.18f, 0.65f), -8);
        CrearDecor(calle.transform, "Charco_Reflejo_Oficina", new Vector2(-19.5f, -20.2f), new Vector2(3.6f, 1.4f), new Color(0.25f, 0.55f, 0.95f, 0.18f), -8, true);
        CrearDecor(calle.transform, "Charco_Reflejo_Almacen", new Vector2(2.5f, -20.2f), new Vector2(3.6f, 1.4f), new Color(0.95f, 0.70f, 0.25f, 0.18f), -8, true);
        CrearDecor(calle.transform, "Charco_Reflejo_Club", new Vector2(24.2f, -20.2f), new Vector2(4.2f, 1.6f), new Color(0.95f, 0.20f, 0.80f, 0.22f), -8, true);

        // Farolas cilíndricas limpias pegadas a la acera sur para NO estorbar el paso norte
        CrearFarola(calle.transform, new Vector2(-30f, -35f));
        CrearFarola(calle.transform, new Vector2(-10f, -35f));
        CrearFarola(calle.transform, new Vector2(12f, -35f));
        CrearFarola(calle.transform, new Vector2(30f, -35f));

        // Pista 1 en la calle (Cartel pegado a la pared exterior oeste - TRIGGER)
        CrearPista(
            calle.transform,
            "Pista_Calle_Aviso",
            new Vector3(-13f, -18f, 0f),
            "Aviso de búsqueda clandestino",
            "Un cartel mojado por la lluvia: 'Se busca a Artur Jones (Vivo o muerto). Recompensa al contactar a la gente del muelle'.",
            "Calle Central de la Ciudad",
            "Hace pocas horas",
            new Color(.92f, .82f, .35f)
        );

        // Enemigos de la calle con 4 de vida
        CrearEnemigo(calle.transform, "Maton_Calle_Oeste", new Vector3(-8f, -27f, 0f), 4, 2.2f, 1, new Color(.65f, .16f, .16f));
        CrearEnemigo(calle.transform, "Maton_Calle_Este", new Vector3(14f, -27f, 0f), 4, 2.2f, 1, new Color(.65f, .16f, .16f));

        // Rótulo luminoso y Puerta 1: Calle -> Edificio de Artur
        CrearRotuloPuerta(calle.transform, "Rotulo_Oficina", new Vector2(-20f, -14.8f), "OFICINA", new Color(.30f, .55f, .85f));
        GameObject puertaOfiObj = new GameObject("Puerta_Calle_A_Oficina");
        puertaOfiObj.transform.SetParent(calle.transform);
        puertaOfiObj.transform.position = new Vector3(-20f, -16f, 0f);
        BoxCollider2D colOfi = puertaOfiObj.AddComponent<BoxCollider2D>();
        colOfi.size = new Vector2(1.5f, 1.4f);
        colOfi.isTrigger = true; // <-- TRIGGER para entrar sin chocar
        SolidSprite spriteP1 = puertaOfiObj.AddComponent<SolidSprite>();
        spriteP1.Configurar(new Color(.28f, .48f, .75f), new Vector2(2f, 1.8f), false, 3);
        puertaAOficina = puertaOfiObj.AddComponent<DoorTransition>();

        // Rótulo luminoso y Puerta 2: Calle -> Almacén Industrial
        CrearRotuloPuerta(calle.transform, "Rotulo_Almacen", new Vector2(2f, -14.8f), "ALMACÉN", new Color(.75f, .60f, .30f));
        GameObject puertaAlmObj = new GameObject("Puerta_Calle_A_Almacen");
        puertaAlmObj.transform.SetParent(calle.transform);
        puertaAlmObj.transform.position = new Vector3(2f, -16f, 0f);
        BoxCollider2D colAlm = puertaAlmObj.AddComponent<BoxCollider2D>();
        colAlm.size = new Vector2(1.5f, 1.4f);
        colAlm.isTrigger = true; // <-- TRIGGER
        SolidSprite spriteP2 = puertaAlmObj.AddComponent<SolidSprite>();
        spriteP2.Configurar(new Color(.65f, .50f, .25f), new Vector2(2f, 1.8f), false, 3);
        puertaAAlmacen = puertaAlmObj.AddComponent<DoorTransition>();

        // Rótulo luminoso y Puerta 3: Calle -> Club Nocturno Vortex
        CrearRotuloPuerta(calle.transform, "Rotulo_Club", new Vector2(24f, -14.8f), "CLUB VORTEX", new Color(.85f, .25f, .75f));
        GameObject puertaClubObj = new GameObject("Puerta_Calle_A_Club");
        puertaClubObj.transform.SetParent(calle.transform);
        puertaClubObj.transform.position = new Vector3(24f, -16f, 0f);
        BoxCollider2D colClub = puertaClubObj.AddComponent<BoxCollider2D>();
        colClub.size = new Vector2(1.5f, 1.4f);
        colClub.isTrigger = true; // <-- TRIGGER
        SolidSprite spriteP3 = puertaClubObj.AddComponent<SolidSprite>();
        spriteP3.Configurar(new Color(.85f, .20f, .70f), new Vector2(2f, 1.8f), false, 3);
        puertaAClub = puertaClubObj.AddComponent<DoorTransition>();
    }

    // =========================================================================
    // 2. EDIFICIO 1: OFICINA DE ARTUR (x: -34 a -6, y: 2 a 22)
    // =========================================================================
    private static void ConstruirOficina(
        Transform parent,
        out EvidenceInteractable pistaOficina,
        out EvidenceInteractable pistaSecreta,
        out GameObject cajonSecretoVisual,
        out DoorTransition puertaACalle)
    {
        GameObject oficina = new GameObject("Edificio_Oficina_Artur");
        oficina.transform.SetParent(parent);
        Transform t = oficina.transform;

        // ---------------------------------------------------------------------
        // SUELOS: parquet de madera oscura en todo el edificio + baldosas en recepción
        // ---------------------------------------------------------------------
        CrearDecor(t, "Suelo_Oficina", new Vector2(-20f, 12f), new Vector2(28f, 20f), new Color(.10f, .075f, .06f), -10);
        for (int i = 0; i < 28; i++)
        {
            Color tablon = (i % 2 == 0) ? new Color(.15f, .105f, .075f) : new Color(.125f, .09f, .066f);
            CrearDecor(t, "Parquet_" + i, new Vector2(-33.5f + i, 12f), new Vector2(.96f, 20f), tablon, -9);
        }
        // Recepción: baldosas en damero (y 2 a 9.5)
        for (int x = 0; x < 14; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Color baldosa = ((x + y) % 2 == 0) ? new Color(.17f, .18f, .21f) : new Color(.12f, .13f, .16f);
                CrearDecor(t, "Baldosa_" + x + "_" + y, new Vector2(-33f + x * 2f, 2.9f + y * 1.9f), new Vector2(2f, 1.9f), baldosa, -8);
            }
        }

        // ---------------------------------------------------------------------
        // MUROS: perímetro + divisoria de recepción + salas laterales.
        // TODAS las divisiones tienen aberturas amplias (5-6 m) para poder pasar siempre.
        // ---------------------------------------------------------------------
        CrearMuro(t, "Muro_Ofi_Norte", new Vector2(-20f, 22.2f), new Vector2(28f, .6f));
        CrearMuro(t, "Muro_Ofi_Oeste", new Vector2(-34.2f, 12f), new Vector2(.6f, 20f));
        CrearMuro(t, "Muro_Ofi_Este", new Vector2(-5.8f, 12f), new Vector2(.6f, 20f));
        // Sur con abertura de 5 m centrada en la puerta (x -22.5 a -17.5)
        CrearMuro(t, "Muro_Ofi_Sur_Izq", new Vector2(-28.25f, 1.8f), new Vector2(11.5f, .6f));
        CrearMuro(t, "Muro_Ofi_Sur_Der", new Vector2(-11.75f, 1.8f), new Vector2(11.5f, .6f));
        // Divisoria recepción / despachos con abertura de 6 m (x -23 a -17)
        CrearMuro(t, "Muro_Ofi_Recepcion_Izq", new Vector2(-28.5f, 9.5f), new Vector2(11f, .5f));
        CrearMuro(t, "Muro_Ofi_Recepcion_Der", new Vector2(-11.5f, 9.5f), new Vector2(11f, .5f));
        // Salas laterales (Descanso y Archivo) con paso de 4 m (y 13 a 17)
        CrearMuro(t, "Muro_Ofi_Lounge_Sur", new Vector2(-27f, 11.4f), new Vector2(.5f, 3.2f));
        CrearMuro(t, "Muro_Ofi_Lounge_Norte", new Vector2(-27f, 19.45f), new Vector2(.5f, 4.9f));
        CrearMuro(t, "Muro_Ofi_Archivo_Sur", new Vector2(-13f, 11.4f), new Vector2(.5f, 3.2f));
        CrearMuro(t, "Muro_Ofi_Archivo_Norte", new Vector2(-13f, 19.45f), new Vector2(.5f, 4.9f));

        // ---------------------------------------------------------------------
        // RECEPCIÓN (sur): sofás, mostrador, plantas y felpudo
        // ---------------------------------------------------------------------
        CrearDecor(t, "Felpudo", new Vector2(-20f, 3.3f), new Vector2(3f, 1f), new Color(.30f, .12f, .10f), -7);
        CrearMueble(t, "Mueble_Sofa_Recepcion", new Vector2(-30f, 4.3f), new Vector2(4.5f, 1.3f), new Color(.40f, .15f, .15f));
        CrearMueble(t, "Mueble_Mesa_Centro", new Vector2(-30f, 6.6f), new Vector2(2f, 1f), new Color(.30f, .20f, .12f));
        CrearMueble(t, "Mueble_Mostrador", new Vector2(-12f, 6.2f), new Vector2(6f, 1.2f), new Color(.30f, .20f, .13f));
        CrearDecor(t, "Mostrador_Cubierta", new Vector2(-12f, 6.2f), new Vector2(5.6f, .8f), new Color(.45f, .32f, .20f), 3);
        CrearMueble(t, "Mueble_Planta_1", new Vector2(-33f, 3.2f), new Vector2(.9f, .9f), new Color(.14f, .38f, .20f));
        CrearMueble(t, "Mueble_Planta_2", new Vector2(-7.2f, 3.2f), new Vector2(.9f, .9f), new Color(.14f, .38f, .20f));
        CrearMueble(t, "Mueble_Perchero", new Vector2(-24.5f, 3.2f), new Vector2(.5f, .5f), new Color(.35f, .25f, .15f));

        // ---------------------------------------------------------------------
        // DESPACHO DE ARTUR (centro-norte): alfombra, escritorio, estanterías, ventanas
        // ---------------------------------------------------------------------
        CrearDecor(t, "Alfombra_Borde", new Vector2(-20f, 15.2f), new Vector2(9.8f, 7.4f), new Color(.40f, .30f, .12f), -8);
        CrearDecor(t, "Alfombra_Despacho", new Vector2(-20f, 15.2f), new Vector2(9f, 6.6f), new Color(.30f, .07f, .10f), -7);
        CrearMueble(t, "Mueble_Escritorio_Artur", new Vector2(-20f, 19.5f), new Vector2(4.6f, 1.4f), new Color(.30f, .19f, .11f));
        CrearDecor(t, "Escritorio_Cubierta", new Vector2(-20f, 19.5f), new Vector2(4.2f, 1f), new Color(.42f, .28f, .16f), 3);
        CrearDecor(t, "Escritorio_Lampara", new Vector2(-18.4f, 19.5f), new Vector2(.5f, .5f), new Color(1f, .85f, .40f), 4, true);
        CrearDecor(t, "Luz_Lampara", new Vector2(-18.4f, 19.5f), new Vector2(4.5f, 4.5f), new Color(1f, .80f, .35f, .10f), -6, true);
        CrearDecor(t, "Escritorio_Telefono", new Vector2(-21.5f, 19.5f), new Vector2(.6f, .4f), new Color(.08f, .08f, .08f), 4);
        CrearDecor(t, "Sillon_Artur", new Vector2(-20f, 21.1f), new Vector2(1.4f, .8f), new Color(.12f, .10f, .08f), 3);
        CrearMueble(t, "Mueble_Silla_Visita_1", new Vector2(-23.3f, 17.6f), new Vector2(1f, 1f), new Color(.18f, .14f, .12f));
        CrearMueble(t, "Mueble_Silla_Visita_2", new Vector2(-16.7f, 17.6f), new Vector2(1f, 1f), new Color(.18f, .14f, .12f));
        CrearMueble(t, "Mueble_Estanteria_Izq", new Vector2(-24.8f, 21.2f), new Vector2(2.6f, 1f), new Color(.22f, .14f, .09f));
        CrearMueble(t, "Mueble_Estanteria_Der", new Vector2(-15.2f, 21.2f), new Vector2(2.6f, 1f), new Color(.22f, .14f, .09f));
        // Lomos de libros de colores
        Color[] lomos = { new Color(.65f, .20f, .20f), new Color(.20f, .40f, .65f), new Color(.75f, .65f, .25f), new Color(.25f, .55f, .35f) };
        for (int i = 0; i < 6; i++)
        {
            CrearDecor(t, "Libro_I_" + i, new Vector2(-25.8f + i * .4f, 21.2f), new Vector2(.28f, .7f), lomos[i % 4], 4);
            CrearDecor(t, "Libro_D_" + i, new Vector2(-16.2f + i * .4f, 21.2f), new Vector2(.28f, .7f), lomos[(i + 2) % 4], 4);
        }
        // Ventanas con luz azul de luna que entra en el despacho
        CrearDecor(t, "Ventana_Izq", new Vector2(-22.3f, 22.2f), new Vector2(1.6f, .4f), new Color(.45f, .65f, .90f), 3);
        CrearDecor(t, "Ventana_Der", new Vector2(-17.7f, 22.2f), new Vector2(1.6f, .4f), new Color(.45f, .65f, .90f), 3);
        CrearDecor(t, "Luz_Luna_Izq", new Vector2(-22.3f, 17f), new Vector2(1.4f, 9f), new Color(.45f, .65f, 1f, .05f), -6);
        CrearDecor(t, "Luz_Luna_Der", new Vector2(-17.7f, 17f), new Vector2(1.4f, 9f), new Color(.45f, .65f, 1f, .05f), -6);

        // Rastro de huellas doradas que guía desde la puerta hasta el informe
        for (int i = 0; i < 8; i++)
            CrearDecor(t, "Huella_Guia_" + i, new Vector2(-20f, 4.6f + i * 1.45f), new Vector2(.26f, .26f), new Color(1f, .80f, .30f, .55f), -5, true);

        // Pista 1: Informe sobre Mila, sobre la alfombra frente al escritorio (zona totalmente libre)
        CrearDecor(t, "Brillo_Informe", new Vector2(-20f, 17f), new Vector2(2.4f, 2.4f), new Color(1f, .85f, .35f, .22f), 3, true);
        pistaOficina = CrearPista(
            t,
            "Pista_Informe_Mila",
            new Vector3(-20f, 17f, 0f),
            "Informe confidencial sobre Mila",
            "Mila descubrió que la Organización rastreaba los movimientos de Artur hasta el muelle y el almacén industrial.",
            "Despacho de Artur",
            "Noche del 14 de octubre",
            new Color(.95f, .78f, .25f)
        );

        // ---------------------------------------------------------------------
        // SALA DE ARCHIVO (este): archivadores, mesa y corcho de investigación
        // ---------------------------------------------------------------------
        CrearMueble(t, "Mueble_Archivador_A", new Vector2(-6.9f, 19.5f), new Vector2(1.2f, 3f), new Color(.22f, .25f, .30f));
        CrearMueble(t, "Mueble_Archivador_B", new Vector2(-6.9f, 12f), new Vector2(1.2f, 3f), new Color(.22f, .25f, .30f));
        CrearMueble(t, "Mueble_Mesa_Archivo", new Vector2(-10f, 19f), new Vector2(2.8f, 1.2f), new Color(.30f, .20f, .12f));
        CrearDecor(t, "Corcho_Investigacion", new Vector2(-10f, 21.5f), new Vector2(3.4f, .5f), new Color(.50f, .36f, .20f), 3);
        for (int i = 0; i < 4; i++)
            CrearDecor(t, "Chincheta_" + i, new Vector2(-11.2f + i * .8f, 21.5f), new Vector2(.14f, .14f), new Color(.85f, .15f, .15f), 4, true);

        CrearDecor(t, "Brillo_Archivo", new Vector2(-9.6f, 15.2f), new Vector2(2.2f, 2.2f), new Color(1f, .85f, .35f, .20f), 3, true);
        CrearPista(
            t,
            "Pista_Archivo",
            new Vector3(-9.6f, 15.2f, 0f),
            "Expediente antiguo de la Organización",
            "Documentos con los sellos y firmas de los viejos superiores de Artur.",
            "Sala de Archivo de Artur",
            "Años atrás",
            new Color(.80f, .70f, .30f)
        );

        // ---------------------------------------------------------------------
        // SALA DE DESCANSO (oeste): sofá, alfombra y compartimento secreto de Mila
        // ---------------------------------------------------------------------
        CrearDecor(t, "Alfombra_Descanso", new Vector2(-30.5f, 14.8f), new Vector2(5f, 4.6f), new Color(.10f, .26f, .28f), -7);
        CrearMueble(t, "Mueble_Sofa_Descanso", new Vector2(-33f, 11.6f), new Vector2(1.4f, 3f), new Color(.20f, .30f, .36f));
        CrearMueble(t, "Mueble_Mesa_Descanso", new Vector2(-30.6f, 11.6f), new Vector2(1f, 1.4f), new Color(.30f, .20f, .12f));
        CrearMueble(t, "Mueble_Planta_3", new Vector2(-33.1f, 16.2f), new Vector2(.9f, .9f), new Color(.14f, .38f, .20f));
        CrearDecor(t, "Foto_Mila_Marco", new Vector2(-29f, 21.8f), new Vector2(.9f, .6f), new Color(.80f, .70f, .50f), 4);

        // Compartimento Secreto de Mila (oculto hasta regresar con la pista del club)
        cajonSecretoVisual = new GameObject("Cajon_Secreto_Mila");
        cajonSecretoVisual.transform.SetParent(t);
        cajonSecretoVisual.transform.position = new Vector3(-31.5f, 21.2f, 0f);
        SolidSprite spriteCajon = cajonSecretoVisual.AddComponent<SolidSprite>();
        spriteCajon.Configurar(new Color(.95f, .55f, .15f), new Vector2(1.8f, .8f), false, 4);

        pistaSecreta = CrearPista(
            t,
            "Pista_Carta_Mila",
            new Vector3(-31.5f, 19.4f, 0f),
            "La última carta de Mila",
            "'Papá: sé lo que fuiste y por qué te alejaste. Descubrí que la Organización no te perdonó y que venían por tus archivos. Si no regreso, recuerda que fuiste mi héroe.'",
            "Compartimento Secreto de la Oficina",
            "Recuerdo recuperado",
            new Color(1f, .88f, .42f)
        );
        pistaSecreta.gameObject.SetActive(false);

        // ---------------------------------------------------------------------
        // PUERTA DE SALIDA hacia la calle
        // ---------------------------------------------------------------------
        GameObject puertaSalida = new GameObject("Puerta_Oficina_A_Calle");
        puertaSalida.transform.SetParent(t);
        puertaSalida.transform.position = new Vector3(-20f, 2.5f, 0f);
        BoxCollider2D colSalida = puertaSalida.AddComponent<BoxCollider2D>();
        colSalida.size = new Vector2(1.5f, 1.4f);
        colSalida.isTrigger = true;
        SolidSprite spriteSalida = puertaSalida.AddComponent<SolidSprite>();
        spriteSalida.Configurar(new Color(.28f, .48f, .75f), new Vector2(2f, 1.8f), false, 3);
        puertaACalle = puertaSalida.AddComponent<DoorTransition>();
        CrearEtiquetaMundo(t, "Label_Recepcion", new Vector2(-20f, 6.2f), "RECEPCIÓN", new Color(0.75f, 0.82f, 0.92f, 0.45f), 3.2f);
        CrearEtiquetaMundo(t, "Label_Despacho", new Vector2(-20f, 13.2f), "DESPACHO DE ARTUR", new Color(0.95f, 0.82f, 0.45f, 0.55f), 3.4f);
        CrearEtiquetaMundo(t, "Label_Archivo", new Vector2(-9.6f, 12.2f), "ARCHIVO", new Color(0.75f, 0.82f, 0.92f, 0.45f), 3.0f);
        CrearEtiquetaMundo(t, "Label_Descanso", new Vector2(-30.5f, 17.5f), "SALA DE DESCANSO", new Color(0.55f, 0.88f, 0.88f, 0.45f), 2.8f);
        CrearEtiquetaMundo(t, "Label_Salida_Ofi", new Vector2(-20f, 1.1f), "[E] SALIDA A LA CALLE", new Color(0.55f, 0.80f, 1f, 0.85f), 2.8f);
    }

    // Elemento decorativo sin colisión (alfombras, luces, detalles).
    private static void CrearDecor(Transform parent, string nombre, Vector2 posicion, Vector2 tamaño, Color color, int orden, bool circular = false)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(posicion.x, posicion.y, 0f);
        SolidSprite sprite = obj.AddComponent<SolidSprite>();
        sprite.Configurar(color, tamaño, circular, orden);
    }

    // =========================================================================
    // 3. EDIFICIO 2: ALMACÉN INDUSTRIAL (x: -12 a 16, y: 2 a 24)
    // =========================================================================
    private static void ConstruirAlmacen(
        Transform parent,
        out EvidenceInteractable pistaAlmacen,
        out DoorTransition puertaACalle,
        out DialogueNPC informante)
    {
        GameObject almacen = new GameObject("Edificio_Almacen_Industrial");
        almacen.transform.SetParent(parent);

        // Suelo hormigón industrial
        GameObject suelo = new GameObject("Suelo_Almacen");
        suelo.transform.SetParent(almacen.transform);
        suelo.transform.position = new Vector3(2f, 13f, 0f);
        SolidSprite spriteSuelo = suelo.AddComponent<SolidSprite>();
        spriteSuelo.Configurar(new Color(.055f, .075f, .065f), new Vector2(28f, 22f), false, -10);

        // Muros perimetrales
        CrearMuro(almacen.transform, "Muro_Alm_Norte", new Vector2(2f, 24.2f), new Vector2(28f, .6f));
        CrearMuro(almacen.transform, "Muro_Alm_Sur_Izq", new Vector2(-5f, 1.8f), new Vector2(14f, .6f));
        CrearMuro(almacen.transform, "Muro_Alm_Sur_Der", new Vector2(10f, 1.8f), new Vector2(14f, .6f));
        CrearMuro(almacen.transform, "Muro_Alm_Oeste", new Vector2(-12.2f, 13f), new Vector2(.6f, 22f));
        CrearMuro(almacen.transform, "Muro_Alm_Este", new Vector2(16.2f, 13f), new Vector2(.6f, 22f));

        // Cajas de carga laterales bien espaciadas para dejar un pasillo central ancho de 6 metros
        CrearMueble(almacen.transform, "Cajas_Oeste_1", new Vector2(-7f, 18f), new Vector2(3f, 2.2f), new Color(.24f, .22f, .16f));
        CrearMueble(almacen.transform, "Cajas_Oeste_2", new Vector2(-7f, 8f), new Vector2(3f, 2.2f), new Color(.24f, .22f, .16f));
        CrearMueble(almacen.transform, "Cajas_Este_1", new Vector2(11f, 18f), new Vector2(3f, 2.2f), new Color(.24f, .22f, .16f));
        CrearMueble(almacen.transform, "Cajas_Este_2", new Vector2(11f, 8f), new Vector2(3f, 2.2f), new Color(.24f, .22f, .16f));

        // Pista secundaria en el muelle
        CrearPista(
            almacen.transform,
            "Pista_Secundaria_Almacen",
            new Vector3(-7f, 13f, 0f),
            "Notas de embarque de armas",
            "Registros de contenedores descargados de noche sin inspección aduanera.",
            "Almacén Industrial",
            "Semana pasada",
            new Color(.85f, .75f, .30f)
        );

        // Múltiples enemigos en el almacén con 4 HP
        CrearEnemigo(almacen.transform, "Guardia_Muelle", new Vector3(2f, 9f, 0f), 4, 2.2f, 1, new Color(.68f, .14f, .14f));
        CrearEnemigo(almacen.transform, "Guardia_Almacen_1", new Vector3(-3f, 15f, 0f), 4, 2.3f, 1, new Color(.68f, .14f, .14f));
        CrearEnemigo(almacen.transform, "Guardia_Almacen_2", new Vector3(7f, 15f, 0f), 4, 2.2f, 1, new Color(.68f, .14f, .14f));

        // Guardia de élite (6 HP) custodiando el contenedor del fondo
        CrearEnemigo(almacen.transform, "Guardia_Elite_Deposito", new Vector3(2f, 18f, 0f), 6, 2.4f, 1, new Color(.82f, .10f, .10f));

        // Informante: "El Cuervo" situado cerca de las cajas
        GameObject informanteObj = new GameObject("NPC_ElCuervo");
        informanteObj.transform.SetParent(almacen.transform);
        informanteObj.transform.position = new Vector3(8f, 12f, 0f);
        CircleCollider2D colInf = informanteObj.AddComponent<CircleCollider2D>();
        colInf.radius = .55f;
        SolidSprite spriteInf = informanteObj.AddComponent<SolidSprite>();
        spriteInf.Configurar(new Color(.75f, .58f, .20f), new Vector2(1.15f, 1.15f), true, 10);

        informante = informanteObj.AddComponent<DialogueNPC>();
        DialogueLine[] lineasCuervo = new DialogueLine[]
        {
            new DialogueLine("El Cuervo", "Artur... sabía que vendrías por la calle. La noticia de Mila corrió rápido por el bajo mundo."),
            new DialogueLine("Artur", "¿Quién estuvo aquí esa noche, Cuervo? Necesito nombres."),
            new DialogueLine("El Cuervo", "Hombres de tu antigua Organización. Descargaron armas y hablaron de registrar tu casa."),
            new DialogueLine("El Cuervo", "Tienen un contenedor sospechoso al fondo custodiado por un guardia pesado. Ahí guardaron el Pase VIP para el Club Vortex."),
            new DialogueLine("Artur", "Iré por ese contenedor. No dejes que te vean conmigo.")
        };
        DialogueLine[] repCuervo = new DialogueLine[]
        {
            new DialogueLine("El Cuervo", "Neutraliza al guardia del fondo y abre el contenedor, Artur. El Club Vortex en la avenida es donde están los jefes.")
        };
        informante.Configurar("El Cuervo", "Informante Clandestino", lineasCuervo, repCuervo);
        CrearEtiquetaMundo(almacen.transform, "Label_Cuervo", new Vector2(8f, 13.1f), "El Cuervo (Informante)", new Color(1f, 0.85f, 0.35f, 0.9f), 2.8f, 12);
        CrearEtiquetaMundo(almacen.transform, "Label_Deposito", new Vector2(2f, 22.8f), "CONTENEDOR VIP", new Color(0.95f, 0.78f, 0.30f, 0.65f), 3.0f);
        CrearEtiquetaMundo(almacen.transform, "Label_Salida_Alm", new Vector2(2f, 1.1f), "[E] SALIDA A LA CALLE", new Color(0.95f, 0.78f, 0.35f, 0.85f), 2.8f);

        // Pista Principal: Contenedor con manifiesto y Pase VIP
        pistaAlmacen = CrearPista(
            almacen.transform,
            "Pista_Pase_VIP",
            new Vector3(2f, 21.5f, 0f),
            "Manifiesto de Carga y Pase VIP",
            "Documentos con las rutas de escape de la Organización y un pase exclusivo con el sello del Club Nocturno Vortex.",
            "Depósito del Almacén",
            "Madrugada del 15 de octubre",
            new Color(.95f, .75f, .22f)
        );

        // Puerta de regreso a la calle
        GameObject puertaSalida = new GameObject("Puerta_Almacen_A_Calle");
        puertaSalida.transform.SetParent(almacen.transform);
        puertaSalida.transform.position = new Vector3(2f, 2.5f, 0f);
        BoxCollider2D colSalida = puertaSalida.AddComponent<BoxCollider2D>();
        colSalida.size = new Vector2(1.5f, 1.4f);
        colSalida.isTrigger = true; // <-- TRIGGER
        SolidSprite spriteSalida = puertaSalida.AddComponent<SolidSprite>();
        spriteSalida.Configurar(new Color(.65f, .50f, .25f), new Vector2(2f, 1.8f), false, 3);
        puertaACalle = puertaSalida.AddComponent<DoorTransition>();





    }

    // =========================================================================
    // 4. EDIFICIO 3: CLUB NOCTURNO VORTEX (x: 10 a 38, y: 2 a 24)
    // =========================================================================
    private static void ConstruirClubNocturno(
        Transform parent,
        out DoorTransition puertaACalle,
        out DialogueNPC valeria)
    {
        GameObject club = new GameObject("Edificio_Club_Nocturno");
        club.transform.SetParent(parent);

        // Suelo violeta/púrpura medianoche
        GameObject suelo = new GameObject("Suelo_Club");
        suelo.transform.SetParent(club.transform);
        suelo.transform.position = new Vector3(24f, 13f, 0f);
        SolidSprite spriteSuelo = suelo.AddComponent<SolidSprite>();
        spriteSuelo.Configurar(new Color(.08f, .04f, .11f), new Vector2(28f, 22f), false, -10);

        // Muros perimetrales
        CrearMuro(club.transform, "Muro_Club_Norte", new Vector2(24f, 24.2f), new Vector2(28f, .6f));
        CrearMuro(club.transform, "Muro_Club_Sur_Izq", new Vector2(17f, 1.8f), new Vector2(14f, .6f));
        CrearMuro(club.transform, "Muro_Club_Sur_Der", new Vector2(31f, 1.8f), new Vector2(14f, .6f));
        CrearMuro(club.transform, "Muro_Club_Oeste", new Vector2(9.8f, 13f), new Vector2(.6f, 22f));
        CrearMuro(club.transform, "Muro_Club_Este", new Vector2(38.2f, 13f), new Vector2(.6f, 22f));

        // Pista de baile central con luces neón
        CrearPistaBaileNeon(club.transform, new Vector2(24f, 12f));

        // Gran Barra de neón a la izquierda
        CrearMueble(club.transform, "Barra_Neon_1", new Vector2(14f, 18f), new Vector2(4.5f, 1.3f), new Color(.65f, .12f, .55f));

        // Pista secundaria en la barra
        CrearPista(
            club.transform,
            "Pista_Secundaria_Club",
            new Vector3(14f, 21f, 0f),
            "Libro de sobornos y contactos VIP",
            "Anotaciones con pagos a autoridades y rutas de escape de la Organización.",
            "Barra del Club Vortex",
            "Anoche",
            new Color(.85f, .35f, .75f)
        );

        // Seguridad del club (5 HP)
        CrearEnemigo(club.transform, "Seguridad_Club", new Vector3(20f, 9f, 0f), 5, 2.2f, 1, new Color(.70f, .12f, .45f));

        // Reservado VIP a la derecha (Sofás de terciopelo morado)
        CrearMueble(club.transform, "Sillon_VIP_A", new Vector2(34f, 20f), new Vector2(3.5f, 1.4f), new Color(.24f, .10f, .30f));
        CrearMueble(club.transform, "Sillon_VIP_B", new Vector2(34f, 14f), new Vector2(3.5f, 1.4f), new Color(.24f, .10f, .30f));

        // Contacto: "Valeria" en el reservado VIP
        GameObject valeriaObj = new GameObject("NPC_Valeria");
        valeriaObj.transform.SetParent(club.transform);
        valeriaObj.transform.position = new Vector3(34f, 17f, 0f);
        CircleCollider2D colVal = valeriaObj.AddComponent<CircleCollider2D>();
        colVal.radius = .55f;
        SolidSprite spriteVal = valeriaObj.AddComponent<SolidSprite>();
        spriteVal.Configurar(new Color(.85f, .25f, .55f), new Vector2(1.15f, 1.15f), true, 10);

        valeria = valeriaObj.AddComponent<DialogueNPC>();
        DialogueLine[] lineasValeria = new DialogueLine[]
        {
            new DialogueLine("Valeria", "Artur Jones... pensé que jamás volvería a ver esa mirada helada."),
            new DialogueLine("Artur", "Valeria. Tú manejas la información de este club. ¿Quién dio la orden contra mi hija?"),
            new DialogueLine("Valeria", "Escúchame bien, porque esto no te va a gustar: no querían matar a Mila."),
            new DialogueLine("Artur", "¿De qué estás hablando?"),
            new DialogueLine("Valeria", "La Organización buscaba tus viejos archivos de contactos. Ella los descubrió rebuscando en tu oficina y la silenciaron."),
            new DialogueLine("Valeria", "Toma esta grabación. Prueba que registraron tu despacho. Artur, si Mila protegió algo antes de morir, sigue oculto en tu propia oficina. ¡Regresa ya!")
        };
        DialogueLine[] repValeria = new DialogueLine[]
        {
            new DialogueLine("Valeria", "Vuelve a tu oficina, Artur. Hay un compartimento secreto que Mila cerró antes de morir.")
        };
        valeria.Configurar("Valeria", "Antiguo Contacto", lineasValeria, repValeria);
        CrearEtiquetaMundo(club.transform, "Label_Valeria", new Vector2(34f, 18.1f), "Valeria (Contacto)", new Color(1f, 0.45f, 0.80f, 0.9f), 2.8f, 12);
        CrearEtiquetaMundo(club.transform, "Label_VIP", new Vector2(34f, 22.2f), "RESERVADO VIP", new Color(0.95f, 0.40f, 0.85f, 0.65f), 3.0f);
        CrearEtiquetaMundo(club.transform, "Label_Salida_Club", new Vector2(24f, 1.1f), "[E] SALIDA A LA CALLE", new Color(0.95f, 0.45f, 0.85f, 0.85f), 2.8f);

        // Puerta de regreso a la calle
        GameObject puertaSalida = new GameObject("Puerta_Club_A_Calle");
        puertaSalida.transform.SetParent(club.transform);
        puertaSalida.transform.position = new Vector3(24f, 2.5f, 0f);
        BoxCollider2D colSalida = puertaSalida.AddComponent<BoxCollider2D>();
        colSalida.size = new Vector2(1.5f, 1.4f);
        colSalida.isTrigger = true; // <-- TRIGGER
        SolidSprite spriteSalida = puertaSalida.AddComponent<SolidSprite>();
        spriteSalida.Configurar(new Color(.85f, .20f, .70f), new Vector2(2f, 1.8f), false, 3);
        puertaACalle = puertaSalida.AddComponent<DoorTransition>();





    }

    // =========================================================================
    // CONEXIONES DE PUERTAS BIDIRECCIONALES
    // =========================================================================
    private static void ConectarPuertas(
        DoorTransition calleAOficina, DoorTransition oficinaACalle,
        DoorTransition calleAAlmacen, DoorTransition almacenACalle,
        DoorTransition calleAClub, DoorTransition clubACalle)
    {
        // Spawns en la Calle
        GameObject spawnCalleDesdeOficina = new GameObject("Spawn_Calle_DesdeOficina");
        spawnCalleDesdeOficina.transform.position = new Vector3(-20f, -19.5f, 0f);

        GameObject spawnCalleDesdeAlmacen = new GameObject("Spawn_Calle_DesdeAlmacen");
        spawnCalleDesdeAlmacen.transform.position = new Vector3(2f, -19.5f, 0f);

        GameObject spawnCalleDesdeClub = new GameObject("Spawn_Calle_DesdeClub");
        spawnCalleDesdeClub.transform.position = new Vector3(24f, -19.5f, 0f);

        // Spawns interiores
        GameObject spawnOficinaEntrada = new GameObject("Spawn_Oficina_Entrada");
        spawnOficinaEntrada.transform.position = new Vector3(-20f, 5f, 0f);

        GameObject spawnAlmacenEntrada = new GameObject("Spawn_Almacen_Entrada");
        spawnAlmacenEntrada.transform.position = new Vector3(2f + DesplazamientoAlmacen.x, 5f + DesplazamientoAlmacen.y, 0f);

        GameObject spawnClubEntrada = new GameObject("Spawn_Club_Entrada");
        spawnClubEntrada.transform.position = new Vector3(24f + DesplazamientoClub.x, 5f + DesplazamientoClub.y, 0f);

        // Límites de cámara
        Vector2 limitesCalleMin = new Vector2(-36f, -38f);
        Vector2 limitesCalleMax = new Vector2(36f, -10f);

        Vector2 limitesOficinaMin = new Vector2(-35f, 1f);
        Vector2 limitesOficinaMax = new Vector2(-5f, 23f);

        Vector2 limitesAlmacenMin = new Vector2(-13f, 1f) + DesplazamientoAlmacen;
        Vector2 limitesAlmacenMax = new Vector2(17f, 25f) + DesplazamientoAlmacen;

        Vector2 limitesClubMin = new Vector2(9f, 1f) + DesplazamientoClub;
        Vector2 limitesClubMax = new Vector2(39f, 25f) + DesplazamientoClub;

        // Calle <-> Oficina
        calleAOficina.Configurar("Oficina de Artur", spawnOficinaEntrada.transform, LocationID.Oficina, limitesOficinaMin, limitesOficinaMax, false);
        oficinaACalle.Configurar("Calle Central", spawnCalleDesdeOficina.transform, LocationID.Calle, limitesCalleMin, limitesCalleMax, false);

        // Calle <-> Almacén
        calleAAlmacen.Configurar("Almacén Industrial", spawnAlmacenEntrada.transform, LocationID.Almacen, limitesAlmacenMin, limitesAlmacenMax, true, "Requiere examinar el informe sobre Mila en tu oficina.");
        almacenACalle.Configurar("Calle Central", spawnCalleDesdeAlmacen.transform, LocationID.Calle, limitesCalleMin, limitesCalleMax, false);

        // Calle <-> Club Nocturno
        calleAClub.Configurar("Club Nocturno Vortex", spawnClubEntrada.transform, LocationID.ClubNocturno, limitesClubMin, limitesClubMax, true, "Requiere el Pase VIP de la Organización encontrado en el almacén.");
        clubACalle.Configurar("Calle Central", spawnCalleDesdeClub.transform, LocationID.Calle, limitesCalleMin, limitesCalleMax, false);
    }

    // =========================================================================
    // ELEMENTOS URBANOS Y DECORACIÓN
    // =========================================================================
    private static void CrearPasoPeatones(Transform parent, Vector2 centro)
    {
        for (int i = -3; i <= 3; i++)
        {
            GameObject linea = new GameObject("Linea_Peatonal_" + i);
            linea.transform.SetParent(parent);
            linea.transform.position = new Vector3(centro.x + (i * 0.9f), centro.y, 0f);
            SolidSprite sprite = linea.AddComponent<SolidSprite>();
            sprite.Configurar(new Color(.85f, .88f, .92f, .75f), new Vector2(0.45f, 5.5f), false, -8);
        }
    }

    private static void CrearFarola(Transform parent, Vector2 posicion)
    {
        GameObject farola = new GameObject("Farola");
        farola.transform.SetParent(parent);
        farola.transform.position = posicion;

        // Poste limpio pegado a la acera sur que no estorba
        SolidSprite spritePoste = farola.AddComponent<SolidSprite>();
        spritePoste.Configurar(new Color(.45f, .48f, .55f), new Vector2(.35f, 1.8f), false, 2);
    }

    private static void CrearRotuloPuerta(Transform parent, string nombre, Vector2 posicion, string texto, Color color)
    {
        GameObject rotulo = new GameObject(nombre);
        rotulo.transform.SetParent(parent);
        rotulo.transform.position = posicion;

        SolidSprite fondo = rotulo.AddComponent<SolidSprite>();
        fondo.Configurar(color, new Vector2(4.2f, .85f), false, 4);
        CrearEtiquetaMundo(parent, nombre + "_Texto", posicion, texto, Color.white, 3.6f, 6);
    }

    private static void CrearEtiquetaMundo(Transform parent, string nombre, Vector2 posicion, string texto, Color color, float fontSize = 3.2f, int sortingOrder = 5)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(posicion.x, posicion.y, 0f);
        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.sortingOrder = sortingOrder;
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(10f, 2f);
    }

    private static void CrearPistaBaileNeon(Transform parent, Vector2 centro)
    {
        Color[] colores = new Color[]
        {
            new Color(.55f, .10f, .70f, .35f),
            new Color(.10f, .45f, .85f, .35f),
            new Color(.85f, .10f, .50f, .35f)
        };

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                GameObject baldosa = new GameObject("Neon_" + x + "_" + y);
                baldosa.transform.SetParent(parent);
                baldosa.transform.position = new Vector3(centro.x + (x * 1.8f), centro.y + (y * 1.8f), 0f);
                SolidSprite sprite = baldosa.AddComponent<SolidSprite>();
                Color colorAzar = colores[Mathf.Abs(x + y) % colores.Length];
                sprite.Configurar(colorAzar, new Vector2(1.6f, 1.6f), false, -8);
            }
        }
    }

    private static void CrearMuro(Transform parent, string nombre, Vector2 posicion, Vector2 tamaño)
    {
        GameObject muro = new GameObject(nombre);
        muro.transform.SetParent(parent);
        muro.transform.position = posicion;

        BoxCollider2D collider = muro.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one; // El SolidSprite ya escala el objeto a "tamaño": el collider mide 1x1 local

        // Color más claro y contrastado con el suelo para que los muros se vean claramente
        SolidSprite visual = muro.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.28f, .32f, .38f), tamaño, false, 1);
    }

    private static void CrearMueble(Transform parent, string nombre, Vector2 posicion, Vector2 tamaño, Color color)
    {
        GameObject mueble = new GameObject(nombre);
        mueble.transform.SetParent(parent);
        mueble.transform.position = posicion;

        BoxCollider2D collider = mueble.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one; // El SolidSprite ya escala el objeto a "tamaño": el collider mide 1x1 local

        SolidSprite visual = mueble.AddComponent<SolidSprite>();
        visual.Configurar(color, tamaño, false, 2);
    }

    private static EvidenceInteractable CrearPista(
        Transform parent,
        string nombre,
        Vector3 posicion,
        string titulo,
        string descripcion,
        string lugar,
        string fecha,
        Color color)
    {
        GameObject pista = new GameObject(nombre);
        pista.transform.SetParent(parent);
        pista.transform.position = posicion;

        // IMPORTANTE: isTrigger = true para que el jugador nunca choque como si fuera un muro
        BoxCollider2D collider = pista.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(2f / 1.2f, 1.8f / .85f); // Medida local: el sprite se escala a 1.2 x .85
        collider.isTrigger = true;

        SolidSprite visual = pista.AddComponent<SolidSprite>();
        visual.Configurar(color, new Vector2(1.2f, .85f), false, 5);

        EvidenceInteractable evidencia = pista.AddComponent<EvidenceInteractable>();
        evidencia.ConfigurarCompleto(titulo, descripcion, lugar, fecha);
        return evidencia;
    }

    private static GameObject CrearEnemigo(Transform parent, string nombre, Vector3 posicion, int vida, float vel, int daño, Color colorVisual)
    {
        GameObject enemigo = new GameObject(nombre);
        enemigo.transform.SetParent(parent);
        enemigo.transform.position = posicion;

        CircleCollider2D collider = enemigo.AddComponent<CircleCollider2D>();
        collider.radius = .48f;

        SolidSprite visual = enemigo.AddComponent<SolidSprite>();
        visual.Configurar(colorVisual, new Vector2(1.2f, 1.2f), true, 10);

        EnemySimple enemigoComp = enemigo.AddComponent<EnemySimple>();
        enemigoComp.Configurar(vida, vel, daño, nombre);

        return enemigo;
    }

    // =========================================================================
    // INTERFAZ DE USUARIO: HUD, PROMPT Y DIARIO CENTRADO RESPONSIVE
    // =========================================================================
    private static void CrearInterfaz(Transform parent)
    {
        GameObject canvasObject = new GameObject("Interfaz_Fase2");
        canvasObject.transform.SetParent(parent);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // CanvasScaler responsive a 1920x1080
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        TMP_FontAsset fuente = TMP_Settings.defaultFontAsset;
        if (fuente == null)
        {
            TMP_FontAsset[] fuentes = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (fuentes.Length > 0)
                fuente = fuentes[0];
        }

        // HUD: Salud de Artur y Objetivo actual (Anclado arriba a la izquierda)
        GameObject hudPanel = new GameObject("HUD_Superior");
        hudPanel.transform.SetParent(canvasObject.transform);
        RectTransform rectHud = hudPanel.AddComponent<RectTransform>();
        rectHud.anchorMin = new Vector2(0f, 1f);
        rectHud.anchorMax = new Vector2(0f, 1f);
        rectHud.pivot = new Vector2(0f, 1f);
        rectHud.anchoredPosition = new Vector2(30f, -30f);
        rectHud.sizeDelta = new Vector2(650f, 120f);

        TMP_Text saludText = CrearTextoRelativo(hudPanel.transform, "SaludHUD", fuente, new Vector2(0f, 0f), new Vector2(600f, 36f), 24);
        canvasObject.AddComponent<PlayerHealthHUD>().Configurar(saludText);

        TMP_Text objetivoText = CrearTextoRelativo(hudPanel.transform, "ObjetivoHUD", fuente, new Vector2(0f, -40f), new Vector2(620f, 70f), 20);

        TMP_Text ayudaText = CrearTextoRelativo(canvasObject.transform, "AyudaHUD", fuente, new Vector2(0f, -500f), new Vector2(850f, 35f), 18);
        ayudaText.alignment = TextAlignmentOptions.Center;

        GameObject hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(canvasObject.transform);
        hudObj.AddComponent<ObjectiveHUD>().Configurar(objetivoText, ayudaText);

        // Indicador de interacción centrado
        GameObject promptBox = new GameObject("FondoPrompt");
        promptBox.transform.SetParent(canvasObject.transform);
        RectTransform rectPromptBox = promptBox.AddComponent<RectTransform>();
        rectPromptBox.anchorMin = new Vector2(0.5f, 0.22f);
        rectPromptBox.anchorMax = new Vector2(0.5f, 0.22f);
        rectPromptBox.pivot = new Vector2(0.5f, 0.5f);
        rectPromptBox.anchoredPosition = Vector2.zero;
        rectPromptBox.sizeDelta = new Vector2(560f, 50f);
        Image promptImg = promptBox.AddComponent<Image>();
        promptImg.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

        TMP_Text promptText = CrearTextoRelativo(promptBox.transform, "PromptInteraccion", fuente, Vector2.zero, new Vector2(540f, 44f), 22);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0.85f, 0.3f);

        GameObject prompt = new GameObject("Indicador_Interaccion");
        prompt.transform.SetParent(canvasObject.transform);
        InteractionPrompt promptComponent = prompt.AddComponent<InteractionPrompt>();
        promptComponent.Configurar(promptText, promptBox);

        // Panel de Diálogo cinemático (inferior)
        GameObject panelDialogo = new GameObject("PanelDialogo");
        panelDialogo.transform.SetParent(canvasObject.transform);
        RectTransform rectDialogo = panelDialogo.AddComponent<RectTransform>();
        rectDialogo.anchorMin = new Vector2(0.5f, 0.18f);
        rectDialogo.anchorMax = new Vector2(0.5f, 0.18f);
        rectDialogo.pivot = new Vector2(0.5f, 0.5f);
        rectDialogo.anchoredPosition = Vector2.zero;
        rectDialogo.sizeDelta = new Vector2(960f, 200f);
        Image fondoDialogo = panelDialogo.AddComponent<Image>();
        fondoDialogo.color = new Color(0.03f, 0.04f, 0.07f, 0.96f);

        TMP_Text nombreDialogo = CrearTextoRelativo(panelDialogo.transform, "NombrePersonaje", fuente, new Vector2(0f, 65f), new Vector2(900f, 36f), 23);
        nombreDialogo.color = new Color(1f, 0.78f, 0.25f);
        nombreDialogo.fontStyle = FontStyles.Bold;

        TMP_Text cuerpoDialogo = CrearTextoRelativo(panelDialogo.transform, "TextoDialogo", fuente, new Vector2(0f, 8f), new Vector2(900f, 90f), 20);
        cuerpoDialogo.color = Color.white;

        TMP_Text ayudaDialogo = CrearTextoRelativo(panelDialogo.transform, "AyudaDialogo", fuente, new Vector2(0f, -70f), new Vector2(900f, 26f), 16);
        ayudaDialogo.alignment = TextAlignmentOptions.Right;
        ayudaDialogo.color = new Color(0.7f, 0.7f, 0.7f);

        DialogueUI dialogoUI = canvasObject.AddComponent<DialogueUI>();
        dialogoUI.Configurar(panelDialogo, nombreDialogo, cuerpoDialogo, ayudaDialogo);

        // Panel del Diario de Investigación (J) centrado con márgenes de seguridad
        GameObject diarioObject = new GameObject("Diario");
        diarioObject.transform.SetParent(canvasObject.transform);

        RectTransform rectDiarioRoot = diarioObject.AddComponent<RectTransform>();
        rectDiarioRoot.anchorMin = Vector2.zero;
        rectDiarioRoot.anchorMax = Vector2.one;
        rectDiarioRoot.offsetMin = Vector2.zero;
        rectDiarioRoot.offsetMax = Vector2.zero;
        rectDiarioRoot.localPosition = Vector3.zero;

        GameObject panelDiario = new GameObject("PanelDiario");
        panelDiario.transform.SetParent(diarioObject.transform);
        Image panelImage = panelDiario.AddComponent<Image>();
        panelImage.color = new Color(.025f, .035f, .055f, .98f);

        RectTransform panelRect = panelDiario.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1060f, 740f);

        // Borde superior dorado
        GameObject barraDorada = new GameObject("Barra_Dorada");
        barraDorada.transform.SetParent(panelDiario.transform);
        RectTransform rectBarra = barraDorada.AddComponent<RectTransform>();
        rectBarra.anchorMin = new Vector2(0f, 1f);
        rectBarra.anchorMax = new Vector2(1f, 1f);
        rectBarra.pivot = new Vector2(0.5f, 1f);
        rectBarra.anchoredPosition = Vector2.zero;
        rectBarra.sizeDelta = new Vector2(0f, 6f);
        Image imgBarra = barraDorada.AddComponent<Image>();
        imgBarra.color = new Color(0.95f, 0.75f, 0.25f, 0.95f);

        // Texto del diario con márgenes interiores
        GameObject textoDiarioObj = new GameObject("TextoDiario");
        textoDiarioObj.transform.SetParent(panelDiario.transform);
        RectTransform rectTexto = textoDiarioObj.AddComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = new Vector2(50f, 40f);
        rectTexto.offsetMax = new Vector2(-50f, -40f);
        rectTexto.pivot = new Vector2(0.5f, 0.5f);
        rectTexto.localPosition = Vector3.zero;

        TMP_Text diarioText = textoDiarioObj.AddComponent<TextMeshProUGUI>();
        diarioText.font = fuente;
        diarioText.fontSize = 18f;
        diarioText.color = Color.white;
        diarioText.alignment = TextAlignmentOptions.TopLeft;
        diarioText.textWrappingMode = TextWrappingModes.Normal;

        EvidenceJournal diario = diarioObject.AddComponent<EvidenceJournal>();
        diario.Configurar(panelDiario, diarioText);
    }

    private static TMP_Text CrearTextoRelativo(Transform parent, string nombre, TMP_FontAsset fuente, Vector2 posicion, Vector2 tamaño, float fontSize)
    {
        GameObject textoObject = new GameObject(nombre);
        textoObject.transform.SetParent(parent);

        RectTransform rect = textoObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(.5f, .5f);
        rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;

        TMP_Text texto = textoObject.AddComponent<TextMeshProUGUI>();
        texto.font = fuente;
        texto.fontSize = fontSize;
        texto.color = Color.white;
        texto.textWrappingMode = TextWrappingModes.Normal;
        return texto;
    }
}

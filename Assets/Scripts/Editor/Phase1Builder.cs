using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builder automático para la Fase 1: genera la base jugable y repara la posición de pistas y vida del enemigo.
public static class Phase1Builder
{
    private const string RootName = "ARTUR_Fase1_Generado";

    [MenuItem("ARTUR/Construir Fase 1")]
    public static void Construir()
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Limpieza previa para garantizar idempotencia
        GameObject rootViejo = GameObject.Find(RootName);
        if (rootViejo != null)
            Object.DestroyImmediate(rootViejo);

        GameObject root = new GameObject(RootName);
        root.AddComponent<Phase1Runtime>();

        CrearJugador(root.transform);
        CrearCamara(root.transform);
        CrearSueloYLimites(root.transform);

        // Pistas centradas y bien ubicadas
        EvidenceInteractable pistaMila = CrearPista(root.transform, "Documento_Mila", new Vector3(2.5f, 0f, 0f),
            "El sobre de Mila", "La organización conocía cada movimiento de Mila antes de matarla.");
        EvidenceInteractable pistaRegresoOficina = CrearPista(root.transform, "Nueva_Evidencia_Oficina", new Vector3(-2.5f, 0f, 0f),
            "La marca del pasado", "El símbolo pertenece a una célula que Artur creyó destruida.");
        pistaRegresoOficina.gameObject.SetActive(false);

        MissionProgression progresion = root.AddComponent<MissionProgression>();
        progresion.ConfigurarPistaRegreso(pistaRegresoOficina);

        CrearEnemigo(root.transform);
        CrearInterfaz(root.transform);

        EditorSceneManager.SaveScene(escena, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("ARTUR: Fase 1 construida con éxito en Assets/Scenes/Main.unity.");
    }

    private static void CrearJugador(Transform parent)
    {
        GameObject jugador = new GameObject("Artur");
        jugador.transform.SetParent(parent);
        jugador.transform.position = Vector3.zero;

        Rigidbody2D rb = jugador.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D collider = jugador.AddComponent<CircleCollider2D>();
        collider.radius = .42f;

        SolidSprite sprite = jugador.AddComponent<SolidSprite>();
        sprite.Configurar(new Color(.12f, .35f, .55f), new Vector2(1.1f, 1.1f), true, 10);

        jugador.AddComponent<PlayerMovement>();
        jugador.AddComponent<PlayerInteraction>();
        jugador.AddComponent<PlayerCombat>();
        Health salud = jugador.AddComponent<Health>();
        salud.Configurar(3);
    }

    private static void CrearCamara(Transform parent)
    {
        GameObject camara = new GameObject("Camara_Principal");
        camara.transform.SetParent(parent);
        camara.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = camara.AddComponent<Camera>();
        camara.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.015f, .02f, .03f);

        CameraFollow seguimiento = camara.AddComponent<CameraFollow>();
        seguimiento.Configurar(null, new Vector2(-20f, -12f), new Vector2(20f, 12f));
        camara.AddComponent<AudioListener>();
    }

    private static void CrearSueloYLimites(Transform parent)
    {
        GameObject suelo = new GameObject("Zona_Oficina");
        suelo.transform.SetParent(parent);
        SolidSprite visual = suelo.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.055f, .07f, .09f), new Vector2(40f, 24f), false, -10);

        CrearMuro(parent, "Muro_Norte", new Vector2(0f, 12.2f), new Vector2(40f, .4f));
        CrearMuro(parent, "Muro_Sur", new Vector2(0f, -12.2f), new Vector2(40f, .4f));
        CrearMuro(parent, "Muro_Este", new Vector2(20.2f, 0f), new Vector2(.4f, 24f));
        CrearMuro(parent, "Muro_Oeste", new Vector2(-20.2f, 0f), new Vector2(.4f, 24f));
    }

    private static void CrearMuro(Transform parent, string nombre, Vector2 posicion, Vector2 tamaño)
    {
        GameObject muro = new GameObject(nombre);
        muro.transform.SetParent(parent);
        muro.transform.position = posicion;
        BoxCollider2D collider = muro.AddComponent<BoxCollider2D>();
        collider.size = tamaño;
        SolidSprite visual = muro.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.18f, .20f, .23f), tamaño, false, 1);
    }

    private static EvidenceInteractable CrearPista(Transform parent, string nombre, Vector3 posicion, string titulo, string descripcion)
    {
        GameObject pista = new GameObject(nombre);
        pista.transform.SetParent(parent);
        pista.transform.position = posicion;

        BoxCollider2D collider = pista.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(.9f, .6f);

        SolidSprite visual = pista.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.85f, .72f, .32f), new Vector2(.9f, .6f), false, 5);

        EvidenceInteractable evidencia = pista.AddComponent<EvidenceInteractable>();
        evidencia.Configurar(titulo, descripcion);
        return evidencia;
    }

    private static void CrearEnemigo(Transform parent)
    {
        GameObject enemigo = new GameObject("Guardia_Almacen");
        enemigo.transform.SetParent(parent);
        enemigo.transform.position = new Vector3(8f, 0f, 0f);

        CircleCollider2D collider = enemigo.AddComponent<CircleCollider2D>();
        collider.radius = .48f;

        SolidSprite visual = enemigo.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.65f, .12f, .12f), new Vector2(1.15f, 1.15f), true, 10);

        // EnemySimple añade automáticamente Health con 3 de vida y la barra EnemyHealthBar flotante
        enemigo.AddComponent<EnemySimple>();
    }

    private static void CrearInterfaz(Transform parent)
    {
        GameObject canvasObject = new GameObject("Interfaz_Fase1");
        canvasObject.transform.SetParent(parent);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        TMP_FontAsset fuente = TMP_Settings.defaultFontAsset;
        if (fuente == null)
        {
            TMP_FontAsset[] fuentes = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (fuentes.Length > 0)
                fuente = fuentes[0];
        }

        // HUD de Objetivo y Salud de Artur
        TMP_Text objetivo = CrearTexto(canvasObject.transform, "ObjetivoHUD", fuente, new Vector2(-300f, 320f), new Vector2(500f, 65f), 20);
        TMP_Text saludText = CrearTexto(canvasObject.transform, "SaludHUD", fuente, new Vector2(-300f, 370f), new Vector2(400f, 35f), 22);
        canvasObject.AddComponent<PlayerHealthHUD>().Configurar(saludText);

        TMP_Text ayuda = CrearTexto(canvasObject.transform, "AyudaHUD", fuente, new Vector2(0f, -380f), new Vector2(750f, 35f), 17);
        ayuda.alignment = TextAlignmentOptions.Center;

        // Prompt de interacción centrado con caja semitransparente
        GameObject promptBox = new GameObject("FondoPrompt");
        promptBox.transform.SetParent(canvasObject.transform);
        RectTransform rectPromptBox = promptBox.AddComponent<RectTransform>();
        rectPromptBox.anchorMin = new Vector2(0.5f, 0.22f);
        rectPromptBox.anchorMax = new Vector2(0.5f, 0.22f);
        rectPromptBox.pivot = new Vector2(0.5f, 0.5f);
        rectPromptBox.anchoredPosition = Vector2.zero;
        rectPromptBox.sizeDelta = new Vector2(520f, 46f);
        Image promptImg = promptBox.AddComponent<Image>();
        promptImg.color = new Color(0.04f, 0.05f, 0.08f, 0.90f);

        TMP_Text promptText = CrearTexto(promptBox.transform, "PromptInteraccion", fuente, Vector2.zero, new Vector2(500f, 40f), 22);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0.85f, 0.3f);

        GameObject prompt = new GameObject("Indicador_Interaccion");
        prompt.transform.SetParent(canvasObject.transform);
        InteractionPrompt promptComponent = prompt.AddComponent<InteractionPrompt>();
        promptComponent.Configurar(promptText, promptBox);

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvasObject.transform);
        hud.AddComponent<ObjectiveHUD>().Configurar(objetivo, ayuda);

        // Panel del Diario (J)
        GameObject diarioObject = new GameObject("Diario");
        diarioObject.transform.SetParent(canvasObject.transform);
        GameObject panelObject = new GameObject("PanelDiario");
        panelObject.transform.SetParent(diarioObject.transform);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(.03f, .04f, .06f, .97f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(.5f, .5f);
        panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.pivot = new Vector2(.5f, .5f);
        panelRect.sizeDelta = new Vector2(760f, 620f);

        TMP_Text diarioText = CrearTexto(panelObject.transform, "TextoDiario", fuente, Vector2.zero, new Vector2(700f, 560f), 18);
        diarioText.alignment = TextAlignmentOptions.TopLeft;

        EvidenceJournal diario = diarioObject.AddComponent<EvidenceJournal>();
        diario.Configurar(panelObject, diarioText);
    }

    private static TMP_Text CrearTexto(Transform parent, string nombre, TMP_FontAsset fuente, Vector2 posicion, Vector2 tamaño, float fontSize)
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

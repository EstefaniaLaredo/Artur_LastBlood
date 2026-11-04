using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Phase1Builder
{
    private const string RootName = "ARTUR_Fase1_Generado";

    [MenuItem("ARTUR/Construir Fase 1")]
    public static void Construir()
    {
        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject(RootName);
        root.AddComponent<Phase1Runtime>();

        CrearJugador(root.transform);
        CrearCamara(root.transform);
        CrearSueloYLimites(root.transform);
        CrearPista(root.transform);
        CrearInterfaz(root.transform);

        EditorSceneManager.SaveScene(escena, "Assets/Scenes/Main.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("ARTUR: Fase 1 construida en Assets/Scenes/Main.unity.");
    }

    private static void CrearJugador(Transform parent)
    {
        GameObject jugador = new GameObject("Artur");
        jugador.transform.SetParent(parent);
        jugador.transform.position = Vector3.zero;
        jugador.AddComponent<Rigidbody2D>();
        CircleCollider2D collider = jugador.AddComponent<CircleCollider2D>();
        collider.radius = .42f;
        SolidSprite sprite = jugador.AddComponent<SolidSprite>();
        sprite.Configurar(new Color(.12f, .35f, .55f), new Vector2(1.1f, 1.1f), true);
        jugador.AddComponent<PlayerMovement>();
        jugador.AddComponent<PlayerInteraction>();
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
        camara.AddComponent<CameraFollow>();
        camara.AddComponent<AudioListener>();
    }

    private static void CrearSueloYLimites(Transform parent)
    {
        GameObject suelo = new GameObject("Zona_Oficina");
        suelo.transform.SetParent(parent);
        SolidSprite visual = suelo.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.055f, .07f, .09f), new Vector2(28f, 16f), false);
        CrearMuro(parent, "Muro_Norte", new Vector2(0f, 8.2f), new Vector2(28f, .4f));
        CrearMuro(parent, "Muro_Sur", new Vector2(0f, -8.2f), new Vector2(28f, .4f));
        CrearMuro(parent, "Muro_Este", new Vector2(14.2f, 0f), new Vector2(.4f, 16f));
        CrearMuro(parent, "Muro_Oeste", new Vector2(-14.2f, 0f), new Vector2(.4f, 16f));
    }

    private static void CrearMuro(Transform parent, string nombre, Vector2 posicion, Vector2 tamaño)
    {
        GameObject muro = new GameObject(nombre);
        muro.transform.SetParent(parent);
        muro.transform.position = posicion;
        BoxCollider2D collider = muro.AddComponent<BoxCollider2D>();
        collider.size = tamaño;
        SolidSprite visual = muro.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.18f, .20f, .23f), tamaño, false);
    }

    private static void CrearPista(Transform parent)
    {
        GameObject pista = new GameObject("Documento_Mila");
        pista.transform.SetParent(parent);
        pista.transform.position = new Vector3(3f, 1.5f, 0f);
        BoxCollider2D collider = pista.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(.8f, .55f);
        SolidSprite visual = pista.AddComponent<SolidSprite>();
        visual.Configurar(new Color(.85f, .72f, .32f), new Vector2(.8f, .55f), false);
        pista.AddComponent<EvidenceInteractable>();
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
        TMP_Text objetivo = CrearTexto(canvasObject.transform, "ObjetivoHUD", fuente, new Vector2(24f, -24f), new Vector2(460f, 70f), 22);
        TMP_Text ayuda = CrearTexto(canvasObject.transform, "AyudaHUD", fuente, new Vector2(24f, 24f), new Vector2(700f, 40f), 18);
        TMP_Text promptText = CrearTexto(canvasObject.transform, "PromptInteraccion", fuente, new Vector2(0f, -390f), new Vector2(420f, 55f), 28);
        promptText.alignment = TextAlignmentOptions.Center;
        GameObject prompt = new GameObject("Indicador_Interaccion");
        prompt.transform.SetParent(canvasObject.transform);
        InteractionPrompt promptComponent = prompt.AddComponent<InteractionPrompt>();
        promptComponent.Configurar(promptText);

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvasObject.transform);
        hud.AddComponent<ObjectiveHUD>().Configurar(objetivo, ayuda);

        GameObject diarioObject = new GameObject("Diario");
        diarioObject.transform.SetParent(canvasObject.transform);
        Image panelImage = diarioObject.AddComponent<Image>();
        panelImage.color = new Color(.03f, .04f, .06f, .97f);
        RectTransform panelRect = diarioObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(.5f, .5f);
        panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.pivot = new Vector2(.5f, .5f);
        panelRect.sizeDelta = new Vector2(760f, 620f);
        TMP_Text diarioText = CrearTexto(diarioObject.transform, "TextoDiario", fuente, Vector2.zero, new Vector2(700f, 560f), 20);
        diarioText.alignment = TextAlignmentOptions.TopLeft;
        EvidenceJournal diario = diarioObject.AddComponent<EvidenceJournal>();
        diario.Configurar(diarioObject, diarioText);
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
        texto.enableWordWrapping = true;
        return texto;
    }
}

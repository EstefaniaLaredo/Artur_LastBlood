using UnityEngine;

// Generador procedural de Pixel Art 2D top-down y cargador automático de sprites externos.
// Si colocas un PNG en Assets/Resources/Sprites/ con el nombre del objeto (ej. Artur.png, Muro.png, Pista.png),
// lo usará automáticamente; si no existe, dibuja pixel art detallado por código.
public class SolidSprite : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Vector2 tamaño = Vector2.one;
    [SerializeField] private bool circular;
    [SerializeField] private int ordenCapa = 0;
    [SerializeField] private Sprite spritePersonalizado;

    private void Awake()
    {
        GenerarSprite();
    }

    public void Configurar(Color nuevoColor, Vector2 nuevoTamaño, bool formaCircular)
    {
        Configurar(nuevoColor, nuevoTamaño, formaCircular, DeterminarOrdenPorDefecto());
    }

    public void Configurar(Color nuevoColor, Vector2 nuevoTamaño, bool formaCircular, int nuevoOrden)
    {
        color = nuevoColor;
        tamaño = nuevoTamaño;
        circular = formaCircular;
        ordenCapa = nuevoOrden;

        GenerarSprite();
    }

    private int DeterminarOrdenPorDefecto()
    {
        if (gameObject.name.StartsWith("Suelo") || gameObject.name.StartsWith("Zona"))
            return -10;
        if (gameObject.name == "Artur")
            return 10;
        if (gameObject.name.StartsWith("Documento") || gameObject.name.StartsWith("Pista") || gameObject.name.StartsWith("Evidencia"))
            return 5;
        if (gameObject.name.StartsWith("Muro") || gameObject.name.StartsWith("Mueble"))
            return 1;
        return 0;
    }

    private Sprite IntentarCargarSpriteExterno(string nombre)
    {
        if (spritePersonalizado != null)
            return spritePersonalizado;

        // 1. Buscar por nombre exacto en Assets/Resources/Sprites/
        Sprite exacto = Resources.Load<Sprite>("Sprites/" + nombre);
        if (exacto != null)
            return exacto;

        // 2. Buscar por categoría general
        if (nombre.StartsWith("Guardia") || nombre.StartsWith("Maton") || nombre.StartsWith("Seguridad"))
            return Resources.Load<Sprite>("Sprites/Enemigo");
        if (nombre.StartsWith("Pista") || nombre.StartsWith("Documento") || nombre.StartsWith("Evidencia"))
            return Resources.Load<Sprite>("Sprites/Pista");
        if (nombre.StartsWith("Puerta"))
            return Resources.Load<Sprite>("Sprites/Puerta");
        if (nombre.StartsWith("Muro") || nombre.StartsWith("Fachada"))
            return Resources.Load<Sprite>("Sprites/Muro");

        return null;
    }

    private void GenerarSprite()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        renderer.sortingOrder = ordenCapa != 0 ? ordenCapa : DeterminarOrdenPorDefecto();

        string n = gameObject.name;
        Sprite externo = IntentarCargarSpriteExterno(n);
        if (externo != null)
        {
            renderer.sprite = externo;
            renderer.color = Color.white;
            Vector2 bounds = externo.bounds.size;
            float sx = bounds.x > 0.001f ? tamaño.x / bounds.x : tamaño.x;
            float sy = bounds.y > 0.001f ? tamaño.y / bounds.y : tamaño.y;
            transform.localScale = new Vector3(sx, sy, 1f);
        }
        else
        {
            Texture2D texture = GenerarTexturaPixelArt(n);
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), (float)texture.width);
            renderer.color = Color.white;
            transform.localScale = new Vector3(tamaño.x, tamaño.y, 1f);
        }

        // El sprite mide 1x1 y el objeto se escala a "tamaño": un collider sólido debe medir 1x1 local
        // (así coincide exactamente con lo que se ve). Los triggers (puertas, pistas) conservan su tamaño.
        BoxCollider2D caja = GetComponent<BoxCollider2D>();
        if (caja != null && !caja.isTrigger)
        {
            caja.size = Vector2.one;
            caja.offset = Vector2.zero;
        }
    }

    private Texture2D GenerarTexturaPixelArt(string n)
    {
        const int res = 48;
        Texture2D tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        bool esPersonaje = circular && (n == "Artur" || n.StartsWith("Guardia") || n.StartsWith("Maton") || n.StartsWith("Seguridad") || n.StartsWith("NPC_"));
        bool esPista = n.StartsWith("Pista") || n.StartsWith("Documento") || n.StartsWith("Evidencia");
        bool esPuerta = n.StartsWith("Puerta");
        bool esMuro = n.StartsWith("Muro") || n.StartsWith("Fachada") || n.StartsWith("Columna");
        bool esCaja = n.StartsWith("Cajas") || n.StartsWith("Cajon");
        bool esPlanta = n.Contains("Planta");
        bool esSofa = n.Contains("Sofa") || n.Contains("Sillon");
        bool esMesa = n.Contains("Escritorio") || n.Contains("Mesa") || n.Contains("Mostrador") || n.Contains("Estanteria");
        bool esArchivador = n.Contains("Archivador");
        bool esSuelo = n.StartsWith("Suelo") || n.StartsWith("Asfalto") || n.StartsWith("Acera") || n.StartsWith("Parquet") || n.StartsWith("Baldosa") || n.StartsWith("Alfombra");

        if (esPersonaje)
        {
            PintarPersonajeTopDown(tex, res, n);
        }
        else if (esPista)
        {
            PintarCarpetaPista(tex, res);
        }
        else if (esPuerta)
        {
            PintarPuerta(tex, res);
        }
        else if (esMuro)
        {
            PintarMuroBiselado(tex, res);
        }
        else if (esCaja)
        {
            PintarCajaMadera(tex, res);
        }
        else if (esPlanta)
        {
            PintarPlantaMaceta(tex, res);
        }
        else if (esSofa)
        {
            PintarSofa(tex, res);
        }
        else if (esMesa)
        {
            PintarMesaMadera(tex, res, n.Contains("Escritorio"));
        }
        else if (esArchivador)
        {
            PintarArchivador(tex, res);
        }
        else if (circular)
        {
            PintarCirculoSuave(tex, res);
        }
        else if (esSuelo)
        {
            PintarSueloTexturizado(tex, res, n);
        }
        else
        {
            PintarRectanguloBiselado(tex, res);
        }

        tex.Apply();
        return tex;
    }

    // Personaje top-down con gabardina/hombros, cabeza, cabello y sombra proyectada
    private void PintarPersonajeTopDown(Texture2D tex, int res, string n)
    {
        Color sombra = new Color(0f, 0f, 0f, 0.42f);
        Color abrigo = color;
        Color abrigoOscuro = MultiplicarColor(color, 0.65f);
        Color abrigoClaro = MultiplicarColor(color, 1.22f);
        Color piel = new Color(0.92f, 0.76f, 0.62f, 1f);
        Color pelo = n == "Artur" ? new Color(0.16f, 0.12f, 0.10f, 1f)
                   : n.Contains("Valeria") ? new Color(0.38f, 0.08f, 0.18f, 1f)
                   : n.Contains("Cuervo") ? new Color(0.22f, 0.20f, 0.18f, 1f)
                   : new Color(0.12f, 0.12f, 0.14f, 1f);

        Vector2 centro = new Vector2(res * 0.5f, res * 0.5f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                tex.SetPixel(x, y, Color.clear);

                // Sombra bajo los pies
                float distSombra = Vector2.Distance(new Vector2(x, y), centro + new Vector2(2f, -3f));
                if (distSombra < 19f)
                    tex.SetPixel(x, y, sombra);

                // Cuerpo / Hombros (elipse ancha)
                float dx = (x - centro.x) / 17.5f;
                float dy = (y - (centro.y - 1.5f)) / 13.5f;
                float distCuerpo = dx * dx + dy * dy;
                if (distCuerpo <= 1f)
                {
                    if (distCuerpo > 0.80f)
                        tex.SetPixel(x, y, abrigoOscuro);
                    else if (Mathf.Abs(x - centro.x) < 3f && y < centro.y)
                        tex.SetPixel(x, y, abrigoOscuro); // Solapa de la gabardina
                    else if (y > centro.y + 2f)
                        tex.SetPixel(x, y, abrigoClaro);
                    else
                        tex.SetPixel(x, y, abrigo);
                }

                // Manos / Puños al frente
                float manoIzq = Vector2.Distance(new Vector2(x, y), centro + new Vector2(-11f, 9f));
                float manoDer = Vector2.Distance(new Vector2(x, y), centro + new Vector2(11f, 9f));
                if (manoIzq < 3.6f || manoDer < 3.6f)
                {
                    tex.SetPixel(x, y, (manoIzq > 2.6f && manoDer > 2.6f) ? abrigoOscuro : piel);
                }

                // Cabeza central
                float distCabeza = Vector2.Distance(new Vector2(x, y), centro + new Vector2(0f, 2f));
                if (distCabeza < 8.5f)
                {
                    if (distCabeza > 7.2f)
                        tex.SetPixel(x, y, abrigoOscuro);
                    else if (y < centro.y + 4.5f)
                        tex.SetPixel(x, y, pelo); // Cabello / Gorra vista desde arriba
                    else
                        tex.SetPixel(x, y, piel); // Rostro mirando al frente
                }
            }
        }
    }

    // Dossier / Carpeta confidencial con hoja blanca, texto y sello rojo
    private void PintarCarpetaPista(Texture2D tex, int res)
    {
        Color carpeta = color;
        Color bordeCarpeta = MultiplicarColor(color, 0.55f);
        Color papel = new Color(0.95f, 0.94f, 0.88f, 1f);
        Color lineaTexto = new Color(0.22f, 0.24f, 0.28f, 1f);
        Color selloRojo = new Color(0.85f, 0.18f, 0.18f, 1f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 4 || x >= res - 4 || y < 5 || y >= res - 5)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                // Borde exterior de la carpeta
                if (x < 7 || x >= res - 7 || y < 8 || y >= res - 8)
                {
                    tex.SetPixel(x, y, bordeCarpeta);
                    continue;
                }

                tex.SetPixel(x, y, carpeta);

                // Hoja de papel interior
                if (x >= 11 && x < res - 10 && y >= 10 && y < res - 10)
                {
                    tex.SetPixel(x, y, papel);

                    // Renglones de texto simulados
                    if (y % 5 == 0 && x >= 14 && x < res - 14 && y < res - 14)
                        tex.SetPixel(x, y, lineaTexto);

                    // Sello rojo "CONFIDENCIAL" en esquina inferior derecha
                    if (x >= res - 21 && x < res - 12 && y >= 12 && y < 19)
                        tex.SetPixel(x, y, selloRojo);
                }
            }
        }
    }

    // Puerta con marco metálico, paneles dobles y franja luminosa
    private void PintarPuerta(Texture2D tex, int res)
    {
        Color marco = MultiplicarColor(color, 0.45f);
        Color hoja = MultiplicarColor(color, 0.85f);
        Color brillo = MultiplicarColor(color, 1.35f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 3 || x >= res - 3 || y < 3 || y >= res - 3)
                    tex.SetPixel(x, y, marco);
                else if (Mathf.Abs(x - res / 2) <= 1)
                    tex.SetPixel(x, y, marco); // Junta central de puerta doble
                else if (y >= res - 9 && y < res - 5 && x > 8 && x < res - 8)
                    tex.SetPixel(x, y, brillo); // Indicador luminoso de entrada
                else
                    tex.SetPixel(x, y, hoja);
            }
        }
    }

    // Muro 3D top-down con bisel iluminado arriba, sombra abajo y juntas de ladrillo/hormigón
    private void PintarMuroBiselado(Texture2D tex, int res)
    {
        Color bordeOscuro = MultiplicarColor(color, 0.42f);
        Color biselLuz = MultiplicarColor(color, 1.30f);
        Color junta = MultiplicarColor(color, 0.78f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 2 || x >= res - 2 || y < 2 || y >= res - 2)
                    tex.SetPixel(x, y, bordeOscuro);
                else if (y >= res - 6 || x < 5)
                    tex.SetPixel(x, y, biselLuz);
                else if (y < 6 || x >= res - 5)
                    tex.SetPixel(x, y, MultiplicarColor(color, 0.65f));
                else if (y % 12 == 0 || ((x + (y / 12) * 16) % 24 == 0))
                    tex.SetPixel(x, y, junta);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Caja de madera industrial con refuerzos diagonales
    private void PintarCajaMadera(Texture2D tex, int res)
    {
        Color borde = MultiplicarColor(color, 0.50f);
        Color tablon = MultiplicarColor(color, 1.12f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                bool marco = x < 5 || x >= res - 5 || y < 5 || y >= res - 5;
                bool diagonal = Mathf.Abs(x - y) <= 2 || Mathf.Abs(x - (res - 1 - y)) <= 2;

                if (x < 2 || x >= res - 2 || y < 2 || y >= res - 2)
                    tex.SetPixel(x, y, MultiplicarColor(color, 0.35f));
                else if (marco || diagonal)
                    tex.SetPixel(x, y, tablon);
                else if (y % 8 == 0)
                    tex.SetPixel(x, y, borde);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Planta decorativa vista desde arriba con maceta de terracota
    private void PintarPlantaMaceta(Texture2D tex, int res)
    {
        Vector2 c = new Vector2(res * 0.5f, res * 0.5f);
        Color maceta = new Color(0.55f, 0.26f, 0.14f, 1f);
        Color hojaClara = MultiplicarColor(color, 1.35f);
        Color hojaOscura = MultiplicarColor(color, 0.65f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                if (d > 21f)
                    tex.SetPixel(x, y, Color.clear);
                else if (d > 17.5f)
                    tex.SetPixel(x, y, maceta);
                else if ((x + y) % 5 == 0 || d < 6f)
                    tex.SetPixel(x, y, hojaClara);
                else if ((x * y) % 7 == 0)
                    tex.SetPixel(x, y, hojaOscura);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Sofá acolchado con reposabrazos y costuras
    private void PintarSofa(Texture2D tex, int res)
    {
        Color borde = MultiplicarColor(color, 0.50f);
        Color cojin = MultiplicarColor(color, 1.15f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 2 || x >= res - 2 || y < 2 || y >= res - 2)
                    tex.SetPixel(x, y, borde);
                else if (y >= res - 10 || x < 7 || x >= res - 7)
                    tex.SetPixel(x, y, MultiplicarColor(color, 0.80f)); // Respaldo y apoyabrazos
                else if (x == res / 2 || x == res / 3 || x == (res * 2) / 3)
                    tex.SetPixel(x, y, borde); // Costura entre cojines
                else
                    tex.SetPixel(x, y, cojin);
            }
        }
    }

    // Escritorio o mesa de madera con veta y carpeta de trabajo
    private void PintarMesaMadera(Texture2D tex, int res, bool conCarpeta)
    {
        Color borde = MultiplicarColor(color, 0.48f);
        Color veta = MultiplicarColor(color, 0.88f);
        Color tapete = new Color(0.14f, 0.24f, 0.18f, 1f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 3 || x >= res - 3 || y < 3 || y >= res - 3)
                    tex.SetPixel(x, y, borde);
                else if (conCarpeta && x > 14 && x < res - 14 && y > 10 && y < res - 12)
                    tex.SetPixel(x, y, tapete);
                else if (y % 6 == 0)
                    tex.SetPixel(x, y, veta);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Archivador metálico con tiradores
    private void PintarArchivador(Texture2D tex, int res)
    {
        Color borde = MultiplicarColor(color, 0.50f);
        Color metalClaro = MultiplicarColor(color, 1.25f);
        Color tirador = new Color(0.78f, 0.80f, 0.84f, 1f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (x < 2 || x >= res - 2 || y < 2 || y >= res - 2 || y == res / 2)
                    tex.SetPixel(x, y, borde);
                else if (x > res / 2 - 5 && x < res / 2 + 5 && (y == 6 || y == res / 2 + 6))
                    tex.SetPixel(x, y, tirador);
                else if (x < 5 || y > res - 5)
                    tex.SetPixel(x, y, metalClaro);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Suelos con textura sutil de asfalto, parquet, hormigón o alfombra
    private void PintarSueloTexturizado(Texture2D tex, int res, string n)
    {
        Color variacionOscura = MultiplicarColor(color, 0.90f);
        Color variacionClara = MultiplicarColor(color, 1.10f);
        bool esBaldosa = n.StartsWith("Baldosa") || n.StartsWith("Acera");

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (esBaldosa && (x == 0 || y == 0 || x == res - 1 || y == res - 1))
                {
                    tex.SetPixel(x, y, variacionOscura);
                    continue;
                }

                int hash = ((x * 73856093) ^ (y * 19349663)) & 0xFF;
                if (hash < 22)
                    tex.SetPixel(x, y, variacionOscura);
                else if (hash > 238)
                    tex.SetPixel(x, y, variacionClara);
                else
                    tex.SetPixel(x, y, color);
            }
        }
    }

    private void PintarCirculoSuave(Texture2D tex, int res)
    {
        Vector2 c = new Vector2((res - 1) * 0.5f, (res - 1) * 0.5f);
        float radio = res * 0.46f;
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                tex.SetPixel(x, y, d <= radio ? color : Color.clear);
            }
        }
    }

    private void PintarRectanguloBiselado(Texture2D tex, int res)
    {
        Color borde = MultiplicarColor(color, 0.72f);
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                bool enBorde = color.a > 0.5f && (x == 0 || y == 0 || x == res - 1 || y == res - 1);
                tex.SetPixel(x, y, enBorde ? borde : color);
            }
        }
    }

    private static Color MultiplicarColor(Color baseColor, float factor)
    {
        return new Color(
            Mathf.Clamp01(baseColor.r * factor),
            Mathf.Clamp01(baseColor.g * factor),
            Mathf.Clamp01(baseColor.b * factor),
            baseColor.a
        );
    }
}

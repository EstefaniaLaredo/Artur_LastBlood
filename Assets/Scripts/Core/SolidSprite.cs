using UnityEngine;

public class SolidSprite : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Vector2 tamaño = Vector2.one;
    [SerializeField] private bool circular;

    private void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = gameObject.name == "Zona_Oficina" ? -10 : 1;
        if (gameObject.name == "Artur")
            renderer.sortingOrder = 10;
        else if (gameObject.name == "Documento_Mila")
            renderer.sortingOrder = 5;
        Texture2D texture = new Texture2D(32, 32);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                bool dentro = !circular || Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15f;
                texture.SetPixel(x, y, dentro ? color : Color.clear);
            }
        }
        texture.Apply();
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 32f);
        transform.localScale = new Vector3(tamaño.x, tamaño.y, 1f);
    }

    public void Configurar(Color nuevoColor, Vector2 nuevoTamaño, bool formaCircular)
    {
        color = nuevoColor;
        tamaño = nuevoTamaño;
        circular = formaCircular;
    }
}

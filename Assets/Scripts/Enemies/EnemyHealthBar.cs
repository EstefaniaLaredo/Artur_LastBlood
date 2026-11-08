using UnityEngine;

// Barra de vida flotante sobre la cabeza del enemigo creada proceduralmente con sprites.
// Mantiene proporciones estables y no se deforma al recibir daño.
public class EnemyHealthBar : MonoBehaviour
{
    private Transform barraRelleno;
    private Health salud;

    private float anchoRelleno = 1.3f;
    private float altoRelleno = 0.16f;

    private void Awake()
    {
        salud = GetComponentInParent<Health>();
        if (salud != null)
        {
            salud.OnVidaCambiada += ActualizarBarra;
        }
    }

    private void OnDestroy()
    {
        if (salud != null)
            salud.OnVidaCambiada -= ActualizarBarra;
    }

    private void LateUpdate()
    {
        // Mantener la barra siempre horizontal encima de la cabeza aunque el enemigo rote
        if (transform.parent != null)
        {
            transform.rotation = Quaternion.identity;
            transform.position = transform.parent.position + new Vector3(0f, 0.85f, 0f);
        }
    }

    public static EnemyHealthBar CrearBarra(GameObject enemigo)
    {
        GameObject contenedor = new GameObject("BarraVida_Enemigo");
        contenedor.transform.SetParent(enemigo.transform);
        contenedor.transform.localPosition = new Vector3(0f, 0.85f, 0f);

        // Compensar la escala del padre para que la barra siempre mida lo mismo
        Vector3 escalaPadre = enemigo.transform.localScale;
        float factorX = escalaPadre.x != 0 ? 1f / escalaPadre.x : 1f;
        float factorY = escalaPadre.y != 0 ? 1f / escalaPadre.y : 1f;
        contenedor.transform.localScale = new Vector3(factorX, factorY, 1f);

        // Marco / Fondo oscuro
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(contenedor.transform);
        fondo.transform.localPosition = Vector3.zero;
        SolidSprite spriteFondo = fondo.AddComponent<SolidSprite>();
        spriteFondo.Configurar(new Color(0.08f, 0.09f, 0.12f, 0.95f), new Vector2(1.38f, 0.24f), false, 15);

        // Barra de vida (Relleno rojo vivo)
        GameObject relleno = new GameObject("Relleno");
        relleno.transform.SetParent(contenedor.transform);
        relleno.transform.localPosition = Vector3.zero;
        SolidSprite spriteRelleno = relleno.AddComponent<SolidSprite>();
        spriteRelleno.Configurar(new Color(0.88f, 0.16f, 0.16f), new Vector2(1.3f, 0.16f), false, 16);

        EnemyHealthBar barra = contenedor.AddComponent<EnemyHealthBar>();
        barra.barraRelleno = relleno.transform;
        barra.anchoRelleno = 1.3f;
        barra.altoRelleno = 0.16f;

        return barra;
    }

    public void ActualizarBarra(int actual, int maximo)
    {
        if (barraRelleno == null || maximo <= 0)
            return;

        float porcentaje = Mathf.Clamp01((float)actual / maximo);

        // Mantiene SIEMPRE la altura fija (altoRelleno) y solo reduce el ancho de forma proporcional
        barraRelleno.localScale = new Vector3(porcentaje * anchoRelleno, altoRelleno, 1f);

        // Desplaza el centro para que se contraiga de derecha a izquierda
        float offset = (1f - porcentaje) * anchoRelleno * 0.5f;
        barraRelleno.localPosition = new Vector3(-offset, 0f, 0f);
    }
}

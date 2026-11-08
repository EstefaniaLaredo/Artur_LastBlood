using System;
using System.Collections;
using UnityEngine;

// Sistema de vida con eventos, parpadeo de daño y empuje por impacto.
public class Health : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private int vidaActual;

    public int VidaMaxima => vidaMaxima;
    public int VidaActual => vidaActual;
    public bool EstaVivo => vidaActual > 0;

    public event Action<int, int> OnVidaCambiada;
    public event Action OnMuerte;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal = Color.white;
    private Coroutine corrutinaParpadeo;
    private Rigidbody2D rb;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer != null)
            colorOriginal = spriteRenderer.color;
    }

    public void Configurar(int nuevaVidaMaxima)
    {
        vidaMaxima = nuevaVidaMaxima;
        vidaActual = vidaMaxima;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            colorOriginal = spriteRenderer.color;
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }

    public void RecibirDaño(int daño)
    {
        RecibirDaño(daño, null);
    }

    public void RecibirDaño(int daño, Vector2? puntoOrigen)
    {
        if (!EstaVivo)
            return;

        vidaActual = Mathf.Max(0, vidaActual - daño);
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);

        // Retroalimentación visual: parpadeo rojo/blanco
        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            if (corrutinaParpadeo != null)
                StopCoroutine(corrutinaParpadeo);
            corrutinaParpadeo = StartCoroutine(EfectoParpadeo());
        }

        // Empuje (retroceso) si hay un origen definido y un Rigidbody2D
        if (puntoOrigen.HasValue && rb != null)
        {
            Vector2 direccion = ((Vector2)transform.position - puntoOrigen.Value).normalized;
            rb.AddForce(direccion * 4f, ForceMode2D.Impulse);
        }

        if (vidaActual == 0)
        {
            OnMuerte?.Invoke();
            EnemySimple enemigo = GetComponent<EnemySimple>();
            if (enemigo != null)
                enemigo.Morir();
        }
    }

    private IEnumerator EfectoParpadeo()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = new Color(1f, 0.25f, 0.25f);
        yield return new WaitForSeconds(0.12f);
        if (spriteRenderer != null)
            spriteRenderer.color = colorOriginal;
    }
}

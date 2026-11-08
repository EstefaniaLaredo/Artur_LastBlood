using UnityEngine;

// Combate cuerpo a cuerpo de Artur Jones con retroalimentación visual de impacto estilo beat 'em up.
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float alcance = 1.45f;
    [SerializeField] private int daño = 1;
    [SerializeField] private float cadenciaAtaque = 0.35f;
    private float siguienteAtaque;

    private void Update()
    {
        if (DialogueUI.EstaEnDialogo)
            return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= siguienteAtaque)
        {
            siguienteAtaque = Time.time + cadenciaAtaque;
            Atacar();
        }
    }

    private void Atacar()
    {
        Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position, alcance);
        bool golpeAcertado = false;

        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.gameObject == gameObject)
                continue;

            EnemySimple enemigo = objetivo.GetComponent<EnemySimple>();
            if (enemigo != null)
            {
                enemigo.RecibirDaño(daño, transform.position);
                GenerarDestelloImpacto(enemigo.transform.position);
                golpeAcertado = true;
            }
        }

        // Si no golpeó a ningún enemigo, generar un rastro de golpe en la dirección hacia donde mira Artur
        if (!golpeAcertado)
        {
            GenerarDestelloImpacto(transform.position + transform.up * 0.85f, new Color(0.9f, 0.9f, 1f, 0.5f));
        }
    }

    private void GenerarDestelloImpacto(Vector3 posicion, Color? color = null)
    {
        GameObject destello = new GameObject("FX_ChispaImpacto");
        destello.transform.position = Vector3.Lerp(transform.position, posicion, 0.65f);

        SolidSprite sprite = destello.AddComponent<SolidSprite>();
        sprite.Configurar(color ?? new Color(1f, 0.85f, 0.25f, 0.95f), new Vector2(0.55f, 0.55f), true, 25);

        Destroy(destello, 0.08f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}

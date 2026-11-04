
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float suavidad = 5f;
    [SerializeField] private Vector2 limitesMinimos = new Vector2(-14f, -8f);
    [SerializeField] private Vector2 limitesMaximos = new Vector2(14f, 8f);
    private Camera camara;

    private void Awake()
    {
        camara = GetComponent<Camera>();
        EnlazarConJugador();
    }

    private void Start()
    {
        EnlazarConJugador();
        SeguirInmediatamente();
    }

    private void EnlazarConJugador()
    {
        if (objetivo != null)
            return;

        PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
        if (jugador != null)
            objetivo = jugador.transform;
    }

    public void Configurar(Transform nuevoObjetivo, Vector2 minimos, Vector2 maximos)
    {
        objetivo = nuevoObjetivo;
        limitesMinimos = minimos;
        limitesMaximos = maximos;
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            EnlazarConJugador();
            if (objetivo == null)
                return;
        }

        Vector3 posicionDeseada = new Vector3(
            objetivo.position.x,
            objetivo.position.y,
            transform.position.z
        );

        if (camara == null)
            camara = GetComponent<Camera>();

        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;
        float minimoX = limitesMinimos.x + mitadAncho;
        float maximoX = limitesMaximos.x - mitadAncho;
        float minimoY = limitesMinimos.y + mitadAlto;
        float maximoY = limitesMaximos.y - mitadAlto;

        if (minimoX > maximoX)
        {
            minimoX = (limitesMinimos.x + limitesMaximos.x) * .5f;
            maximoX = minimoX;
        }
        if (minimoY > maximoY)
        {
            minimoY = (limitesMinimos.y + limitesMaximos.y) * .5f;
            maximoY = minimoY;
        }

        Vector3 posicionLimitada = new Vector3(
            Mathf.Clamp(posicionDeseada.x, minimoX, maximoX),
            Mathf.Clamp(posicionDeseada.y, minimoY, maximoY),
            posicionDeseada.z);

        Vector3 posicionAnterior = transform.position;
        transform.position = Vector3.Lerp(
            transform.position,
            posicionLimitada,
            1f - Mathf.Exp(-suavidad * Time.deltaTime));

        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y))
            transform.position = posicionAnterior;
    }

    public void SeguirInmediatamente()
    {
        if (objetivo == null)
            return;
        transform.position = new Vector3(objetivo.position.x, objetivo.position.y, transform.position.z);
    }
}

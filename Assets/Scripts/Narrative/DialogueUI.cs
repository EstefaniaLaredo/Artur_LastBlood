using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Interfaz cinematográfica para conversaciones en pantalla completa o barra inferior.
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instancia { get; private set; }
    public static bool EstaEnDialogo { get; private set; }

    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text nombreText;
    [SerializeField] private TMP_Text contenidoText;
    [SerializeField] private TMP_Text ayudaText;

    private readonly Queue<DialogueLine> colaLineas = new Queue<DialogueLine>();
    private Action onDialogoTerminado;

    public bool EstaActivo => EstaEnDialogo;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        if (panelDialogo != null)
            panelDialogo.SetActive(false);
        EstaEnDialogo = false;
    }

    public void Configurar(GameObject panel, TMP_Text nombre, TMP_Text contenido, TMP_Text ayuda)
    {
        panelDialogo = panel;
        nombreText = nombre;
        contenidoText = contenido;
        ayudaText = ayuda;

        if (panelDialogo != null)
            panelDialogo.SetActive(false);
    }

    public void IniciarDialogo(DialogueLine[] lineas, Action alTerminar = null)
    {
        if (lineas == null || lineas.Length == 0)
            return;

        colaLineas.Clear();
        foreach (DialogueLine linea in lineas)
            colaLineas.Enqueue(linea);

        onDialogoTerminado = alTerminar;
        EstaEnDialogo = true;

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        MostrarSiguienteLinea();
    }

    private void Update()
    {
        if (!EstaEnDialogo)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            MostrarSiguienteLinea();
        }
    }

    private void MostrarSiguienteLinea()
    {
        if (colaLineas.Count == 0)
        {
            TerminarDialogo();
            return;
        }

        DialogueLine lineaActual = colaLineas.Dequeue();

        if (nombreText != null)
            nombreText.text = lineaActual.personaje;

        if (contenidoText != null)
            contenidoText.text = lineaActual.texto;

        if (ayudaText != null)
            ayudaText.text = "[E / ESPACIO] Continuar";
    }

    private void TerminarDialogo()
    {
        EstaEnDialogo = false;
        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        Action callback = onDialogoTerminado;
        onDialogoTerminado = null;
        callback?.Invoke();
    }
}


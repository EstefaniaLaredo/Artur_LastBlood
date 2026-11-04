
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EvidenceJournal : MonoBehaviour
{
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalText;

    private readonly List<Pista> pistas = new List<Pista>();
    private string objetivoActual = "Regresa a tu oficina y busca una respuesta.";

    public void Configurar(GameObject panel, TMP_Text texto)
    {
        journalPanel = panel;
        journalText = texto;
    }

    private void Start()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
        ActualizarDiario();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (journalPanel != null)
                journalPanel.SetActive(!journalPanel.activeSelf);
        }
    }

    public void AgregarPista(string titulo, string descripcion)
    {
        AgregarPista(titulo, descripcion, "Desconocido", "Fecha desconocida");
    }

    public void AgregarPista(string titulo, string descripcion, string lugar, string fecha)
    {
        if (pistas.Exists(p => p.titulo == titulo))
            return;
        pistas.Add(new Pista(titulo, descripcion, lugar, fecha));
        ActualizarDiario();
        Debug.Log("Nueva pista agregada al diario.");
    }

    public void EstablecerObjetivo(string nuevoObjetivo)
    {
        objetivoActual = nuevoObjetivo;
        ActualizarDiario();
    }

    private void ActualizarDiario()
    {
        if (journalText == null)
            return;

        if (pistas.Count == 0)
        {
            journalText.text =
                "DIARIO DE INVESTIGACIÓN\n\nOBJETIVO\n" + objetivoActual +
                "\n\nTodavía no encontraste pistas.";
            return;
        }

        string contenido = "DIARIO DE INVESTIGACIÓN\n\nOBJETIVO\n" + objetivoActual + "\n\nPISTAS\n";
        foreach (Pista pista in pistas)
            contenido += "\n• " + pista.titulo + "\n" + pista.descripcion +
                         "\nLugar: " + pista.lugar + "\nFecha: " + pista.fecha + "\n";
        journalText.text = contenido;
    }

    [System.Serializable]
    private class Pista
    {
        public string titulo;
        public string descripcion;
        public string lugar;
        public string fecha;

        public Pista(string titulo, string descripcion, string lugar, string fecha)
        {
            this.titulo = titulo;
            this.descripcion = descripcion;
            this.lugar = lugar;
            this.fecha = fecha;
        }
    }
}

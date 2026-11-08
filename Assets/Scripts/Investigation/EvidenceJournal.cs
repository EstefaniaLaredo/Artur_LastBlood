using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Diario de investigación ampliado con soporte para objetivos, estados de lugares, contactos y pistas.
public class EvidenceJournal : MonoBehaviour
{
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalText;

    private readonly List<Pista> pistas = new List<Pista>();
    private readonly List<string> contactos = new List<string>();
    private string objetivoActual = "Investiga tu oficina y busca respuestas.";

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
        if (DialogueUI.EstaEnDialogo)
            return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (journalPanel != null)
                journalPanel.SetActive(!journalPanel.activeSelf);
        }
    }

    public void AgregarPista(string titulo, string descripcion)
    {
        AgregarPista(titulo, descripcion, "Oficina de Artur", "Noche del 14 de octubre");
    }

    public void AgregarPista(string titulo, string descripcion, string lugar, string fecha)
    {
        if (pistas.Exists(p => p.titulo == titulo))
            return;

        pistas.Add(new Pista(titulo, descripcion, lugar, fecha));
        ActualizarDiario();
        Debug.Log($"[DIARIO] Pista agregada: {titulo}");
    }

    public void AgregarContacto(string nombreYDetalle)
    {
        if (!contactos.Contains(nombreYDetalle))
        {
            contactos.Add(nombreYDetalle);
            ActualizarDiario();
        }
    }

    public void EstablecerObjetivo(string nuevoObjetivo)
    {
        objetivoActual = nuevoObjetivo;
        ActualizarDiario();
    }

    public void RefrescarVista()
    {
        ActualizarDiario();
    }

    private void ActualizarDiario()
    {
        if (journalText == null)
            return;

        string contenido = "<b><size=22><color=#FFC107>DIARIO DE INVESTIGACIÓN — ARTUR JONES</color></size></b>\n\n";

        // Sección de Objetivo
        contenido += "<b><color=#4FC3F7>OBJETIVO ACTUAL</color></b>\n" + objetivoActual + "\n\n";

        // Sección de Estados de Lugares
        if (LocationStateManager.Instancia != null)
        {
            contenido += LocationStateManager.Instancia.GenerarTextoResumenLugares() + "\n";
        }

        // Sección de Contactos
        if (contactos.Count > 0)
        {
            contenido += "<b><color=#BA68C8>PERSONAJES Y CONTACTOS</color></b>\n";
            foreach (string contacto in contactos)
                contenido += $"• {contacto}\n";
            contenido += "\n";
        }

        // Sección de Pistas
        contenido += "<b><color=#FFD54F>EVIDENCIAS Y PISTAS REGISTRADAS</color></b>\n";
        if (pistas.Count == 0)
        {
            contenido += "<i>Todavía no has recogido pistas.</i>\n";
        }
        else
        {
            foreach (Pista pista in pistas)
            {
                contenido += $"• <b>{pista.titulo}</b>\n";
                contenido += $"  {pista.descripcion}\n";
                contenido += $"  <i><color=#90A4AE>Lugar: {pista.lugar} | Fecha: {pista.fecha}</color></i>\n";
            }
        }

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

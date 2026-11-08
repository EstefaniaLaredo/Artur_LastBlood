// Estados de investigación por lugar según la sección 8 del GDD.
public enum LocationInvestigationState
{
    NoInvestigado,               // Todavía no se ha visitado
    InvestigadoParcialmente,     // Se encontraron algunas pistas, pero falta información
    NuevaInformacion,            // Una pista o acontecimiento permite regresar y descubrir algo nuevo
    InvestigacionCompletada      // Se han encontrado las evidencias necesarias
}


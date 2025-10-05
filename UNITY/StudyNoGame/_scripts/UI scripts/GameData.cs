using System.Collections.Generic;

/// <summary>
/// Contenedor estático (Singleton de datos) para almacenar información global del juego,
/// accesible desde cualquier script sin necesidad de referencias a GameObjects.
/// </summary>
public static class GameData
{
    /// <summary>
    /// Lista de Kanjis seleccionados por el jugador en el menú principal.
    /// Esta lista persiste mientras el juego está activo (no se destruye con las escenas).
    /// </summary>
    public static List<string> kanjisSelect = new List<string>();

    /// <summary>
    /// Almacena el Kanji que el jugador debe buscar, identificar o 'disparar'
    /// en los niveles de juego (ej. el Kanji objetivo actual en un juego de Shooter).
    /// </summary>
    public static string kanjiObjetivo;
}
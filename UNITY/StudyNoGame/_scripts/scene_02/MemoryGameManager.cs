using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controlador principal del minijuego de memoria (Memory Game).
/// Se encarga de la generación de pares de Kanjis, la gestión del estado del juego
/// y la verificación de las tarjetas seleccionadas por el jugador.
/// </summary>
public class MemoryGameManager : MonoBehaviour
{
    public static MemoryGameManager Instance; // Implementación del patrón Singleton.

    [Header("Configuración de Spawn")]
    public GameObject kanjiMemoryPrefab; // Prefab de la tarjeta individual (debe tener KanjiMemoryCard adjunto).
    public Transform spawnArea;         // Transform (opcional) que actuaría como padre de las tarjetas.
    public int paresACrear = 5;         // Número de pares de Kanjis a crear (total de tarjetas = paresACrear * 2).

    // --- Variables de Estado de Juego ---
    private List<GameObject> tarjetas = new List<GameObject>(); // Lista de todas las tarjetas instanciadas.
    private KanjiMemoryCard tarjetaSeleccionada1 = null;      // Primera tarjeta volteada en el turno actual.
    private KanjiMemoryCard tarjetaSeleccionada2 = null;      // Segunda tarjeta volteada en el turno actual.
    private bool puedeSeleccionar = true;                     // Bandera para prevenir selección de tarjetas durante la espera (yield return).

    private void Awake()
    {
        // Lógica del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        IniciarJuego();
    }

    /// <summary>
    /// Prepara el tablero para una nueva partida, destruyendo tarjetas viejas y creando nuevas.
    /// </summary>
    public void IniciarJuego()
    {
        // Destruye todos los GameObjects de tarjetas de la partida anterior.
        foreach (var tarjeta in tarjetas)
        {
            // Usamos el operador ternario para verificar si el objeto existe antes de intentar destruirlo
            if (tarjeta != null) Destroy(tarjeta);
        }
        tarjetas.Clear(); // Limpia la lista de referencias.

        CrearPares(); // Llama al método para generar y posicionar las nuevas tarjetas.
    }

    /// <summary>
    /// Crea los pares de Kanjis, los mezcla y los instancia en posiciones de grid.
    /// </summary>
    private void CrearPares()
    {
        if (GameData.kanjisSelect.Count == 0)
        {
            Debug.LogError("No hay kanjis en la lista de GameData para crear pares!");
            return;
        }

        List<string> kanjisParaPares = new List<string>();

        // 1. Crea la lista duplicada de Kanjis para formar pares.
        for (int i = 0; i < Mathf.Min(paresACrear, GameData.kanjisSelect.Count); i++)
        {
            // Añade cada Kanji dos veces a la lista.
            kanjisParaPares.Add(GameData.kanjisSelect[i]);
            kanjisParaPares.Add(GameData.kanjisSelect[i]);
        }

        // 2. Mezcla la lista para asignar Kanjis aleatoriamente a las posiciones.
        kanjisParaPares = MezclarLista(kanjisParaPares);

        // 3. Calcula las posiciones en el mundo (grid).
        Vector3[] posiciones = CalcularPosicionesGrid(kanjisParaPares.Count);

        // 4. Instancia las tarjetas.
        for (int i = 0; i < kanjisParaPares.Count; i++)
        {
            GameObject tarjeta = Instantiate(kanjiMemoryPrefab, posiciones[i], Quaternion.identity);
            string kanjiAsignado = kanjisParaPares[i].Trim();

            KanjiMemoryCard memoryCard = tarjeta.GetComponent<KanjiMemoryCard>();
            if (memoryCard != null)
            {
                // Inicializa la tarjeta con su Kanji y una referencia a este GameManager.
                memoryCard.Inicializar(kanjiAsignado, this);
            }

            tarjetas.Add(tarjeta);
        }
    }

    /// <summary>
    /// Punto de entrada llamado por cada tarjeta individual (KanjiMemoryCard) cuando es seleccionada.
    /// </summary>
    /// <param name="tarjeta">La tarjeta que acaba de ser volteada.</param>
    public void OnTarjetaSeleccionada(KanjiMemoryCard tarjeta)
    {
        // 1. Verificación de reglas (Ignora si: no se puede seleccionar, ya se seleccionó, o ya está emparejada).
        if (!puedeSeleccionar || tarjeta == tarjetaSeleccionada1 || tarjeta.fueEmparejada)
            return;

        if (tarjetaSeleccionada1 == null)
        {
            // Primera selección: guarda la tarjeta.
            tarjetaSeleccionada1 = tarjeta;
        }
        else
        {
            // Segunda selección: guarda la segunda tarjeta e inicia la verificación.
            tarjetaSeleccionada2 = tarjeta;
            puedeSeleccionar = false; // Bloquea la selección de más tarjetas.
            StartCoroutine(VerificarPar());
        }
    }

    /// <summary>
    /// Coroutine que espera un momento y luego compara las dos tarjetas seleccionadas.
    /// </summary>
    private IEnumerator VerificarPar()
    {
        yield return new WaitForSeconds(1f); // Espera 1 segundo para que el jugador vea las tarjetas.

        // Comprueba si los Kanjis de ambas tarjetas coinciden.
        if (tarjetaSeleccionada1.kanjiTexto == tarjetaSeleccionada2.kanjiTexto)
        {
            // Par Correcto: Las tarjetas se quedan volteadas.
            tarjetaSeleccionada1.MostrarTextoPermanente();
            tarjetaSeleccionada2.MostrarTextoPermanente();
        }
        else
        {
            // Par Incorrecto: Se ocultan de nuevo (vuelven a estar boca abajo).
            tarjetaSeleccionada1.OcultarTexto();
            tarjetaSeleccionada2.OcultarTexto();
        }

        // Restablece las referencias de las tarjetas seleccionadas.
        tarjetaSeleccionada1 = null;
        tarjetaSeleccionada2 = null;
        puedeSeleccionar = true; // Desbloquea la selección para el siguiente turno.

        VerificarVictoria(); // Comprueba si todas las tarjetas han sido emparejadas.
    }

    /// <summary>
    /// Itera sobre todas las tarjetas para ver si todas están marcadas como emparejadas.
    /// </summary>
    private void VerificarVictoria()
    {
        foreach (var tarjeta in tarjetas)
        {
            KanjiMemoryCard memoryCard = tarjeta.GetComponent<KanjiMemoryCard>();
            // Si encuentra CUALQUIER tarjeta que NO haya sido emparejada, sale.
            if (memoryCard != null && !memoryCard.fueEmparejada)
                return;
        }

        // Si el bucle termina sin encontrar tarjetas no emparejadas: ¡Victoria!
        Debug.Log("¡Victoria!");
        StartCoroutine(ReiniciarJuego());
    }

    /// <summary>
    /// Coroutine que espera un momento y luego llama a IniciarJuego() para una nueva partida.
    /// </summary>
    private IEnumerator ReiniciarJuego()
    {
        yield return new WaitForSeconds(2f);
        IniciarJuego();
    }

    /// <summary>
    /// Calcula las posiciones de las tarjetas en un formato de grid centrado en la cámara.
    /// Asume un diseño de 2 filas y 5 columnas.
    /// </summary>
    private Vector3[] CalcularPosicionesGrid(int totalTarjetas)
    {
        Vector3[] posiciones = new Vector3[totalTarjetas];
        Camera cam = Camera.main;

        // Define límites X para centrar el grid en la pantalla.
        float minX = cam.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x;
        float maxX = cam.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x;

        // Define la altura para las dos filas (basado en el Viewport Y).
        float yFilaInferior = cam.ViewportToWorldPoint(new Vector3(0, 0.35f, 0)).y;
        float yFilaSuperior = cam.ViewportToWorldPoint(new Vector3(0, 0.65f, 0)).y;

        int gridCols = 5; // Asume 5 columnas.
        float totalWidth = maxX - minX;

        // Calcula el espaciado y el margen.
        float espaciadoDeseado = 3.1f;
        float spacingX = espaciadoDeseado;

        // Calcula el margen izquierdo para centrar el grid completo horizontalmente.
        float espacioTotalNecesario = (gridCols - 1) * spacingX;
        float margenIzquierdo = (totalWidth - espacioTotalNecesario) / 2;

        for (int i = 0; i < totalTarjetas; i++)
        {
            int row = i / gridCols; // Fila (0 o 1)
            int col = i % gridCols; // Columna (0 a 4)

            // Calcula la posición X: inicio + margen + (columna * espaciado).
            float x = minX + margenIzquierdo + (col * spacingX);

            // Asigna la posición Y según la fila.
            float y = (row == 0) ? yFilaInferior : yFilaSuperior;

            posiciones[i] = new Vector3(x, y, 0);
        }

        return posiciones;
    }

    /// <summary>
    /// Implementación del algoritmo de mezcla (shuffle) Fisher-Yates para aleatorizar la lista.
    /// </summary>
    private List<string> MezclarLista(List<string> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            int rnd = Random.Range(i, lista.Count);
            string temp = lista[i];
            lista[i] = lista[rnd];
            lista[rnd] = temp;
        }
        return lista;
    }
}
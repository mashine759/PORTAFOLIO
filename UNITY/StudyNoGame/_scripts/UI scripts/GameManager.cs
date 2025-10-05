using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para usar Coroutines (IEnumerator)
using System.Collections.Generic; // Necesario para usar Listas
using TMPro; // Para usar TextMeshPro

/// <summary>
/// GameManager principal de la escena de juego.
/// Controla la generaci�n (spawn), el reordenamiento, la asignaci�n de objetivos 
/// y el flujo del juego basado en los Kanjis seleccionados.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Implementaci�n del patr�n Singleton.

    public Transform spawnArea;      // �rea o padre donde se generar�n las tarjetas (opcional, usado para mantener el orden).
    public GameObject kanjiPrefab;    // Prefab de la tarjeta/enemigo que contiene el Kanji y el componente KanjiTarget.
    public int cantidad = 5;         // N�mero de tarjetas a spawnear por ronda.

    [Header("UI")]
    public TMP_Text objetivoText;    // Componente de texto que muestra el Kanji objetivo.
    public GameObject objetivoPanel;  // Panel para mostrar/ocultar el objetivo.

    [Header("UI - Bot�n Reordenar")]
    public Button botonReordenar;           // Bot�n para reordenar las tarjetas.
    public float tiempoEsperaReordenar = 1f; // Tiempo de espera antes de que el bot�n de reordenar se reactive.

    private string objetivoBaseText; // Almacena el texto base (ej. "Encuentra:") antes del Kanji.

    /// <summary>
    /// Guarda el texto base del objetivo (se usa para componer el mensaje final).
    /// </summary>
    public void SetObjetivoBaseText(string text)
    {
        objetivoBaseText = text;
    }


    private void Awake()
    {
        // L�gica del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Debug.Log("GameManager iniciando...");
        Debug.Log("Kanjis disponibles: " + GameData.kanjisSelect.Count);

        // Verificaci�n de referencias esenciales
        if (spawnArea == null)
        {
            Debug.LogError("spawnArea no est� asignado en el Inspector");
            GameObject tempSpawn = new GameObject("TempSpawnArea");
            spawnArea = tempSpawn.transform;
            spawnArea.position = Vector3.zero;
        }

        if (kanjiPrefab == null)
        {
            Debug.LogError("kanjiPrefab no est� asignado en el Inspector!");
            return;
        }

        SpawnTarjetas(); // Inicia la generaci�n inicial de tarjetas.

        // Configuraci�n inicial del bot�n Reordenar
        if (botonReordenar != null)
        {
            botonReordenar.onClick.AddListener(ReordenarTarjetas);
            botonReordenar.gameObject.SetActive(true);
        }

        // Guarda el texto inicial del objetivo (ej. "Encuentra: ") para usarlo despu�s.
        objetivoBaseText = objetivoText.text;

    }

    // M�TODO PARA REORDENAR
    /// <summary>
    /// Inicia el proceso de destruir las tarjetas existentes y respawnearlas inmediatamente
    /// en nuevas posiciones basadas en el grid.
    /// </summary>
    public void ReordenarTarjetas()
    {
        StartCoroutine(ReordenarCoroutine());
    }

    private IEnumerator ReordenarCoroutine()
    {
        // 1. Desactivar bot�n para prevenir clics dobles.
        if (botonReordenar != null)
            botonReordenar.interactable = false;

        // 2. Recolectar datos y destruir las tarjetas actuales.
        KanjiTarget[] tarjetas = FindObjectsOfType<KanjiTarget>();
        List<string> kanjisActuales = new List<string>();

        foreach (var tarjeta in tarjetas)
        {
            kanjisActuales.Add(tarjeta.kanjiTexto);
            Destroy(tarjeta.gameObject);
        }

        yield return new WaitForSeconds(0.5f); // Pausa visual.

        // 3. Generar nuevas posiciones y respawnear las tarjetas con los mismos Kanjis.
        Camera mainCamera = Camera.main;
        Vector3[] nuevasPosiciones = CalcularPosicionesGrid(kanjisActuales.Count, mainCamera);

        for (int i = 0; i < kanjisActuales.Count; i++)
        {
            GameObject tarjeta = Instantiate(kanjiPrefab, nuevasPosiciones[i], Quaternion.identity);
            string kanjiAsignado = kanjisActuales[i];

            KanjiTarget target = tarjeta.GetComponent<KanjiTarget>();
            target.Inicializar(kanjiAsignado);

            AsignarTexto(tarjeta, kanjiAsignado); // Asigna el texto visualmente.
        }

        yield return new WaitForSeconds(tiempoEsperaReordenar); // Espera antes de reactivar.

        // 4. Reactivar bot�n.
        if (botonReordenar != null)
            botonReordenar.interactable = true;
    }

    /// <summary>
    /// Genera la lista inicial de tarjetas basadas en los Kanjis seleccionados en GameData.
    /// </summary>
    public void SpawnTarjetas()
    {
        if (GameData.kanjisSelect.Count == 0)
        {
            Debug.LogError("No hay kanjis en la lista! (GameData.kanjisSelect est� vac�o)");
            return;
        }

        // 1. Prepara y mezcla la lista de Kanjis.
        List<string> kanjis = new List<string>(GameData.kanjisSelect);
        kanjis = MezclarLista(kanjis);

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No hay c�mara principal!");
            return;
        }

        // 2. Define los l�mites de spawn en coordenadas de mundo usando el Viewport.
        float minX = mainCamera.ViewportToWorldPoint(new Vector3(0.1f, 0, 0)).x;
        float maxX = mainCamera.ViewportToWorldPoint(new Vector3(0.9f, 0, 0)).x;
        float minY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.2f, 0)).y;
        float maxY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.8f, 0)).y;

        // 3. Itera para spawnear las tarjetas en posiciones aleatorias.
        for (int i = 0; i < cantidad && i < kanjis.Count; i++)
        {
            // Calcula posici�n aleatoria dentro de los l�mites definidos.
            Vector3 pos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );

            GameObject tarjeta = Instantiate(kanjiPrefab, pos, Quaternion.identity);
            string kanjiAsignado = kanjis[i].Trim();

            // Inicializa el componente KanjiTarget con el Kanji.
            KanjiTarget target = tarjeta.GetComponent<KanjiTarget>();
            if (target != null)
            {
                target.Inicializar(kanjiAsignado);
            }
            // Asigna el texto visualmente (utilizando el m�todo AsignarTexto).
            AsignarTexto(tarjeta, kanjiAsignado);
        }

        // 4. Selecciona un Kanji objetivo de la lista para la ronda.
        GameData.kanjiObjetivo = kanjis[Random.Range(0, Mathf.Min(cantidad, kanjis.Count))];
        Debug.Log($"Objetivo seleccionado: {GameData.kanjiObjetivo}");

        // 5. Muestra el objetivo.
        StartCoroutine(MostrarObjetivo());
    }


    /// <summary>
    /// Calcula posiciones espaciadas tipo grid dentro del �rea de la c�mara.
    /// Usado para el reordenamiento.
    /// </summary>
    private Vector3[] CalcularPosicionesGrid(int cantidad, Camera cam)
    {
        Vector3[] posiciones = new Vector3[cantidad];

        // Define l�mites de spawn basados en el Viewport (0.1 a 0.9, etc.)
        float minX = cam.ViewportToWorldPoint(new Vector3(0.1f, 0, 0)).x;
        float maxX = cam.ViewportToWorldPoint(new Vector3(0.9f, 0, 0)).x;
        float minY = cam.ViewportToWorldPoint(new Vector3(0, 0.2f, 0)).y;
        float maxY = cam.ViewportToWorldPoint(new Vector3(0, 0.8f, 0)).y;

        // Calcula dimensiones del grid (ej. si son 5, 3x2).
        int gridCols = Mathf.CeilToInt(Mathf.Sqrt(cantidad));
        int gridRows = Mathf.CeilToInt((float)cantidad / gridCols);

        // Calcula el espaciado entre las posiciones.
        float spacingX = (maxX - minX) / (gridCols + 1);
        float spacingY = (maxY - minY) / (gridRows + 1);

        for (int i = 0; i < cantidad; i++)
        {
            int row = i / gridCols;
            int col = i % gridCols;

            // Calcula la posici�n en el mundo, a�adiendo el espaciado inicial.
            float x = minX + spacingX + (col * spacingX);
            float y = minY + spacingY + (row * spacingY);

            // Agrega una peque�a variaci�n aleatoria para un look menos rob�tico.
            float randomOffsetX = Random.Range(-spacingX * 0.2f, spacingX * 0.2f);
            float randomOffsetY = Random.Range(-spacingY * 0.2f, spacingY * 0.2f);

            posiciones[i] = new Vector3(x + randomOffsetX, y + randomOffsetY, 0);
        }

        return posiciones;
    }

    /// <summary>
    /// Llamado cuando el jugador selecciona el Kanji objetivo correctamente.
    /// Destruye las tarjetas actuales e inicia el spawn de la siguiente ronda.
    /// </summary>
    public void KanjiCorrecto()
    {
        // Destruye todas las tarjetas de la ronda.
        KanjiTarget[] tarjetas = FindObjectsOfType<KanjiTarget>();
        foreach (var t in tarjetas)
        {
            Destroy(t.gameObject);
        }

        // Inicia el spawn de la pr�xima ronda con un delay.
        StartCoroutine(SpawnTarjetasDelay());
    }

    private IEnumerator SpawnTarjetasDelay()
    {
        yield return new WaitForSeconds(2f); // Pausa de 2 segundos.
        SpawnTarjetas(); // Genera las nuevas tarjetas.
        StartCoroutine(MostrarObjetivo()); // Muestra el nuevo objetivo.
    }

    /// <summary>
    /// Muestra el panel del objetivo temporalmente al inicio de la ronda.
    /// </summary>
    private IEnumerator MostrarObjetivo()
    {
        if (objetivoPanel != null && objetivoText != null)
        {
            // Espera a que el objetivo haya sido asignado en GameData.
            while (string.IsNullOrEmpty(GameData.kanjiObjetivo))
            {
                yield return null;
            }

            // Espera a que el texto base (ej. "Encuentra:") haya sido guardado en Start().
            while (string.IsNullOrEmpty(objetivoBaseText))
            {
                yield return null;
            }

            objetivoPanel.SetActive(true);
            objetivoText.text = $"{objetivoBaseText} {GameData.kanjiObjetivo}"; // Combina texto base y objetivo.

            yield return new WaitForSeconds(2f); // Muestra por 2 segundos.
            objetivoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Guarda el texto inicial del objetivo. (Aunque ya est� en Start, este m�todo es redundante y puede eliminarse si no se usa externamente).
    /// </summary>
    public void GuardarTextoBaseObjetivo()
    {
        objetivoBaseText = objetivoText.text;
    }

    /// <summary>
    /// Implementaci�n del algoritmo de mezcla (shuffle) Fisher-Yates.
    /// Reordena aleatoriamente los elementos de una lista.
    /// </summary>
    private List<string> MezclarLista(List<string> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            int rnd = Random.Range(i, lista.Count); // Selecciona un �ndice aleatorio en la parte restante de la lista.
            string temp = lista[i];                 // Swap:
            lista[i] = lista[rnd];
            lista[rnd] = temp;
        }
        return lista;
    }

    /// <summary>
    /// M�todo utilitario que busca TextMeshPro o TextMesh en los hijos de la tarjeta
    /// y asigna el Kanji al componente de texto.
    /// </summary>
    private void AsignarTexto(GameObject tarjeta, string texto)
    {
        // Intenta encontrar TextMeshPro
        TextMeshPro tmpText = tarjeta.GetComponentInChildren<TextMeshPro>();
        if (tmpText != null)
        {
            Debug.Log($"Encontrado TextMeshPro en {tarjeta.name}");
            tmpText.text = texto;
            return;
        }

        // Si falla, intenta encontrar TextMesh (Legacy)
        TextMesh textMesh = tarjeta.GetComponentInChildren<TextMesh>();
        if (textMesh != null)
        {
            Debug.Log($"Encontrado TextMesh en {tarjeta.name}");
            textMesh.text = texto;
            return;
        }

        // Debug de error si no se encuentra ning�n componente de texto.
        Debug.LogError($"No se encontr� texto en {tarjeta.name}. Hijos: {tarjeta.transform.childCount}");
    }
}
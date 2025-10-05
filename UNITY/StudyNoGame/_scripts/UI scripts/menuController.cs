using UnityEngine;
using UnityEngine.SceneManagement; // Para gestionar eventos de cambio de escena.
using UnityEngine.UI; // Para interactuar con componentes de UI como Button y GameObject.

/// <summary>
/// Gestiona la navegación principal del menú, la visibilidad de los paneles y la conexión
/// con el gestor de Kanjis (addkanji) para controlar el input nativo.
/// Implementa el patrón Singleton.
/// </summary>
public class menuController : MonoBehaviour
{
    public static menuController Instance; // Referencia estática al Singleton.

    private addkanji kanjiManager; // Referencia al Singleton del script de gestión de Kanjis y DOM.

    [Header("Paneles")]
    public GameObject panelAdd;     // Panel para agregar Kanjis.
    public GameObject panelInicio;  // Panel principal.
    public GameObject Options;      // Panel de opciones.
    public GameObject HowShooter;   // Panel de instrucciones del juego Shooter.
    public GameObject HowMemory;    // Panel de instrucciones del juego Memoria.
    public GameObject HowRain;      // Panel de instrucciones del juego Lluvia.

    [Header("Botones")]
    // Referencias a botones para abrir/cerrar cada panel.
    public Button btnAbrirAdd;
    public Button btnCerrarAdd;
    public Button btnAbrirOptions;
    public Button btnCerrarOptions;
    public Button btnOpenHowShooter;
    public Button btnCloseHowShooter;
    public Button btnOpenHowMemory;
    public Button btnCloseHowMemory;
    public Button btnOpenHowRain;
    public Button btnCloseHowRain;

    private void Awake()
    {
        // Implementación del Singleton (asegura que solo exista una instancia).
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste el objeto entre escenas (esencial para un controlador de menú global).
            SceneManager.sceneLoaded += OnSceneLoaded; // Se suscribe al evento de carga de escena.
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LimpiarReferencias();   // Anula referencias para forzar la búsqueda en la nueva escena.
        ReconectarUI();         // Busca todos los GameObjects de la UI en la escena.
        AsignarBotones();       // Asigna los métodos de control a los botones encontrados.

        // Conecta la referencia al gestor de Kanjis (addkanji)
        if (kanjiManager == null)
        {
            kanjiManager = FindObjectOfType<addkanji>(); // Lo busca, asumiendo que addkanji también es DontDestroyOnLoad.
        }
    }

    private void OnEnable()
    {
        // Lógica de reconexión si el objeto se habilita (útil en el editor o si se desactiva/activa).
        ReconectarUI();
        AsignarBotones();
    }

    /// <summary>
    /// Anula todas las referencias de UI para que puedan ser reconectadas de forma segura
    /// cuando se carga una nueva escena de menú.
    /// </summary>
    private void LimpiarReferencias()
    {
        panelAdd = null;
        panelInicio = null;
        Options = null;
        HowShooter = null;
        HowMemory = null;
        HowRain = null;

        btnAbrirAdd = null;
        btnCerrarAdd = null;
        btnAbrirOptions = null;
        btnCerrarOptions = null;
        btnOpenHowShooter = null;
        btnCloseHowShooter = null;
        btnOpenHowMemory = null;
        btnCloseHowMemory = null;
        btnOpenHowRain = null;
        btnCloseHowRain = null;
    }

    /// <summary>
    /// Busca y reconecta todos los GameObjects de los paneles y botones en la escena actual
    /// utilizando sus nombres de Unity.
    /// </summary>
    private void ReconectarUI()
    {
        // --- Paneles ---
        panelAdd = FindUIObject("PanelAdd");
        panelInicio = FindUIObject("PanelInicio");
        Options = FindUIObject("Options");
        HowShooter = FindUIObject("PanelHowShooter");
        HowMemory = FindUIObject("PanelHowMemory");
        HowRain = FindUIObject("PanelHowRain");

        // --- Botones ---
        btnAbrirAdd = GetButton("BtnAbrirAdd");
        btnCerrarAdd = GetButton("BtnCerrarAdd");
        btnAbrirOptions = GetButton("BtnAbrirOptions");
        btnCerrarOptions = GetButton("BtnCerrarOptions");
        btnOpenHowShooter = GetButton("BtnOpenHowShooter");
        btnCloseHowShooter = GetButton("BtnCloseHowShooter");
        btnOpenHowMemory = GetButton("BtnOpenHowMemory");
        btnCloseHowMemory = GetButton("BtnCloseHowMemory");
        btnOpenHowRain = GetButton("BtnOpenHowRain");
        btnCloseHowRain = GetButton("BtnCloseHowRain");
    }

    /// <summary>
    /// Busca un GameObject por nombre en la escena actual, incluyendo objetos inactivos.
    /// Es esencial para encontrar paneles o elementos que están deshabilitados.
    /// </summary>
    private GameObject FindUIObject(string name)
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            // Busca en todos los hijos, activos o inactivos (true)
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null; // Devuelve null si no se encuentra.
    }

    /// <summary>
    /// Busca un GameObject por nombre y retorna su componente Button, o null.
    /// </summary>
    private Button GetButton(string name)
    {
        var obj = FindUIObject(name);
        return obj ? obj.GetComponent<Button>() : null;
    }

    /// <summary>
    /// Asigna los métodos de manejo de eventos (listeners) a cada botón.
    /// Es vital usar RemoveAllListeners() primero para evitar duplicar llamadas al cargar la escena.
    /// </summary>
    private void AsignarBotones()
    {
        // --- Panel Add ---
        if (btnAbrirAdd != null)
        {
            btnAbrirAdd.onClick.RemoveAllListeners();
            btnAbrirAdd.onClick.AddListener(AbrirPanelAdd);
        }
        if (btnCerrarAdd != null)
        {
            btnCerrarAdd.onClick.RemoveAllListeners();
            btnCerrarAdd.onClick.AddListener(CerrarPanelAdd);
        }

        // --- Options ---
        if (btnAbrirOptions != null)
        {
            btnAbrirOptions.onClick.RemoveAllListeners();
            btnAbrirOptions.onClick.AddListener(AbrirOptions);
        }
        if (btnCerrarOptions != null)
        {
            btnCerrarOptions.onClick.RemoveAllListeners();
            btnCerrarOptions.onClick.AddListener(CerrarOptions);
        }

        // --- HowTo Shooter ---
        if (btnOpenHowShooter != null)
        {
            btnOpenHowShooter.onClick.RemoveAllListeners();
            btnOpenHowShooter.onClick.AddListener(OpenHowToShoot);
        }
        if (btnCloseHowShooter != null)
        {
            btnCloseHowShooter.onClick.RemoveAllListeners();
            btnCloseHowShooter.onClick.AddListener(CloseHowToShoot);
        }

        // --- HowTo Memory ---
        if (btnOpenHowMemory != null)
        {
            btnOpenHowMemory.onClick.RemoveAllListeners();
            btnOpenHowMemory.onClick.AddListener(OpenHowMemory);
        }
        if (btnCloseHowMemory != null)
        {
            btnCloseHowMemory.onClick.RemoveAllListeners();
            btnCloseHowMemory.onClick.AddListener(CloseHowMemory);
        }

        // --- HowTo Rain ---
        if (btnOpenHowRain != null)
        {
            btnOpenHowRain.onClick.RemoveAllListeners();
            btnOpenHowRain.onClick.AddListener(OpenHowRain);
        }
        if (btnCloseHowRain != null)
        {
            btnCloseHowRain.onClick.RemoveAllListeners();
            btnCloseHowRain.onClick.AddListener(CloseHowRain);
        }
        // ... (lógica similar para todos los botones de instrucciones y opciones) ...
    }

    // --- MÉTODOS DE MANEJO DE PANELES ---

    /// <summary>
    /// Abre el panel de adición de Kanjis y gestiona la entrada nativa.
    /// </summary>
    public void AbrirPanelAdd()
    {
        if (panelAdd) panelAdd.SetActive(true);
        if (panelInicio) panelInicio.SetActive(false);

        if (kanjiManager != null)
        {
            kanjiManager.MostrarInputField(); // Muestra el input DOM (WebGL).
            kanjiManager.SetKeyboardInputCapture(false); // Desactiva la captura de teclado de Unity para usar el DOM.
        }
    }

    /// <summary>
    /// Cierra el panel de adición de Kanjis y gestiona la entrada nativa.
    /// </summary>
    public void CerrarPanelAdd()
    {
        if (panelAdd) panelAdd.SetActive(false);
        if (panelInicio) panelInicio.SetActive(true);

        if (kanjiManager != null)
        {
            kanjiManager.OcultarInputField(); // Oculta el input DOM.
            kanjiManager.SetKeyboardInputCapture(true); // Reactiva la captura de teclado de Unity (para jugar o navegar).
        }
    }

    // --- Otros métodos de navegación de paneles ---
    public void AbrirOptions()
    {
        if (Options) Options.SetActive(true);
        if (panelInicio) panelInicio.SetActive(false);
    }

    public void CerrarOptions()
    {
        if (Options) Options.SetActive(false);
        if (panelInicio) panelInicio.SetActive(true);
    }

    // Métodos simples para abrir/cerrar paneles de instrucciones.
    public void OpenHowToShoot() { if (HowShooter) HowShooter.SetActive(true); }
    public void CloseHowToShoot() { if (HowShooter) HowShooter.SetActive(false); }
    public void OpenHowMemory() { if (HowMemory) HowMemory.SetActive(true); }
    public void CloseHowMemory() { if (HowMemory) HowMemory.SetActive(false); }
    public void OpenHowRain() { if (HowRain) HowRain.SetActive(true); }
    public void CloseHowRain() { if (HowRain) HowRain.SetActive(false); }
}
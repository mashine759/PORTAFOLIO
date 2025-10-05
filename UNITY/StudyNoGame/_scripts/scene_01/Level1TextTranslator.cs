using TMPro; // Necesario para interactuar con componentes TextMeshPro.
using UnityEngine; // Biblioteca principal de Unity.

/// <summary>
/// Gestiona la traducci�n de los textos espec�ficos de la interfaz de la escena de juego (Nivel 1),
/// como el texto base del objetivo y los mensajes de error.
/// Se sincroniza con el LanguageController y el GameManager.
/// </summary>
public class Level1TextTranslator : MonoBehaviour
{
    [Header("Objetivo Text")]
    public TMP_Text objetivoText;       // Referencia al componente de texto de Unity para el objetivo.
    [TextArea(2, 4)] public string objetivoSpanish; // Texto base del objetivo en espa�ol (ej: "Busca: ").
    [TextArea(2, 4)] public string objetivoEnglish; // Texto base del objetivo en ingl�s (ej: "Find: ").

    [Header("Error Text")]
    public TMP_Text errorText;          // Referencia al componente de texto de Unity para el error.
    [TextArea(2, 4)] public string errorSpanish;    // Mensaje de error en espa�ol (ej: "�Incorrecto!").
    [TextArea(2, 4)] public string errorEnglish;    // Mensaje de error en ingl�s (ej: "Incorrect!").

    void Start()
    {
        // Al iniciar la escena, se llama a UpdateTexts() para aplicar el idioma actual.
        UpdateTexts();
    }

    /// <summary>
    /// Actualiza todos los textos de esta escena bas�ndose en el idioma actual del LanguageController.
    /// </summary>
    public void UpdateTexts()
    {
        // Si el controlador de idioma no est� inicializado, sale para evitar un error de NullReference.
        if (LanguageController.Instance == null) return;

        // 1. Determina el texto base del objetivo seg�n el idioma actual.
        string baseText = LanguageController.Instance.currentLanguage == LanguageController.Language.Spanish
            ? objetivoSpanish
            : objetivoEnglish;

        // 2. Aplica las traducciones a los componentes de texto de la UI.
        objetivoText.text = baseText;

        errorText.text = LanguageController.Instance.currentLanguage == LanguageController.Language.Spanish
            ? errorSpanish
            : errorEnglish;

        // 3. Sincroniza el texto base del objetivo con el GameManager.
        // El GameManager usar� este texto base para construir el mensaje final (ej: "Busca: [Kanji]").
        // El operador ?. asegura que la llamada solo ocurra si GameManager.Instance no es nulo.
        GameManager.Instance?.SetObjetivoBaseText(baseText);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject mainMenuCanvas;     // El menú con Jugar / Opciones / Salir
    public GameObject optionsCanvas;      // El panel de Opciones

    [Header("Primeros selectables")]
    public GameObject firstOptionSelectable;   // Primer slider (Música)
    public GameObject optionsButtonInMainMenu; // El botón Opciones en el menú principal
    public GameObject startGameButton; // El botón Opciones en el menú principal

    private bool playerCanMove;
    void Start()
    {
        AudioManager.GetOrCreate().PlayMusic("music1");
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);

        // Seleccionar el botón de Opciones (o Jugar, el que quieras al inicio)
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startGameButton);
        AudioManager.GetOrCreate().PlaySFX("button_navigate");
    }

    public void startGame()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        SceneManager.LoadScene("Nivel1");
    }
    public void backToMenu()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        SceneManager.LoadScene("Menu");
    }

    public void toggleOptionsCanvas()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        Scene currentScene = SceneManager.GetActiveScene();
        // Si está cerrado → Abrir
        if (!optionsCanvas.activeSelf)
        {
            mainMenuCanvas.SetActive(false);
            optionsCanvas.SetActive(true);

            // Forzar selección al primer slider
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstOptionSelectable);
        }
        else
        {
            // Si está abierto → Cerrar 
            optionsCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);

            // Volver a seleccionar el botón Opciones
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionsButtonInMainMenu);
        }
    }

    public void closeGame()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        Application.Quit();
    }

}



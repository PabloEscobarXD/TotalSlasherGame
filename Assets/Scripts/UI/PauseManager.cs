using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    public PlayerControllerBlocker playerBlocker;

    public GameObject firstPauseSelectable;
    public GameObject optionsButtonInMainMenu;
    public GameObject optionsCanvas;
    public GameObject firstOptionSelectable;

    private bool isPaused = false;
    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
    }

    public void TogglePause()
    {
        // Si estamos dentro del menú de opciones → cerrar todo el menú
        if (optionsCanvas.activeSelf)
        {
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(false);

            // Se despausa
            playerBlocker.SetPaused(false);
            Time.timeScale = 1f;
            isPaused = false;

            // Volver a activar controles de juego
            playerInput.SwitchCurrentActionMap("Player");
            return;
        }

        // Caso normal: alternar pausa
        isPaused = !isPaused;

        pauseCanvas.SetActive(isPaused);
        playerBlocker.SetPaused(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;

            // Cambiar controles a UI
            playerInput.SwitchCurrentActionMap("UI");

            EventSystem.current.SetSelectedGameObject(firstPauseSelectable);
        }
        else
        {
            Time.timeScale = 1f;

            // Volver al Action Map Gameplay
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    public void toggleOptionsCanvas()
    {
        playerBlocker.SetPaused(true);

        // Si está cerrado → Abrir
        if (!optionsCanvas.activeSelf)
        {
            pauseCanvas.SetActive(false);
            optionsCanvas.SetActive(true);

            // Cambiar a UI para navegar
            playerInput.SwitchCurrentActionMap("UI");

            // Forzar selección al primer slider
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstOptionSelectable);
        }
        else
        {
            // Si está abierto → Cerrar 
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(true);

            // Mantener UI activo y volver al botón Opciones
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionsButtonInMainMenu);
        }
    }

    public void backToMenu()
    {
        // Volver al menú principal
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;

        // Restablecer Action Map
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TogglePause();
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Caso 1: Si estoy en opciones → volver al menú de pausa
        if (optionsCanvas.activeSelf)
        {
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseSelectable);
            return;
        }

        // Caso 2: Si ya estoy en menú de pausa → cerrar todo
        if (pauseCanvas.activeSelf)
        {
            TogglePause();
        }
    }

}

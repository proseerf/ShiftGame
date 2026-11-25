using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject winPanel;
    public string mainMenuScene = "MainMenu";

    public static bool IsPaused { get; private set; }

    public SettingsMenu settingsMenu;

    public WinMenu winMenu;

    void Start()
    {
        pausePanel.SetActive(false);
        IsPaused = false;
        Time.timeScale = 1f;
        SetCursorLocked(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                if (settingsMenu != null && settingsMenu.IsOpen)
                    settingsMenu.Close();
                else if (winMenu.IsOpen)
                {
                    //Do Nothing
                }
                else
                  Resume();
            } 
            else
            {                
                pausePanel.SetActive(true);
                Pause();
            }
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        SetCursorLocked(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        SetCursorLocked(true);
    }

    public void OpenSettings()
    {
        settingsMenu.Open(pausePanel);
    }

    public void ExitToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        
        SetCursorLocked(false);
        SceneManager.LoadScene(mainMenuScene);
    }

    void SetCursorLocked(bool locked)
    {
        // cursors position lock toggle
        if (locked) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }

        // cursors visibility toggle
        if (locked) {
            Cursor.visible = false;
        } else {
            Cursor.visible = true;
        }
    }

    public void WinLevel(int percentage)
    {
        Pause();
        winMenu.Open(winPanel, percentage);
    }
}

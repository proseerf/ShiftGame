using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;

    GameObject returnTo;

    public bool IsOpen
    {
        get { return settingsPanel.activeSelf; }
    }

    void Start()
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    {
        settingsPanel.SetActive(false);

        sensitivityXSlider.value = GameSettings.GetSensitivityX();
        sensitivityYSlider.value = GameSettings.GetSensitivityY();

        sensitivityXSlider.onValueChanged.AddListener(GameSettings.SetSensitivityX);
        sensitivityYSlider.onValueChanged.AddListener(GameSettings.SetSensitivityY);
    }

    public void Open(GameObject from)
    {
        returnTo = from;
        if (returnTo != null)
            returnTo.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void Close()
    {
        settingsPanel.SetActive(false);
        if (returnTo != null)
            returnTo.SetActive(true);
    }
}

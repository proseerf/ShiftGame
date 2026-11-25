using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinMenu : MonoBehaviour
{
    public GameObject winPanel;
    // public Slider sensitivityXSlider;
    // public Slider sensitivityYSlider;

    public TextMeshProUGUI scoreText;

    GameObject returnTo;

    public bool IsOpen
    {
        get { return winPanel.activeSelf; }
    }

    void Start()
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    {
        winPanel.SetActive(false);
    }

    public void Open(GameObject from, int percentage)
    {
        returnTo = from;
        if (returnTo != null)
            returnTo.SetActive(false);

        winPanel.SetActive(true);

        SetWinText(percentage);
    }

    public void Close()
    {
        winPanel.SetActive(false);
        if (returnTo != null)
            returnTo.SetActive(true);
    }

    private void SetWinText(int percentage)
    {
        scoreText.text = "Score: " + percentage + "%";
    }
}

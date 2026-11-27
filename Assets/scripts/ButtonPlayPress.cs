using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPlayPress : MonoBehaviour
{

    [SerializeField] string sceneToLoad = "SampleScene";

    public void PlayGame()
    {
        if(sceneToLoad == "this")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
    }
}

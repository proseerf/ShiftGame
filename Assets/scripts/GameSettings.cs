using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static float GetSensitivityX()
    {
        return PlayerPrefs.GetFloat("MouseSensitivityX", 2f);
    }

    public static void SetSensitivityX(float newValue)
    {
        PlayerPrefs.SetFloat("MouseSensitivityX", newValue);
        PlayerPrefs.Save();
    }

    public static float GetSensitivityY()
    {
        return PlayerPrefs.GetFloat("MouseSensitivityY", 1f);
    }

    public static void SetSensitivityY(float newValue)
    {
        PlayerPrefs.SetFloat("MouseSensitivityY", newValue);
        PlayerPrefs.Save();
    }
}

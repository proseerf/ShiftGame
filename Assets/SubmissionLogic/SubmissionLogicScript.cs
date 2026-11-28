using UnityEngine;
using System.IO;
using TMPro;
using System;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using System.Text.RegularExpressions;

public class SubmissionLogicScript : MonoBehaviour
{
    public RenderTexture shadowRenderTexture; // The live shadow from your camera
    public RenderTexture goalRenderTexture;             // The target shape silhouette
    
    [Range(0f, 10f)] public float overspillPenalty = 2f;
    public float matchPercentage;
    [Range(0.5f, 1.5f)]public float scoreAdjustemnt = 1.1f;

    [Range(0, 100)] public int winPercent = 90;

    private Texture2D tempGoal; 
    private Texture2D tempShadow;

    public TextMeshProUGUI winPercentText;
    public TextMeshProUGUI penaltyPercentText;

    public Camera viewCamera;
    public GameObject scoreUI;


    public PauseMenu pauseMenu;
    public bool finished = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempShadow = new Texture2D(shadowRenderTexture.width, shadowRenderTexture.height, TextureFormat.RGBA32, false);
        tempGoal = new Texture2D(goalRenderTexture.width, goalRenderTexture.height, TextureFormat.RGBA32, false);
    }

    // Update is called once per frame
    void Update()
    {
        // only calculate while looking through the theatre camera
        bool inTheatreView = false;

        if (viewCamera.enabled)
        {
            inTheatreView = true;
        }

        if (scoreUI != null)
        {
            bool isShowing = scoreUI.activeSelf;

            if (isShowing != inTheatreView)
            {
                scoreUI.SetActive(inTheatreView);
            }
        }

        if (inTheatreView)
        {
            CalculateFill();
        }
    }


    void CalculateFill()
    {
        // 1. Copy RenderTexture to our temporary Texture2D
        RenderTexture.active = shadowRenderTexture;
        tempShadow.ReadPixels(new Rect(0, 0, shadowRenderTexture.width, shadowRenderTexture.height), 0, 0);
        tempShadow.Apply();
        RenderTexture.active = null;
        RenderTexture.active = goalRenderTexture;
        tempGoal.ReadPixels(new Rect(0, 0, goalRenderTexture.width, goalRenderTexture.height), 0, 0);
        tempGoal.Apply();
        RenderTexture.active = null;

        // 2. Get pixel arrays
        Color32[] shadowPixels = tempShadow.GetPixels32();
        Color32[] goalPixels = tempGoal.GetPixels32();

        int totalGoalPixels = 0;
        int matchingPixels = 0;
        int overspillPixels = 0;

        // 3. Loop and compare (Textures MUST be the same resolution!)
        for (int i = 0; i < goalPixels.Length; i++)
        {
            // Assuming your silhouettes are black (alpha > 0 or RGB near 0)
            bool isGoal = goalPixels[i].a > 0.1f; 
            bool isShadow = shadowPixels[i].a > 0.1f;

            if (isGoal)
            {
                totalGoalPixels++;
                if (isShadow) 
                {
                    matchingPixels++;
                }
            }
            else if (isShadow)
            {
                overspillPixels++;
            }
        }
        
        int penPercent = 0;

        // 4. Calculate final score using the formula
        if (totalGoalPixels > 0)
        {
            float penaltyScore = Mathf.Clamp01((overspillPixels * overspillPenalty) / totalGoalPixels);
            penPercent = Convert.ToInt32(Math.Floor(penaltyScore * 100));
            
            float score = (float) (matchingPixels * scoreAdjustemnt)/ totalGoalPixels;
            score = Mathf.Clamp01(score);

            score -= penaltyScore;
            score = Mathf.Clamp01(score);

            matchPercentage = score * 100;
        }

        setWinPercentText(Convert.ToInt32(Math.Ceiling(matchPercentage)));
        setPenaltyText(penPercent);


        if(matchPercentage >= winPercent)
        {
            WinLevel(Convert.ToInt32(Math.Ceiling(matchPercentage)));
        }
    }

    [ContextMenu("Export Renders to PNG")]
    public void ExportRendersToPNG()
    {
        // 1. Save the Live Shadow Texture
        byte[] shadowBytes = tempShadow.EncodeToPNG();
        string shadowPath = Path.Combine(Application.dataPath, "Debug_CurrentShadow.png");
        File.WriteAllBytes(shadowPath, shadowBytes);
        Debug.Log($"Saved live shadow render to: {shadowPath}");

        // 2. Save the Goal Texture
        if (tempGoal != null)
        {
            byte[] goalBytes = tempGoal.EncodeToPNG();
            string goalPath = Path.Combine(Application.dataPath, "Debug_CurrentGoal.png");
            File.WriteAllBytes(goalPath, goalBytes);
            Debug.Log($"Saved live goal render to: {goalPath}");
        }
        else
        {
            Debug.LogError("goalRenderTexture is null! Cannot export.");
        }
        
        // Refresh Unity's Asset Database so they show up in the project window immediately
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private string getUIPercentText(int percentage)
    {
        return "Shadow Coverage: " + percentage + "%";
    }

    private void setWinPercentText(int percentage)
    {
        if(percentage >= winPercent)
        {
            winPercentText.text = getUIPercentText(percentage);
            winPercentText.color = Color.green;
        }
        else if (percentage >= (winPercent - 10))
        {
            winPercentText.text = getUIPercentText(percentage);
            winPercentText.color = Color.greenYellow;
        }
        else if (percentage >= (winPercent - 20))
        {
            winPercentText.text = getUIPercentText(percentage);
            winPercentText.color = Color.yellow;
        }
        else
        {
            winPercentText.text = getUIPercentText(percentage);
            winPercentText.color = Color.orange;
        }
    }

    private string getUIPenPercentText(int percentage)
    {
        return "Overfill Penalty: " + percentage + "%";
    }

    private void setPenaltyText(int percentage)
    {
        if(percentage == 0)
        {
            penaltyPercentText.text = "";
        }
        else if (percentage < 5)
        {
            penaltyPercentText.text = getUIPenPercentText(percentage);
            penaltyPercentText.color = Color.orange;
        }
        else if (percentage < 10)
        {
            penaltyPercentText.text = getUIPenPercentText(percentage);
            penaltyPercentText.color = Color.orangeRed;
        }
        else
        {
            penaltyPercentText.text = getUIPenPercentText(percentage);
            penaltyPercentText.color = Color.red;
        }
    }

    private void WinLevel(int percentage)
    {
        // Pause Game logic
        pauseMenu.WinLevel(percentage);
        finished = true;

        // Show UI Screen

    }
}

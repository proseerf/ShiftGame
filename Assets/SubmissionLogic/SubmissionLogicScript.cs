using UnityEngine;
using System.IO;

public class SubmissionLogicScript : MonoBehaviour
{
    public RenderTexture shadowRenderTexture; // The live shadow from your camera
    public RenderTexture goalRenderTexture;             // The target shape silhouette
    
    [Range(0f, 1f)] public float overspillPenalty = 0.5f;
    public float matchPercentage;

    private Texture2D tempGoal; 
    private Texture2D tempShadow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tempShadow = new Texture2D(shadowRenderTexture.width, shadowRenderTexture.height, TextureFormat.RGBA32, false);
        tempGoal = new Texture2D(goalRenderTexture.width, goalRenderTexture.height, TextureFormat.RGBA32, false);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateFill();

        // Press Spacebar in Play Mode to export both renders to PNG
        if (Input.GetKeyDown(KeyCode.X))
        {
            ExportRendersToPNG();
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
                if (isShadow) matchingPixels++;
            }
            else if (isShadow)
            {
                overspillPixels++;
            }
        }

        // 4. Calculate final score using the formula
        if (totalGoalPixels > 0)
        {
            float score = (matchingPixels - (overspillPixels * overspillPenalty)) / totalGoalPixels;
            matchPercentage = Mathf.Clamp01(score);
            // Debug.Log(matchPercentage);
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
}

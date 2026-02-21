using UnityEditor;
using UnityEngine;

public class WireBoardConfigs
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        
        // Find GameManager in scene
        GameObject gmGO = GameObject.Find("GameManager");
        if (gmGO == null)
        {
            return "ERROR: GameManager not found in scene";
        }
        
        GameManager gm = gmGO.GetComponent<GameManager>();
        if (gm == null)
        {
            return "ERROR: GameManager component not found";
        }
        
        SerializedObject gmSO = new SerializedObject(gm);
        
        // Load both BoardConfig assets
        BoardConfig classic = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/BoardConfigs/Classic_3x3.asset");
        BoardConfig pyramid = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/BoardConfigs/Pyramid_XO.asset");
        
        if (classic == null) sb.AppendLine("WARNING: Classic_3x3.asset not found");
        if (pyramid == null) sb.AppendLine("WARNING: Pyramid_XO.asset not found");
        
        // Set the boardConfigs array
        SerializedProperty configsArray = gmSO.FindProperty("boardConfigs");
        configsArray.arraySize = 2;
        configsArray.GetArrayElementAtIndex(0).objectReferenceValue = classic;
        configsArray.GetArrayElementAtIndex(1).objectReferenceValue = pyramid;
        
        gmSO.ApplyModifiedProperties();
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        sb.AppendLine($"Wired boardConfigs array: [0]={classic?.modeName ?? "null"}, [1]={pyramid?.modeName ?? "null"}");
        sb.AppendLine("Scene marked dirty. Save to persist.");
        
        return sb.ToString();
    }
}

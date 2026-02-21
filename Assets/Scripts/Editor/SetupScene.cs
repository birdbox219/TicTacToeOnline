using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupScene
{
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();
        
        // ── Step 1: Create GridLine prefab from existing Line object ──
        // Find the first Line object to use as a template
        GameObject lineTemplate = GameObject.Find("Line");
        if (lineTemplate != null)
        {
            // Create a copy for the prefab
            GameObject linePrefab = Object.Instantiate(lineTemplate);
            linePrefab.name = "GridLine";
            // Reset transform
            linePrefab.transform.position = Vector3.zero;
            linePrefab.transform.rotation = Quaternion.identity;
            linePrefab.transform.localScale = new Vector3(6f, 1f, 1f);
            
            // Create Prefabs folder if needed
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            
            // Save as prefab
            string prefabPath = "Assets/Prefabs/GridLine.prefab";
            PrefabUtility.SaveAsPrefabAsset(linePrefab, prefabPath);
            Object.DestroyImmediate(linePrefab);
            sb.AppendLine($"Created GridLine prefab at {prefabPath}");
        }
        else
        {
            sb.AppendLine("WARNING: Could not find 'Line' object to create GridLine prefab");
        }
        
        // ── Step 2: Create GridSpawner GameObject ──
        GameObject gridSpawnerGO = new GameObject("GridSpawner");
        GridSpawner spawner = gridSpawnerGO.AddComponent<GridSpawner>();
        
        // Wire prefab references via SerializedObject
        SerializedObject spawnerSO = new SerializedObject(spawner);
        
        // Set gridCellPrefab to Assets/Prefabs/GridPos.prefab
        GameObject gridPosPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GridPos.prefab");
        if (gridPosPrefab != null)
        {
            spawnerSO.FindProperty("gridCellPrefab").objectReferenceValue = gridPosPrefab;
            sb.AppendLine("Wired GridPos.prefab to GridSpawner.gridCellPrefab");
        }
        else
        {
            sb.AppendLine("WARNING: GridPos.prefab not found at Assets/Prefabs/GridPos.prefab");
        }
        
        // Set gridLinePrefab to Assets/Prefabs/GridLine.prefab
        GameObject gridLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GridLine.prefab");
        if (gridLinePrefab != null)
        {
            spawnerSO.FindProperty("gridLinePrefab").objectReferenceValue = gridLinePrefab;
            sb.AppendLine("Wired GridLine.prefab to GridSpawner.gridLinePrefab");
        }
        else
        {
            sb.AppendLine("WARNING: GridLine.prefab not found at Assets/Prefabs/GridLine.prefab");
        }
        
        spawnerSO.ApplyModifiedProperties();
        
        // ── Step 3: Wire GameManager references ──
        GameObject gmGO = GameObject.Find("GameManager");
        if (gmGO != null)
        {
            GameManager gm = gmGO.GetComponent<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                
                // Set boardConfig
                BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/BoardConfigs/Classic_3x3.asset");
                if (config != null)
                {
                    gmSO.FindProperty("boardConfig").objectReferenceValue = config;
                    sb.AppendLine("Wired Classic_3x3 BoardConfig to GameManager.boardConfig");
                }
                
                // Set gridSpawner
                gmSO.FindProperty("gridSpawner").objectReferenceValue = spawner;
                sb.AppendLine("Wired GridSpawner to GameManager.gridSpawner");
                
                gmSO.ApplyModifiedProperties();
            }
        }
        else
        {
            sb.AppendLine("WARNING: GameManager not found in scene");
        }
        
        // ── Step 4: Remove old manual GridPos objects ──
        string[] gridPosNames = {
            "GridPos_0_0", "GridPos_0_1", "GridPos_0_2",
            "GridPos_1_0", "GridPos_1_1", "GridPos_1_2",
            "GridPos_2_0", "GridPos_2_1", "GridPos_2_2"
        };
        
        int removedGridPos = 0;
        foreach (string name in gridPosNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
                removedGridPos++;
            }
        }
        sb.AppendLine($"Removed {removedGridPos} old GridPos objects");
        
        // ── Step 5: Remove old manual Line objects ──
        string[] lineNames = { "Line", "Line (1)", "Line (2)", "Line (3)" };
        int removedLines = 0;
        foreach (string name in lineNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
                removedLines++;
            }
        }
        sb.AppendLine($"Removed {removedLines} old Line objects");
        
        // ── Step 6: Mark scene dirty so it can be saved ──
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        sb.AppendLine("Scene setup complete! Save the scene to preserve changes.");
        
        return sb.ToString();
    }
}

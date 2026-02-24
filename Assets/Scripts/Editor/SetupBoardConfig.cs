using UnityEditor;
using UnityEngine;

public class SetupBoardConfig
{
    public static string Execute()
    {

        if (!AssetDatabase.IsValidFolder("Assets/BoardConfigs"))
        {
            AssetDatabase.CreateFolder("Assets", "BoardConfigs");
        }

        // Create Classic 3x3 BoardConfig
        BoardConfig config = ScriptableObject.CreateInstance<BoardConfig>();
        config.modeName = "Classic 3x3";
        config.width = 3;
        config.height = 3;
        config.winLength = 3;
        config.cellSpacing = 3.1f;
        config.validCells = new bool[9];
        for (int i = 0; i < 9; i++) config.validCells[i] = true;

        AssetDatabase.CreateAsset(config, "Assets/BoardConfigs/Classic_3x3.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return "Created Classic_3x3 BoardConfig at Assets/BoardConfigs/Classic_3x3.asset";
    }
}

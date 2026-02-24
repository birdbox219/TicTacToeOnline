using UnityEditor;
using UnityEngine;

public class SetupPyramidConfig
{
    public static string Execute()
    {


        // Row 2 (top):     _  _  X  _  _     (1 cell)
        // Row 1 (mid):     _  X  X  X  _     (3 cells)
        // Row 0 (bottom):  X  X  X  X  X     (5 cells)
        // Total: 9 cells, win = 3

        BoardConfig config = ScriptableObject.CreateInstance<BoardConfig>();
        config.modeName = "Pyramid XO";
        config.width = 5;
        config.height = 3;
        config.winLength = 3;
        config.cellSpacing = 3.1f;

        // validCells: index = y * width + x
        config.validCells = new bool[15];

        config.validCells[0]  = true;  // (0,0)
        config.validCells[1]  = true;  // (1,0)
        config.validCells[2]  = true;  // (2,0)
        config.validCells[3]  = true;  // (3,0)
        config.validCells[4]  = true;  // (4,0)
        config.validCells[5]  = false; // (0,1)
        config.validCells[6]  = true;  // (1,1)
        config.validCells[7]  = true;  // (2,1)
        config.validCells[8]  = true;  // (3,1)
        config.validCells[9]  = false; // (4,1)
        config.validCells[10] = false; // (0,2)
        config.validCells[11] = false; // (1,2)
        config.validCells[12] = true;  // (2,2)
        config.validCells[13] = false; // (3,2)
        config.validCells[14] = false; // (4,2)

        if (!AssetDatabase.IsValidFolder("Assets/BoardConfigs"))
            AssetDatabase.CreateFolder("Assets", "BoardConfigs");

        AssetDatabase.CreateAsset(config, "Assets/BoardConfigs/Pyramid_XO.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return "Created Pyramid_XO BoardConfig at Assets/BoardConfigs/Pyramid_XO.asset (5x3 grid, 9 valid cells, win=3)";
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "NewBoardConfig", menuName = "TicTacToe/Board Config")]
public class BoardConfig : ScriptableObject
{
    public string modeName = "Classic 3x3";
    public int width = 3;
    public int height = 3;
    public int winLength = 3;
    public float cellSpacing = 3.1f;
    public bool[] validCells;


    public bool IsCellValid(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (validCells == null || validCells.Length != width * height) return true; // default: all valid
        return validCells[y * width + x];
    }


    public int GetValidCellCount()
    {
        if (validCells == null || validCells.Length != width * height)
            return width * height;

        int count = 0;
        for (int i = 0; i < validCells.Length; i++)
        {
            if (validCells[i]) count++;
        }
        return count;
    }

#if UNITY_EDITOR

    [ContextMenu("Reset Valid Cells (All True)")]
    private void ResetValidCells()
    {
        validCells = new bool[width * height];
        for (int i = 0; i < validCells.Length; i++)
            validCells[i] = true;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

using NUnit.Framework;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameVisualManager : NetworkBehaviour
{







    /*
     Use Visual Studio’s Rename (Refactor) feature to change the variable name safely across the scope. Steps:

1. Place the caret on the variable name you want to rename (Prefab).
2. Press F2 (default) or right‑click → Refactor → Rename… (or use Ctrl+R, R).
3. In the Rename dialog/typebox, enter the new name: prefab
4. Press Enter (or click Apply). Visual Studio will preview (if enabled) and apply the rename to all references in scope.

Notes and tips:
* This is a semantic rename — it updates declarations and all usages (not just text).
* If you only want to rename occurrences in one file (and not symbols with the same name elsewhere), the rename will still be scoped to the symbol’s definition (local variable, field, property, etc.). If there are other symbols named Prefab in other scopes, they will not be changed.
* You can enable “Preview changes” in the dialog to review replacements before applying.
* If F2 doesn’t work, use Ctrl+R, Ctrl+R (older VS shortcuts) or open the Quick Actions menu (lightbulb) and choose Rename.

If you prefer a plain text replace:
* Edit → Find and Replace → Replace in Files (Ctrl+Shift+H).
* Find: \bPrefab\b
* Replace: prefab
* Check “Use Regular Expressions” and set Look in: Current Project or Entire Solution.
Warning: Replace in Files is blind text replacement and may change unrelated symbols; prefer Refactor → Rename for safety.

If you want, tell me whether Prefab is a local variable, field, or property and I’ll give exact steps for that case.
     
     */



    /*
     Here are the Visual Studio 2022 keyboard shortcuts you can use to quickly select, edit, or delete whole lines.

Quickly select the current line
* Select whole line (caret anywhere on the line): Shift+End then Shift+Home
  * Move caret to line end and select to start: End then Shift+Home (two-step)
* Faster single-step alternatives:
  * Select from caret to line end: Shift+End
  * Select from caret to line start: Shift+Home
* Select the entire line (including newline) in one go:
  * Ctrl+L — cuts the current line (same as Ctrl+X when no selection), but acts as a line selection+cut command.
  * If you want selection only (not cut), press Home, then Shift+End.

Delete a single line
* Ctrl+L — remove (cut) the current line. This deletes the whole line and places it on the clipboard.
* Alternatively, to delete without affecting clipboard:
    1. Home then Shift+End to select the line.
    2. Delete (or Backspace) — Delete removes selection without preserving previous clipboard contents.

Other useful line-edit shortcuts
* Ctrl+Shift+L — remove all occurrences of the current selection (not the same as line delete; depends on extensions).
* Ctrl+X when no selection — cuts the whole current line (same behavior as Ctrl+L).
* Ctrl+Y — Redo (useful after accidental deletes).
* Ctrl+Z — Undo.

Recommended quick workflows
* Delete current line and keep clipboard: Ctrl+L
* Select entire line without cutting: Home → Shift+End
* Select from caret to line end quickly: Shift+End

If you want, I can show these as a printable cheat-sheet or map them to different keyboard layouts (e.g., Mac or custom keybindings).
     
     */

    private const float GRID_SIZE = 3.1f;


    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }




    private void Start()
    {
        GameManager.instance.OnClickOnGridPosition += GameManager_OnClickOnGridPosition;
        GameManager.instance.OnGameWin += Instance_OnGameWin;
        GameManager.instance.OnRematch += Instance_OnRematch;
    }

    private void Instance_OnRematch(object sender, System.EventArgs e)
    {

        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
    }

    private void Instance_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }


        Quaternion rotation = Quaternion.identity;
        switch (e.line.oriantation)
        {
            case GameManager.Oriantation.Horizontal:
                rotation = Quaternion.Euler(0, 0, 0f);
                break;
            case GameManager.Oriantation.Vertical:
                rotation = Quaternion.Euler(0, 0, 90f );
                break;
            case GameManager.Oriantation.DiagonalA:
                rotation = Quaternion.Euler(0, 0, 45f);
                break;
            case GameManager.Oriantation.DiagonalB:
                rotation = Quaternion.Euler(0, 0 ,-45f);
                break;
        }

        Transform lineCompleteTransform = Instantiate(lineCompletePrefab , GetWorldPosition(e.line.centerPos.x , e.line.centerPos.y),  rotation);
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnClickOnGridPosition(object sender, GameManager.OnClickOnGridPositionEventArgs e)
    {
        
        SpawnObjectRpc(e.x , e.y , e.playerType );
    }


    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x , int y , GameManager.PlayerType playerType)
    {
        Transform prefab;


        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                    prefab = crossPrefab;
                    break;
            case GameManager.PlayerType.Cricle:
                    prefab = circlePrefab;
                    break;


        }





        Transform spawnedCircleTrans = Instantiate(prefab , GetWorldPosition(x ,y ) , Quaternion.identity);
        spawnedCircleTrans.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(spawnedCircleTrans.gameObject);
        
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);

    }
}

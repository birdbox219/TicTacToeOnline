using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;
    [SerializeField] private Transform winSfxPrefab;
    [SerializeField] private Transform loseSfxPrefab;



    private void Start()
    {
        GameManager.instance.OnPlaceObject += Instance_OnPlaceObject;
        GameManager.instance.OnGameWin += Instance_OnGameWin;
    }

    private void Instance_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winPlayerType == GameManager.instance.GetLocalplayerType())
        {
            Transform sfxTransform = Instantiate(winSfxPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
        else
        {
            Transform sfxTransform = Instantiate(loseSfxPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
    }

    private void Instance_OnPlaceObject(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSFXPrefab);
        Destroy(sfxTransform.gameObject, 2f);

    }
}

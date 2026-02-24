using UnityEngine;


public enum SoundType
{
    AdjustingAudioLevel, //UI
    FinalSceneRandom01,
    FinalSceneRandom02,
    GoalTrigger,
    LaserGun01, 
    LaserGun02, 
    MenuBackButtons,//UI
    MenuButtonSelected01,//UI
    MenuButtonSelected02,//UI
    PongBounce01, 
    PongBounce02, 
    Portal01,
    Portal02,
    RedPongHitPlayer,
    ScoreIncrease,
    ScoreReduction,
    SprintPressedTrigger,
    StaminaRegain01,
    //StaminaRegain02,
    //StaminaRegain03,
    TransitionMenuButton,//UI
    endCredit
}



public enum MusicType
{
    MainMenu,
    Gameplay,
    FinalScene,
    Credits
}




public enum VoiceType
{
    MainMenuNarration,
    GameplayNarration,
    FinalSceneNarration,
    CreditsNarration
}

public class ISoundManager : MonoBehaviour
{
    public static ISoundManager Instance { get; private set; }

    [Header("Clips")]
    [Tooltip("Order must match the SoundType Enum exactly!")]
    [SerializeField] private AudioClip[] soundList;

    [Header("Music Playlist")]
    [SerializeField] private AudioClip[] musicList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource voiceSource;

    private int currentMusicIndex;
    private bool playlistActive;
    private bool musicStopped = false;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartPlaylist();
    }

    private void Update()
    {
        HandlePlaylist();
    }

    /* PLAYLIST  */

    private void StartPlaylist()
    {
        if (musicList.Length == 0)
            return;

        playlistActive = true;
        currentMusicIndex = Mathf.Clamp(currentMusicIndex, 0, musicList.Length - 1);

        PlayCurrentTrack();
    }

    private void PlayCurrentTrack()
    {
        if (musicSource == null) return;
        musicSource.clip = musicList[currentMusicIndex];
        musicSource.loop = false;
        musicSource.Play();
    }

    private void HandlePlaylist()
    {
        if (!playlistActive || musicStopped || musicSource == null)
            return;

        if (!musicSource.isPlaying && musicSource.time == 0f)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        currentMusicIndex = (currentMusicIndex + 1) % musicList.Length;
        PlayCurrentTrack();
    }

    /* SFX  */

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (Instance == null) return;
        if (Instance.sfxSource == null) return;


        int index = (int)sound;
        if (index >= Instance.soundList.Length)
        {
            Debug.LogError($"SoundManager: You tried to play {sound} but the Sound List array is too small!");
            return;
        }

        Instance.sfxSource.PlayOneShot(
            Instance.soundList[index],
            volume
        );
    }

    /* VOICE OVER */

    public static void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (Instance == null) return;
        if (Instance.voiceSource == null) return;


        Instance.voiceSource.Stop();

        Instance.voiceSource.PlayOneShot(clip, volume);
    }




    public static void PlayMusic(MusicType Music, float volume = 1f)
    {
        if (Instance == null) return;
        if (Instance.musicSource == null) return;

        Instance.musicStopped = false;
        Instance.playlistActive = true;

        // Stop current music to prevent overlap
        Instance.musicSource.Stop();

        int index = (int)Music;
        if (index >= Instance.musicList.Length)
        {
            Debug.LogError($"SoundManager: You tried to play {Music} but the Music List array is too small!");
            return;
        }

        Instance.currentMusicIndex = index;

        Instance.musicSource.clip = Instance.musicList[index];
        Instance.musicSource.volume = volume;

        Instance.musicSource.loop = false;

        Instance.musicSource.Play();
    }



    /* VOLUME  */

    private float _musicSliderValue = 1f;
    private float _sfxSliderValue = 1f;

    public static void SetMusicVolume(float volume)
    {
        if (Instance == null) return;

        Instance._musicSliderValue = volume;
        if (Instance.musicSource) Instance.musicSource.volume = volume;
    }

    public static void PlayVoiceOver(AudioClip clip, float volume = 1f)
    {
        if (Instance == null) return;
        if (Instance.voiceSource == null) return;

        Instance.voiceSource.Stop();

        Instance.voiceSource.PlayOneShot(clip, volume);
    }
    
    public static void SetSFXVolume(float volume)
    {
        if (Instance == null) return;

        Instance._sfxSliderValue = volume;
        if (Instance.sfxSource) Instance.sfxSource.volume = volume;
    }

    public static void StopMusic()
    {
        if (Instance == null) return;

        Instance.musicStopped = true;
        Instance.playlistActive = false;

        if (Instance.musicSource) Instance.musicSource.Stop();
        if (Instance.sfxSource) Instance.sfxSource.Stop();
        if (Instance.voiceSource) Instance.voiceSource.Stop();
    }


    public static void StopMusicMusiconly()
    {
        if (Instance == null) return;

        Instance.musicStopped = true;
        Instance.playlistActive = false;

        if (Instance.musicSource) Instance.musicSource.Stop();
    }

    public static void StopVoice()
    {
        if (Instance == null) return;
        if (Instance.voiceSource) Instance.voiceSource.Stop();
    }

    public static void StopSFX()
    {
        if (Instance == null) return;
        if (Instance.sfxSource) Instance.sfxSource.Stop();
    }

    public static void ResumeMusic()
    {
        if (Instance == null) return;

        Instance.musicStopped = false;
        Instance.playlistActive = true;
        Instance.PlayCurrentTrack();
    }

    public float InstanceMusicVolume => _musicSliderValue;
    public float InstanceSFXVolume => _sfxSliderValue;
}
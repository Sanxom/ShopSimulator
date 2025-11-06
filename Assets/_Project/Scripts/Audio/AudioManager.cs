using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private AudioSource _titleMusic;
    [SerializeField] private List<AudioSource> _allBGM = new();
    [SerializeField] private List<AudioSource> _allSFX = new();
    #endregion

    #region Private Fields
    private int _currentTrack;
    private bool _isBGMPlaying;
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {
        if (!_isBGMPlaying) return;

        if (!_allBGM[_currentTrack].isPlaying)
        {
            PlayBGM();
        }
    }
    #endregion

    #region Public Methods
    public void StopMusic()
    {
        _titleMusic.Stop();

        foreach (AudioSource track in _allBGM)
        {
            track.Stop();
        }
        _isBGMPlaying = false;
    }

    public void StartTitleMusic()
    {
        StopMusic();
        _titleMusic.Play();
    }

    public void PlayBGM()
    {
        StopMusic();
        _isBGMPlaying = true;
        _currentTrack = Random.Range(0, _allBGM.Count);
        _allBGM[_currentTrack].Play();
    }

    public void PlaySFX(int sfxToPlay)
    {
        _allSFX[sfxToPlay].Stop();
        _allSFX[sfxToPlay].Play();
    }
    #endregion

    #region Private Methods
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
}
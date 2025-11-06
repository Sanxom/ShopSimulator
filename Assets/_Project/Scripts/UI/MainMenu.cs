using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private string _mainShopScene;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        AudioManager.Instance.StartTitleMusic();
    }
    #endregion

    #region Public Methods
    public void StartGame()
    {
        SceneManager.LoadScene(_mainShopScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Private Methods
    #endregion
}
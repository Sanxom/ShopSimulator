using PrimeTween;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    #region Events
    public static event Action OnUIPanelClosed;
    public static event Action OnUIPanelOpened;
    #endregion

    #region Serialized Fields
    [Header("Input References")]
    private InputSystem_Actions _gameInput;
    private InputAction _playerOpenCloseBuyMenuAction;
    private InputAction _pauseAction;
    private InputAction _submitAction;
    private InputAction _cancelAction;
    private InputAction _UIOpenCloseBuyMenuAction;

    [Header("Interaction Panel")]
    [SerializeField] private GameObject _interactionPromptPanel;
    [SerializeField] private TMP_Text _interactionPromptText;

    [Header("Price Update Panel")]
    [SerializeField] private GameObject _updatePricePanel;
    [SerializeField] private TMP_Text _basePriceText;
    [SerializeField] private TMP_Text _currentPriceText;
    [SerializeField] private TMP_InputField _priceInputField;

    [Header("Buy Menu")]
    [SerializeField] private GameObject _buyMenuPanel;

    [Header("Pause Menu")]
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private string _mainMenuSceneName;

    [Header("Money Display")]
    [SerializeField] private TMP_Text _moneyText;
    #endregion

    #region Private Fields
    private StockInfo _activeStockInfo;
    #endregion

    #region Properties
    public GameObject UpdatePricePanel => _updatePricePanel;
    public GameObject BuyMenuPanel => _buyMenuPanel;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
        InitializeInput();
    }

    private void OnEnable()
    {
        _gameInput.Enable();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        _gameInput.Disable();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void InitializeInput()
    {
        _gameInput = PlayerController.Instance.GameInput;

        _playerOpenCloseBuyMenuAction = _gameInput.Player.OpenCloseBuyMenu;
        _pauseAction = _gameInput.Player.Pause;

        _UIOpenCloseBuyMenuAction = _gameInput.UI.OpenCloseBuyMenu;
        _submitAction = _gameInput.UI.Submit;
        _cancelAction = _gameInput.UI.Cancel;
    }

    private void SubscribeToEvents()
    {
        _playerOpenCloseBuyMenuAction.performed += OpenCloseBuyMenu;
        _UIOpenCloseBuyMenuAction.performed += OpenCloseBuyMenu;
        _pauseAction.performed += Pause;
        _submitAction.performed += OnSubmitPerformed;
        _cancelAction.performed += OnCancelPerformed;
    }

    private void UnsubscribeFromEvents()
    {
        _playerOpenCloseBuyMenuAction.performed -= OpenCloseBuyMenu;
        _UIOpenCloseBuyMenuAction.performed -= OpenCloseBuyMenu;
        _pauseAction.performed -= Pause;
        _submitAction.performed -= OnSubmitPerformed;
        _cancelAction.performed -= OnCancelPerformed;
    }
    #endregion

    #region Interaction Panel
    public string SetInteractionText(string text)
    {
        return _interactionPromptText.text = text;
    }

    public void ShowInteractionPrompt()
    {
        _interactionPromptPanel.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        _interactionPromptPanel.SetActive(false);
    }
    #endregion

    #region Price Update Panel
    public void OnOpenUpdatePricePanel(StockInfo stockToUpdate)
    {
        if (stockToUpdate == null) return;

        _updatePricePanel.SetActive(true);
        _activeStockInfo = stockToUpdate;

        UpdatePriceDisplayTexts(stockToUpdate);
        SetInputFieldValue(stockToUpdate.currentPrice);
        OnUIPanelOpened?.Invoke();
    }

    private void UpdatePriceDisplayTexts(StockInfo stock)
    {
        if (_basePriceText != null)
            _basePriceText.text = $"{stock.basePrice:0.00}";

        if (_currentPriceText != null)
            _currentPriceText.text = $"{stock.currentPrice:0.00}";
    }

    private void SetInputFieldValue(float price)
    {
        if (_priceInputField != null)
            _priceInputField.text = $"{price:0.00}";
    }

    public void ApplyPriceUpdate()
    {
        if (_activeStockInfo == null) return;

        if (float.TryParse(_priceInputField.text, out float newPrice))
            _activeStockInfo.currentPrice = newPrice;

        if (_currentPriceText != null)
            _currentPriceText.text = $"${_activeStockInfo.currentPrice:0.00}";

        if (StockInfoController.Instance != null)
            StockInfoController.Instance.UpdatePrice(_activeStockInfo.Name, _activeStockInfo.currentPrice);

        CloseUpdatePricePanel();
    }

    public void CloseUpdatePricePanel()
    {
        if (!_updatePricePanel.activeSelf) return;

        OnUIPanelClosed?.Invoke();
        _updatePricePanel.SetActive(false);
    }
    #endregion

    #region Buy Menu
    public void OpenCloseBuyMenu(InputAction.CallbackContext context)
    {
        bool isOpen = _buyMenuPanel.activeSelf;

        if (isOpen)
            CloseBuyMenu();
        else
            OpenBuyMenu();
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context) => ApplyPriceUpdate();

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        CloseUpdatePricePanel();
        CloseBuyMenu();
        UnpauseGame();
    }

    private void OpenBuyMenu()
    {
        if (BuyMenuPanel.activeSelf) return;

        _buyMenuPanel.SetActive(true);
        OnUIPanelOpened?.Invoke();
    }

    private void CloseBuyMenu()
    {
        if (!_buyMenuPanel.activeSelf) return;

        OnUIPanelClosed?.Invoke();
        _buyMenuPanel.SetActive(false);
    }
    #endregion

    #region Pause Menu
    public void MainMenu()
    {
        SceneManager.LoadScene(_mainMenuSceneName);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Pause(InputAction.CallbackContext context) => PauseGame();

    public void PauseGame()
    {
        if (_pauseMenuPanel.activeSelf) return;
        _pauseMenuPanel.SetActive(true);
        OnUIPanelOpened?.Invoke();
        Time.timeScale = 0f;
        Tween.SetPausedAll(true);
    }

    public void UnpauseGame()
    {
        if (!_pauseMenuPanel.activeSelf) return;
        OnUIPanelClosed?.Invoke();
        _pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Tween.SetPausedAll(false);
    }
#endregion

    #region Money Display
    public void UpdateMoney(float currentMoney)
    {
        if (_moneyText != null)
            _moneyText.text = $"${currentMoney:0.00}";
    }
    #endregion
}
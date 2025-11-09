using UnityEngine;

public class BuyMenuController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Panel References")]
    [SerializeField] private GameObject _stockPanel;
    [SerializeField] private GameObject _furniturePanel;
    #endregion

    #region Public Methods
    public void OpenStockPanel() => SetPanelStates(true, false);

    public void OpenFurniturePanel() => SetPanelStates(false, true);
    #endregion

    #region Private Methods
    private void SetPanelStates(bool stockActive, bool furnitureActive)
    {
        if (_stockPanel != null)
            _stockPanel.SetActive(stockActive);

        if (_furniturePanel != null)
            _furniturePanel.SetActive(furnitureActive);
    }
    #endregion
}
using UnityEngine;

[CreateAssetMenu(fileName = "StockInfo", menuName = "Data/StockInfo")]
public class StockInfo : ScriptableObject
{
    public enum StockType
    {
        Cereal,
        BigDrink,
        TubeChips,
        Fruit,
        FruitLarge,
        Vegetable
    }

    #region Event Fields
    #endregion

    #region Public Fields
    public string Name;
    public StockObject stockObject;
    public StockType typeOfStock;
    public float basePrice;
    public float currentPrice;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        currentPrice = basePrice;
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion
}
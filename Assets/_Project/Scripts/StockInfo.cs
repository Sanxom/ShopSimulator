using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StockInfo
{
    public enum StockType
    {
        Cereal,
        BigDrink,
        ChipsTube,
        Fruit,
        FruitLarge,
        Vegetable
    }

    #region Event Fields
    #endregion

    #region Public Fields
    public StockType typeOfStock;
    public string name;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion
}
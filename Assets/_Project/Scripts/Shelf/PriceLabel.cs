using UnityEngine;

public class PriceLabel : InteractableObject
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private ShelfSpaceController myShelf;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        base.Awake();
        _outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
    }
    #endregion

    #region Public Methods
    public override string GetInteractionPrompt(PlayerInteraction player)
    {
        UIController.Instance.ShowInteractionPrompt();

        if (myShelf.ObjectsOnShelf.Count == 0 || player.IsHoldingSomething)
            return UIController.Instance.SetInteractionText($"{DisplayName}");
        else
            return UIController.Instance.SetInteractionText($"Set price of {myShelf.StockInfo.name}");
    }

    public override void OnInteract(PlayerInteraction player)
    {
        if (player.IsHoldingSomething) return; // TODO: We can remove this if we want to update prices even while holding something.  This might get annoying, though.

        myShelf.StartPriceUpdate();
    }
    #endregion

    #region Private Methods
    #endregion
}
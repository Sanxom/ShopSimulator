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
    }
    #endregion

    #region Public Methods
    //public void OnInteract(Transform holdPoint = null)
    //{
    //    myShelf.StartPriceUpdate();
    //}

    public override void OnInteract(PlayerInteraction player)
    {
        if (player.IsHoldingSomething) return; // TODO: We can remove this if we want to update prices even while holding something.  This might get annoying, though.

        myShelf.StartPriceUpdate();
    }
    #endregion

    #region Private Methods
    #endregion
}
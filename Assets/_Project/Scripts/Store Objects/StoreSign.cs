using TMPro;
using UnityEngine;

public class StoreSign : InteractableObject
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private TMP_Text storeSignText;
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
    public override void OnInteract(PlayerInteraction player)
    {
        // TODO: Bring up a window to change the Text of the Store Sign with Player Input
    }
    #endregion

    #region Private Methods
    #endregion
}
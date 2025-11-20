using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : InteractableObject
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override string GetInteractionPrompt(PlayerInteraction player)
    {
        if (player.HeldStock != null && player.HeldStock.CanTrash)
        {
            UIController.Instance.ShowInteractionPrompt();
            return UIController.Instance.SetInteractionText($"Trash {player.HeldStock.DisplayName}");
        }

        if (player.HeldBox != null && player.HeldBox.CanTrash)
        {
            UIController.Instance.ShowInteractionPrompt();
            return UIController.Instance.SetInteractionText($"Trash {player.HeldBox.DisplayName}");
        }

        return base.GetInteractionPrompt(player);
    }

    public override void OnInteract(PlayerInteraction player)
    {
        if (player.HeldObject == null) return;
        if (!player.CanBeTrashed(player.HeldObject)) return;

        if (player.HeldObject.TryGetComponent(out ITrashable trashable))
        {
            player.RemoveHeldObjectReference();
            trashable.TrashObject();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : InteractableObject
{
    protected override void Awake()
    {
        base.Awake();
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
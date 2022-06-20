using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] int grassPlacedAmountNeeded = 3;

    [SerializeField] int grassRemovingAmountNeeded = 3;

    bool waitingForGrassSelection = true;

    bool waitingForGrassPlacement = false;

    bool waitingForGrassRemoving = false;

    bool waitingForStartPlacement = false;

    bool waitingForFinishplacement = false;

    int grassPlaced = 0;

    int grassRemoved = 0;

    private void OnEnable()
    {
        EventSystemNew<int>.Subscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, GrassSelected);

        EventSystemNew.Subscribe(Event_Type.TUTORIAL_TILE_PLACED, GrassPlaced);

        EventSystemNew.Subscribe(Event_Type.TUTORIAL_TILE_DELETED, GrassDeleted);

        EventSystemNew.Subscribe(Event_Type.TUTORIAL_PREFAB_PLACED, StartPlaced);
    }

    private void OnDisable()
    {
        EventSystemNew<int>.Unsubscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, GrassSelected);

        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_TILE_PLACED, GrassPlaced);

        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_TILE_DELETED, GrassDeleted);

        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_PREFAB_PLACED, StartPlaced);
        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_PREFAB_PLACED, FinishPlaced);
    }

    public void StartTutorial()
    {
        SetAnimationTrigger(animator, "StartTutorial");
    }

    private void GrassSelected(int _itemControllerID)
    {
        if (waitingForGrassSelection)
        {
            waitingForGrassSelection = false;

            waitingForGrassPlacement = true;

            SetAnimationTrigger(animator, "GrassClicked");
        }
    }

    private void GrassPlaced()
    {
        if (waitingForGrassPlacement)
        {
            grassPlaced++;

            if (grassPlaced >= grassPlacedAmountNeeded)
            {
                waitingForGrassPlacement = false;

                waitingForGrassRemoving = true;

                SetAnimationTrigger(animator, "GrassPlaced");
            }
        }
    }

    private void GrassDeleted()
    {
        if (waitingForGrassRemoving)
        {
            grassRemoved++;

            if (grassRemoved >= grassRemovingAmountNeeded)
            {
                waitingForGrassRemoving = false;

                waitingForStartPlacement = true;

                SetAnimationTrigger(animator, "GrassRemoved");
            }
        }
    }

    private void StartPlaced()
    {
        if (waitingForStartPlacement)
        {
            waitingForStartPlacement = false;
            waitingForFinishplacement = true;

            EventSystemNew.Subscribe(Event_Type.TUTORIAL_PREFAB_PLACED, FinishPlaced);

            SetAnimationTrigger(animator, "StartPlaced");
        }
    }

    private void FinishPlaced()
    {
        if (waitingForFinishplacement)
        {
            waitingForFinishplacement = false;

            SetAnimationTrigger(animator, "FinishPlaced");
        }
    }

    public void ItemSettingsWheelClicked()
    {
        if (waitingForStartPlacement)
        {
            SetAnimationTrigger(animator, "StartClicked");
        }

        if (waitingForFinishplacement)
        {
            SetAnimationTrigger(animator, "FinishClicked");
        }
    }

    private void SetAnimationTrigger(Animator _animator, string _trigger)
    {
        _animator.SetTrigger(_trigger);
    }
}

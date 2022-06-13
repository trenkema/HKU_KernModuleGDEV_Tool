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

    int grassPlaced = 0;

    int grassRemoved = 0;

    private void OnEnable()
    {
        EventSystemNew<int>.Subscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, GrassSelected);

        EventSystemNew.Subscribe(Event_Type.TUTORIAL_TILE_PLACED, GrassPlaced);

        EventSystemNew.Subscribe(Event_Type.TUTORIAL_TILE_DELETED, GrassDeleted);
    }

    private void OnDisable()
    {
        EventSystemNew<int>.Unsubscribe(Event_Type.ACTIVATE_ITEM_CONTROLLER, GrassSelected);

        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_TILE_PLACED, GrassPlaced);

        EventSystemNew.Unsubscribe(Event_Type.TUTORIAL_TILE_DELETED, GrassDeleted);
    }

    private void Start()
    {
        
    }

    public void StartTutorial()
    {
        animator.SetTrigger("StartTutorial");
    }

    private void GrassSelected(int _itemControllerID)
    {
        if (waitingForGrassSelection)
        {
            waitingForGrassSelection = false;

            animator.SetTrigger("GrassClicked");

            waitingForGrassPlacement = true;
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

                animator.SetTrigger("GrassPlaced");
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

                animator.SetTrigger("GrassRemoved");
            }
        }
    }
}

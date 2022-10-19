using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InputNavigator : MonoBehaviour
{
    private EventSystem eventSystem;

    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchInputField();
        }
    }

    private void SwitchInputField()
    {
        Selectable next = eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

        if (next != null)
        {
            TMP_InputField inputField = next.GetComponent<TMP_InputField>();
            
            if (inputField != null)
            {
                inputField.OnPointerClick(new PointerEventData(eventSystem));
            }

            eventSystem.SetSelectedGameObject(next.gameObject, new BaseEventData(eventSystem));
        }
    }
}

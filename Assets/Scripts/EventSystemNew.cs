using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Event_Type
{
    EQUIP_PREFAB,
    EQUIP_TILE,
    ENABLE_LEVEL_EDITOR,

    ACTIVATE_ITEM_CONTROLLER,
    ROTATE_ITEM,
    DESTROY_DRAG_IMAGE,

    LOADING_SCREEN,

    LOAD_LEVEL_DATA,
    DEACTIVATE_LEVEL,
    ACTIVATE_LEVEL,

    SET_LEVEL_ID,
    LOAD_LEVEL,
    QUICK_LOAD_LEVEL,
    SAVE_LEVEL,

    START_ADDED,
    FINISH_ADDED,
    COLLECTABLE_ADDED,
    COLLECTABLE_COLLECTED,
    ALL_COLLECTABLES_COLLECTED,

    GAME_STARTED,
    LEVEL_COMPLETED,
    LEVEL_FAILED,
    EDIT_LEVEL,

    CHARACTER_FINISHED,
    CHARACTER_DIED,
    UPLOAD_SCORE,

    TOGGLE_DRAGGING,
    STOP_ITEMS,
    DRAGGING,

    TOGGLE_ZOOM,

    PLAYER_TRANSFORM,

    TUTORIAL_TILE_PLACED,
    TUTORIAL_TILE_DELETED,
    TUTORIAL_PREFAB_PLACED,
    TUTORIAL_PREFAB_DELETED
}

public static class EventSystemNew
{
    private static Dictionary<Event_Type, System.Action> eventRegister = new Dictionary<Event_Type, System.Action>();

    public static void Subscribe(Event_Type evt, System.Action func)
    {
        if (!eventRegister.ContainsKey(evt))
        {
            eventRegister.Add(evt, null);
        }

        eventRegister[evt] += func;
    }

    public static void Unsubscribe(Event_Type evt, System.Action func)
    {
        if (eventRegister.ContainsKey(evt))
        {
            eventRegister[evt] -= func;
        }
    }

    public static void RaiseEvent(Event_Type evt)
    {
        if (eventRegister.ContainsKey(evt))
        {
            eventRegister[evt]?.Invoke();
        }
        else
            Debug.Log("Event: " + evt + " doesn't Subscribe.");
    }
}

public static class EventSystemNew<T>
{
    private static Dictionary<Event_Type, System.Action<T>> eventRegister = new Dictionary<Event_Type, System.Action<T>>();

    public static void Subscribe(Event_Type evt, System.Action<T> func)
    {
        if (!eventRegister.ContainsKey(evt))
        {
            eventRegister.Add(evt, null);
        }

        eventRegister[evt] += func;
    }

    public static void Unsubscribe(Event_Type evt, System.Action<T> func)
    {
        if (eventRegister.ContainsKey(evt))
        {
            eventRegister[evt] -= func;
        }
    }

    public static void RaiseEvent(Event_Type evt, T arg)
    {
        if (eventRegister.ContainsKey(evt))
        {
            eventRegister[evt]?.Invoke(arg);
        }
        else
            Debug.Log("Event: " + evt + " doesn't Subscribe.");
    }
}

public static class EventSystemNew<A, T>
{
    private static Dictionary<Event_Type, System.Action<A, T>> eventRegister = new Dictionary<Event_Type, System.Action<A, T>>();

    public static void Subscribe(Event_Type _evt, System.Action<A, T> _func)
    {
        if (!eventRegister.ContainsKey(_evt))
        {
            eventRegister.Add(_evt, null);
        }

        eventRegister[_evt] += _func;
    }

    public static void Unsubscribe(Event_Type _evt, System.Action<A, T> _func)
    {
        if (eventRegister.ContainsKey(_evt))
        {
            eventRegister[_evt] -= _func;
        }
    }

    public static void RaiseEvent(Event_Type _evt, A _arg,  T _arg2)
    {
        if (eventRegister.ContainsKey(_evt))
        {
            eventRegister[_evt]?.Invoke(_arg, _arg2);
        }
        else
            Debug.Log("Event: " + _evt + " doesn't Subscribe.");
    }
}

public static class EventSystemNew<A, B, C>
{
    private static Dictionary<Event_Type, System.Action<A, B, C>> eventRegister = new Dictionary<Event_Type, System.Action<A, B, C>>();

    public static void Subscribe(Event_Type _evt, System.Action<A, B, C> _func)
    {
        if (!eventRegister.ContainsKey(_evt))
        {
            eventRegister.Add(_evt, null);
        }

        eventRegister[_evt] += _func;
    }

    public static void Unsubscribe(Event_Type _evt, System.Action<A, B, C> _func)
    {
        if (eventRegister.ContainsKey(_evt))
        {
            eventRegister[_evt] -= _func;
        }
    }

    public static void RaiseEvent(Event_Type _evt, A _arg, B _arg2, C _arg3)
    {
        if (eventRegister.ContainsKey(_evt))
        {
            eventRegister[_evt]?.Invoke(_arg, _arg2, _arg3);
        }
        else
            Debug.Log("Event: " + _evt + " doesn't Subscribe.");
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Event_Type
{
    EQUIP_PREFAB,
    EQUIP_TILE,
    ENABLE_LEVEL_EDITOR,
    ROTATE_ITEM,
    LOAD_PREFABS,
    SAVE_LEVEL,
    LOAD_LEVEL,
    CLOSE_DOWNLOAD_MENU
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
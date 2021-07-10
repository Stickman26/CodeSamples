using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StateList
{
    public enum States
    {
        NoChange = 0,
        Wander,
        Flee,
        Squirm,
        Baited,
        Death
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class BaseState
{
    protected NewtMain newt;

    public void Init(NewtMain newt)
    {
        this.newt = newt;
    }

    //The following 4 functions will be overridden in its inherited classes

    public virtual void StartState()
    {
        //Empty
    }

    public virtual void UpdateState(float DeltaTime)
    {
        //Empty
    }

    public virtual void EndState()
    {
        //Empty
    }

    public virtual StateList.States TransitionState()
    {
        return StateList.States.NoChange; //Default Case
    }
}

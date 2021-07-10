using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaitedState : BaseState
{
    public override void StartState()
    {
        //Call animation for walking
        newt.AgentMovement(true);
        newt.BaitedWayPointStart();
    }

    public override void UpdateState(float DeltaTime)
    {
        newt.BaitedWayPointUpdate(DeltaTime);
    }

    public override void EndState()
    {
        //Empty
    }

    public override StateList.States TransitionState()
    {
        //If has no eyes, Swap to Death
        if (!newt.StillHaveEyes())
        {
            return StateList.States.Death;
        }
        //If not baited, Swap to Wander

        //If player grabs/captures newt, Swap to Squirm
        if (newt.IsGrabbed)
        {
            return StateList.States.Squirm;
        }
        //If player gets too close, Swap to Flee
        if (newt.CheckRadiusPlayer(newt.GetFleeDistance()))
        {
            return StateList.States.Flee;
        }
        return StateList.States.NoChange; //Default Case
    }
}

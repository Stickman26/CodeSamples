using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FleeState : BaseState
{
    public override void StartState()
    {
        //Generate a point opposite of the player
        newt.AgentMovement(true);
        newt.AgentSpeed(newt.GetFleeSpeed());
        newt.FleeWayPointStart();
    }

    public override void UpdateState(float DeltaTime)
    {
        //Flee from the player
        newt.FleeWayPointUpdate(DeltaTime);
    }

    public override void EndState()
    {
        //Nothing here
    }

    public override StateList.States TransitionState()
    {
        //If player grabs/captures newt, Swap to Squirm
        if (newt.IsGrabbed)
        {
            return StateList.States.Squirm;
        }
        //If the bait is near and not the player, Swap to baited
        if (newt.CheckBaitedDistance() && !newt.CheckRadiusPlayer(newt.GetFleeDistance()))
        {
            return StateList.States.Baited;
        }
        //If player is further away, Swap to Wander
        if (!newt.CheckRadiusPlayer(newt.GetFleeDistance()))
        {
            return StateList.States.Wander;
        }
        //If has no eyes, Swap to Death
        if (!newt.StillHaveEyes())
        {
            return StateList.States.Death;
        }
        return StateList.States.NoChange;
    }
}

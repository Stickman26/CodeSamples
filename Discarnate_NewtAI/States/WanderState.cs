using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WanderState : BaseState
{
    public override void StartState()
    {
        //Choose a location
        //newt.GetNavAgent().destination = newt.RandomPointForAgent(newt.GetWanderMod());
        newt.AgentMovement(true);
        newt.AgentSpeed(newt.GetWanderSpeed());
        newt.WanderWayPointStart();
    }

    public override void UpdateState(float DeltaTime)
    {
        //Move to location, make it wobbly
        //When reaching a waypoint (within a tolerance), generate a new location
        newt.WanderWayPointUpdate(DeltaTime);
    }

    public override void EndState()
    {
        //End animations, end move protocals
    }

    public override StateList.States TransitionState()
    {
        //If the bait is near, Swap to baited
        if (newt.CheckBaitedDistance())
        {
            return StateList.States.Baited;
        }
        //If player gets too close, Swap to Flee
        if (newt.CheckRadiusPlayer(newt.GetFleeDistance()))
        {
            return StateList.States.Flee;
        }
        //If player has captured/grabbed the newt, Swap to Squirm
        if (newt.IsGrabbed)
        {
            return StateList.States.Squirm;
        }
        //In case of no eyes, default check, Swap to Death
        if (!newt.StillHaveEyes())
        {
            return StateList.States.Death;
        }

        return StateList.States.NoChange;
    }
}

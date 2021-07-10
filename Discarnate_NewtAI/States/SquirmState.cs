using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SquirmState : BaseState
{
    public override void StartState()
    {
        //Call on wiggle animation
        newt.AgentMovement(false); //Keeps the newt from trying to pathfind while grabbed
        //newt.GetComponent<Rigidbody>().useGravity = false;
        newt.DisableAgent(true);
    }

    public override void UpdateState(float DeltaTime)
    {
        //Might need to put in a constant call here? depends on how we do animations
    }

    public override void EndState()
    {
        //Turn off wiggle animation
        newt.AgentMovement(true);
        //newt.GetComponent<Rigidbody>().useGravity = true;
        newt.DisableAgent(false);
    }

    public override StateList.States TransitionState()
    {
        //If player lets go off or frees the newt, Swap to Flee
        if (!newt.IsGrabbed)
        {
            return StateList.States.Flee;
        }
        //If has no eyes, Swap to Death
        if (!newt.StillHaveEyes())
        {
            return StateList.States.Death;
        }
        return StateList.States.NoChange;
    }
}

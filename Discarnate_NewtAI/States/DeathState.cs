using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeathState : BaseState
{
    //May need a start, but no transition, maybe an update
    public override void StartState()
    {
        //Stop agents movement
        newt.AgentMovement(false);
        newt.DisableAgent(true);

        //Initiate rag dollstate
    }
}

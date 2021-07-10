using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Newt_SM : MonoBehaviour
{
    private NewtMain newtMain;

    [SerializeField]
    private List<BaseState> stateList = new List<BaseState>();
    private StateList.States currentState;
    private StateList.States stateChange;

    void Start()
    {
        newtMain = gameObject.GetComponent<NewtMain>();

        //Add Wander
        stateList.Add(new WanderState());
        stateList[0].Init(newtMain);

        //Add Flee
        stateList.Add(new FleeState());
        stateList[1].Init(newtMain);

        //Add Squirm
        stateList.Add(new SquirmState());
        stateList[2].Init(newtMain);

        //Add Baited
        stateList.Add(new BaitedState());
        stateList[3].Init(newtMain);

        //Add Death
        stateList.Add(new DeathState());
        stateList[4].Init(newtMain);


        currentState = StateList.States.Wander;
        stateList[(int)currentState - 1].StartState();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == StateList.States.Death) return;

        float time = Time.deltaTime;
        stateList[(int)currentState - 1].UpdateState(time);
        stateChange = stateList[(int)currentState - 1].TransitionState();

        if (stateChange != StateList.States.NoChange)
        {
            Debug.Log(stateChange);
            stateList[(int)currentState - 1].EndState();
            currentState = stateChange;
            stateList[(int)currentState - 1].StartState();
        }
    }
}

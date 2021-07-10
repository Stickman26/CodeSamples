using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewtMain : MonoBehaviour
{
    private NavMeshAgent agent; //Navmesh Agent for pathfinding
    private Transform player;   //Player Position

    private List<Transform> Waypoints = new List<Transform>();
    [SerializeField]
    private int currentIndex = 0;
    [SerializeField]
    private int fleeIndex;

    private int eyeCount = 2;

    [SerializeField]
    private float wanderDistanceMod = 5.0f; //How far a wander waypoint can be set

    [SerializeField]
    private float fleeRadius = 10.0f;

    //Wander Variables
    private float wTime = 2.5f;
    private float wTimeElapsed = 0.0f;

    [SerializeField, Tooltip("Wander Speed")]
    private float wSpeed = 2.5f;

    //Flee Variables
    private float fTime = 0.5f;
    private float fTimeElapsed = 0.0f;

    [SerializeField, Tooltip("Flee Speed")]
    private float fSpeed = 5.0f;

    private float moveTimer = 0.0f;
    private float moveDelay = 1.0f;

    //Baited Variables
    private Vector3 baitPos = Vector3.zero;
    [SerializeField] private float baitRadius = 5f;

    //private Vector3 randPoint = Vector3.zero;
    private Vector3 navHitPoint = Vector3.zero;

    //Insert Scritpatble object here for statemachine
    //[SearlizeField]

    private bool isGrabbed = false;

    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }

    public Vector3 BaitPos { get => baitPos; set => baitPos = value; }


    // Start is called before the first frame update
    void Start()
    {
        InitWayPoints();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = gameObject.GetComponent<NavMeshAgent>();

        agent.Warp(gameObject.transform.position);

        gameObject.GetComponent<Newt_SM>().enabled = true;
    }

    private void InitWayPoints()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Waypoint");
        for (int i = 0; i < temp.Length; ++i)
        {
            Waypoints.Add(temp[i].transform);
        }
        Waypoints.Reverse();

        fleeIndex = (int)(Waypoints.Count / 2.0f);
    }

    public bool CheckRadiusPlayer(float radius)
    {
        Vector2 newtPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerPosition = new Vector2(player.position.x, player.position.z);

        return Vector2.Distance(newtPosition, playerPosition) < radius;
    }

    private bool CheckDistance(int index)
    {
        return agent.remainingDistance < 0.5f;
        //return (Vector3.Distance(gameObject.transform.position, Waypoints[index].position) < 1f);
    }

    //Wander Functions
    public void WanderWayPointStart()
    {
        Vector3 target;
        if ((!agent.hasPath || !agent.pathPending) && WanderWayPoint(out target))
        {
            agent.ResetPath();
            agent.SetDestination(target);
        }
    }

    public void WanderWayPointUpdate(float time)
    {
        if (CheckDistance(currentIndex))
        {

            ++currentIndex;
            if (currentIndex >= Waypoints.Count)
            {
                currentIndex = 0;
            }
            ++fleeIndex;
            if (fleeIndex >= Waypoints.Count)
            {
                fleeIndex = 0;
            }

            Vector3 target;
            if ((!agent.hasPath || !agent.pathPending) && WanderWayPoint(out target))
            {
                agent.ResetPath();
                agent.SetDestination(target);
            }
        }
    }

    private bool WanderWayPoint(out Vector3 result)
    {
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(Waypoints[currentIndex].position, out navHit, 100f, NavMesh.AllAreas))
        {
            navHitPoint = navHit.position;
            result = navHit.position;
            return true;
        }
        else
        {
            navHitPoint = navHit.position;
            result = gameObject.transform.position;
            return false;
        }
    }

    /*
     * Feauter was replaced by waypoint system, but solid code so not full removal
     * 
    public void WanderUpdate(float time)
    {
        wTimeElapsed += time;
        if (wTimeElapsed >= wTime)
        {
            wTimeElapsed = 0.0f;

            Vector3 target;
            if ((!agent.hasPath || !agent.pathPending) && WanderPoint(out target))
            {
                agent.ResetPath();  // THIS WAS THE BUG FIX, also added new conditions above to ensure there's only one path
                agent.SetDestination(target);
            }
        }
    }

    private bool WanderPoint(out Vector3 result)
    {
        randPoint = gameObject.transform.position + Random.insideUnitSphere * wanderDistanceMod;

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(randPoint, out navHit, wanderDistanceMod, NavMesh.AllAreas))
        {
            navHitPoint = navHit.position;
            result = navHit.position;
            return true;
        }
        else
        {
            navHitPoint = navHit.position;
            result = gameObject.transform.position;
            return false;
        }
    }
    */

    //Flee State Functions
    public void FleeWayPointStart()
    {
        Vector3 target;
        if (FleeWayPoint(out target))
        {
            agent.ResetPath();
            agent.SetDestination(target);

            SwapFleeIndex();
        }
    }

    public void FleeWayPointUpdate(float time)
    {
        
        if (CheckDistance(currentIndex))
        {
            agent.ResetPath();

            Vector3 target;
            if ((!agent.hasPath || !agent.pathPending) && FleeWayPoint(out target))
            {
                agent.ResetPath();
                agent.SetDestination(target);

                SwapFleeIndex();
            }
        }
    }

    private bool FleeWayPoint(out Vector3 result)
    {
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(Waypoints[fleeIndex].position, out navHit, 100f, NavMesh.AllAreas))
        {
            navHitPoint = navHit.position;
            result = navHit.position;
            return true;
        }
        else
        {
            navHitPoint = navHit.position;
            result = gameObject.transform.position;
            return false;
        }
    }

    private void SwapFleeIndex()
    {
        int temp = currentIndex;
        currentIndex = fleeIndex;
        fleeIndex = temp;
    }

    //Baited State Functions
    public bool CheckBaitedDistance()
    {
        return Vector3.Distance(gameObject.transform.position, baitPos) < baitRadius;
    }

    public void BaitedWayPointStart()
    {
        //Run call to find bait object pos

        //Set radius equal to distance to nearest waypoint + modifier

        Vector3 target;
        if ((!agent.hasPath || !agent.pathPending) && BaitedWayPoint(out target))
        {
            agent.ResetPath();
            agent.SetDestination(target);
        }
    }

    public void BaitedWayPointUpdate(float time)
    {
        if (CheckBaitedDistance())
        {
            Vector3 target;
            if ((!agent.hasPath || !agent.pathPending) && BaitedWayPoint(out target))
            {
                agent.ResetPath();
                agent.SetDestination(target);
            }
        }
        else if (CheckDistance(currentIndex))
        {

            ++currentIndex;
            if (currentIndex >= Waypoints.Count)
            {
                currentIndex = 0;
            }
            ++fleeIndex;
            if (fleeIndex >= Waypoints.Count)
            {
                fleeIndex = 0;
            }

            Vector3 target;
            if ((!agent.hasPath || !agent.pathPending) && WanderWayPoint(out target))
            {
                agent.ResetPath();
                agent.SetDestination(target);
            }
        }
    }

    public bool BaitedWayPoint(out Vector3 result)
    {
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(baitPos, out navHit, 100f, NavMesh.AllAreas))
        {
            navHitPoint = navHit.position;
            result = navHit.position;
            return true;
        }
        else
        {
            navHitPoint = navHit.position;
            result = gameObject.transform.position;
            return false;
        }
    }

    //Animation calls
    public void RunAnimation(string param, float val)
    {
        //Run an animation
    }

    //Setters
    public void AgentMovement(bool isMoving)
    {
        agent.isStopped = isMoving;
    }

    public void AgentSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void WarpAgent(Vector3 target)
    {
        agent.Warp(target);
    }

    public void DisableAgent(bool val)
    {
        agent.enabled = !val;
    }

    public void RemoveAnEye()
    {
        --eyeCount;
    }

    //Getters
    public float GetFleeDistance() { return fleeRadius; }

    public float GetFleeSpeed() { return fSpeed; }

    public float GetWanderSpeed() { return wSpeed; }

    public bool StillHaveEyes() { return (eyeCount > 0); }


    //Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderDistanceMod);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(navHitPoint, 0.1f);
    }
}

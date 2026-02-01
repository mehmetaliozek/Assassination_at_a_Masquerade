using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MovementController))]
public class Guard : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase }

    [Header("Durum")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Fark Edilme Ayarları")]
    public float timeToDetect = 3f;
    [Range(0, 1)]
    public float currentDetectionLevel = 0f;

    [Header("Hareket Ayarları")]
    public float patrolRadius = 15f;
    public float waitTime = 2f;
    public float stopDistanceTolerance = 1.5f;

    [Header("Takılma Kontrolü")]
    public float stuckCheckTime = 0.5f;
    public float minMoveDistance = 0.1f;

    [Header("Görüş")]
    public float viewRadius = 15f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Hedef")]
    public Transform playerTransform;
    private Player playerScript;

    private NavMeshAgent agent;
    private MovementController movementController;

    private float waitTimer;
    private float stuckTimer;
    private float detectionTimer = 0f;
    private Vector3 lastPosition;
    private bool hasPatrolTarget = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        movementController = GetComponent<MovementController>();
        waitTimer = waitTime;

        agent.updatePosition = false;
        agent.updateRotation = false;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
                playerScript = p.GetComponent<Player>();
            }
        }
        else
        {
            playerScript = playerTransform.GetComponent<Player>();
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        agent.nextPosition = transform.position;

        bool canSee = CanSeePlayer();

        if (currentState == EnemyState.Chase)
        {
            if (!canSee)
            {
                SwitchState(EnemyState.Idle);
                detectionTimer = 0f;
            }
        }
        else
        {
            if (canSee)
            {
                if (playerScript != null && playerScript.GetComponent<AnimationController>().GetThreat())
                {
                    detectionTimer = timeToDetect;
                    SwitchState(EnemyState.Chase);
                }
                else
                {
                    detectionTimer += Time.deltaTime;

                    if (detectionTimer >= timeToDetect)
                    {
                        SwitchState(EnemyState.Chase);
                    }
                }
            }
            else
            {
                detectionTimer -= Time.deltaTime;
                if (detectionTimer < 0) detectionTimer = 0;
            }
        }

        currentDetectionLevel = Mathf.Clamp01(detectionTimer / timeToDetect);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                CheckIfStuck();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
        }
    }

    void CheckIfStuck()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < minMoveDistance)
            {
                SwitchState(EnemyState.Idle);
                agent.ResetPath();
            }
            lastPosition = transform.position;
            stuckTimer = 0;
        }
    }

    void HandleIdle()
    {
        movementController.Move(0, 0);
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0) FindNewPatrolPoint();
    }

    void HandlePatrol()
    {
        if (!hasPatrolTarget || !agent.hasPath)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, agent.destination);
        if (dist <= stopDistanceTolerance) SwitchState(EnemyState.Idle);
        else MoveAgent();
    }

    void HandleChase()
    {
        if (playerTransform == null) return;
        agent.SetDestination(playerTransform.position);

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > stopDistanceTolerance) MoveAgent();
        else movementController.Move(0, 0);
    }

    void MoveAgent()
    {
        Vector3 dir = (agent.steeringTarget - transform.position);
        dir.y = 0;
        dir.Normalize();
        movementController.Move(dir.x, dir.z);
    }

    void FindNewPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = Random.insideUnitSphere * patrolRadius;
            randomPoint += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 4.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                hasPatrolTarget = true;
                SwitchState(EnemyState.Patrol);
                lastPosition = transform.position;
                stuckTimer = 0;
                return;
            }
        }
        waitTimer = 0.5f;
    }

    void SwitchState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        if (newState == EnemyState.Idle)
        {
            waitTimer = waitTime;
            hasPatrolTarget = false;
        }
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < viewRadius)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 aimDir = GetAimDirection();

            if (Vector3.Angle(aimDir, dirToPlayer) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, dist, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3 GetAimDirection()
    {
        if (movementController.Velocity.magnitude > 0.1f)
        {
            return movementController.Velocity.normalized;
        }

        return transform.forward;
    }
}
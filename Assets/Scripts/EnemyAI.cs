using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chasing, Searching }

    [Header("Persecución")]
    public float chaseSpeed = 4.5f;
    // Muy alta a propósito: así el agente alcanza su velocidad casi al instante
    // en vez de acelerar progresivamente, dando una velocidad constante desde que arranca
    public float acceleration = 999f;
    public float destinationUpdateInterval = 0.2f;

    [Header("Detección")]
    public float detectionRadius = 9f;
    public float eyeHeight = 1.5f;
    public float loseSightDelay = 0.6f; // cuánto tolera perder de vista al jugador antes de dejar de perseguir

    [Header("Búsqueda")]
    public float searchDuration = 2f; // tiempo que espera en el último punto visto antes de rendirse

    [Header("Patrulla")]
    public float patrolSpeed = 1.8f;
    public float patrolWaitTime = 2f;

    private NavMeshAgent agent;
    private Transform player;
    private State state;
    private float nextUpdateTime;
    private float sightLostTimer;
    private float searchTimer;
    private float patrolWaitTimer;
    private Vector3[] navMeshVertices;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = acceleration;
        // Vértices de todo el NavMesh bakeado: de aquí se sortean los puntos de
        // patrulla para que recorra el laberinto completo, no solo su punto de spawn
        navMeshVertices = NavMesh.CalculateTriangulation().vertices;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        EnterPatrol();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        bool canSeePlayer = CanSeePlayer();

        switch (state)
        {
            case State.Patrol:
                if (canSeePlayer) EnterChase();
                else UpdatePatrol();
                break;

            case State.Chasing:
                if (canSeePlayer)
                {
                    sightLostTimer = 0f;
                    // Recalcula la ruta cada cierto intervalo en vez de cada frame: el
                    // pathfinding es costoso y el destino apenas se mueve entre frames
                    if (Time.time >= nextUpdateTime)
                    {
                        agent.SetDestination(player.position);
                        nextUpdateTime = Time.time + destinationUpdateInterval;
                    }
                }
                else
                {
                    sightLostTimer += Time.deltaTime;
                    if (sightLostTimer >= loseSightDelay) EnterSearch();
                }
                break;

            case State.Searching:
                if (canSeePlayer)
                {
                    EnterChase();
                }
                else
                {
                    searchTimer += Time.deltaTime;
                    if (searchTimer >= searchDuration) EnterPatrol();
                }
                break;
        }
    }

    // Solo "ve" al jugador si está dentro del radio de detección y no hay
    // pared de por medio; si ambas se cumplen, empieza o continúa la persecución
    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRadius) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;
        Vector3 toPlayer = target - origin;

        // Se recorta un poco la distancia del rayo para no chocar contra el propio
        // collider del jugador y darlo por "bloqueado" al llegar justo a él
        float rayDistance = Mathf.Max(0f, toPlayer.magnitude - 0.5f);
        bool blocked = Physics.Raycast(origin, toPlayer.normalized, rayDistance);
        return !blocked;
    }

    private void EnterChase()
    {
        state = State.Chasing;
        sightLostTimer = 0f;
        agent.speed = chaseSpeed;
        nextUpdateTime = 0f;
    }

    private void EnterSearch()
    {
        state = State.Searching;
        searchTimer = 0f;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position); // último punto donde lo vio
    }

    private void EnterPatrol()
    {
        state = State.Patrol;
        agent.speed = patrolSpeed;
        patrolWaitTimer = 0f;
        PickNewPatrolPoint();
    }

    private void UpdatePatrol()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;
                PickNewPatrolPoint();
            }
        }
    }

    private void PickNewPatrolPoint()
    {
        // Elige un vértice al azar de todo el NavMesh: cada nueva ronda de patrulla
        // puede caer en cualquier parte del laberinto, no cerca de donde ya estaba
        if (navMeshVertices == null || navMeshVertices.Length == 0) return;

        Vector3 candidate = navMeshVertices[Random.Range(0, navMeshVertices.Length)];
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}

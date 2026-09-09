using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Persecución")]
    public float chaseSpeed = 4.5f;
    // Muy alta a propósito: así el agente alcanza chaseSpeed casi al instante
    // en vez de acelerar progresivamente, dando una velocidad constante desde que arranca
    public float acceleration = 999f;
    public float destinationUpdateInterval = 0.2f;

    private NavMeshAgent agent;
    private Transform player;
    private float nextUpdateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.acceleration = acceleration;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        // Recalcula la ruta cada cierto intervalo en vez de cada frame: el pathfinding
        // por NavMesh ya encuentra la ruta más corta a través del laberinto, y recalcularla
        // constantemente sería un gasto innecesario ya que el destino apenas se mueve entre frames
        if (Time.time >= nextUpdateTime)
        {
            agent.SetDestination(player.position);
            nextUpdateTime = Time.time + destinationUpdateInterval;
        }
    }
}

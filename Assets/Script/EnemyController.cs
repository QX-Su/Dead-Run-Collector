using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach to each enemy in the scene.
/// Requires: NavMeshAgent, a kinematic Rigidbody, and a trigger CapsuleCollider on the enemy.
/// The NavMesh must be baked in the Unity Editor before Play.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float chaseSprintSpeed = 6f;
    public float stoppingDistance = 0.5f;
    public float catchDistance = 1.2f;

    [Header("Activity Bounds (match map)")]
    public float minX = -20f;
    public float maxX = 65f;
    public float minZ = -20f;
    public float maxZ = 20f;

    [Header("Spawn Delay")]
    public float spawnDelay = 3f;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string speedParam = "Speed";

    NavMeshAgent agent;
    Transform player;
    PlayerCharacterController playerCtrl;
    bool isActive;
    Vector3 initialPosition;
    Quaternion initialRotation;
    Renderer[] renderers;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.enabled = false;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCtrl = playerObj.GetComponent<PlayerCharacterController>();
        }

        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        SetVisible(true);
        agent.enabled = true;
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

        if (GameManager.Instance == null || GameManager.Instance.IsEnded)
        {
            if (agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            if (animator != null) animator.SetFloat(speedParam, 0f);
            return;
        }

        if (player == null) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Match zombie speed to player: sprint when player sprints
        bool playerSprinting = playerCtrl != null && playerCtrl.IsSprinting;
        float currentChaseSpeed = playerSprinting ? chaseSprintSpeed : chaseSpeed;
        agent.speed = currentChaseSpeed;

        // Chase player, clamping destination inside the valid map bounds
        Vector3 target = player.position;
        target.x = Mathf.Clamp(target.x, minX, maxX);
        target.z = Mathf.Clamp(target.z, minZ, maxZ);

        agent.SetDestination(target);

        // Hard boundary: warp back if somehow outside bounds (NavMesh should prevent this)
        Vector3 pos = transform.position;
        float clampedX = Mathf.Clamp(pos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);
        if (clampedX != pos.x || clampedZ != pos.z)
        {
            agent.Warp(new Vector3(clampedX, pos.y, clampedZ));
        }

        // Drive animation: 0 = idle, 0.3 = walk, 1.0 = run
        if (animator != null)
        {
            float vel = agent.velocity.magnitude;
            float animSpeed;
            if (vel < 0.1f)
                animSpeed = 0f;             // idle
            else if (playerSprinting)
                animSpeed = 1f;             // run
            else
                animSpeed = 0.3f;           // walk
            animator.SetFloat(speedParam, animSpeed);
        }

        // Distance-based catch detection — reliable regardless of collider setup
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z));
        if (dist <= catchDistance)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.IsEnded) return;

        GameManager.Instance.TriggerGameOver();
    }

    // Called by GameManager.StartRound() when the player restarts
    public void ResetEnemy()
    {
        StopAllCoroutines();
        isActive = false;

        if (agent.enabled)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        SetVisible(false);
        StartCoroutine(ActivateAfterDelay());
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
            r.enabled = visible;
    }
}

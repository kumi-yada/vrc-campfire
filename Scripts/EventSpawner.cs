
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EventSpawner : UdonSharpBehaviour
{
    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 2f;

    [Header("Spawn Area (centered on this object)")]
    [SerializeField] private float xWidth = 10f;
    [SerializeField] private float yWidth = 5f;

    [Header("Terrain Raycast")]
    [Tooltip("Height above candidate spawn position to start the downward raycast.")]
    [SerializeField] private float raycastStartHeight = 50f;
    [Tooltip("Vertical offset to apply above the hit point so the spawned object doesn't clip into the terrain.")]
    [SerializeField] private float spawnYOffset = 0.1f;
    [Tooltip("Layer mask used for the raycast. Set to limit which colliders are considered (default: everything).")]
    [SerializeField] private LayerMask raycastLayerMask = ~0;

    [Header("Spawn Exclusion")]
    [Tooltip("Minimum horizontal distance from the center to avoid spawning (0 = no exclusion).")]
    [SerializeField] private float minSpawnDistance = 0f;

    private VRCObjectPool pool;
    private float nextSpawnTime;
    private EventType eventType = EventType.None;

    void Start()
    {
        pool = GetComponent<VRCObjectPool>();
    }

    public void SetEventType(EventType type)
    {
        eventType = type;
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (eventType == EventType.None) return;
        if (!Utilities.IsValid(pool) || !Networking.IsOwner(gameObject)) return;
        if (Time.time < nextSpawnTime) return;

        SpawnFromPool(eventType);
        ScheduleNextSpawn();
    }

    private void SpawnFromPool(EventType type)
    {
        GameObject spawned = pool.TryToSpawn();
        if (!Utilities.IsValid(spawned)) return;

        Vector3 randomOffset = GetRandomEdgeOffset();
        Vector3 candidatePosition = transform.position + randomOffset;

        // Default spawn position is the candidate position.
        Vector3 spawnPosition = candidatePosition;

        // Raycast down from above the candidate position to find the terrain / ground.
        Vector3 rayOrigin = candidatePosition + Vector3.up * raycastStartHeight;
        float rayDistance = raycastStartHeight * 2f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, raycastLayerMask))
        {
            spawnPosition = hit.point + Vector3.up * spawnYOffset;
        }

        var item = spawned.GetComponent<EventItem>();
        item.SetEventItem(pool, type, spawnPosition);
        // Debug.Log($"Spawned object at {spawnPosition} (candidate {candidatePosition}) with offset {randomOffset}", spawned);
    }

    private Vector3 GetRandomEdgeOffset()
    {
        float width = Mathf.Max(0f, xWidth);
        float depth = Mathf.Max(0f, yWidth);

        if (width <= 0f && depth <= 0f)
            return Vector3.zero;

        float halfX = width * 0.5f;
        float halfZ = depth * 0.5f;

        Vector3 offset = Vector3.zero;

        // Try rejection sampling a few times to find a point outside the exclusion radius.
        int attempts = 0;
        int maxAttempts = 20;
        do
        {
            float randX = Random.Range(-halfX, halfX);
            float randZ = Random.Range(-halfZ, halfZ);
            offset = new Vector3(randX, 0f, randZ);
            attempts++;
        } while (attempts < maxAttempts && offset.magnitude < Mathf.Max(0f, minSpawnDistance));

        // If rejection sampling failed, build a fallback point at the exclusion radius (or box edge).
        if (offset.magnitude < Mathf.Max(0f, minSpawnDistance))
        {
            if (minSpawnDistance <= 0f)
                return offset; // no exclusion requested

            float ang = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
            offset = dir * minSpawnDistance;

            // Clamp to the box so we don't go outside of the spawn area.
            offset.x = Mathf.Clamp(offset.x, -halfX, halfX);
            offset.z = Mathf.Clamp(offset.z, -halfZ, halfZ);

            // If clamping pushed us inside the exclusion again, push to the nearest box edge along dir.
            if (offset.magnitude < minSpawnDistance)
            {
                float tx = dir.x != 0f ? halfX / Mathf.Abs(dir.x) : float.MaxValue;
                float tz = dir.z != 0f ? halfZ / Mathf.Abs(dir.z) : float.MaxValue;
                float t = Mathf.Min(tx, tz);
                offset = dir * t;
            }
        }

        return offset;
    }

    private void ScheduleNextSpawn()
    {
        float minInterval = Mathf.Max(0.01f, minSpawnInterval);
        float maxInterval = Mathf.Max(minInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
        // Debug.Log($"Next spawn scheduled for {nextSpawnTime} (in {nextSpawnTime - Time.time} seconds)");
    }
}

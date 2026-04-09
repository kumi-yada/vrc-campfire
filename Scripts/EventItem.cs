
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EventItem : UdonSharpBehaviour
{

    public GameObject[] visuals;
    public Color[] randomColors;

    private VRCObjectPool objectPool;
    [UdonSynced] private EventType eventType;
    [UdonSynced] private Color color;
    [UdonSynced] private Vector3 spawnPosition;

    void Start()
    {
        UpdateVisuals();
    }

    public void SetEventItem(VRCObjectPool pool, EventType type, Vector3 position)
    {
        eventType = type;
        objectPool = pool;
        spawnPosition = position;

        if (randomColors != null && randomColors.Length > 0)
        {
            color = randomColors[Random.Range(0, randomColors.Length)];
        }
        UpdateVisuals();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        gameObject.transform.position = spawnPosition;
        for (int i = 0; i < visuals.Length; i++)
        {
            var isActive = i == (int)eventType;
            visuals[i].SetActive(isActive);
            if (isActive)
            {
                {
                    var renderers = visuals[i].GetComponentsInChildren<Renderer>();
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        if (renderers[r] != null)
                            renderers[r].material.color = color;
                    }
                }
            }
        }
    }

    public override void Interact()
    {
        int value = PlayerData.GetInt(Networking.LocalPlayer, $"EventItem_{eventType}");
        PlayerData.SetInt($"EventItem_{eventType}", value + 1);
        Debug.Log($"Player {Networking.LocalPlayer.displayName} interacted with {eventType} item. Total: {value + 1}");
        objectPool.Return(gameObject);
    }
}

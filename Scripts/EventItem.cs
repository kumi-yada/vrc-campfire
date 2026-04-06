
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EventItem : UdonSharpBehaviour
{

    public GameObject easterEgg;

    private EventType eventType;
    private VRCObjectPool objectPool;

    void Start()
    {
        UpdateVisuals();
    }

    public void SetEventItem(VRCObjectPool pool, EventType type)
    {
        eventType = type;
        objectPool = pool;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        easterEgg.SetActive(eventType == EventType.Easter);
    }

    void Interact()
    {
        int value = PlayerData.GetInt(Networking.LocalPlayer, $"EventItem_{eventType}");
        PlayerData.SetInt($"EventItem_{eventType}", value + 1);
        Debug.Log($"Player {Networking.LocalPlayer.displayName} interacted with {eventType} item. Total: {value + 1}");
        objectPool.Return(gameObject);
    }
}

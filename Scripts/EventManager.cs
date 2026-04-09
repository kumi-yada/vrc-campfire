
using BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public enum EventType
{
    Easter,
    None,
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class EventManager : UdonSharpBehaviour
{
    public Image eventImage;
    public TextMeshProUGUI eventCount;
    public GameObject eventPanel;
    public EventSpawner spawner;

    public Sprite[] eventSprites;
    public GameObject[] unlockedEvents;

    [UdonSynced] private EventType currentEvent = EventType.None;

    private int eventItemCount = 0;
    private const int maxItemCount = 20;

    void Start()
    {
        foreach (GameObject obj in unlockedEvents)
        {
            obj.SetActive(false);
        }
    }

    private EventType GetCurrentEventType()
    {
        var now = System.DateTime.Now;
        var month = now.Month;

        if (month == 4)
        {
            return EventType.Easter;
        }

        return EventType.None;
    }

    public override void OnDeserialization()
    {
        UpdateEventUI();
    }

    private void UpdateEventUI()
    {
        if (currentEvent == EventType.None)
        {
            eventPanel.SetActive(false);
        }
        else
        {
            eventPanel.SetActive(true);
            eventImage.sprite = eventSprites[(int)currentEvent];
        }
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            currentEvent = GetCurrentEventType();
            UpdateEventUI();
            RequestSerialization();

            if (currentEvent != EventType.None)
            {
                var count = PlayerData.GetInt(Networking.LocalPlayer, $"EventItem_{currentEvent}");
                SetEventItemCount(count);
            }
            Debug.Log($"Player {player.displayName} restored. Current event: {currentEvent}. Item count: {eventItemCount}");
            spawner.SetEventType(currentEvent);

            if (currentEvent != EventType.None && eventItemCount >= maxItemCount)
            {
                unlockedEvents[(int)currentEvent].SetActive(true);
            }
        }
    }

    private void SetEventItemCount(int count)
    {
        eventItemCount = count;
        eventCount.text = $"{eventItemCount}/{maxItemCount}";
    }

}

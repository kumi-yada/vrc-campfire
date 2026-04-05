
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DayNightToggle : UdonSharpBehaviour
{
    public Material DayMat;
    public Material NightMat;
    public GameObject Sun;
    public GameObject Moon;

    [UdonSynced] public bool isDay = true;

    void Start()
    {
        ApplyState();
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        isDay = !isDay;
        ApplyState();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        if (DayMat != null && NightMat != null)
        {
            RenderSettings.skybox = isDay ? DayMat : NightMat;
        }

        if (Sun != null)
        {
            Sun.SetActive(isDay);
        }

        if (Moon != null)
        {
            Moon.SetActive(!isDay);
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDKBase;
using VRC.Udon;

public class Marhsmallow : UdonSharpBehaviour
{
    public GameObject marshmallow;
    public Renderer marshmallowRenderer;
    public AudioSource eatSound;
    public AudioSource putSound;
    public AudioSource roastSound;
    public float maxCookTime = 5f;
    public float coolRate = 1f;

    [UdonSynced]
    private bool _isVisible = false;
    [UdonSynced]
    private bool _isRoasted = false;

    private bool _isNearCampFire = false;
    private float _cookTimer = 0f;

    private bool _prevIsVisible = false;
    private bool _prevIsRoasted = false;

    private const string MarshmallowSpawnTag = "MarshmallowSpawn";
    private const string CampFireTag = "CampFire";


    void Start()
    {
        _prevIsVisible = _isVisible;
        _prevIsRoasted = _isRoasted;
        ApplyVisibility();
        ApplyRoast();
    }

    void Update()
    {
        if (_isNearCampFire && _isVisible)
        {
            _cookTimer += Time.deltaTime;
            Debug.Log($"Cooking marshmallow: {_cookTimer}/{maxCookTime}");
            if (_cookTimer >= maxCookTime)
            {
                Roast();
            }
        }
        else if (!_isNearCampFire && _cookTimer > 0f && !_isRoasted)
        {
            _cookTimer -= coolRate * Time.deltaTime;
            if (_cookTimer < 0f) _cookTimer = 0f;
        }
    }

    public override void OnContactEnter(ContactEnterInfo contactInfo)
    {
        var isCampFire = System.Array.IndexOf(contactInfo.matchingTags, CampFireTag) != -1;
        if (isCampFire)
        {
            _isNearCampFire = true;
        }
        else
        {
            var newState = System.Array.IndexOf(contactInfo.matchingTags, MarshmallowSpawnTag) != -1;
            if (_isVisible && !newState)
            {
                Eat();
            }
            else if (!_isVisible && newState)
            {
                PutNew();
            }
        }
    }

    public override void OnContactExit(ContactExitInfo contactInfo)
    {
        var isCampFire = System.Array.IndexOf(contactInfo.matchingTags, CampFireTag) != -1;
        if (isCampFire)
        {
            _isNearCampFire = false;
        }
    }

    public override void OnDeserialization()
    {
        if (_prevIsVisible && !_isVisible && eatSound != null) eatSound.Play();
        else if (!_prevIsVisible && _isVisible && putSound != null) putSound.Play();
        if (!_prevIsRoasted && _isRoasted && roastSound != null) roastSound.Play();

        _prevIsVisible = _isVisible;
        _prevIsRoasted = _isRoasted;

        ApplyVisibility();
        ApplyRoast();
    }

    private void ApplyVisibility()
    {
        marshmallow.SetActive(_isVisible);
    }

    private void ApplyRoast()
    {
        if (marshmallowRenderer != null)
            marshmallowRenderer.material.SetFloat("_OverlayOpacity", _isRoasted ? 1f : 0f);
    }

    void OnPickupUseDown()
    {
        if (!Networking.IsOwner(gameObject) || !_isVisible) return;
        Eat();
    }

    private void Roast()
    {
        _cookTimer = 0f;
        _isRoasted = true;
        RequestSerialization();
        ApplyRoast();
        if (roastSound != null) roastSound.Play();
        _prevIsRoasted = _isRoasted;
    }

    private void Eat()
    {
        _isVisible = false;
        _prevIsVisible = _isVisible;
        RequestSerialization();
        ApplyVisibility();
        if (eatSound != null)
        {
            eatSound.Play();
        }
    }

    private void PutNew()
    {
        _isVisible = true;
        _isRoasted = false;
        _cookTimer = 0f;
        _prevIsVisible = _isVisible;
        _prevIsRoasted = _isRoasted;
        RequestSerialization();
        ApplyVisibility();
        ApplyRoast();
        if (putSound != null)
        {
            putSound.Play();
        }
    }
}

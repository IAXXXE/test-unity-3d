using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    // cached
    Transform _tr;
    TextMeshPro _tmp;
    Camera _cam;

    // runtime state
    float _lifeTime;
    float _age;
    Vector3 _velocity;
    bool _inUse;

    // inspector-configurable
    public float lifetime = 1.0f;
    public Vector3 initialVelocity = new Vector3(0, 1.0f, 0);
    public float gravity = -1.5f;
    public AnimationCurve scaleByDistance = AnimationCurve.Linear(0, 1, 10, 0.5f);
    public float minDistance = 1f;
    public float maxDistance = 30f;
    public float minScale = 0.5f;
    public float maxScale = 1.0f;
    public bool faceCamera = true;
    public Color normalColor = Color.white;
    public Color critColor = Color.red;
    public float fadeStart = 0.6f; // from normalized lifetime
    public float fadeEnd = 1.0f;

    void Awake()
    {
        _tr = transform;
        _tmp = GetComponent<TextMeshPro>();
        _tmp.enableWordWrapping = false;
        _tmp.alignment = TextAlignmentOptions.Center;
        _tmp.raycastTarget = false;
        _cam = Camera.main;
        gameObject.SetActive(false);
    }

    public void Play(string text, Vector3 worldPos, Camera cam = null, bool isCritical = false, float life = -1f)
    {
        if (cam != null) _cam = cam;
        _tmp.text = text;
        _tmp.color = isCritical ? critColor : normalColor;
        if(isCritical) _tmp.text += "  !!!";
        _lifeTime = life > 0 ? life : lifetime;
        _age = 0f;
        _inUse = true;
        _tr.position = worldPos;
        _velocity = initialVelocity;
        gameObject.SetActive(true);

        // initial scale
        UpdateScaleByDistance();
        // Immediately face camera once
        FaceCamera();
    }

    void Update()
    {
        if (!_inUse) return;

        float dt = Time.deltaTime;
        _age += dt;

        // movement (simple upward + gravity)
        _velocity.y += gravity * dt;
        _tr.position += _velocity * dt;

        // face camera
        FaceCamera();

        // update scale based on distance only when camera or position changed significantly
        UpdateScaleByDistance();

        // fading
        float tNorm = Mathf.Clamp01(_age / _lifeTime);
        if (tNorm >= fadeStart)
        {
            float sub = Mathf.InverseLerp(fadeStart, fadeEnd, tNorm);
            Color c = _tmp.color;
            c.a = Mathf.Lerp(1f, 0f, sub);
            _tmp.color = c;
        }

        if (_age >= _lifeTime)
            Recycle();
    }

    void FaceCamera()
    {
        if (!faceCamera || _cam == null) return;
        // Billboard so text faces camera but keep Z-rotation zero if needed:
        _tr.rotation = Quaternion.LookRotation(_tr.position - _cam.transform.position);
        // Alternative simpler:
        // _tr.rotation = _cam.transform.rotation;
    }

    void UpdateScaleByDistance()
    {
        if (_cam == null) return;
        float sqrDist = (_tr.position - _cam.transform.position).sqrMagnitude;
        float dist = Mathf.Sqrt(sqrDist); // only one sqrt per update per active popup
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);
        float scale = Mathf.Lerp(maxScale, minScale, t);
        // optionally use AnimationCurve for finer control:
        // float curveVal = scaleByDistance.Evaluate(dist);
        // scale *= curveVal;
        _tr.localScale = Vector3.one * scale;
    }

    void Recycle()
    {
        _inUse = false;
        gameObject.SetActive(false);
        DamagePopupPool.Instance.ReturnToPool(this);
    }
}

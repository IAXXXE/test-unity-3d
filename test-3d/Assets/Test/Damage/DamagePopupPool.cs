using System.Collections.Generic;
using UnityEngine;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance { get; private set; }

    [Header("Prefab & Camera")]
    public GameObject popupPrefab;
    public Camera targetCamera;

    [Header("Pool settings")]
    public int initialPool = 50;
    public int maxPool = 200;

    Queue<DamagePopup> _pool = new Queue<DamagePopup>();
    Transform _root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (targetCamera == null) targetCamera = Camera.main;
        _root = new GameObject("DamagePopupPoolRoot").transform;
        _root.SetParent(transform, false);
        WarmPool(initialPool);
    }

    void WarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DamagePopup p = CreateNew();
            p.gameObject.SetActive(false);
            _pool.Enqueue(p);
        }
    }

    DamagePopup CreateNew()
    {
        DamagePopup p = Instantiate(popupPrefab, _root).GetComponent<DamagePopup>();
        return p;
    }

    public void ShowDamage(Vector3 worldPos, int damage, bool isCritical = false, float lifeTime = -1f)
    {
        ShowDamage(worldPos, damage.ToString(), isCritical, lifeTime);
    }

    public void ShowDamage(Vector3 worldPos, string text, bool isCritical = false, float lifeTime = -1f)
    {
        DamagePopup popup = GetFromPool();
        popup.transform.SetParent(null); // detach so it sits in world
        popup.Play(text, worldPos, targetCamera, isCritical, lifeTime);
    }

    DamagePopup GetFromPool()
    {
        if (_pool.Count > 0) return _pool.Dequeue();
        if (_pool.Count + 1 <= maxPool) return CreateNew();
        // pool exhausted: reuse oldest active? for safety, create but warn
        Debug.LogWarning("DamagePopupPool exhausted, creating extra instance");
        return CreateNew();
    }

    public void ReturnToPool(DamagePopup popup)
    {
        // reset parent to pool root to keep hierarchy tidy
        popup.transform.SetParent(_root, false);
        if (_pool.Count < maxPool) _pool.Enqueue(popup);
        else Destroy(popup.gameObject); // keep memory bounded
    }
}

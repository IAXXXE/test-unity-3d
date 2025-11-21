using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }

    [Header("Camera")]
    public Camera targetCamera;
    
    [Header("Prefab")]
    public GameObject popupPrefab;
    public GameObject petStatsPrefab;

    [Header("Pool settings")]
    public int initialPool = 5;
    public int maxPool = 20;

    Queue<DamagePopup> _damagePopupPool = new Queue<DamagePopup>();
    Queue<PetStatsPopup> _petStatsPopupPool = new Queue<PetStatsPopup>();
    Transform _root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (targetCamera == null) targetCamera = Camera.main;
        _root = new GameObject("FloatingTextPoolRoot").transform;
        _root.SetParent(transform, false);
        WarmPool(initialPool);
    }

    void WarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DamagePopup p = CreateNewDamage();
            p.gameObject.SetActive(false);
            _damagePopupPool.Enqueue(p);
        }

        for(int i = 0; i < count; i++)
        {
            PetStatsPopup p = CreateNewPetStats();
            p.gameObject.SetActive(false);
            _petStatsPopupPool.Enqueue(p);
        }
    }

    DamagePopup CreateNewDamage()
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
        DamagePopup popup = GetDamageFromPool();
        popup.transform.SetParent(null); // detach so it sits in world
        popup.Play(text, worldPos, targetCamera, isCritical, lifeTime);
    }

    DamagePopup GetDamageFromPool()
    {
        if (_damagePopupPool.Count > 0) return _damagePopupPool.Dequeue();
        if (_damagePopupPool.Count + 1 <= maxPool) return CreateNewDamage();
        return CreateNewDamage();
    }

    public void ReturnToPool(DamagePopup popup)
    {
        // reset parent to pool root to keep hierarchy tidy
        popup.transform.SetParent(_root, false);
        if (_damagePopupPool.Count < maxPool) _damagePopupPool.Enqueue(popup);
        else Destroy(popup.gameObject); // keep memory bounded
    }

    public void ShowPetStats(Vector3 worldPos, string spriteID, string text, bool isFull = false, float lifeTime = -1f)
    {
        PetStatsPopup popup = GetPetStatsFromPool();
        popup.transform.SetParent(null);
        popup.Play(text, worldPos, spriteID, targetCamera, isFull, lifeTime);
    }

    PetStatsPopup GetPetStatsFromPool()
    {
        if(_petStatsPopupPool.Count > 0)  return _petStatsPopupPool.Dequeue();
        if (_petStatsPopupPool.Count + 1 <= maxPool) return CreateNewPetStats();
        return CreateNewPetStats();
    }

    PetStatsPopup CreateNewPetStats()
    {
        PetStatsPopup p = Instantiate(petStatsPrefab, _root).GetComponent<PetStatsPopup>();
        return p;
    }

    public void ReturnToPool(PetStatsPopup popup)
    {
        popup.transform.SetParent(_root, false);
        if (_petStatsPopupPool.Count < maxPool) _petStatsPopupPool.Enqueue(popup);
        else Destroy(popup.gameObject); // keep memory bounded
    }
}

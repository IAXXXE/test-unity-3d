using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MouseSliceNew : MonoBehaviour
{
    [Header("References")]
    public GameObject plane;
    public Transform ObjectContainer;
    public ScreenLineRenderer lineRenderer;

    [Header("Slice Settings")]
    public float separation = 0.1f;
    public bool drawPlane;
    public float sliceForce = 5f;

    [Header("Optimization")]
    public int maxSlicesPerFrame = 2;
    public bool enableObjectPooling = true;

    private Plane slicePlane = new Plane();
    private MeshCutter meshCutter;
    private TempMesh biggerMesh, smallerMesh;
    
    // 对象池和缓存
    private List<GameObject> sliceableObjects = new List<GameObject>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private int slicesThisFrame = 0;

    // 特效和音效
    public ParticleSystem sliceEffect;
    public AudioClip sliceSound;

    void Start()
    {
        meshCutter = new MeshCutter(256);
        PrecacheSliceableObjects();
        InitializeObjectPool(10);
    }

    void Update()
    {
        slicesThisFrame = 0; // 每帧重置计数
    }

    #region 初始化优化
    void PrecacheSliceableObjects()
    {
        sliceableObjects.Clear();
        sliceableObjects.AddRange(GameObject.FindGameObjectsWithTag("Sliceable"));
    }

    void InitializeObjectPool(int poolSize)
    {
        if (!enableObjectPooling) return;

        for (int i = 0; i < poolSize; i++)
        {
            var poolObj = new GameObject($"PoolObject_{i}");
            poolObj.transform.parent = ObjectContainer;
            poolObj.SetActive(false);
            poolObj.AddComponent<MeshFilter>();
            poolObj.AddComponent<MeshRenderer>();
            poolObj.AddComponent<Rigidbody>();
            poolObj.tag = "Sliceable";
            objectPool.Enqueue(poolObj);
        }
    }
    #endregion

    #region 切割核心逻辑
    private void OnLineDrawn(Vector3 start, Vector3 end, Vector3 depth)
    {
        if (slicesThisFrame >= maxSlicesPerFrame) return;

        var planeTangent = (end - start).normalized;
        if (planeTangent == Vector3.zero)
            planeTangent = Vector3.right;

        var normalVec = Vector3.Cross(depth, planeTangent);

        if (drawPlane) DrawPlane(start, end, normalVec);
        
        StartCoroutine(SliceObjectsCoroutine(start, normalVec));
    }

    IEnumerator SliceObjectsCoroutine(Vector3 point, Vector3 normal)
    {
        List<Transform> positive = new List<Transform>();
        List<Transform> negative = new List<Transform>();

        bool slicedAny = false;

        foreach (var obj in sliceableObjects.ToArray())
        {
            if (obj == null) continue;
            if (slicesThisFrame >= maxSlicesPerFrame) break;

            var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;
            slicePlane.SetNormalAndPosition(
                transformedNormal,
                obj.transform.InverseTransformPoint(point));

            if (SliceObject(ref slicePlane, obj, positive, negative))
            {
                slicedAny = true;
                slicesThisFrame++;
                
                // 播放特效和音效
                // PlaySliceEffects(obj.transform.position, normal);
                
                // 每帧限制处理数量
                if (slicesThisFrame >= maxSlicesPerFrame)
                    yield return null;
            }
        }

        if (slicedAny)
        {
            SeparateMeshes(positive, negative, normal);
            ApplySliceForces(positive, negative, normal);
        }
    }

    bool SliceObject(ref Plane slicePlane, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects)
    {
        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return false;

        var mesh = meshFilter.mesh;

        if (!meshCutter.SliceMesh(mesh, ref slicePlane))
        {
            // 无法切割，按位置分类
            if (slicePlane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
                positiveObjects.Add(obj.transform);
            else
                negativeObjects.Add(obj.transform);
            return false;
        }

        // 确定哪个网格更大
        bool posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
        biggerMesh = posBigger ? meshCutter.PositiveMesh : meshCutter.NegativeMesh;
        smallerMesh = posBigger ? meshCutter.NegativeMesh : meshCutter.PositiveMesh;

        // 创建新对象
        GameObject newObject = GetPooledObjectOrCreate(obj);
        CopyComponentValues(obj, newObject);

        // 更新网格和碰撞器
        ReplaceMesh(meshFilter.mesh, biggerMesh, obj);
        ReplaceMesh(newObject.GetComponent<MeshFilter>().mesh, smallerMesh, newObject);

        // 添加到相应列表
        (posBigger ? positiveObjects : negativeObjects).Add(obj.transform);
        (posBigger ? negativeObjects : positiveObjects).Add(newObject.transform);

        return true;
    }
    #endregion

    #region 对象管理优化
    GameObject GetPooledObjectOrCreate(GameObject original)
    {
        if (enableObjectPooling && objectPool.Count > 0)
        {
            var pooledObj = objectPool.Dequeue();
            pooledObj.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
            pooledObj.SetActive(true);
            return pooledObj;
        }
        else
        {
            return Instantiate(original, ObjectContainer);
        }
    }

    void CopyComponentValues(GameObject source, GameObject destination)
    {
        // 复制材质
        var sourceRenderer = source.GetComponent<MeshRenderer>();
        var destRenderer = destination.GetComponent<MeshRenderer>();
        if (sourceRenderer && destRenderer)
        {
            destRenderer.materials = sourceRenderer.materials;
        }

        // 复制标签和图层
        destination.tag = source.tag;
        destination.layer = source.layer;
    }
    #endregion

    #region 物理和效果
    void ReplaceMesh(Mesh mesh, TempMesh tempMesh, GameObject targetObject)
    {
        mesh.Clear();
        mesh.SetVertices(tempMesh.vertices);
        mesh.SetTriangles(tempMesh.triangles, 0);
        mesh.SetNormals(tempMesh.normals);
        mesh.SetUVs(0, tempMesh.uvs);
        mesh.RecalculateTangents();

        // 强制更新碰撞器
        UpdateMeshCollider(targetObject, mesh);
    }

    void UpdateMeshCollider(GameObject obj, Mesh newMesh)
    {
        // 移除现有碰撞器
        var existingColliders = obj.GetComponents<MeshCollider>();
        foreach (var collider in existingColliders)
        {
            DestroyImmediate(collider);
        }

        // 添加新的凸包碰撞器
        var meshCollider = obj.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = newMesh;
        meshCollider.convex = true;
        
        // 确保有刚体组件
        var rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = obj.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    void ApplySliceForces(List<Transform> positives, List<Transform> negatives, Vector3 normal)
    {
        foreach (var trans in positives)
        {
            var rb = trans.GetComponent<Rigidbody>();
            if (rb) rb.AddForce(normal * sliceForce, ForceMode.Impulse);
        }

        foreach (var trans in negatives)
        {
            var rb = trans.GetComponent<Rigidbody>();
            if (rb) rb.AddForce(-normal * sliceForce, ForceMode.Impulse);
        }
    }

    void PlaySliceEffects(Vector3 position, Vector3 normal)
    {
        // 播放切割特效
        if (sliceEffect)
        {
            var effect = Instantiate(sliceEffect, position, Quaternion.LookRotation(normal));
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        // 播放切割音效
        if (sliceSound)
        {
            AudioSource.PlayClipAtPoint(sliceSound, position);
        }
    }
    #endregion

    #region 工具方法
    void DrawPlane(Vector3 start, Vector3 end, Vector3 normalVec)
    {
        Quaternion rotate = Quaternion.FromToRotation(Vector3.up, normalVec);
        plane.transform.localRotation = rotate;
        plane.transform.position = (end + start) / 2;
        plane.SetActive(true);
    }

    void SeparateMeshes(List<Transform> positives, List<Transform> negatives, Vector3 worldPlaneNormal)
    {
        var separationVector = worldPlaneNormal * separation;

        foreach (var trans in positives)
            trans.position += separationVector;

        foreach (var trans in negatives)
            trans.position -= separationVector;
    }

    // 清理和对象回收
    public void ReturnToPool(GameObject obj)
    {
        if (enableObjectPooling)
        {
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
    #endregion

    private void OnEnable() => lineRenderer.OnLineDrawn += OnLineDrawn;
    private void OnDisable() => lineRenderer.OnLineDrawn -= OnLineDrawn;
}
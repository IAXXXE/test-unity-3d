using UnityEngine;

// [DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class PickupableOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.yellow;
    [Range(0.001f, 0.03f)] public float outlineWidth = 0.008f;

    private Material outlineMaterial;
    private Material[] originalMaterials;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMaterials = rend.sharedMaterials;

        Shader outlineShader = Shader.Find("Custom/Outline");
        if (outlineShader == null)
        {
            Debug.LogError("未找到 Custom/Outline Shader，请确认你已导入 Outline.shader 文件。");
            enabled = false;
            return;
        }

        outlineMaterial = new Material(outlineShader);
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnEnable()
    {
        ApplyOutline();
    }

    void OnDisable()
    {
        RemoveOutline();
    }

    public void ApplyOutline()
    {
        if (rend == null) return;

        var mats = new Material[originalMaterials.Length + 1];
        originalMaterials.CopyTo(mats, 0);
        mats[mats.Length - 1] = outlineMaterial;
        rend.materials = mats;
    }

    public void RemoveOutline()
    {
        if (rend == null) return;
        rend.materials = originalMaterials;
    }

    public void SetColor(Color color)
    {
        outlineColor = color;
        if (outlineMaterial) outlineMaterial.SetColor("_OutlineColor", color);
    }
}

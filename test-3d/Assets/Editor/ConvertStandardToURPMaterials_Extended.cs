using UnityEngine;
using UnityEditor;
using System.IO;

public class ConvertStandardToURPMaterials_Extended : EditorWindow
{
    private string targetFolder = "Assets"; // 默认路径

    [MenuItem("Tools/URP/批量转换Standard材质到URP SimpleLit (含贴图保留)")]
    static void ShowWindow()
    {
        GetWindow<ConvertStandardToURPMaterials_Extended>("Convert Standard to URP");
    }

    void OnGUI()
    {
        GUILayout.Label("批量转换 Standard Shader → URP/Simple Lit", EditorStyles.boldLabel);
        GUILayout.Space(8);

        EditorGUILayout.LabelField("目标文件夹：", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(targetFolder, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择包含材质的文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                // 转换为相对Assets路径
                if (selected.StartsWith(Application.dataPath))
                {
                    targetFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目内的Assets文件夹！", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(12);

        if (GUILayout.Button("开始转换材质", GUILayout.Height(32)))
        {
            if (Directory.Exists(targetFolder))
            {
                ConvertMaterials(targetFolder);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "路径无效，请选择一个有效的项目内文件夹。", "确定");
            }
        }
    }

    static void ConvertMaterials(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            // 仅处理包含 "Standard" 的 Shader
            if (!mat.shader.name.Contains("Standard"))
                continue;

            Debug.Log($"转换材质：{path}");

            // 记录旧属性
            Texture albedoTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Color albedoColor = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            Texture normalTex = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
            float bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;

            Texture metallicTex = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
            float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;

            Texture emissionTex = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
            Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            // 替换为 URP Simple Lit
            mat.shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (mat.shader == null)
            {
                Debug.LogError("❌ 找不到 URP Simple Lit Shader！");
                continue;
            }

            // BaseMap
            if (albedoTex)
                mat.SetTexture("_BaseMap", albedoTex);
            mat.SetColor("_BaseColor", albedoColor);

            // Metallic
            if (metallicTex)
                mat.SetTexture("_MetallicGlossMap", metallicTex);
            mat.SetFloat("_Metallic", metallic);

            // Normal
            if (normalTex)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetFloat("_BumpScale", bumpScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            // Emission
            if (emissionTex || emissionColor.maxColorComponent > 0f)
            {
                mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
            }

            // Surface 设置
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Cull", 0f);    // Both
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.75f);

            // 确保透明混合
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ 已成功转换 {count} 个 Standard 材质为 URP Simple Lit (保留贴图属性)。");
    }
}

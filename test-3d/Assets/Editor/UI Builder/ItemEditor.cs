using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private ItemDataList_SO dataBase;
    private List<ItemData> itemList = new List<ItemData>();
    private VisualTreeAsset itemRowTemplate;
    private ScrollView itemDataSection;
    private ItemData activeItem;
    private VisualElement iconPreview;
    private Sprite defaultIcon;

    private ListView itemListView;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // 加载模板数据
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Icon/bonus_05.png");

        // 变量赋值
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        itemDataSection = root.Q<ScrollView>("ItemDatas");
        iconPreview = itemDataSection.Q<VisualElement>("Icon");

        // 获得增删按钮
        root.Q<Button>("AddButton").clicked += OnAddClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;

        LoadDataBase();
        GenerateListView();
    }

    private void OnAddClicked()
    {
        ItemData newItem = new ItemData();
        newItem.itemName = "New Item";
        newItem.itemID = 100000 + itemList.Count;
        itemList.Add(newItem);

        itemListView.Rebuild();
    }

    private void OnDeleteClicked()
    {
        itemList.Remove(activeItem);
        itemListView.Rebuild();
        itemDataSection.visible = false;
    }

    private void LoadDataBase()
    {
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");

        if (dataArray.Length > 1)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            dataBase = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }
        itemList = dataBase.itemDataList;
        EditorUtility.SetDirty(dataBase);

        Debug.Log(itemList[0].itemID);
    }

    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < itemList.Count)
            {
                if (itemList[i].icon != null)
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].icon.texture;
                e.Q<Label>("Name").text = itemList[i] == null ? "Null" : itemList[i].itemName;
                e.Q<Label>("ID").text = itemList[i] == null ? "0x00" : itemList[i].itemID.ToString();
            }
        };

        itemListView.fixedItemHeight = 60;
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;

        itemListView.selectionChanged += OnListSelectionChange;

        itemDataSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        activeItem = (ItemData)selectedItem.First();
        if (activeItem == null) Debug.Log("Null active Item");
        GetItemDatas();
        itemDataSection.visible = true;
    }

    private void GetItemDatas()
    {
        itemDataSection.MarkDirtyRepaint();

        #region Item Data

        var itemId = itemDataSection.Q<IntegerField>("ItemID");
        itemId.value = activeItem.itemID;
        itemId.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemID = evt.newValue;
            itemListView.Rebuild();
        });

        var itemName = itemDataSection.Q<TextField>("ItemName");
        itemName.value = activeItem.itemName;
        itemName.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemName = evt.newValue;
            itemListView.Rebuild();
        });

        var itemType = itemDataSection.Q<EnumField>("ItemType");
        itemType.Init(activeItem.itemType);
        itemType.value = activeItem.itemType;
        itemType.RegisterValueChangedCallback(evt =>
        {
            activeItem.itemType = (ItemType)evt.newValue;
        });

        var itemIcon = itemDataSection.Q<ObjectField>("ItemIcon");
        iconPreview.style.backgroundImage = activeItem.icon == null ? defaultIcon.texture : activeItem.icon.texture;
        itemIcon.value = activeItem.icon;
        itemIcon.RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            activeItem.icon = newIcon;
            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
            itemListView.Rebuild();
        });

        var itemPrefab = itemDataSection.Q<ObjectField>("ItemPrefab");
        itemPrefab.value = activeItem.worldPrefab;
        itemPrefab.RegisterValueChangedCallback(evt =>
        {
            activeItem.worldPrefab = evt.newValue as GameObject;
        });

        var itemDescription = itemDataSection.Q<TextField>("Description");
        itemDescription.value = activeItem.description;
        itemDescription.RegisterValueChangedCallback(evt =>
        {
            activeItem.description = evt.newValue;
        });

        var position = itemDataSection.Q<Vector3Field>("Position");
        position.value = activeItem.posOffset;
        position.RegisterValueChangedCallback(evt =>
        {
            activeItem.posOffset = evt.newValue;
        });

        var rotation = itemDataSection.Q<Vector3Field>("Rotation");
        rotation.value = activeItem.rotOffset;
        rotation.RegisterValueChangedCallback(evt =>
        {
            activeItem.rotOffset = evt.newValue;
        });

        var scale = itemDataSection.Q<Vector3Field>("Scale");
        scale.value = activeItem.scale;
        scale.RegisterValueChangedCallback(evt =>
        {
            activeItem.scale = evt.newValue;
        });

        var isStackable = itemDataSection.Q<Toggle>("Stackable");
        isStackable.value = activeItem.isStackable;
        isStackable.RegisterValueChangedCallback(evt =>
        {
            activeItem.isStackable = evt.newValue;
        });

        var maxStackSize = itemDataSection.Q<IntegerField>("MaxStackSize");
        maxStackSize.value = activeItem.maxStackSize;
        maxStackSize.RegisterValueChangedCallback(evt =>
        {
            activeItem.maxStackSize = evt.newValue;
        });

        var isUsable = itemDataSection.Q<Toggle>("IsUsable");
        isUsable.value = activeItem.isUsable;
        isUsable.RegisterValueChangedCallback(evt =>
        {
            activeItem.isUsable = evt.newValue;
        });

        var isConsumable = itemDataSection.Q<Toggle>("IsConsumable");
        isConsumable.value = activeItem.isConsumable;
        isConsumable.RegisterValueChangedCallback(evt =>
        {
            activeItem.isConsumable = evt.newValue;
        });

        var useTime = itemDataSection.Q<FloatField>("UseTime");
        useTime.value = activeItem.useTime;
        useTime.RegisterValueChangedCallback(evt =>
        {
            activeItem.useTime = evt.newValue;
        });

        var canSell = itemDataSection.Q<Toggle>("CanSell");
        canSell.value = activeItem.canSell;
        canSell.RegisterValueChangedCallback(evt =>
        {
            activeItem.canSell = evt.newValue;
        });

        var buyPrice = itemDataSection.Q<IntegerField>("BuyPrice");
        buyPrice.value = activeItem.buyPrice;
        buyPrice.RegisterValueChangedCallback(evt =>
        {
            activeItem.buyPrice = evt.newValue;
        });

        var sellPrice = itemDataSection.Q<IntegerField>("SellPrice");
        sellPrice.value = activeItem.sellPrice;
        sellPrice.RegisterValueChangedCallback(evt =>
        {
            activeItem.sellPrice = evt.newValue;
        });

        var satietyRestore = itemDataSection.Q<IntegerField>("Satiety");
        satietyRestore.value = activeItem.satietyRestore;
        satietyRestore.RegisterValueChangedCallback(evt =>
        {
            activeItem.satietyRestore = evt.newValue;
        });

        var thirstyRestore = itemDataSection.Q<IntegerField>("Thirsty");
        thirstyRestore.value = activeItem.thirstyRestore;
        thirstyRestore.RegisterValueChangedCallback(evt =>
        {
            activeItem.thirstyRestore = evt.newValue;
        });

        var healthRestore = itemDataSection.Q<IntegerField>("Health");
        healthRestore.value = activeItem.healthRestore;
        healthRestore.RegisterValueChangedCallback(evt =>
        {
            activeItem.healthRestore = evt.newValue;
        });

        var manaRestore = itemDataSection.Q<IntegerField>("Mana");
        manaRestore.value = activeItem.manaRestore;
        manaRestore.RegisterValueChangedCallback(evt =>
        {
            activeItem.manaRestore = evt.newValue;
        });

        var effectDuration = itemDataSection.Q<FloatField>("EffectDuration");
        effectDuration.value = activeItem.effectDuration;
        effectDuration.RegisterValueChangedCallback(evt =>
        {
            activeItem.effectDuration = evt.newValue;
        });

        // TODO: ToolProperty

        var weaponType = itemDataSection.Q<EnumField>("WeaponType");
        weaponType.Init(activeItem.weaponType);
        weaponType.value = activeItem.weaponType;
        weaponType.RegisterValueChangedCallback(evt =>
        {
            activeItem.weaponType = (WeaponType)evt.newValue;
        });

        var damage = itemDataSection.Q<FloatField>("Damage");
        damage.value = activeItem.damage;
        damage.RegisterValueChangedCallback(evt =>
        {
            activeItem.damage = evt.newValue;
        });

        var damageMultiplier = itemDataSection.Q<FloatField>("DamageMultiplier");
        damageMultiplier.value = activeItem.damageMultiplier;
        damageMultiplier.RegisterValueChangedCallback(evt =>
        {
            activeItem.damageMultiplier = evt.newValue;
        });

        var attackRange = itemDataSection.Q<FloatField>("AttackRange");
        attackRange.value = activeItem.attackRange;
        attackRange.RegisterValueChangedCallback(evt =>
        {
            activeItem.attackRange = evt.newValue;
        });

        var defense = itemDataSection.Q<FloatField>("Defense");
        defense.value = activeItem.defense;
        defense.RegisterValueChangedCallback(evt =>
        {
            activeItem.defense = evt.newValue;
        });

        #endregion

    }
}

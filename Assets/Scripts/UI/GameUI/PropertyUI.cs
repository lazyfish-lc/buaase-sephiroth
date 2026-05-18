using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;
public class PropertyUI : FatherUI{
    public static PropertyUI Instance;
    public CanvasGroup PropertyCanvas;
    public Image DisplayImage; // 显示对象的图片的UI组件,初始为透明
    public TMP_Text DisplayName;// 显示对象名称的UI组件
    public TMP_Text DisplayDescription; // 显示对象描述的UI组件
    public TMP_Text[] DisplayProperties; // 显示对象属性的文本数组，假设最多显示8个属性
    public TMP_InputField[] inputFields; // 显示对象属性的输入框数组，假设最多显示8个属性
    public Button SaveButton; // 保存按钮的UI组件
    public static bool isOpen = false;
    private SmallObject currentSmallObject; // 当前显示标签的物体引用    
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Start()
    {
        //不可见且不可交互
        PropertyCanvas.alpha = 0;
        PropertyCanvas.interactable = false;
        PropertyCanvas.blocksRaycasts = false;
        DisplayImage.preserveAspect = true;
    }
    public void OpenAndClose()
    {
        
        if(!isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
        
    }
    public void Open()
    {
        PlayOpenSFX();
        cleanDisplay();
        isOpen = true;
        PropertyCanvas.alpha = 1;
        PropertyCanvas.interactable = true;
        PropertyCanvas.blocksRaycasts = true;
        ShowProperty();
        
    }
    public void Close()
    {
        PlayCloseSFX();
        isOpen = false;
        PropertyCanvas.alpha = 0;
        PropertyCanvas.interactable = false;
        PropertyCanvas.blocksRaycasts = false;
    }
    private void OnEnable() {
        // 1. 订阅场景中所有SmallObject的事件
        // 当任何SmallObject触发属性显示时，这个方法会被调用，且参数就是那个SmallObject
        var Smalls = FindObjectsByType<SmallObject>(FindObjectsSortMode.None);
        foreach (var Small in Smalls) {
            Small.OnShowProperty += HandleProperty;
        }
    }
    private void OnDisable() {
        var Smalls = FindObjectsByType<SmallObject>(FindObjectsSortMode.None);
        foreach (var Small in Smalls) {
            Small.OnShowProperty -= HandleProperty;
        }
    }
    private void HandleProperty(SmallObject Small) {
        // 2. 捕获SmallObject引用
        currentSmallObject = Small;
        Debug.Log("HandleProperty(" + Small.name + ")");
        Open();
    }
    public void ShowProperty()
    {
        if (currentSmallObject != null)
        {
            var info = DisplayTable.Instance.GetDisplayInfo(currentSmallObject.name, "SmallObject");
            if (info != null)
            {
                DisplayImage.color = new Color(1, 1, 1, 1); // 设置为不透明
                DisplayImage.preserveAspect = true; // 缩放以适配正方形显示
                var spriteRenderer = currentSmallObject.GetComponent<SpriteRenderer>();
                DisplayImage.sprite = spriteRenderer != null ? spriteRenderer.sprite : null; // 数据不存储，直接获取显示物体的图片
                DisplayName.text = info.displayname;
                DisplayDescription.text = info.description;
                // 这里可以根据实际需求将 SmallObject 的属性显示在输入框中
                // 先获取 SmallObject 的属性数量（默认不超过8个）
                // 拿取所有
                // 前面为名字称，后面为属性值
                Dictionary<string, SmallObjectProperty> propertiesDict = currentSmallObject.dynamicState.propertyMap;
                var properties = propertiesDict.Values.ToArray();
                // 然后将属性值显示在输入框中，多出的输入框整个不显示
                for (int i = 0; i < inputFields.Length; i++)
                {
                    if (i < properties.Length)
                    {
                        // 拆分显示：属性名称和属性值分开显示，属性名称显示在 DisplayProperties 中，属性值显示在 inputFields 中
                        var value = properties[i].value;
                        DisplayProperties[i].text = properties[i].name; // 显示属性名称
                        DisplayProperties[i].gameObject.SetActive(true); // 显示属性名称
                        inputFields[i].text = value.ToString(); // 显示属性值
                        inputFields[i].gameObject.SetActive(true); // 显示输入框
                        Debug.Log($"显示属性 {properties[i].name} 的值 {value} 在输入框中");
                        
                    }
                    else
                    {
                        inputFields[i].text = "";
                        inputFields[i].gameObject.SetActive(false); // 隐藏输入框
                        DisplayProperties[i].text = "";
                        DisplayProperties[i].gameObject.SetActive(false); // 隐藏属性名称
                    }
                }
            }
            else
            {
                Debug.LogWarning($"未找到物体 {currentSmallObject.name} 的显示信息");
                cleanDisplay();
            }
        }
        else
        {
            Debug.LogWarning("当前没有 SmallObject 可显示");
            cleanDisplay();
        }
    }
    void cleanDisplay()
    {
        DisplayImage.color = new Color(1, 1, 1, 0); // 初始为透明
        DisplayName.text = "";
        DisplayDescription.text = "";
        // 隐藏所有属性名称和输入框
        for (int i = 0; i < DisplayProperties.Length; i++)
        {
            DisplayProperties[i].text = "";
            DisplayProperties[i].gameObject.SetActive(false);
            // 在子对象text area的placeholder中显示提示文本
            inputFields[i].text = "";
            inputFields[i].gameObject.SetActive(false);
        }
    }
    public void SaveProperties()
    {
        PlayClickSFX();
        if (currentSmallObject != null)
        {
            var properties = currentSmallObject.dynamicState.propertyMap.Values.ToArray();
            NumericalModificationRequest request = new NumericalModificationRequest();
            for (int i = 0; i < properties.Length && i < inputFields.Length; i++)
            {
                string inputValue = inputFields[i].text; // 获取输入框中的文本
                try
                {
                    // 尝试将输入值转换为属性的类型
                    float convertedValue = float.Parse(inputValue); // 目前仅支持 float 类型，后续可以扩展支持其他类型
                    request.AddModification(properties[i], convertedValue);
                    Debug.Log($"已将输入值 '{inputValue}' 转换为float并保存到属性 {properties[i].name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"无法将输入值 '{inputValue}' 转换为 float 类型，保存属性 {properties[i].name} 失败。错误信息: {e.Message}");
                }
            }
            if (request.Count > 0) {
                NumericalRuleManager.TryApplyModificationRequest(request);
            }
            // ...
            Debug.Log("属性已保存");
            HintUI.Instance?.ShowHint("属性修改已保存");
            ShowProperty();
        }
        else
        {
            Debug.LogWarning("没有 SmallObject 可保存属性");
        }
    }
}

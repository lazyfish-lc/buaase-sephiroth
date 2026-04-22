using UnityEngine;

public class IOSubsystem : MonoBehaviour {
    public static IOSubsystem Instance;
    
    public LayerMask interactableLayer;
    public SmallObject playerObject;

    void Awake() { Instance = this; }

    void Update() {
        dealMovement();
        dealInteract();
        dealSwitchTime();
        dealScroll();
        dealSettings();
        dealMenu();
        dealBackpack();
        dealAction();
    }

    void dealMovement() {
        float h = Input.GetAxis(InputConfig.Horizontal);
        float v = Input.GetAxis(InputConfig.Vertical);
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f) {
            InputEventData data = new InputEventData {
                actionType = InputActionType.Movement,
                moveVector = new Vector3(h, 0, v)
            };
            if (playerObject != null) {
                playerObject.HandleInput(data);
            } else {
                Debug.LogWarning("玩家对象未设置，无法处理移动输入");
            }
        }
    }

    void dealInteract() {
        if (Input.GetButtonDown(InputConfig.Interact)) {
            SmallObject target = GetObjectUnderMouse();
            if (target != null) {
                InputEventData data = new InputEventData {
                    actionType = InputActionType.Interact,
                    target = target
                };
                target.HandleInput(data);
            } else {
                Debug.Log("没有可交互对象在鼠标下");
            }
        }
    }

    void dealSwitchTime() {
        if (Input.GetButtonDown(InputConfig.SwitchTime)) {
            GameSceneManager.Instance.ToggleTimeVision();
        }
    }

    void dealScroll() {
        float scrollDelta = Input.GetAxis(InputConfig.Scroll);
        if (Mathf.Abs(scrollDelta) > 0.01f) {
            Debug.Log("滚轮输入，增量: " + scrollDelta);
            // TODO: 处理滚轮输入，触发数值修改事件
        }
    }

    void dealSettings() {
        if (Input.GetButtonDown(InputConfig.Settings)) {
            if (SettingUI.Instance != null) {
                Debug.Log("设置被调用");
                SettingUI.Instance.OpenAndClose();
            }
        }
    }

    void dealMenu() {
        if (Input.GetButtonDown(InputConfig.Menu)) {
            if (MenuUI.Instance != null) {
                Debug.Log("菜单被调用");
                MenuUI.Instance.OpenAndClose();
            }
        }
    }
    void dealBackpack() {
        if (Input.GetButtonDown(InputConfig.Backpack)) {
            if (BackpackUI.Instance != null) {
                Debug.Log("背包被调用");
                BackpackUI.Instance.OpenAndClose();
            }
        }
    }
    void dealAction() {
        if (Input.GetButtonDown(InputConfig.Action)) {
            if (ActionUI.Instance != null) {
                Debug.Log("动作面板被调用");
                ActionUI.Instance.OpenAndClose();
            }
        }
    }

    private SmallObject GetObjectUnderMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer)) {
            return hit.collider.GetComponent<SmallObject>();
        }
        return null;
    }
}
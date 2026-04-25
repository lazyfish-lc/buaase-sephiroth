using UnityEngine;
public class IOSubsystem : MonoBehaviour {
    public static IOSubsystem Instance;
    public LayerMask interactableLayer;
    public PlayerSmallObject playerObject;
    void Awake() { Instance = this; }
    void Update() {
        //鼠标下是否是UI元素，如果鼠标下是非游戏物体不触发
        if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            dealAttack();
        }
        // 只在游戏中处理设置、菜单、背包和动作面板的输入，使用自带的场景管理器来判断当前场景，避免与自定义的GameSceneManager耦合过紧
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene") {
            dealSettings();
            dealMenu();
            dealBackpack();
            dealAction();
            dealMovement();
            dealInteract();
            dealSwitchTime();
            dealScroll();
        }
    }

    void dealAttack() {
        if (Input.GetButtonDown(InputConfig.Attack)) {
            playerObject.Attack();
        }
    }

    void dealMovement() {
        if (playerObject.isHurt) return; // 受击状态下无法移动
        float x = Input.GetAxis(InputConfig.Horizontal);
        float y = Input.GetAxis(InputConfig.Vertical);
            InputEventData data = new InputEventData {
                actionType = InputActionType.Movement,
                moveVector = new Vector3(x, y, 0)
            };
            if (playerObject != null) {
                playerObject.HandleInput(data);
            } else {
                Debug.LogWarning("玩家对象未设置，无法处理移动输入");
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
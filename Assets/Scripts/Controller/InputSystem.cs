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
        if (playerObject.playerState.isHurt) return; // 受击状态下无法移动
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
            // 如果鼠标下对应的对象处于NPC交互范围内，则触发交互事件
            // 判断是否是处于NPC层
            SmallObject target = GetObjectUnderMouse();
            if (target == null) {
                Debug.Log("没有检测到鼠标下的交互对象");
                return;
            }
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
        Camera cam = Camera.main;
        if (cam == null) {
            Debug.LogWarning("未找到 MainCamera，无法进行鼠标拾取");
            return null;
        }

        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 point2D = new Vector2(worldPos.x, worldPos.y);

        // 2D场景中，鼠标拾取优先使用点重叠检测，命中更稳定。
        Collider2D[] overlapHits = Physics2D.OverlapPointAll(point2D);
        if (overlapHits != null && overlapHits.Length > 0) {
            for (int i = 0; i < overlapHits.Length; i++) {
                Collider2D col = overlapHits[i];
                if (col == null) continue;

                // 使用 LayerMask 位运算，天然兼容多层（例如 Enemy | NPC）。
                if (!IsLayerInMask(col.gameObject.layer, interactableLayer)) {
                    continue;
                }

                // 仅接受主物体自身碰撞体：必须是同一 GameObject 上的 SmallObject。
                SmallObject target = col.GetComponent<SmallObject>();
                if (target != null) {
                    return target;
                }
            }
        }

        // 兜底：再尝试2D射线交点，兼容特殊碰撞体设置。
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
        if (rayHits != null && rayHits.Length > 0) {
            for (int i = 0; i < rayHits.Length; i++) {
                Collider2D col = rayHits[i].collider;
                if (col == null) continue;

                if (!IsLayerInMask(col.gameObject.layer, interactableLayer)) {
                    continue;
                }

                SmallObject target = col.GetComponent<SmallObject>();
                if (target != null) {
                    return target;
                }
            }
        }

        Debug.Log($"鼠标下未命中可交互对象。mask={interactableLayer.value}, point={point2D}");
        return null;
    }

    private bool IsLayerInMask(int layer, LayerMask mask) {
        return (mask.value & (1 << layer)) != 0;
    }
}
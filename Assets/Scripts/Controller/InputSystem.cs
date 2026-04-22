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

    private SmallObject GetObjectUnderMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer)) {
            return hit.collider.GetComponent<SmallObject>();
        }
        return null;
    }
}
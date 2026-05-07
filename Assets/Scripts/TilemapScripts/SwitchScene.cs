using UnityEngine;

public class SwitchScene : MonoBehaviour
{

    public string sceneName; // 要切换到的场景名称
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果玩家则切换场景
        Debug.Log("触发场景切换");
        if (collision.gameObject.CompareTag("Player"))
        {
            // 切换到下一个场景
            Debug.Log("切换到下一个场景");
            // 例如，用 "LastExitName" 作为键，存储值 "Exit_House1"
            PlayerPrefs.SetString("LastExitName", sceneName);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}

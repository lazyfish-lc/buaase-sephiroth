using UnityEngine;


public class SwitchScene : MonoBehaviour
{

    public string sceneName; // 要切换到的场景名称
    private void OnTriggerEnter2D(Collider2D player)
    {
        // 如果玩家则切换场景
        Debug.Log("触发场景切换");
        if (player.gameObject.CompareTag("Player"))
        {
            // 切换到下一个场景
            Debug.Log("切换到下一个场景");

            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);        }
    }
}

using UnityEngine;

public class SwitchScene : MonoBehaviour
{

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果玩家则切换场景
        Debug.Log("触发场景切换");
        if (collision.gameObject.CompareTag("Player"))
        {
            // 切换到下一个场景
            Debug.Log("切换到下一个场景");

            UnityEngine.SceneManagement.SceneManager.LoadScene("Map1");
        }
    }
}

using UnityEngine;

public class NPCInteractTrigger : MonoBehaviour
{
    private bool playerInRange = false;
    private NPCPatrol patrolScript;

    void Start()
    {
        patrolScript = GetComponentInParent<NPCPatrol>(); // 找老爸要巡逻脚本
    }

    void Update()
    {
        // 玩家在感应区内，并且按下了 E 键
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (patrolScript != null)
            {
                patrolScript.EducateNPC();
            }
        }
    }

    // 玩家走进圈子
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // 可选：在这里调用 UI 显示“按 E 键进行环保宣传”
        }
    }

    // 玩家离开圈子
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // 可选：隐藏 UI
        }
    }
}
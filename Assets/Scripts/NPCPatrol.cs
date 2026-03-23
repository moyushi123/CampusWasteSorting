using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    [Header("巡逻设置")]
    [Tooltip("把场景里建好的几个空物体拖进这个数组，作为巡逻目标点")]
    public Transform[] waypoints;
    private int currentWaypointIndex;
    private NavMeshAgent agent;

    [Header("扔垃圾行为")]
    [Tooltip("每隔几秒钟在地上悄悄扔一个垃圾")]
    public float throwInterval = 8f;
    private float throwTimer;

    void Start()
    {
        // 获取挂在自己身上的导航组件
        agent = GetComponent<NavMeshAgent>();

        // 游戏一开始，如果设置了路点，就向第一个点走去
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        // ------------------ 行为 1：自动巡逻 ------------------
        // 判断是否已经快走到当前目标点了（距离小于0.5米），并且系统正在计算路径
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // 切换到下一个目标点（用取余数的方法实现循环：0->1->2->0）
            if (waypoints.Length > 0)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }

        // ------------------ 行为 2：定时从池子里捞垃圾扔掉 ------------------
        throwTimer += Time.deltaTime;
        if (throwTimer >= throwInterval)
        {
            ThrowTrash();
            throwTimer = 0f; // 计时器清零，重新开始算时间
        }
    }

    private void ThrowTrash()
    {
        // 核心：直接调用我们之前写好的全局对象池！
        if (TrashPoolManager.Instance != null)
        {
            GameObject trash = TrashPoolManager.Instance.GetTrash();
            if (trash != null)
            {
                // 把从池子里捞出来的垃圾，放在 NPC 当前的脚下
                trash.transform.position = transform.position;

                // 稍微给个随机旋转，让掉在地上的垃圾看起来自然一点，不是整整齐齐的
                trash.transform.rotation = Random.rotation;
            }
        }
    }
}
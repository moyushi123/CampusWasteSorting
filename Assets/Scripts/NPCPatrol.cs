using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] waypoints;
    private int currentWaypointIndex;
    private NavMeshAgent agent;
    private Animator animator; // ⬅️ 新增：用来控制走路动画

    [Header("等待与扔垃圾")]
    public float waitTimeAtWaypoint = 3f; // 到了终点等几秒
    private float waitTimer;
    private bool isWaiting = false;

    [Header("扔垃圾行为")]
    public float throwInterval = 8f;
    private float throwTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // 获取她身上的动画组件

        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        // --- 核心 1：实时更新动画 ---
        if (animator != null && agent != null)
        {
            // 获取她当前移动的速度，传给动画机
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }

        // --- 核心 2：定时扔垃圾 ---
        throwTimer += Time.deltaTime;
        if (throwTimer >= throwInterval)
        {
            ThrowTrash();
            throwTimer = 0f;
        }

        // --- 核心 3：高级巡逻（带停顿状态机） ---
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                // 刚到达目标点，开始停下发呆
                isWaiting = true;
                waitTimer = 0f;
                agent.isStopped = true; // 刹车
            }
            else
            {
                // 正在发呆等待中...
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTimeAtWaypoint)
                {
                    // 等待时间结束，继续走向下一个点
                    isWaiting = false;
                    agent.isStopped = false; // 松开刹车

                    if (waypoints.Length > 0)
                    {
                        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                        agent.SetDestination(waypoints[currentWaypointIndex].position);
                    }
                }
            }
        }
    }

    private void ThrowTrash()
    {
        if (TrashPoolManager.Instance != null)
        {
            GameObject trash = TrashPoolManager.Instance.GetTrash();
            if (trash != null)
            {
                // 从脚底稍微往上抬高 0.5 米扔出来，防止垃圾直接掉进地下室
                trash.transform.position = transform.position + Vector3.up * 0.5f;
                trash.transform.rotation = Random.rotation;
            }
        }
    }


    // ⬅️ 新增：专门用来“接住”官方走路动画发出的脚步事件，防止报错卡死动画
    public void OnFootstep(AnimationEvent animationEvent)
    {
        // 里面什么都不用写。系统找到这个方法就不会报错了。
        // （如果你以后想给 NPC 加高跟鞋走路的声音，就可以把播放音效的代码写在这里）
    }

    // ⬅️ 新增：官方动画可能还会带落地事件，顺便一起接住，以防万一
    public void OnLand(AnimationEvent animationEvent)
    {
    }

}
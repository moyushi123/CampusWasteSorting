using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] waypoints;
    private int currentWaypointIndex;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("等待与扔垃圾")]
    public float waitTimeAtWaypoint = 3f;
    private float waitTimer;
    private bool isWaiting = false;

    [Header("扔垃圾行为")]
    public float throwInterval = 8f;
    private float throwTimer;

    // ⬇️ 新增：环保宣传防刷分机制
    [Header("环保宣传状态")]
    public bool isGoodCitizen = false; // 是否处于不扔垃圾的“好市民”状态
    public float goodCitizenDuration = 60f; // 宣传一次管用 60 秒
    private float goodCitizenTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        if (animator != null && agent != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }

        // ⬇️ 核心改造：状态机分流
        if (isGoodCitizen)
        {
            // 如果是好市民，不增加扔垃圾的时间，而是倒计时“好市民 Buff”
            goodCitizenTimer -= Time.deltaTime;
            if (goodCitizenTimer <= 0)
            {
                isGoodCitizen = false; // 60秒到了，又开始乱扔垃圾了
                if (SystemLogger.Instance != null)
                    SystemLogger.Instance.AddLog($"监控警告：某 NPC 的环保 Buff 结束，重回作案状态...");
            }
        }
        else
        {
            // 只有不是好市民的时候，才定时扔垃圾
            throwTimer += Time.deltaTime;
            if (throwTimer >= throwInterval)
            {
                ThrowTrash();
                throwTimer = 0f;
            }
        }

        // --- 核心 3：高级巡逻（保持不变） ---
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
                agent.isStopped = true;
            }
            else
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTimeAtWaypoint)
                {
                    isWaiting = false;
                    agent.isStopped = false;

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
                trash.transform.position = transform.position + Vector3.up * 0.5f;
                trash.transform.rotation = Random.rotation;

                TrashPickup pickupScript = trash.GetComponent<TrashPickup>();
                string realTrashName = (pickupScript != null) ? pickupScript.trashName : trash.name.Replace("(Clone)", "");

                if (SystemLogger.Instance != null)
                {
                    SystemLogger.Instance.AddLog($"监控警告：发现 NPC 丢弃了 [{realTrashName}]");
                }
            }
        }
    }

    // ⬇️ 新增：暴露给玩家的互动接口
    public void EducateNPC()
    {
        if (isGoodCitizen)
        {
            // 防刷分：已经被教育过了，再按 E 也没用
            if (SystemLogger.Instance != null)
                SystemLogger.Instance.AddLog($"[系统] 该同学目前表现良好，无需重复宣传！");
            return;
        }

        // 核心逻辑：设为好市民，给玩家加分！
        isGoodCitizen = true;
        goodCitizenTimer = goodCitizenDuration;

        if (SystemLogger.Instance != null)
            SystemLogger.Instance.AddLog($"[玩家] 成功进行环保宣传！NPC 停止丢弃垃圾 60 秒。得分 +5");

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(5);

        // 如果你需要更新总分，也可以加一句 DataManager 的代码
    }

    public void OnFootstep(AnimationEvent animationEvent) { }
    public void OnLand(AnimationEvent animationEvent) { }
}
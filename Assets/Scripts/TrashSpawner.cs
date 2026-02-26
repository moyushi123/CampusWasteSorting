using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject trashPrefab; // 垃圾预制体（图纸）
    public int spawnCount = 20;    // 想要生成的垃圾总数

    [Header("锚点集合")]
    public Transform[] spawnPoints; // 你刚才摆放的所有候选点

    void Start()
    {
        SpawnTrash();
    }

    void SpawnTrash()
    {
        // 1. 安全检查：如果没有点位或者没有预制体，就直接退出，防止报错
        if (spawnPoints.Length == 0 || trashPrefab == null) return;

        // 2. 创建一个“抽奖箱”，把所有的候选点都放进去
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        // 3. 确定实际生成的数量（防呆设计：如果你只要20个，但点只有10个，那就只刷10个）
        int actualSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        // 4. 开始“抽卡”！
        for (int i = 0; i < actualSpawnCount; i++)
        {
            // 在剩下的点位里，随机抽一个“号码牌”
            int randomIndex = Random.Range(0, availablePoints.Count);

            // 获取这个号码牌对应的实际坐标点
            Transform selectedPoint = availablePoints[randomIndex];

            // 凭空在这个坐标点生成垃圾
            Instantiate(trashPrefab, selectedPoint.position, selectedPoint.rotation);

            // ⚠️ 最关键的一步：把抽过的点位从箱子里拿走，绝不重复生成！
            availablePoints.RemoveAt(randomIndex);
        }
    }
}
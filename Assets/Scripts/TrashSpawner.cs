using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    [Header("生成设置")]
    [Tooltip("开局要从池子里捞多少个垃圾出来摆在地上")]
    public int spawnCount = 10;

    [Header("锚点集合")]
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnAllTrash();
    }

    private void SpawnAllTrash()
    {
        // 1. 新的安全校验：现在只检查锚点够不够，不再检查旧的预制体数组了
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("没有配置生成锚点！");
            return;
        }

        // 2. 复制一份锚点列表用来抽取（防重叠）
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        // 3. 确保要生成的数量不会超过锚点的总数量
        int actualSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            // 随机抽取一个空闲的生成点
            int randomPointIndex = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[randomPointIndex];

            // ⬅️ 全新核心逻辑：直接向对象池（仓库）申请闲置的垃圾
            GameObject trashFromPool = TrashPoolManager.Instance.GetTrash();

            if (trashFromPool != null)
            {
                // 把捞出来的垃圾瞬移到指定的锚点位置
                trashFromPool.transform.position = point.position;
                trashFromPool.transform.rotation = point.rotation;
            }

            // 把用过的点从备选列表中剔除
            availablePoints.RemoveAt(randomPointIndex);
        }
    }
}
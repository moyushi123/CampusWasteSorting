using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    [Header("生成设置")]
    [Tooltip("把所有做好的不同垃圾预制体，全部拖进这个数组里")]
    public GameObject[] trashPrefabs; // ⬅️ 核心改动：从单个 GameObject 变成了 GameObject[] 数组
    public int spawnCount = 10;

    [Header("锚点集合")]
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnAllTrash();
    }

    private void SpawnAllTrash()
    {
        // 安全校验
        if (trashPrefabs == null || trashPrefabs.Length == 0)
        {
            Debug.LogWarning("没有配置垃圾预制体！");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // 为了防止两个垃圾刷在同一个点上重叠，我们复制一份锚点列表用来抽取
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        // 确保要生成的数量不会超过锚点的总数量
        int actualSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            // 1. 随机抽取一个空闲的生成点
            int randomPointIndex = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[randomPointIndex];

            // 2. 随机抽取一款垃圾预制体
            int randomTrashIndex = Random.Range(0, trashPrefabs.Length);
            GameObject trashToSpawn = trashPrefabs[randomTrashIndex];

            // 3. 在该点生成选中的垃圾
            Instantiate(trashToSpawn, point.position, point.rotation);

            // 4. 把用过的点从备选列表中剔除
            availablePoints.RemoveAt(randomPointIndex);
        }
    }
}
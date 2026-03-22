using UnityEngine;
using System.Collections.Generic;

public class TrashPoolManager : MonoBehaviour
{
    // 单例模式：方便全局随时随地呼叫池子
    public static TrashPoolManager Instance;

    [Header("对象池配置")]
    [Tooltip("把做好的各种真实垃圾预制体拖进来")]
    public GameObject[] trashPrefabs;
    [Tooltip("每种垃圾提前预生成的数量，比如填10，如果有5种垃圾，开局就会在后台生成50个")]
    public int poolSizePerPrefab = 10;

    // 这个 List 就是我们的“内存仓库”
    private List<GameObject> pool = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool(); // 游戏一运行，最优先建立仓库
    }

    private void InitializePool()
    {
        if (trashPrefabs == null || trashPrefabs.Length == 0) return;

        // 遍历每一种垃圾预制体
        foreach (GameObject prefab in trashPrefabs)
        {
            // 每种垃圾各自生成指定的数量
            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                // 生成出来，放在自己名下保持 Hierarchy 整洁
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false); // 核心：刚生成出来时必须是隐藏状态（假死）
                pool.Add(obj);        // 登记入库
            }
        }
    }

    // 提供给外部调用的核心方法：给我捞一个能用的垃圾出来！
    public GameObject GetTrash()
    {
        List<GameObject> inactiveTrash = new List<GameObject>();

        // 遍历仓库，把所有当前闲置（隐藏）的垃圾找出来
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                inactiveTrash.Add(obj);
            }
        }

        // 随机挑一个闲置的垃圾发货
        if (inactiveTrash.Count > 0)
        {
            int randomIndex = Random.Range(0, inactiveTrash.Count);
            GameObject selectedTrash = inactiveTrash[randomIndex];
            selectedTrash.SetActive(true); // 激活它！
            return selectedTrash;
        }

        Debug.LogWarning("严重警告：对象池干枯了！请在 Inspector 调大 Pool Size Per Prefab！");
        return null;
    }
}
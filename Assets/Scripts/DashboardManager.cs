using UnityEngine;
using UnityEngine.UI;

public class DashboardManager : MonoBehaviour
{
    // 单例模式，方便全局调用
    public static DashboardManager Instance;

    [Header("UI 进度条引用")]
    [Tooltip("把对应的 Image(Filled 模式) 拖进来")]
    public Image barRecyclable;
    public Image barHazardous;
    public Image barFood;
    public Image barResidual;

    // 内部数据：记录各大类的正确和错误次数
    private int correctRecyclable, errorRecyclable;
    private int correctHazardous, errorHazardous;
    private int correctFood, errorFood;
    private int correctResidual, errorResidual;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 游戏开始时刷新一次，防止初始显示错乱
        UpdateAllBars();
    }

    // 外部调用的接口：每次答完题，分类系统会把结果汇报给这里
    public void RecordResult(TrashCategory category, bool isCorrect)
    {
        switch (category)
        {
            case TrashCategory.可回收物:
                if (isCorrect) correctRecyclable++; else errorRecyclable++;
                break;
            case TrashCategory.有害垃圾:
                if (isCorrect) correctHazardous++; else errorHazardous++;
                break;
            case TrashCategory.厨余垃圾:
                if (isCorrect) correctFood++; else errorFood++;
                break;
            case TrashCategory.其他垃圾:
                if (isCorrect) correctResidual++; else errorResidual++;
                break;
        }

        // 记录完数据后，立刻刷新对应的 UI 进度条
        UpdateAllBars();
    }

    // 核心算法：计算正确率并更新填充满度 (Fill Amount)
    private void UpdateAllBars()
    {
        UpdateSingleBar(barRecyclable, correctRecyclable, errorRecyclable);
        UpdateSingleBar(barHazardous, correctHazardous, errorHazardous);
        UpdateSingleBar(barFood, correctFood, errorFood);
        UpdateSingleBar(barResidual, correctResidual, errorResidual);
    }

    private void UpdateSingleBar(Image bar, int correctCount, int errorCount)
    {
        if (bar == null) return;

        int totalAttempts = correctCount + errorCount;

        // 如果这个垃圾还从来没被选过，就保持0进度
        if (totalAttempts == 0)
        {
            bar.fillAmount = 0f;
            return;
        }

        // 计算公式：正确率 = 正确次数 / 总尝试次数
        float accuracyRate = (float)correctCount / totalAttempts;
        bar.fillAmount = accuracyRate;
    }
}
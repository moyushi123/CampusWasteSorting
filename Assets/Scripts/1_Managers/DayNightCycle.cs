using UnityEngine;
using UnityEngine.InputSystem;

public class DayNightCycle : MonoBehaviour
{
    [Header("时间流逝设置")]
    [Tooltip("正常的时间流逝速度（数值越大，天黑得越快）")]
    public float normalSpeed = 2f;

    [Tooltip("按住【=】或小键盘【+】键时的加速倍数，用于快速看日落")]
    public float fastForwardMultiplier = 20f;

    void Update()
    {
        float currentSpeed = normalSpeed;

        // 检测是否按住了键盘上的 =/+ 键（主键盘的等号键，或者数字小键盘的加号键）
        if (Keyboard.current.equalsKey.isPressed || Keyboard.current.numpadPlusKey.isPressed)
        {
            currentSpeed = normalSpeed * fastForwardMultiplier;
        }

        // 核心魔法：让挂载这个脚本的物体（太阳）绕着 X 轴缓慢旋转
        transform.Rotate(Vector3.right, currentSpeed * Time.deltaTime);
    }
}
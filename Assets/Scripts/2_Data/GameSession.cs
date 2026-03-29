// 这是一个纯静态的数据枢纽，不需要挂载到任何 Unity 物体上！
public static class GameSession
{
    // 跨场景通行证：记录当前登录的账号名
    public static string CurrentUsername = "";

    // 判断当前是否有人登录
    public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUsername);

    // 登出方法
    public static void Logout()
    {
        CurrentUsername = "";
    }
}
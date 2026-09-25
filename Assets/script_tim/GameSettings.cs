public static class GameSettings
{
    public static int difficulty = 1;
    public static bool soundOn = true;

    public static string DifficultyName()
    {
        if (difficulty == 0) return "EASY";
        if (difficulty == 2) return "HARD";
        return "MEDIUM";
    }

    public static float TotalTime()
    {
        if (difficulty == 0) return 480f;
        if (difficulty == 2) return 300f;
        return 390f;
    }
}

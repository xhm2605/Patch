using UnityEngine;

public class Settings : MonoBehaviour
{
    private int lives;
    private int score;

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        lives = 3;
        score = 0;
    }
    public void LevelComplete()
    {
        score += 1000;
    }

    public void LevelFailed()
    {
        lives--;
        if (lives <= 0)
        {
            NewGame();
        }
    }
}

using UnityEngine;

public class DonkeyKongUI : MonoBehaviour
{
    public GameObject startPanel;

    public static bool gameStarted = false;

    private void Start()
    {
        gameStarted = false;
        startPanel.SetActive(true);
    }

    private void Update()
    {
        if (!gameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            gameStarted = true;
            startPanel.SetActive(false);
        }
    }
}
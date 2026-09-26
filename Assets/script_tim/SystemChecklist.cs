using UnityEngine;
using TMPro;

public class SystemChecklist : MonoBehaviour
{
    public TMP_Text display;

    private string[] ids = { "Engine", "Shield", "Oxygen", "Comms" };
    private string[] labels = { "ENGINE", "SHIELD", "OXYGEN", "COMMS " };

    void Start()
    {
        if (display == null) display = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (display == null) return;

        if (GameManager.Instance == null)
        {
            display.text = "";
            return;
        }

        string txt = "<mspace=0.6em>SYSTEMS\n";

        for (int i = 0; i < ids.Length; i++)
        {
            bool done = GameManager.Instance.IsRepaired(ids[i]);

            if (done)
                txt += "<color=#57E389>[X] " + labels[i] + " " + GameManager.Instance.GetFragmentFor(ids[i]) + "</color>\n";
            else
                txt += "<color=#8A93A5>[ ] " + labels[i] + " ??</color>\n";
        }

        display.text = txt + "</mspace>";
    }
}

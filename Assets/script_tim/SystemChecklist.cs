using UnityEngine;
using TMPro;

public class SystemChecklist : MonoBehaviour
{
    public TMP_Text display;

    private string[] ids = { "Engine", "Shield", "Oxygen", "Comms", "Coolant" };
    private string[] labels = { "ENGINE ", "SHIELD ", "OXYGEN ", "COMMS  ", "COOLANT" };

    void Start()
    {
        if (display == null) display = GetComponent<TMP_Text>();
        if (display == null) return;

        display.fontSize = 25f;

        RectTransform rt = display.rectTransform;
        rt.sizeDelta = new Vector2(400f, 250f);
    }

    void Update()
    {
        if (display == null) return;

        if (GameManager.Instance == null)
        {
            display.text = "";
            return;
        }

        int done = GameManager.Instance.repairedTerminals.Count;
        int total = GameManager.Instance.TerminalCount();

        string txt = "<mspace=0.6em>SYSTEMS " + done + "/" + total + "\n";

        for (int i = 0; i < ids.Length; i++)
        {
            if (GameManager.Instance.IsRepaired(ids[i]))
                txt += "<color=#57E389>[X] " + labels[i] + " " + GameManager.Instance.GetFragmentFor(ids[i]) + "</color>\n";
            else
                txt += "<color=#8A93A5>[ ] " + labels[i] + " ??</color>\n";
        }

        display.text = txt + "</mspace>";
    }
}

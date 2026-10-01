using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodePuzzle : MonoBehaviour
{
    [Header("References UI")]
    public TMP_Text slotText;
    public TMP_Text feedbackText;
    public TMP_Text attemptsText;
    public Button[] fragButtons;
    public Button submitButton;
    public Button resetButton;

    [Header("Reglages")]
    public int maxAttempts = 4;

    private const int Size = 8;

    private string[] letters = new string[Size];
    private bool[] used = new bool[Size];
    private int[] slots = new int[Size];
    private bool[] locked = new bool[Size];

    private int attemptsLeft;
    private bool finished = false;
    private bool built = false;

    private Image[] slotBg = new Image[Size];
    private TMP_Text[] slotLabel = new TMP_Text[Size];
    private Button[] letterButtons = new Button[Size];
    private Image[] letterBg = new Image[Size];
    private TMP_Text[] letterLabel = new TMP_Text[Size];

    private static Sprite roundSprite;

    private static readonly Color SlotEmpty = new Color(0.09f, 0.15f, 0.24f, 0.95f);
    private static readonly Color SlotFilled = new Color(0.16f, 0.30f, 0.46f, 1f);
    private static readonly Color SlotLocked = new Color(0.12f, 0.42f, 0.28f, 1f);
    private static readonly Color LetterIdle = new Color(0.20f, 0.42f, 0.68f, 1f);
    private static readonly Color LetterUsed = new Color(0.12f, 0.17f, 0.24f, 0.7f);

    void OnEnable()
    {
        BuildUI();
        SetupPuzzle();
    }

    // ---------- Construction ----------

    void BuildUI()
    {
        if (built) return;
        built = true;

        if (roundSprite == null)
            roundSprite = MenuStyler.RoundedRect(64, 64, 12f, 2f,
                Color.white, new Color(0.40f, 0.78f, 1f, 0.85f));

        if (slotText != null) slotText.gameObject.SetActive(false);

        if (fragButtons != null)
            foreach (Button b in fragButtons)
                if (b != null) b.gameObject.SetActive(false);

        Caption("SlotCaption", "MASTER CODE", 20f, 214f, new Color(0.55f, 0.72f, 0.88f, 0.9f));
        Caption("PoolCaption", "RECOVERED LETTERS", 20f, 58f, new Color(0.55f, 0.72f, 0.88f, 0.9f));

        for (int i = 0; i < Size; i++)
        {
            int index = i;

            GameObject slot = NewUI("Slot" + i, transform);
            Place(slot.GetComponent<RectTransform>(), -371f + i * 106f, 148f, 92f, 104f);

            slotBg[i] = slot.AddComponent<Image>();
            slotBg[i].sprite = roundSprite;
            slotBg[i].type = Image.Type.Sliced;
            slotBg[i].color = SlotEmpty;

            Button sb = slot.AddComponent<Button>();
            sb.targetGraphic = slotBg[i];
            sb.onClick.AddListener(() => TakeBack(index));

            slotLabel[i] = AddLabel(slot.transform, 52f, new Color(1f, 0.87f, 0.42f));

            GameObject key = NewUI("Letter" + i, transform);
            Place(key.GetComponent<RectTransform>(), -329f + i * 94f, -8f, 80f, 80f);

            letterBg[i] = key.AddComponent<Image>();
            letterBg[i].sprite = roundSprite;
            letterBg[i].type = Image.Type.Sliced;
            letterBg[i].color = LetterIdle;

            letterButtons[i] = key.AddComponent<Button>();
            letterButtons[i].targetGraphic = letterBg[i];
            letterButtons[i].onClick.AddListener(() => PlaceLetter(index));

            letterLabel[i] = AddLabel(key.transform, 40f, Color.white);
        }
    }

    void Caption(string objectName, string content, float size, float y, Color c)
    {
        if (transform.Find(objectName) != null) return;

        GameObject go = NewUI(objectName, transform);
        Place(go.GetComponent<RectTransform>(), 0f, y, 700f, 32f);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.characterSpacing = 9f;
        t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
    }

    TMP_Text AddLabel(Transform parent, float size, Color c)
    {
        GameObject go = NewUI("Label", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);
        return go;
    }

    void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        rt.localScale = Vector3.one;
    }

    // ---------- Partie ----------

    void SetupPuzzle()
    {
        finished = false;
        attemptsLeft = maxAttempts;

        List<string> pool = GameManager.Instance.CollectedLetters();
        while (pool.Count < Size) pool.Add("?");
        Shuffle(pool);

        for (int i = 0; i < Size; i++)
        {
            letters[i] = pool[i];
            used[i] = false;
            slots[i] = -1;
            locked[i] = false;
        }

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(SubmitAttempt);

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetSlots);

        if (GameManager.Instance.hintUnlocked) ApplyHint();

        feedbackText.text = "";
        RefreshUI();
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            string tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    void PlaceLetter(int letterIndex)
    {
        if (finished || used[letterIndex]) return;

        for (int i = 0; i < Size; i++)
        {
            if (!locked[i] && slots[i] < 0)
            {
                slots[i] = letterIndex;
                used[letterIndex] = true;
                SoundManager.PlayClick();
                RefreshUI();
                return;
            }
        }
    }

    void TakeBack(int slotIndex)
    {
        if (finished || locked[slotIndex] || slots[slotIndex] < 0) return;

        used[slots[slotIndex]] = false;
        slots[slotIndex] = -1;
        SoundManager.PlayClick();
        RefreshUI();
    }

    void ResetSlots()
    {
        if (finished) return;

        for (int i = 0; i < Size; i++)
        {
            if (!locked[i] && slots[i] >= 0)
            {
                used[slots[i]] = false;
                slots[i] = -1;
            }
        }

        feedbackText.text = "";
        RefreshUI();
    }

    void SubmitAttempt()
    {
        if (finished) return;

        for (int i = 0; i < Size; i++)
        {
            if (slots[i] < 0)
            {
                feedbackText.text = "FILL ALL EIGHT SLOTS FIRST";
                return;
            }
        }

        List<string> solution = GameManager.Instance.solutionOrder;
        int correct = 0;

        for (int i = 0; i < Size && i < solution.Count; i++)
        {
            if (letters[slots[i]] == solution[i])
            {
                locked[i] = true;
                correct++;
            }
        }

        if (correct >= Size)
        {
            finished = true;
            feedbackText.text = "ACCESS GRANTED";
            RefreshUI();
            GameManager.Instance.Victory();
            return;
        }

        attemptsLeft--;

        if (attemptsLeft <= 0)
        {
            finished = true;
            feedbackText.text = "ACCESS DENIED";
            RefreshUI();
            GameManager.Instance.GameOver("NO ATTEMPTS LEFT");
            return;
        }

        for (int i = 0; i < Size; i++)
        {
            if (!locked[i] && slots[i] >= 0)
            {
                used[slots[i]] = false;
                slots[i] = -1;
            }
        }

        SoundManager.PlayFail();
        feedbackText.text = correct + " LETTER(S) IN THE RIGHT PLACE";
        RefreshUI();
    }

    // L'indice verrouille la premiere lettre du code
    void ApplyHint()
    {
        if (GameManager.Instance.solutionOrder.Count == 0) return;

        string first = GameManager.Instance.solutionOrder[0];

        for (int j = 0; j < Size; j++)
        {
            if (used[j] || letters[j] != first) continue;

            slots[0] = j;
            used[j] = true;
            locked[0] = true;
            return;
        }
    }

    void RefreshUI()
    {
        for (int i = 0; i < Size; i++)
        {
            bool filled = slots[i] >= 0;

            slotLabel[i].text = filled ? letters[slots[i]] : "";
            slotBg[i].color = locked[i] ? SlotLocked : (filled ? SlotFilled : SlotEmpty);
            slotLabel[i].color = locked[i] ? new Color(0.65f, 1f, 0.78f) : new Color(1f, 0.87f, 0.42f);

            letterLabel[i].text = letters[i];
            letterBg[i].color = used[i] ? LetterUsed : LetterIdle;
            letterLabel[i].color = used[i] ? new Color(1f, 1f, 1f, 0.25f) : Color.white;
            letterButtons[i].interactable = !used[i] && !finished;
        }

        attemptsText.text = "ATTEMPTS LEFT : " + attemptsLeft;

        submitButton.interactable = !finished;
        resetButton.interactable = !finished;
    }
}

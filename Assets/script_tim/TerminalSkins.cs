using System.Collections.Generic;
using UnityEngine;

// Chaque systeme a sa propre borne : silhouette, couleur et ecran differents.
public static class TerminalSkins
{
    private const int W = 80;
    private const int H = 104;
    private const float Ppu = 40f;

    private static Dictionary<string, Sprite> broken = new Dictionary<string, Sprite>();
    private static Dictionary<string, Sprite> repaired = new Dictionary<string, Sprite>();

    private class Style
    {
        public Color tint;
        public float bodyWidth;
        public int top;        // 0 plat, 1 trapeze, 2 arrondi, 3 pointe
        public int buttons;
        public bool twinScreen;
    }

    private static Dictionary<string, Style> styles = new Dictionary<string, Style>
    {
        { "Engine",  new Style { tint = new Color(1f, 0.42f, 0.28f), bodyWidth = 70f, top = 0, buttons = 3, twinScreen = false } },
        { "Shield",  new Style { tint = new Color(0.38f, 0.66f, 1f), bodyWidth = 64f, top = 1, buttons = 2, twinScreen = false } },
        { "Oxygen",  new Style { tint = new Color(0.46f, 0.95f, 0.62f), bodyWidth = 56f, top = 2, buttons = 4, twinScreen = false } },
        { "Comms",   new Style { tint = new Color(1f, 0.78f, 0.32f), bodyWidth = 74f, top = 0, buttons = 2, twinScreen = true } },
        { "Coolant", new Style { tint = new Color(0.42f, 0.88f, 1f), bodyWidth = 60f, top = 3, buttons = 3, twinScreen = false } }
    };

    public static Sprite Broken(string id)
    {
        Build(id);
        return broken.ContainsKey(id) ? broken[id] : null;
    }

    public static Sprite Repaired(string id)
    {
        Build(id);
        return repaired.ContainsKey(id) ? repaired[id] : null;
    }

    static void Build(string id)
    {
        if (broken.ContainsKey(id)) return;
        if (!styles.ContainsKey(id)) return;

        Style st = styles[id];
        broken[id] = Make(st, false);
        repaired[id] = Make(st, true);
    }

    static float HalfWidth(int y, Style st)
    {
        float half = st.bodyWidth * 0.5f;

        if (y < 14) return half + 5f;          // socle evase
        if (y < 72) return half;

        float t = (y - 72f) / (H - 1f - 72f);

        if (st.top == 1) return half * (1f - t * 0.28f);
        if (st.top == 2) return half * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t * 0.88f));
        if (st.top == 3) return half * (1f - t * 0.72f);

        return half;
    }

    static Sprite Make(Style st, bool on)
    {
        Color[] px = new Color[W * H];
        float cx = W * 0.5f;

        Color shell = new Color(0.26f, 0.29f, 0.35f);
        Color shellLit = new Color(0.40f, 0.44f, 0.52f);
        Color dark = new Color(0.10f, 0.12f, 0.16f);

        for (int y = 0; y < H; y++)
        {
            float half = HalfWidth(y, st);

            for (int x = 0; x < W; x++)
            {
                float dx = x - cx;
                int i = y * W + x;

                if (Mathf.Abs(dx) > half)
                {
                    px[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                float edge = half - Mathf.Abs(dx);
                float lift = 0.5f + 0.5f * (dx / Mathf.Max(1f, half));
                Color c = Color.Lerp(shell, shellLit, 1f - lift);

                if (edge < 2.5f) c = dark;
                if (y < 14) c = Color.Lerp(dark, shell, y / 14f);

                // Bandeau lumineux au sommet
                if (y >= 86 && y <= 97 && edge > 3f)
                {
                    c = on ? st.tint : st.tint * 0.22f;
                    if (y == 86 || y == 97) c *= 0.5f;
                }

                // Ecran
                bool inScreen = y >= 50 && y <= 82 && Mathf.Abs(dx) < half - 7f;

                if (inScreen)
                {
                    if (st.twinScreen && Mathf.Abs(dx) < 4f)
                    {
                        c = dark;
                    }
                    else if (on)
                    {
                        float band = Mathf.Sin((y - 50) * 0.55f + dx * 0.12f) * 0.5f + 0.5f;
                        c = Color.Lerp(st.tint * 0.30f, st.tint, band * 0.85f);

                        if ((y - 50) % 4 == 0) c *= 0.74f;
                    }
                    else
                    {
                        c = new Color(0.07f, 0.08f, 0.11f);

                        // Fissures sur l'ecran eteint
                        float crack = Mathf.Abs((dx * 1.7f + (y - 66) * 0.9f) % 19f);
                        if (crack < 1.1f) c = new Color(0.30f, 0.33f, 0.40f);
                    }
                }

                // Pupitre incline avec ses boutons
                if (y >= 28 && y <= 44 && Mathf.Abs(dx) < half - 5f)
                {
                    c = new Color(0.17f, 0.19f, 0.24f);

                    for (int k = 0; k < st.buttons; k++)
                    {
                        float bx = -half + 10f + (k + 0.5f) * (2f * half - 20f) / st.buttons;
                        float d = Vector2.Distance(new Vector2(dx, y), new Vector2(bx, 36f));

                        if (d < 3.4f)
                        {
                            Color b = (k % 3 == 0) ? new Color(0.9f, 0.3f, 0.28f)
                                    : (k % 3 == 1) ? new Color(0.95f, 0.8f, 0.3f)
                                                   : new Color(0.4f, 0.8f, 0.95f);
                            c = on ? b : b * 0.42f;
                        }
                    }
                }

                // Voyant d'etat
                float ld = Vector2.Distance(new Vector2(dx, y), new Vector2(half - 7f, 47f));
                if (ld < 2.6f)
                    c = on ? new Color(0.35f, 1f, 0.5f) : new Color(1f, 0.28f, 0.25f);

                px[i] = new Color(c.r, c.g, c.b, 1f);
            }
        }

        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, W, H), new Vector2(0.5f, 0.5f),
            Ppu, 0, SpriteMeshType.FullRect);
    }
}

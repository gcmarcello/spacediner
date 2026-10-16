using UnityEngine;

public enum Item { None, RawMeat, RawVeg, ChoppedMeat, Salad, Burger, Burnt }

public static class ItemInfo
{
    public static string Name(Item i)
    {
        switch (i)
        {
            case Item.RawMeat: return "Carne crua";
            case Item.RawVeg: return "Verdura crua";
            case Item.ChoppedMeat: return "Carne picada";
            case Item.Salad: return "Salada";
            case Item.Burger: return "Hamburguer";
            case Item.Burnt: return "Carvao";
            default: return "";
        }
    }

    public static string Bar(float ratio, int n = 8)
    {
        int k = Mathf.Clamp(Mathf.RoundToInt(ratio * n), 0, n);
        return "[" + new string('#', k) + new string('-', n - k) + "]";
    }
}

// Helpers para gerar sprites e textos em codigo (sem assets externos).
public static class G
{
    static Sprite square, circle;
    static Font font;

    public static Sprite Square { get { if (square == null) square = MakeSprite(false); return square; } }
    public static Sprite Circle { get { if (circle == null) circle = MakeSprite(true); return circle; } }

    static Sprite MakeSprite(bool round)
    {
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float a = 1f;
                if (round)
                {
                    float dx = (x + 0.5f) / n * 2f - 1f;
                    float dy = (y + 0.5f) / n * 2f - 1f;
                    a = Mathf.Clamp01((1f - Mathf.Sqrt(dx * dx + dy * dy)) * n / 2f);
                }
                px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
    }

    public static SpriteRenderer Spr(string name, Sprite s, Color c, Vector2 pos, Vector2 size, int order, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = c;
        sr.sortingOrder = order;
        return sr;
    }

    public static TextMesh Label(Transform parent, string text, Vector2 pos, float size, Color c, int order = 50)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.text = text;
        tm.fontSize = 64;
        tm.characterSize = size * 0.1f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = c;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = font.material;
        mr.sortingOrder = order;
        return tm;
    }
}

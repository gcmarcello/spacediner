using UnityEngine;

// Monta o jogo inteiro em codigo: basta apertar Play em qualquer cena.
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public const float WallX = 0.35f;
    public static readonly float[] LaneY = { -3f, -0.5f, 2f };
    const int MaxLost = 6;

    public float Money = 40f;
    public float StationHp = 100f;
    public float MaxHp = 100f;
    public int Wave, Served, Lost;
    public bool Over;
    public string Banner = "";
    string overReason = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (I == null) new GameObject("SpaceDiner").AddComponent<GameManager>();
    }

    void Awake()
    {
        I = this;
        Time.timeScale = 1f;
        Station.All.Clear();
        Enemy.All.Clear();
        BuildWorld();
        gameObject.AddComponent<WaveSpawner>();
        gameObject.AddComponent<CustomerSpawner>();
    }

    void OnDestroy() { if (I == this) I = null; }

    public void DamageStation(float dmg)
    {
        if (Over) return;
        StationHp -= dmg;
        if (StationHp <= 0f) EndGame("A estacao foi destruida!");
    }

    void EndGame(string reason)
    {
        Over = true;
        overReason = reason;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!Over && Lost >= MaxLost) EndGame("Clientes demais foram embora com fome!");
        if (Over && Input.GetKeyDown(KeyCode.R)) Restart();
    }

    void Restart()
    {
        foreach (var go in FindObjectsOfType<GameObject>())
            if (go != gameObject && go.transform.parent == null && go.GetComponent<Camera>() == null) Destroy(go);
        Money = 40f; StationHp = MaxHp; Wave = 0; Served = 0; Lost = 0; Over = false; Banner = "";
        Time.timeScale = 1f;
        Station.All.Clear();
        Enemy.All.Clear();
        DestroyImmediate(GetComponent<WaveSpawner>());
        DestroyImmediate(GetComponent<CustomerSpawner>());
        BuildWorld();
        gameObject.AddComponent<WaveSpawner>();
        gameObject.AddComponent<CustomerSpawner>();
    }

    // ---------------------------------------------------------------- mundo
    void BuildWorld()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6.2f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.07f);

        for (int i = 0; i < 90; i++)
        {
            float s = Random.Range(0.04f, 0.12f);
            G.Spr("Estrela", G.Circle, new Color(1f, 1f, 1f, Random.Range(0.3f, 0.9f)),
                new Vector2(Random.Range(-11.5f, 11.5f), Random.Range(-6.2f, 6.2f)), new Vector2(s, s), -100);
        }

        G.Spr("Chao", G.Square, new Color(0.16f, 0.18f, 0.25f), new Vector2(-5.2f, 0f), new Vector2(10.6f, 10.6f), -50);
        G.Spr("Parede", G.Square, new Color(0.45f, 0.55f, 0.8f), new Vector2(WallX, 0f), new Vector2(0.3f, 10.6f), -40);
        foreach (float y in LaneY)
            G.Spr("Pista", G.Square, new Color(0.07f, 0.07f, 0.14f), new Vector2(6.2f, y), new Vector2(11f, 1.8f), -60);

        // cozinha
        Make<Crate>("CARNE", new Color(0.75f, 0.25f, 0.25f), new Vector2(-9.5f, 1.2f), new Vector2(1.2f, 1.2f)).Gives = Item.RawMeat;
        Make<Crate>("VERDURA", new Color(0.25f, 0.65f, 0.3f), new Vector2(-9.5f, -0.8f), new Vector2(1.2f, 1.2f)).Gives = Item.RawVeg;
        Make<Board>("TABUA", new Color(0.6f, 0.42f, 0.25f), new Vector2(-6.5f, 1.2f), new Vector2(1.4f, 1.2f));
        Make<Board>("TABUA", new Color(0.6f, 0.42f, 0.25f), new Vector2(-6.5f, -0.8f), new Vector2(1.4f, 1.2f));
        Make<Grill>("CHAPA", new Color(0.8f, 0.4f, 0.15f), new Vector2(-3.5f, 1.2f), new Vector2(1.4f, 1.2f));
        Make<Grill>("CHAPA", new Color(0.8f, 0.4f, 0.15f), new Vector2(-3.5f, -0.8f), new Vector2(1.4f, 1.2f));
        Make<Trash>("LIXO", new Color(0.4f, 0.4f, 0.45f), new Vector2(-9.5f, -3.2f), new Vector2(1.2f, 1.2f));

        // balcao de clientes
        float[] slotX = { -8f, -5.7f, -3.4f, -1.1f };
        foreach (float x in slotX)
            Make<CustomerSlot>("BALCAO", new Color(0.2f, 0.55f, 0.6f), new Vector2(x, 4f), new Vector2(1.7f, 0.8f));

        // torres
        foreach (float y in LaneY)
            Make<TurretSlot>("TORRE", new Color(0.35f, 0.3f, 0.5f), new Vector2(3.5f, y), new Vector2(1.2f, 1.2f));

        var chef = new GameObject("Chef");
        chef.transform.position = new Vector3(-6f, -2.5f, 0f);
        chef.AddComponent<Player>();
    }

    T Make<T>(string label, Color col, Vector2 pos, Vector2 size) where T : Station
    {
        var go = new GameObject(label);
        go.transform.position = pos;
        var s = go.AddComponent<T>();
        s.Size = size;
        G.Spr("Corpo", G.Square, col, Vector2.zero, size, 0, go.transform);
        G.Label(go.transform, label, new Vector2(0f, 0.1f), 0.4f, Color.white);
        s.Status = G.Label(go.transform, "", new Vector2(0f, -size.y / 2f - 0.3f), 0.35f, Color.yellow);
        return s;
    }

    // ---------------------------------------------------------------- HUD
    void OnGUI()
    {
        int fs = Mathf.Max(14, Screen.height / 32);
        var st = new GUIStyle(GUI.skin.label) { fontSize = fs, fontStyle = FontStyle.Bold };
        st.normal.textColor = Color.white;

        GUI.Label(new Rect(12, 6, Screen.width, fs * 2f),
            "$ " + Mathf.FloorToInt(Money) + "     Servidos: " + Served + "     Perdidos: " + Lost + "/" + MaxLost, st);

        float w = Screen.width * 0.28f;
        var bar = new Rect(12, fs * 2f, w, fs * 0.9f);
        GUI.color = Color.black;
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        float r = Mathf.Clamp01(StationHp / MaxHp);
        GUI.color = Color.Lerp(Color.red, Color.green, r);
        GUI.DrawTexture(new Rect(bar.x, bar.y, w * r, bar.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(bar.x + w + 8, bar.y - 4, 300, fs * 1.6f), "Estacao " + Mathf.CeilToInt(StationHp), st);

        var center = new GUIStyle(st) { alignment = TextAnchor.UpperCenter };
        GUI.Label(new Rect(0, 6, Screen.width, fs * 2f), Banner, center);

        var small = new GUIStyle(st) { fontSize = Mathf.Max(11, fs * 2 / 3), alignment = TextAnchor.LowerCenter };
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height - 6),
            "WASD/Setas: mover   E/Espaco: interagir (SEGURE na tabua para cortar)   R: reiniciar", small);

        if (Over)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            var big = new GUIStyle(st) { fontSize = fs * 2, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, fs * 3f), "FIM DE JOGO", big);
            GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, fs * 2f),
                overReason + "  Onda " + Wave + " | Servidos " + Served, center);
            GUI.Label(new Rect(0, Screen.height * 0.55f, Screen.width, fs * 2f), "Aperte R para tentar de novo", center);
        }
    }
}

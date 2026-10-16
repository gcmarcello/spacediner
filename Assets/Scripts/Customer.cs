using UnityEngine;

public class Customer : MonoBehaviour
{
    const float MaxPatience = 45f;

    public Item Order;
    public CustomerSlot Slot;
    float patience = MaxPatience;
    TextMesh bar;

    public static Customer Create(CustomerSlot slot)
    {
        var go = new GameObject("Cliente");
        go.transform.position = slot.transform.position + new Vector3(0f, 1.15f, 0f);
        var c = go.AddComponent<Customer>();
        c.Slot = slot;
        c.Order = Random.value < 0.6f ? Item.Burger : Item.Salad;
        slot.Occupant = c;

        Color skin = Color.HSVToRGB(Random.value, 0.45f, 0.95f);
        G.Spr("Corpo", G.Circle, skin, Vector2.zero, new Vector2(1f, 1f), 10, go.transform);
        G.Spr("OlhoE", G.Circle, Color.white, new Vector2(-0.2f, 0.12f), new Vector2(0.25f, 0.25f), 11, go.transform);
        G.Spr("OlhoD", G.Circle, Color.white, new Vector2(0.2f, 0.12f), new Vector2(0.25f, 0.25f), 11, go.transform);
        G.Spr("PupE", G.Circle, Color.black, new Vector2(-0.2f, 0.12f), new Vector2(0.1f, 0.1f), 12, go.transform);
        G.Spr("PupD", G.Circle, Color.black, new Vector2(0.2f, 0.12f), new Vector2(0.1f, 0.1f), 12, go.transform);
        G.Label(go.transform, ItemInfo.Name(c.Order), new Vector2(0f, -0.2f), 0.4f, Color.black, 60);
        c.bar = G.Label(go.transform, "", new Vector2(0f, 0.75f), 0.4f, Color.green, 60);
        return c;
    }

    void Update()
    {
        patience -= Time.deltaTime;
        float r = Mathf.Clamp01(patience / MaxPatience);
        bar.text = new string('|', Mathf.CeilToInt(r * 10f));
        bar.color = Color.Lerp(Color.red, Color.green, r);

        if (patience <= 0f)
        {
            var gm = GameManager.I;
            gm.Money = Mathf.Max(0f, gm.Money - 10f);
            gm.Lost++;
            Slot.Occupant = null;
            Destroy(gameObject);
        }
    }

    public void Serve()
    {
        var gm = GameManager.I;
        gm.Money += 15f + Mathf.Round(patience / MaxPatience * 15f);
        gm.Served++;
        Slot.Occupant = null;
        Destroy(gameObject);
    }
}

public class CustomerSpawner : MonoBehaviour
{
    float timer = 3f;

    void Update()
    {
        var gm = GameManager.I;
        if (gm.Over) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Mathf.Max(4.5f, 8f - gm.Wave * 0.4f);

        var free = new System.Collections.Generic.List<CustomerSlot>();
        foreach (var s in Station.All)
        {
            var cs = s as CustomerSlot;
            if (cs != null && cs.Occupant == null) free.Add(cs);
        }
        if (free.Count > 0) Customer.Create(free[Random.Range(0, free.Count)]);
    }
}

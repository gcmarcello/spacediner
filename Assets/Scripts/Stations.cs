using UnityEngine;

public abstract class Station : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<Station> All = new System.Collections.Generic.List<Station>();

    public Vector2 Size = Vector2.one;
    public TextMesh Status;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public float DistanceTo(Vector2 p)
    {
        Vector2 c = transform.position;
        float dx = Mathf.Max(Mathf.Abs(p.x - c.x) - Size.x / 2f, 0f);
        float dy = Mathf.Max(Mathf.Abs(p.y - c.y) - Size.y / 2f, 0f);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public virtual void Interact(Player p) { }
    public virtual void Hold(Player p, float dt) { }
    protected virtual void Tick(float dt) { }
    protected virtual string Describe() { return ""; }

    void Update()
    {
        Tick(Time.deltaTime);
        if (Status != null) Status.text = Describe();
    }
}

public class Crate : Station
{
    public Item Gives;

    public override void Interact(Player p)
    {
        if (p.Held == Item.None) p.Held = Gives;
    }
}

public class Trash : Station
{
    public override void Interact(Player p) { p.Held = Item.None; }
}

// Tabua: coloque ingrediente cru e SEGURE E para cortar.
public class Board : Station
{
    const float ChopTime = 2.5f;
    Item content;
    float progress;

    public override void Interact(Player p)
    {
        if (content == Item.None && (p.Held == Item.RawMeat || p.Held == Item.RawVeg))
        {
            content = p.Held;
            p.Held = Item.None;
            progress = 0f;
        }
        else if (content != Item.None && p.Held == Item.None)
        {
            p.Held = content;
            content = Item.None;
            progress = 0f;
        }
    }

    public override void Hold(Player p, float dt)
    {
        if (content != Item.RawMeat && content != Item.RawVeg) return;
        progress += dt / ChopTime;
        if (progress >= 1f)
        {
            content = content == Item.RawMeat ? Item.ChoppedMeat : Item.Salad;
            progress = 0f;
        }
    }

    protected override string Describe()
    {
        if (content == Item.None) return "";
        if (content == Item.RawMeat || content == Item.RawVeg)
            return ItemInfo.Name(content) + "\n" + ItemInfo.Bar(progress);
        return ItemInfo.Name(content);
    }
}

// Chapa: carne picada vira hamburguer sozinha. Se esquecer la, queima!
public class Grill : Station
{
    const float CookTime = 6f;
    const float BurnTime = 7f;
    Item content;
    float t;

    public override void Interact(Player p)
    {
        if (content == Item.None && p.Held == Item.ChoppedMeat)
        {
            content = Item.ChoppedMeat;
            p.Held = Item.None;
            t = 0f;
        }
        else if (content != Item.None && p.Held == Item.None)
        {
            p.Held = content;
            content = Item.None;
            t = 0f;
        }
    }

    protected override void Tick(float dt)
    {
        if (content == Item.ChoppedMeat)
        {
            t += dt;
            if (t >= CookTime) { content = Item.Burger; t = 0f; }
        }
        else if (content == Item.Burger)
        {
            t += dt;
            if (t >= BurnTime) { content = Item.Burnt; t = 0f; }
        }
    }

    protected override string Describe()
    {
        switch (content)
        {
            case Item.ChoppedMeat: return "Cozinhando\n" + ItemInfo.Bar(t / CookTime);
            case Item.Burger: return t > BurnTime * 0.6f ? "QUEIMANDO!!" : "PRONTO!";
            case Item.Burnt: return "QUEIMADO";
            default: return "";
        }
    }
}

// Bancada de atendimento: entregue o prato certo para o cliente.
public class CustomerSlot : Station
{
    public Customer Occupant;

    public override void Interact(Player p)
    {
        if (Occupant != null && p.Held == Occupant.Order)
        {
            p.Held = Item.None;
            Occupant.Serve();
        }
    }
}

// Ponto de torre: construa, abasteca com pratos (municao) e melhore.
public class TurretSlot : Station
{
    const int BuildCost = 60;
    const int MaxLevel = 3;
    const int AmmoCap = 30;
    const float Range = 8f;
    static readonly int[] UpgradeCost = { 0, 0, 90, 140 };

    int level, ammo;
    float cooldown;
    Transform pivot;
    SpriteRenderer body;

    void Start()
    {
        body = GetComponentInChildren<SpriteRenderer>();
    }

    public override void Interact(Player p)
    {
        var gm = GameManager.I;
        if (level == 0)
        {
            if (gm.Money >= BuildCost) { gm.Money -= BuildCost; Build(); }
            return;
        }
        if (p.Held == Item.Burger || p.Held == Item.Salad)
        {
            ammo = Mathf.Min(AmmoCap, ammo + (p.Held == Item.Burger ? 6 : 10));
            p.Held = Item.None;
        }
        else if (p.Held == Item.None && level < MaxLevel && gm.Money >= UpgradeCost[level + 1])
        {
            gm.Money -= UpgradeCost[level + 1];
            level++;
        }
    }

    void Build()
    {
        level = 1;
        ammo = 6;
        if (body == null) body = GetComponentInChildren<SpriteRenderer>();
        pivot = new GameObject("Barrel").transform;
        pivot.SetParent(transform, false);
        G.Spr("Cano", G.Square, new Color(0.9f, 0.9f, 1f), new Vector2(0.45f, 0f), new Vector2(0.9f, 0.22f), 5, pivot);
        G.Spr("Base", G.Circle, new Color(0.3f, 0.35f, 0.5f), Vector2.zero, new Vector2(0.7f, 0.7f), 4, transform);
    }

    protected override void Tick(float dt)
    {
        if (level == 0) return;
        cooldown -= dt;

        Enemy target = null;
        float best = Range;
        foreach (var e in Enemy.All)
        {
            if (e.Dead) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < best) { best = d; target = e; }
        }
        if (target == null) return;

        Vector2 dir = (Vector2)target.transform.position - (Vector2)transform.position;
        pivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        if (ammo > 0 && cooldown <= 0f)
        {
            ammo--;
            cooldown = 0.7f - 0.15f * (level - 1);
            Projectile.Create(transform.position, target, 10f * (1f + 0.6f * (level - 1)));
        }
    }

    protected override string Describe()
    {
        if (level == 0) return "VAZIO\nConstruir $" + BuildCost;
        string s = "Nv" + level + "  Muni " + ammo;
        if (level < MaxLevel) s += "\nMelhorar $" + UpgradeCost[level + 1];
        return s;
    }
}

using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Monster, Mafia, Pirate }

public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public bool Dead;
    EnemyType type;
    float hp, maxHp, speed, dps, stopX;
    int reward;
    SpriteRenderer hpBar;
    float barWidth;

    public static Enemy Create(EnemyType t, Vector2 pos, float scale)
    {
        var go = new GameObject(t.ToString());
        go.transform.position = pos;
        var e = go.AddComponent<Enemy>();
        e.Init(t, scale);
        return e;
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Init(EnemyType t, float scale)
    {
        type = t;
        Color col;
        float size;
        Sprite sprite;
        string tag;
        stopX = GameManager.WallX + 0.7f;

        switch (t)
        {
            case EnemyType.Monster: // lento, tanque
                maxHp = 55f; speed = 0.9f; dps = 12f; reward = 6;
                col = new Color(0.35f, 0.85f, 0.3f); size = 1.3f; sprite = G.Circle; tag = "MONSTRO";
                break;
            case EnemyType.Mafia: // ataca de longe
                maxHp = 38f; speed = 1.5f; dps = 8f; reward = 9;
                col = new Color(0.45f, 0.2f, 0.6f); size = 1.0f; sprite = G.Square; tag = "MAFIA";
                stopX = GameManager.WallX + 3.5f;
                break;
            default: // pirata: rapido e fraco
                maxHp = 20f; speed = 2.6f; dps = 6f; reward = 5;
                col = new Color(1f, 0.55f, 0.15f); size = 0.8f; sprite = G.Circle; tag = "PIRATA";
                break;
        }

        maxHp *= scale;
        hp = maxHp;
        G.Spr("Corpo", sprite, col, Vector2.zero, new Vector2(size, size), 15, transform);
        G.Label(transform, tag, Vector2.zero, 0.3f, Color.white, 60);
        barWidth = size;
        G.Spr("HpFundo", G.Square, new Color(0f, 0f, 0f, 0.7f), new Vector2(0f, size / 2f + 0.2f), new Vector2(barWidth, 0.12f), 30, transform);
        hpBar = G.Spr("Hp", G.Square, Color.red, new Vector2(0f, size / 2f + 0.2f), new Vector2(barWidth, 0.12f), 31, transform);
    }

    void Update()
    {
        if (Dead) return;
        var gm = GameManager.I;
        if (transform.position.x > stopX)
            transform.position += Vector3.left * speed * Time.deltaTime;
        else
            gm.DamageStation(dps * Time.deltaTime);
    }

    public void Hit(float damage)
    {
        if (Dead) return;
        hp -= damage;
        float r = Mathf.Clamp01(hp / maxHp);
        hpBar.transform.localScale = new Vector3(barWidth * r, 0.12f, 1f);
        hpBar.transform.localPosition = new Vector3(-(1f - r) * barWidth / 2f, hpBar.transform.localPosition.y, 0f);
        if (hp <= 0f)
        {
            Dead = true;
            GameManager.I.Money += reward;
            Destroy(gameObject);
        }
    }
}

public class Projectile : MonoBehaviour
{
    const float Speed = 16f;
    Enemy target;
    float damage;

    public static void Create(Vector2 pos, Enemy target, float damage)
    {
        var go = new GameObject("Tiro");
        go.transform.position = pos;
        var p = go.AddComponent<Projectile>();
        p.target = target;
        p.damage = damage;
        G.Spr("Bala", G.Circle, new Color(1f, 0.95f, 0.4f), Vector2.zero, new Vector2(0.25f, 0.25f), 40, go.transform);
    }

    void Update()
    {
        if (target == null || target.Dead) { Destroy(gameObject); return; }
        Vector3 to = target.transform.position - transform.position;
        float step = Speed * Time.deltaTime;
        if (to.magnitude <= step + 0.3f)
        {
            target.Hit(damage);
            Destroy(gameObject);
            return;
        }
        transform.position += to.normalized * step;
    }
}

public class WaveSpawner : MonoBehaviour
{
    float timer = 8f;
    int toSpawn;
    float spawnCd;
    bool active;

    void Update()
    {
        var gm = GameManager.I;
        if (gm.Over) return;

        if (!active)
        {
            timer -= Time.deltaTime;
            gm.Banner = "Onda " + (gm.Wave + 1) + " em " + Mathf.CeilToInt(Mathf.Max(0f, timer)) + "s";
            if (timer <= 0f)
            {
                gm.Wave++;
                toSpawn = 3 + gm.Wave * 2;
                spawnCd = 0f;
                active = true;
            }
            return;
        }

        gm.Banner = "ONDA " + gm.Wave + " - inimigos: " + (toSpawn + Enemy.All.Count);

        if (toSpawn > 0)
        {
            spawnCd -= Time.deltaTime;
            if (spawnCd <= 0f)
            {
                Spawn(gm.Wave);
                toSpawn--;
                spawnCd = Mathf.Max(0.6f, 1.8f - gm.Wave * 0.1f);
            }
        }
        else if (Enemy.All.Count == 0)
        {
            active = false;
            timer = 7f;
            gm.Money += 10 + gm.Wave * 5; // bonus por sobreviver
        }
    }

    void Spawn(int wave)
    {
        EnemyType t = EnemyType.Monster;
        float roll = Random.value;
        if (wave >= 3 && roll < 0.3f) t = EnemyType.Mafia;
        else if (wave >= 2 && roll < 0.6f) t = EnemyType.Pirate;

        float y = GameManager.LaneY[Random.Range(0, GameManager.LaneY.Length)] + Random.Range(-0.3f, 0.3f);
        Enemy.Create(t, new Vector2(11.5f, y), 1f + 0.12f * (wave - 1));
    }
}

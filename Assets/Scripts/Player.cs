using UnityEngine;

public class Player : MonoBehaviour
{
    const float Speed = 6f;
    const float Reach = 0.8f;

    public Item Held;
    TextMesh heldLabel;
    SpriteRenderer highlight;

    void Start()
    {
        G.Spr("Corpo", G.Circle, new Color(0.3f, 0.7f, 1f), Vector2.zero, new Vector2(0.9f, 0.9f), 20, transform);
        G.Spr("Visor", G.Circle, new Color(0.1f, 0.15f, 0.3f), new Vector2(0.12f, 0.1f), new Vector2(0.45f, 0.3f), 21, transform);
        heldLabel = G.Label(transform, "", new Vector2(0f, 0.85f), 0.45f, Color.yellow, 60);
        highlight = G.Spr("Destaque", G.Square, new Color(1f, 1f, 0.3f, 0.45f), Vector2.zero, Vector2.one, -1);
        highlight.enabled = false;
    }

    void Update()
    {
        if (GameManager.I.Over) return;

        var d = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (d.sqrMagnitude > 1f) d.Normalize();
        Vector2 p = (Vector2)transform.position + d * Speed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, -10.3f, 10.3f);
        p.y = Mathf.Clamp(p.y, -4.6f, 3.3f);
        transform.position = p;

        Station near = null;
        float best = Reach;
        foreach (var s in Station.All)
        {
            float dist = s.DistanceTo(p);
            if (dist < best) { best = dist; near = s; }
        }

        highlight.enabled = near != null;
        if (near != null)
        {
            highlight.transform.position = near.transform.position;
            highlight.transform.localScale = new Vector3(near.Size.x + 0.25f, near.Size.y + 0.25f, 1f);
            bool press = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
            bool hold = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
            if (press) near.Interact(this);
            if (hold) near.Hold(this, Time.deltaTime);
        }

        heldLabel.text = ItemInfo.Name(Held);
    }
}

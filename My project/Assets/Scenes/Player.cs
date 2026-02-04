using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;
    public float jumpUp = 1;
    public Vector3 direction;

    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sprite;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        direction = Vector3.zero;
    }

    void KeyInput()
    {
        direction.x = Input.GetAxisRaw("Horizontal");

        if (direction.x < 0)
        {
            sprite.flipX = true;
        }
        else if (direction.x > 0)
        {
            sprite.flipX = false;
        }

        rb.linearVelocity = new Vector2(direction.x * Speed, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction.x * Speed, rb.linearVelocity.y);
    }

    void Update()
    {
        KeyInput();
    }
}

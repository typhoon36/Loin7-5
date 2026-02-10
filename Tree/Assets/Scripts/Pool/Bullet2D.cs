using UnityEngine;

public class Bullet2D : MonoBehaviour
{
    [Header("총알 설정")]
    public float speed = 8f;        // 이동 속도
    public float lifeTime = 2f;     // 자동 반환까지 시간(초)

    float timer;
    BulletPool pool;

    // 풀 참조 설정 (스포너에서 호출)
    public void SetPool(BulletPool pool)
    {
        this.pool = pool;
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        // 위쪽으로 이동 (2D)
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        // 수명 초과 시 풀로 반환
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    // 화면 밖으로 나가면 반환
    void OnBecameInvisible()
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (pool != null)
            pool.ReturnBullet(gameObject);
        else
            gameObject.SetActive(false);
    }
}

using UnityEngine;

public class ShotEnemy : MonoBehaviour
{
    [Header("Enemy Setting")]
    public float detectionRange = 10f;
    public float shootingInterval = 2f;
    public GameObject BulletObj;

    [Header("Ref")]
    public Transform firepoint;
    private Transform player;
    private float shootTimer;
    private SpriteRenderer spriteRenderer;
    private Animator animator;


    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        shootTimer = shootingInterval;
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (player == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (player.position.x < transform.position.x)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;

            shootTimer -= Time.deltaTime;

            if (shootTimer <= 0f)
            {
                Shot();
                shootTimer = shootingInterval;
            }
        }

    }

    void Shot()
    {
        //미사일 생성
        GameObject bulletObj = Instantiate(BulletObj, firepoint.position, Quaternion.identity);


        //플레이어 방향으로 발사 방향 설정
        Vector2 direction = (player.position - firepoint.position).normalized;
        bulletObj.GetComponent<EnemyBullet>().SetDirection(direction);
        bulletObj.GetComponent<SpriteRenderer>().flipX = (player.position.x < transform.position.x);
    }

    //디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }


    //적 캐릭터 사망 애니메이션 재생
    public void PlayDeathAnimation()
    {
        animator.SetBool("Death", true);

        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }

}

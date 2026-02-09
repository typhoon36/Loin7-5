using UnityEngine;

public class ShootingEnemy : MonoBehaviour
{
    [Header("적 캐릭터 속성")]
    public float detectionRange = 10f; //플레이어를 감지할 수있는 최대 거리
    public float shootingInterval = 2f; //미사일 발사 사이의 대기 시간
    public GameObject missilePrefab; //발사할 미사일 프리팹

    [Header("참조 컴포넌트")]
    public Transform firepoint; //미사일 발사 위치
    private Transform player;  //플레이어의 위치 정보
    private float shootTimer; //발사 타이머
    private SpriteRenderer spriteRenderer; //스프라이트 방향전환위한 컴포넌트가져오기
    private Animator animator; //애니메이션컨트롤러


    void Start()
    {
        //필요한 컴포넌트 초기화
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        shootTimer = shootingInterval; //타이머초기화
        animator = GetComponent<Animator>(); //애니메이터 초기화
    }

    
    void Update()
    {
        if (player == null) return; //플레이어가 없으면 실행하지 않음

        //플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if(distanceToPlayer <= detectionRange)
        {
            //플레이어 방향으로 스프라이트 회전
            spriteRenderer.flipX = (player.position.x < transform.position.x);

            //미사일 발사 로직
            shootTimer -= Time.deltaTime; //타이머 감소

            if(shootTimer <=0)
            {
                Shoot(); //미사일 발사
                shootTimer = shootingInterval; //타이머리셋
            }

        }

    }


    void Shoot()
    {
        //미사일 생성
        GameObject missile = Instantiate(missilePrefab, firepoint.position, Quaternion.identity);


        //플레이어 방향으로 발사 방향 설정
        Vector2 direction = (player.position - firepoint.position).normalized;
        missile.GetComponent<EnemyMissile>().SetDirection(direction);
        missile.GetComponent<SpriteRenderer>().flipX = (player.position.x < transform.position.x);
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
        //선택사항: 애니메이션 종료후 오브젝트 제거를 위해
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }




}


using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5;
    public float jumpUp = 1;
    public float power = 5;
    public Vector3 direction;
    public GameObject slash;


    //그림자
    public GameObject Shadow1;
    List<GameObject> sh = new List<GameObject>();


    //레이저 공격 오브젝트
    public GameObject hit_lazer;

    Animator pAnimator;
    Rigidbody2D pRig2D;
    SpriteRenderer sp;



    //벽 체크용
    public Transform wallChk; //벽 체크용 기준 위치
    public float wallchkDistance; // 벽 체크 레이의 길이(거리)
    public LayerMask wLayer;  //벽으로 인식할 레이어 마스크
    bool isWall; //현재 벽에 붙어있는지 여부 플래그
    public float slidingSpeed; //벽에 붙었을때 적용할 미끄럼 속도 계수
    public float wallJumpPower; //벽점프에 사용할 추진력
    public bool isWallJump; //벽점프 진행 중인지 여부 플래그
    float isRight = 1;      //플레이어가 바라보는 방향(오른쪽: 1, 왼쪽 : -1)

    //점프먼지
    public GameObject Jdust;

    //벽점프 먼지
    public GameObject walldust;    



    void Start()
    {
        pAnimator = GetComponent<Animator>(); //애니메이터 컴포넌트 가져오기
        pRig2D = GetComponent<Rigidbody2D>(); //리지드바디2D 컴포넌트 가져오기
        sp = GetComponent<SpriteRenderer>(); //스프라이트렌더러 컴포넌트 가져오기
        direction = Vector2.zero; //방향 초기화
    }

    void KeyInput()
    {
        direction.x = Input.GetAxisRaw("Horizontal"); // -1 0 1

        if(direction.x <0)
        {
            //left
            sp.flipX = true;
            pAnimator.SetBool("Run", true);

            //방향 플래그
            isRight = -1;

            //그림자 뒤집기
            for(int i =0; i<sh.Count; i++)
            {
                sh[i].GetComponent<SpriteRenderer>().flipX = sp.flipX;
            }


        }
        else if(direction.x >0)
        {
            //right
            sp.flipX = false;
            pAnimator.SetBool("Run", true);

            //방향 플래그
            isRight = 1;

            //그림자 뒤집기
            for (int i = 0; i < sh.Count; i++)
            {
                sh[i].GetComponent<SpriteRenderer>().flipX = sp.flipX;
            }

        }
        else if(direction.x == 0)
        {
            pAnimator.SetBool("Run", false);

            //그림자 리스트 제거
            for(int i =0; i<sh.Count; i++)
            {
                Destroy(sh[i]); //게임오브젝트파괴
                sh.RemoveAt(i); //리스트에서 제거
            }
        }

        if(Input.GetMouseButtonDown(0)) //왼쪽 마우스 버튼 클릭
        {
            pAnimator.SetTrigger("Attack"); //공격 애니메이션 트리거 설정
            Instantiate(hit_lazer, transform.position, Quaternion.identity); //레이저 공격 오브젝트 생성
        }





    }




  
    void Update()
    {

        //LeftShift로 슬로우모션 활성화
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            TimeController.Instance.SetSlowMotion(true);
        }




        KeyInput();
        Move();


      

        //벽여부 검사
        isWall = Physics2D.Raycast(wallChk.position, Vector2.right * isRight, wallchkDistance, wLayer);//wall체크 레이 발사 및 충돌판정
        pAnimator.SetBool("Grab", isWall); //벽 잡기(Grab) 애니메이션 상태 설정


        if (Input.GetKeyDown(KeyCode.W))
        {
            //점프 입력 (착지 상태일 때만)
            if (pAnimator.GetBool("Jump") == false)
            {
                Jump();

                if(!isWall)
                {
                    Instantiate(Jdust, transform.position, Quaternion.identity);
                    Debug.Log("점프 이펙트 생성 완료");
                }
            }
        }



        if (isWall)
        {
            isWallJump = false; //벽에 붙으면 벽점프 상태 초기화
            //벽에 붙어 있을 때 미끄럼 속도 적용
            pRig2D.linearVelocity = new Vector2(pRig2D.linearVelocityX, pRig2D.linearVelocityY * slidingSpeed); //수직 속도 감속 적용(미끄럼)
            //벽에서 점프 입력 처리
            if(Input.GetKeyDown(KeyCode.W))
            {
                isWallJump = true; //벽점프 상태 활성화
                //벽점프 이펙트 생성
                GameObject go = Instantiate(walldust, transform.position + new Vector3(-0.35f* isRight, 0, 0), Quaternion.identity);
                go.GetComponent<SpriteRenderer>().flipX = sp.flipX; //이펙트 스프라이트 방향 맞춤

                Invoke("FreezeX", 0.3f); //일정 시간후 해제호출
                //벽점프 추진력 적용
                pRig2D.linearVelocity = new Vector2(-isRight * wallJumpPower, 0.9f * wallJumpPower); //수평은 반대방향,수직은 위쪽으로힘 적용

                //방향 전환
                sp.flipX = sp.flipX == false ? true : false; //플레이어 스프라이트 좌우 반전
                isRight = -isRight; //바라보는 방향값 반전

            }
           
        }    

    }

    void FreezeX()
    {
        isWallJump = false;
    }

    //벽 체크 레이 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(wallChk.position, Vector2.right * isRight * wallchkDistance); //씬 뷰에서 벽 체크 레이 시각화
    }




    public  void Jump()
    {
        //점프 전 속도 초기화
        pRig2D.linearVelocity = Vector2.zero;

        //점프 힘 적용
        pRig2D.AddForce(new Vector2(0, jumpUp), ForceMode2D.Impulse);
    }

    void Move()
    {
        //이동
        transform.position += direction * speed * Time.deltaTime;

        //pRig2D.linearVelocity = direction * speed;

    }

    private const float GROUND_CHECK_DISTANCE = 0.7f;
    private void FixedUpdate()
    {
        Debug.DrawRay(pRig2D.position, Vector3.down, new Color(0, GROUND_CHECK_DISTANCE, 0));

        //바닥 레이어로 레이캐스트
        RaycastHit2D rayHit = Physics2D.Raycast(pRig2D.position, Vector3.down, GROUND_CHECK_DISTANCE, LayerMask.GetMask("Ground"));

        CheckGroundedState(rayHit);
    }

    void CheckGroundedState(RaycastHit2D rayHit)
    {
        //그라운드체크
        bool isGrounded = rayHit.collider != null && rayHit.distance < GROUND_CHECK_DISTANCE;

        
        if(isGrounded)
        {
            //착지 상태
            pAnimator.SetBool("Jump", false);

          
        }
        else
        {
            //공중상태 벽체크로직나올예정
            if(!isWall)
            {
                pAnimator.SetBool("Jump", true);
            }
            else
            {
                //벽에 붙은 상태
                pAnimator.SetBool("Grab", true);
            }
        

        }



    }



    //달리기/효과용 먼지 생성
    public void LandDust(GameObject dust)
    {
        Instantiate(dust, transform.position + new Vector3(-0.114f, -0.467f, 0), Quaternion.identity);
    }

    public void AttSlash()
    {
        if (sp.flipX == false)
        {
            pRig2D.AddForce(Vector2.right * power, ForceMode2D.Impulse);
            GameObject go = Instantiate(slash, transform.position, Quaternion.identity);
           // go.GetComponent<SpriteRenderer>().flipX = sp.flipX;
        }
        else
        {
            pRig2D.AddForce(Vector2.left * power, ForceMode2D.Impulse);
            GameObject go = Instantiate(slash, transform.position, Quaternion.identity);
            //go.GetComponent<SpriteRenderer>().flipX = sp.flipX;
        }

    }

    // 그림자 생성
    public void RunShadow()
    {
        // 최대 6개의 그림자만 생성
        if (sh.Count < 6)
        {
            //그림자생성
            GameObject go = Instantiate(Shadow1, transform.position, Quaternion.identity);
            //그림자 속도 설정
            go.GetComponent<Shadow>().TwSpeed = 10 - sh.Count;
            //리스트에 추가
            sh.Add(go);
        }
    }









}

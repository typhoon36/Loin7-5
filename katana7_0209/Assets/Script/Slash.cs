using UnityEngine;

public class Slash : MonoBehaviour
{
    private GameObject p;// 플레이어 오브젝트
    Vector2 MousePos; //    마우스 위치
    Vector3 dir; //    플레이어와 마우스 위치의 방향 벡터

    float angle; //    회전 각도
    Vector3 dirNo;// 단위 방향 벡터

    public Vector3 direction = Vector3.right; // 기본 방향 벡터


    void Start()
    {
        p = GameObject.FindGameObjectWithTag("Player");//플레이어찾기

        Transform tr = p.GetComponent<Transform>(); //트랜스폼가져오기
        MousePos = Input.mousePosition; //마우스포지션
        MousePos = Camera.main.ScreenToWorldPoint(MousePos);
        Vector3 Pos = new Vector3(MousePos.x, MousePos.y, 0);
        dir = Pos - tr.position;   //A - B

        //바라보는 각도 구하기
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;



    }

  
    void Update()
    {
        //회전적용
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = p.transform.position;
    }


    public void Des()
    {
        Destroy(gameObject);
    }













}

using UnityEngine;

public class Slash : MonoBehaviour
{
     GameObject p;// 플레이어 오브젝트
    Vector2 MousePos; //    마우스 위치
    Vector3 dir; //    플레이어와 마우스 위치의 방향 벡터

    float angle; //    회전 각도
    Vector3 dirNo;// 단위 방향 벡터

    public Vector3 direction = Vector3.right; // 기본 방향 벡터

     void Start()
    {
        p = GameObject.FindWithTag("Player");
        Transform tr = GetComponent<Transform>();
        MousePos = Input.mousePosition;
        MousePos = Camera.main.ScreenToWorldPoint(MousePos);
        Vector3 Dir = new Vector3(MousePos.x, MousePos.y, 0);
        dir = Dir - tr.transform.position;

        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
     void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.position = p.transform.position;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }


}

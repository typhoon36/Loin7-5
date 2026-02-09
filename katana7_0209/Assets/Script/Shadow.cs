using UnityEngine;

public class Shadow : MonoBehaviour
{
    private GameObject player; // 플레이어 오브젝트
    public float TwSpeed = 10;// 그림자가 플레이어에게 다가가는 속도
    void Start()
    {

    }


    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");//플레이어찾기
        //그림자가 플레이어에게 다가가기
        transform.position = Vector3.Lerp(transform.position, player.transform.position, TwSpeed * Time.deltaTime);


    }
}

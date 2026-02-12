using UnityEngine;

public enum PlayerState
{
    Idle,  //0
    Running,  //1
    Jumping,  //2
    Attacking  //3
}




public class Player : MonoBehaviour
{
    public PlayerState playerstate;

    //enum = 문자열 대신 쓰는 안전한 상태 값
    void Start()
    {
        if(playerstate == PlayerState.Attacking)
        {
            Debug.Log("Player is attacking!");
        }
    }

   
    
}

using UnityEngine;

public class Shadow : MonoBehaviour
{
    private GameObject player;
    public float TwSpeed = 10;
    void Start()
    {

    }


    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        transform.position = Vector3.Lerp(transform.position, player.transform.position, TwSpeed * Time.deltaTime);
    }
}

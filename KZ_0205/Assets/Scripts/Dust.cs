using UnityEngine;

public class Dust : MonoBehaviour
{
    public float lifetime = 0.5f;


     void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}

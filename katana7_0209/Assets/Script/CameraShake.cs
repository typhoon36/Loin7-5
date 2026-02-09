using DG.Tweening;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeStrength = 0.3f;
    [SerializeField] int shakeVibrato = 10;

    void Start()
    {
        Shake();
    }

    public void Shake()
    {
        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
    }
}

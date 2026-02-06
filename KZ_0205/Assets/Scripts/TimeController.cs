using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeController : MonoBehaviour
{
    private static TimeController instance;
    public static TimeController Instance => instance;

    public float slowMotionTimeScale = 0.3f;
    public float slowMotionDuration = 0.5f;

    private float slowMotionTimer;
    public bool isSlowMotion { get; private set; }

    [Header("Post Processing (URP)")]
    public Volume volume;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        // URP Volume에서 효과 가져오기
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
    }

    void Update()
    {
        if (isSlowMotion)
        {
            slowMotionTimer += Time.unscaledDeltaTime;

            if (slowMotionTimer >= slowMotionDuration)
            {
                SetSlowMotion(false);
            }
        }
    }

    public float GetTimeScale()
    {
        return isSlowMotion ? slowMotionTimeScale : 1f;
    }

    public void SetSlowMotion(bool slow)
    {
        isSlowMotion = slow;

        if (slow)
        {
            slowMotionTimer = 0f;

            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // 🎬 슬로우 모션 시 연출
            vignette.intensity.value = 0.6f;

            colorAdjustments.saturation.value = -40f;
            colorAdjustments.contrast.value = 20f;
            colorAdjustments.postExposure.value = -1f;
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // 효과 원복
            vignette.intensity.value = 0f;

            colorAdjustments.saturation.value = 0f;
            colorAdjustments.contrast.value = 0f;
            colorAdjustments.postExposure.value = 0f;
        }
    }
}

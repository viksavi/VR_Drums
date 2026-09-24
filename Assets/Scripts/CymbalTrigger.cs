using UnityEngine;
using System.Collections;

public class CymbalTrigger : MonoBehaviour
{
    [Header("Sound")]
    public AudioSource cymbalAudio;
    public AudioClip cymbalClip;
    [Tooltip("Scales the incoming force value (0-1) before using it as volume.")]
    [Range(0f, 1f)] public float volumeMultiplier = 1f;

    [Tooltip("Minimum force (0-1) required to trigger a hit at all. 0.25 = 25%.")]
    [Range(0f, 1f)] public float forceThreshold = 0.25f;

    [Header("Hit animation")]
    [Tooltip("The pivot transform that swings the stick down onto the cymbal.")]
    public Transform stickPivot;
    public Vector3 restLocalEuler = Vector3.zero;
    public Vector3 hitLocalEuler = new Vector3(30f, 0f, 0f);
    public float downDuration = 0.08f;
    public float upDuration = 0.15f;

    [Header("Lighting")]
    [Tooltip("The cymbal's light still reacts to the actual force value (unlike the main drum's).")]
    public ForceLightController[] linkedLights;

    private Coroutine hitRoutine;

    void OnEnable()
    {
        UDPForceReceiver.OnForceReceived += HandleForceReceived;
    }

    void OnDisable()
    {
        UDPForceReceiver.OnForceReceived -= HandleForceReceived;
    }

    private void HandleForceReceived(float force)
    {
        if (force < forceThreshold) return; // too weak to count as a hit at all

        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(PlayHit(force));
    }

    private IEnumerator PlayHit(float force)
    {
        Quaternion restRot = Quaternion.Euler(restLocalEuler);
        Quaternion hitRot = Quaternion.Euler(hitLocalEuler);

        float t = 0f;
        while (t < downDuration)
        {
            t += Time.deltaTime;
            if (stickPivot != null)
                stickPivot.localRotation = Quaternion.Slerp(restRot, hitRot, t / downDuration);
            yield return null;
        }
        if (stickPivot != null) stickPivot.localRotation = hitRot;

        if (cymbalAudio != null && cymbalClip != null)
            cymbalAudio.PlayOneShot(cymbalClip, force * volumeMultiplier);

        if (linkedLights != null)
        {
            foreach (var light in linkedLights)
            {
                if (light != null) light.SetForceSnapshot(force);
            }
        }

        t = 0f;
        while (t < upDuration)
        {
            t += Time.deltaTime;
            if (stickPivot != null)
                stickPivot.localRotation = Quaternion.Slerp(hitRot, restRot, t / upDuration);
            yield return null;
        }
        if (stickPivot != null) stickPivot.localRotation = restRot;
    }
}
using UnityEngine;
using System.Collections;

public class DrumHandTrigger : MonoBehaviour
{
    public AudioSource stickAudio;
    [Range(0f, 1f)] public float fallbackVolume = 0.8f;
    public float cooldownSeconds = 0.25f;

    public Transform stickPivot;
    public Vector3 restLocalEuler = Vector3.zero;
    public Vector3 hitLocalEuler = new Vector3(30f, 0f, 0f);
    public float downDuration = 0.08f;
    public float upDuration = 0.15f;
    public ForceLightController[] linkedLights;

    private float lastHitTime = -999f;
    private Coroutine hitRoutine;

    public void OnFist()
    {
        if (Time.time - lastHitTime < cooldownSeconds) return;
        lastHitTime = Time.time;

        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(PlayHit());
    }

    private IEnumerator PlayHit()
    {
        Quaternion restRot = Quaternion.Euler(restLocalEuler);
        Quaternion hitRot = Quaternion.Euler(hitLocalEuler);

        // Swing down to the drum.
        float t = 0f;
        while (t < downDuration)
        {
            t += Time.deltaTime;
            if (stickPivot != null)
                stickPivot.localRotation = Quaternion.Slerp(restRot, hitRot, t / downDuration);
            yield return null;
        }
        if (stickPivot != null) stickPivot.localRotation = hitRot;

        // Play the sound right as the stick reaches the drum.
        if (stickAudio != null)
        {
            if (stickAudio.clip != null)
                stickAudio.PlayOneShot(stickAudio.clip, fallbackVolume);
        }

        // Snapshot the current force value into every linked light, right at the hit.
        float currentForce = UDPForceReceiver.CurrentForce;
        if (linkedLights != null)
        {
            foreach (var light in linkedLights)
            {
                if (light != null)
                    light.SetForceSnapshot(currentForce);
            }
        }

        // Swing back up to rest.
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
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class StickCollisionHit : MonoBehaviour
{
    [Header("References")]
    public XRGrabInteractable grabInteractable;
    public AudioSource stickAudio;
    public ForceLightController[] linkedLights;

    [Header("Drum hit")]
    public AudioClip drumHitClip;
    [Range(0f, 1f)] public float fallbackVolume = 0.8f;
    public float drumCooldownSeconds = 0.25f;

    [Header("Stick-to-stick clack")]
    public AudioClip stickClackClip;
    [Tooltip("Swing speed (m/s) that maps to full clack volume.")]
    public float clackMaxSpeed = 3f;
    public float clackCooldownSeconds = 0.15f;

    private Rigidbody rb;
    private float lastDrumHitTime = -999f;
    private float lastClackTime = -999f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Drum hit — only counts while actively held.
        if (collision.gameObject.CompareTag("Drum"))
        {
            if (Time.time - lastDrumHitTime < drumCooldownSeconds) return;
            if (grabInteractable == null || !grabInteractable.isSelected) return;

            lastDrumHitTime = Time.time;

            if (stickAudio != null && drumHitClip != null)
                stickAudio.PlayOneShot(drumHitClip, fallbackVolume);

            if (linkedLights != null)
            {
                foreach (var lightController in linkedLights)
                {
                    if (lightController != null) lightController.ToggleBetweenMinMax();
                }
            }
            return;
        }

        // Stick-to-stick clack — scaled by how fast they collided.
        if (collision.gameObject.CompareTag("Stick"))
        {
            if (Time.time - lastClackTime < clackCooldownSeconds) return;
            lastClackTime = Time.time;

            float speed = collision.relativeVelocity.magnitude;
            float volume = Mathf.Clamp01(speed / clackMaxSpeed) * 0.2f;

            if (stickAudio != null && stickClackClip != null)
                stickAudio.PlayOneShot(stickClackClip, volume);
        }
    }
}
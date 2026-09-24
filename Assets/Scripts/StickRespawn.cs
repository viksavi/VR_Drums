using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class StickRespawner : MonoBehaviour
{
    [Tooltip("The stick's own XR Grab Interactable, so we don't respawn it mid-swing while held.")]
    public XRGrabInteractable grabInteractable;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Floor")) return;
        if (grabInteractable != null && grabInteractable.isSelected) return; // don't respawn while held

        Respawn();
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FistGrabController : MonoBehaviour
{
    [Tooltip("This hand's Near-Far Interactor.")]
    public NearFarInteractor interactor;

    [Tooltip("A live-tracked transform near the hand to search around when the fist closes. Pinch Grab Pose works fine here even though it drifts during curl, since we only sample it once at the moment of the gesture.")]
    public Transform detectionAnchor;

    [Tooltip("How far around the detection anchor to search for a grabbable stick.")]
    public float detectionRadius = 0.15f;

    [Tooltip("Optional: restrict detection to a specific layer (e.g. a 'Sticks' layer) so a fist near the drum/table doesn't accidentally grab something else. Leave as Everything if you haven't set up layers for this.")]
    public LayerMask detectionLayerMask = ~0;

    [Tooltip("How long the fist has to genuinely open before actually releasing, to ignore brief one-frame tracking flicker while gripping.")]
    public float releaseDelay = 0.15f;

    private Coroutine pendingRelease;

    // Call from Static Hand Gesture's "Gesture Performed" event.
    public void OnFistGrabStart()
    {
        // A grab is happening again, so cancel any pending release from a flicker.
        if (pendingRelease != null)
        {
            StopCoroutine(pendingRelease);
            pendingRelease = null;
        }

        if (interactor == null)
        {
            Debug.LogWarning("[FistGrab] Interactor field is not assigned!");
            return;
        }
        if (detectionAnchor == null)
        {
            Debug.LogWarning("[FistGrab] Detection Anchor field is not assigned!");
            return;
        }
        if (interactor.hasSelection)
        {
            Debug.Log("[FistGrab] Ignored — already holding something.");
            return; // already holding something
        }

        Collider[] nearby = Physics.OverlapSphere(detectionAnchor.position, detectionRadius, detectionLayerMask);
        Debug.Log($"[FistGrab] OverlapSphere found {nearby.Length} collider(s) within {detectionRadius}m of {detectionAnchor.name}.");

        XRGrabInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var col in nearby)
        {
            var grabbable = col.GetComponentInParent<XRGrabInteractable>();
            Debug.Log($"[FistGrab]  - Collider '{col.name}' -> XRGrabInteractable found: {grabbable != null}");
            if (grabbable == null) continue;

            float dist = Vector3.Distance(detectionAnchor.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = grabbable;
            }
        }

        if (closest != null)
        {
            Debug.Log($"[FistGrab] Calling StartManualInteraction on '{closest.name}'.");
            interactor.StartManualInteraction(closest as IXRSelectInteractable);
        }
        else
        {
            Debug.Log("[FistGrab] No valid XRGrabInteractable found nearby — nothing to grab.");
        }
    }

    // Call from Static Hand Gesture's "Gesture Ended" event.
    public void OnFistGrabEnd()
    {
        if (interactor == null || !interactor.hasSelection) return;

        if (pendingRelease != null) StopCoroutine(pendingRelease);
        pendingRelease = StartCoroutine(ReleaseAfterDelay());
    }

    private IEnumerator ReleaseAfterDelay()
    {
        yield return new WaitForSeconds(releaseDelay);
        if (interactor != null && interactor.hasSelection)
            interactor.EndManualInteraction();
        pendingRelease = null;
    }
}
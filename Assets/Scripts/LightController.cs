using UnityEngine;

[RequireComponent(typeof(Light))]
public class ForceLightController : MonoBehaviour
{
    public Color lowForceColor = Color.blue;
    public Color highForceColor = Color.red;
    public float minIntensity = 1f;
    public float maxIntensity = 5f;

    [Tooltip("How quickly the light eases into a new snapshot value. Higher = snappier.")]
    public float transitionSpeed = 8f;

    private Light lightComponent;
    private float targetForce = 0f; // 0-1, only updated by SetForceSnapshot()

    void Awake()
    {
        lightComponent = GetComponent<Light>();
        // Start at rest (0 force) so the light has a sensible default before the first hit.
        lightComponent.color = lowForceColor;
        lightComponent.intensity = minIntensity;
    }
    public void SetForceSnapshot(float force)
    {
        targetForce = Mathf.Clamp01(force);
    }

    private bool isAtHigh = false;
    public void ToggleBetweenMinMax()
    {
        isAtHigh = !isAtHigh;
        targetForce = isAtHigh ? 1f : 0f;
    }

    void Update()
    {
        Color targetColor = Color.Lerp(lowForceColor, highForceColor, targetForce);
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, targetForce);

        lightComponent.color = Color.Lerp(lightComponent.color, targetColor, Time.deltaTime * transitionSpeed);
        lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }
}
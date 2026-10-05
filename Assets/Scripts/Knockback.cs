using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class KnockbackObstacle : MonoBehaviour
{
    [Header("Dependencies (Optional)")]
    [Tooltip("If left empty, script defaults to Max Force. If assigned or found in parent, force scales with spin speed.")]
    [SerializeField] private SpinnerHazard spinner;

    [Header("Knockback Settings")]
    [SerializeField] private float maxKnockbackForce = 20f;
    [SerializeField] private float maxUpwardModifier = 2.5f;
    public float duration;

    void Start()
    {
        // Try to find the spinner on the parent if it wasn't manually assigned
        if (spinner == null)
        {
            spinner = GetComponentInParent<SpinnerHazard>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody targetRb = collision.gameObject.GetComponent<Rigidbody>();
        if (targetRb == null) return;

        float speedMultiplier = (spinner != null) ? spinner.SpeedPercentage : 1f;

        if (spinner != null && speedMultiplier <= 0.05f) return;

        Vector3 pushDirection = -collision.contacts[0].normal;

        pushDirection.y += maxUpwardModifier * speedMultiplier;
        pushDirection = pushDirection.normalized;

        float finalForce = maxKnockbackForce * speedMultiplier;
        Vector3 impulse = pushDirection * finalForce;

        var kart = targetRb.GetComponent<ArcadeKart>(); // your controller's class name
        if (kart != null) kart.ApplyKnockback(impulse, duration);
        else targetRb.AddForce(impulse, ForceMode.Impulse);
    }
/*
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody targetRb = other.GetComponent<Rigidbody>();
        if (targetRb == null) return;

        // Fallback: If spinner exists, use its speed percentage. Otherwise, default to 100% force.
        float speedMultiplier = (spinner != null) ? spinner.SpeedPercentage : 1f;

        if (spinner != null && speedMultiplier <= 0.05f) return;

        // Calculate direction from center out
        Vector3 pushDirection = other.transform.position - transform.position;

        pushDirection.y += maxUpwardModifier * speedMultiplier;
        pushDirection = pushDirection.normalized;

        float finalForce = maxKnockbackForce * speedMultiplier;
        targetRb.AddForce(pushDirection * finalForce, ForceMode.Impulse);
    }*/
}

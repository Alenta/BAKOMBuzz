using UnityEngine;

public class SpinnerHazard : MonoBehaviour
{
    public enum SpinState { Idle, RampUp, Spin, RampDown }

    [Header("Target")]
    [SerializeField] private Transform targetTransform;

    [Header("Speed Settings")]
    [SerializeField] private float maxSpinSpeed = 720f; // Degrees per second
    [SerializeField] private float rampUpDuration = 2.0f;
    [SerializeField] private float spinDuration = 5.0f;
    [SerializeField] private float rampDownDuration = 2.0f;
    [SerializeField] private float idleDuration = 3.0f;

    private SpinState currentState = SpinState.Idle;
    private float stateTimer = 0f;
    private float currentSpeed = 0f;

    // Expose the current speed percentage (0.0 to 1.0) to other scripts
    public float SpeedPercentage => maxSpinSpeed > 0 ? Mathf.Abs(currentSpeed / maxSpinSpeed) : 0f;


    void Start()
    {
        // Default to self if no transform is assigned
        if (targetTransform == null)
        {
            targetTransform = this.transform;
        }

        ResetState(SpinState.Idle);
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case SpinState.Idle:
                currentSpeed = 0f;
                if (stateTimer >= idleDuration)
                {
                    ResetState(SpinState.RampUp);
                }
                break;

            case SpinState.RampUp:
                // Smoothly interpolate speed from 0 to max
                float rampUpProgress = Mathf.Clamp01(stateTimer / rampUpDuration);
                currentSpeed = Mathf.Lerp(0f, maxSpinSpeed, rampUpProgress);

                if (stateTimer >= rampUpDuration)
                {
                    ResetState(SpinState.Spin);
                }
                break;

            case SpinState.Spin:
                currentSpeed = maxSpinSpeed;
                if (stateTimer >= spinDuration)
                {
                    ResetState(SpinState.RampDown);
                }
                break;

            case SpinState.RampDown:
                // Smoothly interpolate speed from max to 0
                float rampDownProgress = Mathf.Clamp01(stateTimer / rampDownDuration);
                currentSpeed = Mathf.Lerp(maxSpinSpeed, 0f, rampDownProgress);

                if (stateTimer >= rampDownDuration)
                {
                    ResetState(SpinState.Idle);
                }
                break;
        }

        // Apply the continuous rotation around the Y-axis
        targetTransform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime, Space.Self);
    }

    private void ResetState(SpinState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }
}

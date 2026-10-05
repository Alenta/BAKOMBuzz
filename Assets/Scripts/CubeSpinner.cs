using KartGame.KartSystems;
using UnityEngine;
using static KartGame.KartSystems.ArcadeKart;

public class CubeSpinner : MonoBehaviour
{
    public Transform cube;
    public Transform bobOnly;
    [Header("Spin")]
    [SerializeField] Vector3 spinSpeed = new Vector3(30f, 90f, 0f); // degrees/sec

    [Header("Bob")]
    [SerializeField] float bobHeight = 0.25f;
    [SerializeField] float bobSpeed = 2f;

    [Header("Pickup")]
    [SerializeField] string requiredTag = "Player";
    [SerializeField] bool respawn = true;
    [SerializeField] float respawnDelay = 3f;

    Vector3 startPos;
    Vector3 bobOnlyStart;
    float phaseOffset;
    public ParticleSystem ps;

    void Start()
    {
        startPos = cube.transform.position;
        if(bobOnly)
           bobOnlyStart = bobOnly.transform.position;
        phaseOffset = Random.value * Mathf.PI * 2f; // desyncs multiple pickups
    }

    void Update()
    {
        cube.transform.Rotate(spinSpeed * Time.deltaTime, Space.Self);
        
        float y = Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobHeight;
        if(bobOnly)
           bobOnly.transform.position = bobOnlyStart + Vector3.up * y;
        cube.transform.position = startPos + Vector3.up * y;
    }

    [Header("Pickup")]
    [SerializeField] StatPowerup powerup;   
    void OnTriggerEnter(Collider other)
    {
        var kart = other.GetComponentInParent<ArcadeKart>();
        if (kart == null) return;

        kart.AddPowerup(new StatPowerup
        {
            modifiers = powerup.modifiers,
            PowerUpID = powerup.PowerUpID,
            MaxTime = powerup.MaxTime,
            ElapsedTime = 0f
        });
        ps.Play();
        SetActive(false);
        if (respawn) Invoke(nameof(Respawn), respawnDelay);
    }
    void Respawn() => SetActive(true);

    // Toggle renderer + collider instead of the GameObject so Invoke still runs
    void SetActive(bool active)
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = active;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = active;
    }
}
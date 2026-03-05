using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public enum SizeClass
    {
        Big,
        Medium,
        Small
    }

    [Header("General")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float torqueForce = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private float explosionRadius = 2f;

    [SerializeField] private SizeClass sizeClass;
    private bool initialized = false;
    private bool hasBroken = false;

    private GameManager game; // reference to GameManager

    public void Initialize(SizeClass newSize)
    {
        // Set asteroid size and apply corresponding scale
        sizeClass = newSize;
        initialized = true;
        ApplySize();
        // Rename asteroid to match its current size class
        gameObject.name = $"Asteroid_{sizeClass}";
    }

    void Start()
    {
        // Find GameManager in scene to track asteroids
        game = FindFirstObjectByType<GameManager>();

        // Prevent Y-axis movement, keep on 2D plane
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        if (!initialized)
        {
            // Randomize size if not initialized externally
            sizeClass = (SizeClass)Random.Range(0, 3);
            ApplySize();
            gameObject.name = $"Asteroid_{sizeClass}";
        }

        // Apply random movement direction
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0f;
        randomDirection.Normalize();
        rb.AddForce(randomDirection * moveForce, ForceMode.Impulse);

        // Apply random rotation for tumbling
        rb.AddTorque(Random.onUnitSphere * torqueForce, ForceMode.Impulse);
    }

    void ApplySize()
    {
        // Scale asteroid based on its size class
        switch (sizeClass)
        {
            case SizeClass.Big:
                transform.localScale = Vector3.one * 2f;
                break;
            case SizeClass.Medium:
                transform.localScale = Vector3.one * 1.5f;
                break;
            case SizeClass.Small:
                transform.localScale = Vector3.one * 1f;
                break;
        }
        // Update name whenever size is applied
        gameObject.name = $"Asteroid_{sizeClass}";
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only react to bullets once
        if (hasBroken) return;
        if (!collision.gameObject.CompareTag("Bullet")) return;

        hasBroken = true;
        Vector3 hitPoint = collision.contacts[0].point;

        // Break asteroid into smaller chunks if applicable
        BreakApart(hitPoint);

        // Inform GameManager this asteroid is gone
        if (game != null)
            game.RemoveAsteroid(gameObject);

        PersistentScoreManager.Instance.AddScoreBySize(sizeClass.ToString());

        Destroy(gameObject);
    }

    void BreakApart(Vector3 hitPoint)
    {
        int spawnCount = 0;
        SizeClass nextSize = SizeClass.Small;

        // Determine number of chunks and next size class
        if (sizeClass == SizeClass.Big)
        {
            spawnCount = 3;
            nextSize = SizeClass.Medium;
        }
        else if (sizeClass == SizeClass.Medium)
        {
            spawnCount = 2;
            nextSize = SizeClass.Small;
        }
        else
        {
            return; // Small asteroids do not break further
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // Random offset for chunk position
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0f;
            Vector3 spawnPos = transform.position + offset;
            spawnPos.y = 0f;

            // Spawn chunk and initialize size
            GameObject chunk = Instantiate(gameObject, spawnPos, Random.rotation);
            Asteroid asteroid = chunk.GetComponent<Asteroid>();
            asteroid.Initialize(nextSize);

            // Reset physics before applying explosion
            Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();
            chunkRb.linearVelocity = Vector3.zero;
            chunkRb.angularVelocity = Vector3.zero;

            // Apply outward force from hit point
            chunkRb.AddExplosionForce(explosionForce, hitPoint, explosionRadius, 0f, ForceMode.Impulse);

            // Track the new chunk in GameManager
            if (game != null)
                game.AddAsteroid(chunk);
        }
    }
}
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

    private GameManager game;
    private AsteroidPickupDropper dropper;

    public void Initialize(SizeClass newSize)
    {
        sizeClass = newSize;
        initialized = true;
        ApplySize();
        gameObject.name = $"Asteroid_{sizeClass}";
    }

    void Awake()
    {
        dropper = GetComponent<AsteroidPickupDropper>();
    }

    void Start()
    {
        game = FindFirstObjectByType<GameManager>();

        rb.constraints = RigidbodyConstraints.FreezePositionY;

        if (!initialized)
        {
            sizeClass = (SizeClass)Random.Range(0, 3);
            ApplySize();
            gameObject.name = $"Asteroid_{sizeClass}";
        }

        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0f;
        randomDirection.Normalize();

        rb.AddForce(randomDirection * moveForce, ForceMode.Impulse);

        rb.AddTorque(Random.onUnitSphere * torqueForce, ForceMode.Impulse);
    }

    void ApplySize()
    {
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

        gameObject.name = $"Asteroid_{sizeClass}";
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBroken) return;
        if (!collision.gameObject.CompareTag("Bullet")) return;

        hasBroken = true;

        // Destroy the bullet immediately so it can't hit spawned chunks
        Destroy(collision.gameObject);

        Vector3 hitPoint = collision.contacts[0].point;

        BreakApart(hitPoint);

        if (game != null)
            game.RemoveAsteroid(gameObject);

        PersistentScoreManager.Instance.AddScoreBySize(sizeClass.ToString());

        if (dropper != null)
            dropper.TryDropPickup(transform.position);

        Destroy(gameObject);
    }

    void BreakApart(Vector3 hitPoint)
    {
        int spawnCount = 0;
        SizeClass nextSize = SizeClass.Small;

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
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0f;

            Vector3 spawnPos = transform.position + offset;
            spawnPos.y = 0f;

            GameObject chunk = Instantiate(gameObject, spawnPos, Random.rotation);

            Asteroid asteroid = chunk.GetComponent<Asteroid>();
            asteroid.Initialize(nextSize);

            Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();

            chunkRb.linearVelocity = Vector3.zero;
            chunkRb.angularVelocity = Vector3.zero;

            chunkRb.AddExplosionForce(
                explosionForce,
                hitPoint,
                explosionRadius,
                0f,
                ForceMode.Impulse
            );

            if (game != null)
                game.AddAsteroid(chunk);
        }
    }
}
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] public float speed = 20f;
    [SerializeField] public float lifetime = 1.5f;

    [Header("Impact Effect")]
    [SerializeField] private GameObject impactParticlePrefab;

    void Start()
    {
        rb.linearVelocity = transform.up * speed;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void SpawnImpactEffect()
    {
        if (impactParticlePrefab != null)
        {
            Instantiate(
                impactParticlePrefab,
                transform.position,
                Quaternion.identity
            );
        }
    }
}
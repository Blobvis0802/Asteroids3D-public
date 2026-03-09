using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float timeAlive = 5f;

    protected GameManager game;

    void Awake()
    {
        game = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        timeAlive -= Time.deltaTime;

        if (timeAlive <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Activate(Player player)
    {
        Destroy(gameObject);
    }
}
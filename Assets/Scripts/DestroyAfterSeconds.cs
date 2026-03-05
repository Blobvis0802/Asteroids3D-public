using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField] private float destroyAfter = 2f;

    void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}
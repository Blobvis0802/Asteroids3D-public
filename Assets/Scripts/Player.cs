using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    public float m_Thrust = 20f;
    public float maxSpeed = 10f;

    private float turnSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    public float shootCooldown = 0.3f;

    private bool mouseSteering;

    [Header("Invincibility Settings")]
    private bool isImmune = false;
    private float immuneTimer = 0f;
    [SerializeField] private float immuneDuration = 3f;
    [SerializeField] private Animator animator;

    [Header("Layers for Invincibility")]
    [SerializeField] private int normalLayer = 6;
    [SerializeField] private int invincibleLayer = 7;

    [Header("Thruster Particle")]
    [SerializeField] private ParticleSystem thruster;

    private float cooldownTimer = 0f;
    private float LInput, RInput, FwInput, BrakeInput;

    private Renderer[] shipRenderers;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blasterClip;

    void Awake()
    {
        shipRenderers = GetComponentsInChildren<Renderer>();
        if (shipRenderers == null || shipRenderers.Length == 0)
            Debug.LogWarning("No renderers found on player or children!");
    }

    void Update()
    {
        // --- Pull latest settings from SettingsManager ---
        if (SettingsManager.Instance != null)
        {
            mouseSteering = SettingsManager.Instance.MouseAim;
            turnSpeed = SettingsManager.Instance.TurnSpeed;
        }

        // --- Input ---
        LInput = Mathf.Max(Keyboard.current.aKey.ReadValue(), Keyboard.current.leftArrowKey.ReadValue());
        RInput = Mathf.Max(Keyboard.current.dKey.ReadValue(), Keyboard.current.rightArrowKey.ReadValue());
        FwInput = Mathf.Max(Keyboard.current.wKey.ReadValue(), Keyboard.current.upArrowKey.ReadValue());
        BrakeInput = Mathf.Max(Keyboard.current.sKey.ReadValue(), Keyboard.current.downArrowKey.ReadValue());

        // --- STOP INPUT IF GAME IS PAUSED ---
        if (Time.timeScale <= 0f)
            return;

        // --- Thruster emission toggle ---
        if (thruster != null)
        {
            var emission = thruster.emission;
            emission.enabled = FwInput > 0f;
        }

        // --- Shooting ---
        cooldownTimer -= Time.deltaTime;

        bool shootInput =
            mouseSteering ?
            Mouse.current.leftButton.wasPressedThisFrame :
            Keyboard.current.spaceKey.wasPressedThisFrame;

        if (shootInput && cooldownTimer <= 0f)
        {
            Shoot();
            cooldownTimer = shootCooldown;
        }

        // --- Invincibility flicker ---
        if (isImmune)
        {
            immuneTimer -= Time.deltaTime;
            float flicker = Mathf.PingPong(Time.time * 10f, 1f);
            float alpha = flicker > 0.5f ? 1f : 0.2f;
            SetRendererAlpha(alpha);

            if (immuneTimer <= 0f)
                EndInvincibility();
        }
    }

    void FixedUpdate()
    {
        // --- Stop movement if paused ---
        if (Time.timeScale <= 0f)
            return;

        if (FwInput > 0f)
            rb.AddForce(transform.forward * m_Thrust);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        if (mouseSteering)
            RotateTowardMouse();
        else
        {
            float turn = (RInput - LInput) * turnSpeed;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn * Time.fixedDeltaTime, 0f));
        }

        if (BrakeInput > 0f)
            rb.linearVelocity *= 0.95f;
    }

    void RotateTowardMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.linearVelocity = bulletSpawnPoint.forward * 20f;

        if (audioSource != null && blasterClip != null)
        {
            audioSource.pitch = Random.Range(0.7f, 1.2f);
            audioSource.PlayOneShot(blasterClip);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid") && !isImmune)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            gm.ReportPlayerHit();
        }
    }

    public void Respawn()
    {
        Vector3 spawnPos = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(spawnPos, 2f, LayerMask.GetMask("Asteroid"));

        if (hits.Length > 0)
        {
            spawnPos += Random.insideUnitSphere * 2f;
            spawnPos.y = 0f;
        }

        transform.position = spawnPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        StartInvincibility();
    }

    private void StartInvincibility()
    {
        isImmune = true;
        immuneTimer = immuneDuration;
        gameObject.layer = invincibleLayer;

        if (animator != null)
            animator.SetBool("Invincible", true);
    }

    private void EndInvincibility()
    {
        isImmune = false;
        gameObject.layer = normalLayer;

        if (animator != null)
            animator.SetBool("Invincible", false);
    }

    private void SetRendererAlpha(float alpha)
    {
        if (shipRenderers == null) return;

        foreach (Renderer r in shipRenderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Color c = mats[i].color;
                mats[i].color = new Color(c.r, c.g, c.b, alpha);
            }
        }
    }

    public bool IsInvincible()
    {
        return isImmune;
    }
}
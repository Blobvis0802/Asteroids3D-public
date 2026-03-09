using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Asteroid Settings")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private int startingAsteroids = 3;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private LayerMask obstacleLayers; // assign player + asteroids

    [Header("Active Asteroids (Live)")]
    [SerializeField] private List<GameObject> spawnedAsteroids = new List<GameObject>();

    [Header("Player Settings")]
    [SerializeField] private Player player;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private GameObject[] lifeIcons;  // UI hartjes of blokjes
    [SerializeField] private string gameOverSceneName = "GameOver";

    private int currentRound = 0;
    private int currentLives;

    void Start()
    {
        currentLives = maxLives;
        UpdateLivesUI();
        StartNewRound();
    }

    public void StartNewRound()
    {
        currentRound++;
        int asteroidsToSpawn = startingAsteroids + currentRound - 1;

        int attempts;
        for (int i = 0; i < asteroidsToSpawn; i++)
        {
            attempts = 0;
            Vector3 spawnPos;
            do
            {
                spawnPos = Random.insideUnitSphere * spawnRadius;
                spawnPos.y = 0f; // keep on ground plane
                attempts++;
            }
            // --- Check obstacles AND distance from player ---
            while ((Physics.OverlapSphere(spawnPos, 1f, obstacleLayers).Length > 0
                    || Vector3.Distance(spawnPos, player.transform.position) < 3f)
                   && attempts < 50);

            GameObject go = Instantiate(asteroidPrefab, spawnPos, Random.rotation);
            spawnedAsteroids.Add(go);
        }

        Debug.Log($"Round {currentRound} started with {asteroidsToSpawn} asteroids!");
    }

    public void RemoveAsteroid(GameObject asteroidToRemove)
    {
        spawnedAsteroids.Remove(asteroidToRemove);

        if (spawnedAsteroids.Count == 0)
        {
            Debug.Log("All asteroids destroyed! Starting new round...");
            StartNewRound();
        }
    }

    public void AddAsteroid(GameObject asteroid)
    {
        spawnedAsteroids.Add(asteroid);
    }

    void Update()
    {
        spawnedAsteroids.RemoveAll(a => a == null);
    }

    // Lives & Player Hit
    public void ReportPlayerHit()
    {
        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            player.Respawn();
        }
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(i < currentLives);
        }
    }

    public void AddLives(int amount)
    {
        currentLives += amount;

        if (currentLives > lifeIcons.Length)
            currentLives = lifeIcons.Length;

        UpdateLivesUI();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Draw a green circle around the player to show the "no spawn zone"
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.transform.position, 3f); // matches the safe distance in code
        }

        // Draw the asteroid spawn radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // semi-transparent orange
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}
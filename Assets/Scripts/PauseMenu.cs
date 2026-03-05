using UnityEngine;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Key Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Panels To Enable When Paused")]
    [SerializeField] private List<GameObject> panelsToEnable;

    [Header("Panels To Disable When Paused")]
    [SerializeField] private List<GameObject> panelsToDisable;

    [Header("Optional Canvases")]
    [SerializeField] private List<Canvas> canvasesToEnable;
    [SerializeField] private List<Canvas> canvasesToDisable;

    [Header("Panels To Automatically Disable When Pause Button Pressed")]
    [SerializeField] private List<GameObject> panelsToForceDisable;

    [Header("Behavior")]
    [SerializeField] private bool pauseTime = true;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            ForceDisablePanels();

            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        ToggleObjects(panelsToEnable, true);
        ToggleObjects(panelsToDisable, false);

        ToggleCanvases(canvasesToEnable, true);
        ToggleCanvases(canvasesToDisable, false);

        if (pauseTime)
            Time.timeScale = 0f;

        isPaused = true;
    }

    public void Resume()
    {
        ToggleObjects(panelsToEnable, false);
        ToggleObjects(panelsToDisable, true);

        ToggleCanvases(canvasesToEnable, false);
        ToggleCanvases(canvasesToDisable, true);

        if (pauseTime)
            Time.timeScale = 1f;

        isPaused = false;
    }

    void ToggleObjects(List<GameObject> list, bool state)
    {
        foreach (GameObject obj in list)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }

    void ToggleCanvases(List<Canvas> list, bool state)
    {
        foreach (Canvas canvas in list)
        {
            if (canvas != null)
                canvas.enabled = state;
        }
    }

    void ForceDisablePanels()
    {
        foreach (GameObject panel in panelsToForceDisable)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    // --- DYNAMIC BUTTON METHODS ---
    public void EnablePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void DisablePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void EnableCanvas(Canvas canvas)
    {
        if (canvas != null)
            canvas.enabled = true;
    }

    public void DisableCanvas(Canvas canvas)
    {
        if (canvas != null)
            canvas.enabled = false;
    }

    public void ToggleCanvas(Canvas canvas)
    {
        if (canvas != null)
            canvas.enabled = !canvas.enabled;
    }

    // --- Change pause key at runtime ---
    public void SetPauseKey(KeyCode newKey)
    {
        pauseKey = newKey;
    }
}
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public float TurnSpeed = 250f;
    public bool MouseAim = false;

    private const string TURN_SPEED_KEY = "TurnSpeed";
    private const string MOUSE_AIM_KEY = "MouseAim";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    // --- Setters ---
    public void SetTurnSpeed(float value)
    {
        TurnSpeed = value;
        PlayerPrefs.SetFloat(TURN_SPEED_KEY, TurnSpeed);
        PlayerPrefs.Save();
    }

    public void SetMouseAim(bool value)
    {
        MouseAim = value;
        PlayerPrefs.SetInt(MOUSE_AIM_KEY, MouseAim ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- Load saved settings ---
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(TURN_SPEED_KEY))
            TurnSpeed = PlayerPrefs.GetFloat(TURN_SPEED_KEY);

        if (PlayerPrefs.HasKey(MOUSE_AIM_KEY))
            MouseAim = PlayerPrefs.GetInt(MOUSE_AIM_KEY) == 1;
    }
}
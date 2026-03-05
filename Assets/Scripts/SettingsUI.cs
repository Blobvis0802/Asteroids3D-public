using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider turnSpeedSlider;
    public Toggle invertMouseToggle;

    void Start()
    {
        if (SettingsManager.Instance == null)
            return;

        // Sync UI with saved values
        turnSpeedSlider.value = SettingsManager.Instance.TurnSpeed;
        invertMouseToggle.isOn = SettingsManager.Instance.MouseAim;

        // Listen for changes
        turnSpeedSlider.onValueChanged.AddListener(OnTurnSpeedChanged);
        invertMouseToggle.onValueChanged.AddListener(OnInvertMouseChanged);
    }

    void OnTurnSpeedChanged(float value)
    {
        SettingsManager.Instance.SetTurnSpeed(value);
    }

    void OnInvertMouseChanged(bool value)
    {
        SettingsManager.Instance.SetMouseAim(value);
    }
}
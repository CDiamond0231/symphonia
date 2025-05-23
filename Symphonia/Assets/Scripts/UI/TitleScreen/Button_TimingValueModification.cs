using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_TimingValueModification : Button_Base
{
    public enum TimingType
    {
        GameStartDelay,
        MusicStartDelay,
        SecondsPerScreenLength,
        MidiTempo,
    }

    public MusicRoadManager MusicRoadManager;
    public UnityEngine.UI.Text TimingTextDisplay;
    public Color TimingTextHighlightColour = new Color(0.667f, 1.0f, 1.0f, 1.0f);

    public TimingType TimingToModify = TimingType.MusicStartDelay;
    public float ValueModificationAmount = 0.1f;

    public float MinValue = 0.0f;
    public float MaxValue = 1000.0f;

    protected override void OnTrigger()
    {
        base.OnTrigger();

        float currentValue = GetCurrentTimingValue();
        float newValue = currentValue + ValueModificationAmount;

        if (newValue < MinValue)
        {
            return;
        }
        if (newValue > MaxValue)
        {
            return;
        }

        SetTimingValue(newValue);
        SetTimingText(newValue);
    }

    protected float GetCurrentTimingValue()
    {
        if (MusicRoadManager == null)
        {
            Debug.LogError($"{MusicRoadManager} has not been defined");
            return -1.0f;
        }

        switch (TimingToModify)
        {
            case TimingType.MusicStartDelay:
                return MusicRoadManager.MusicStartDelayTime;

            case TimingType.GameStartDelay:
                return MusicRoadManager.GameStartDelayTime;

            case TimingType.SecondsPerScreenLength:
                return MusicRoadManager.SecondsPerScreenLength;

            case TimingType.MidiTempo:
                return (float)MusicRoadManager.DesiredMidiTempo;

            default:
                Debug.LogError($"{TimingToModify} has not been defined in {nameof(GetCurrentTimingValue)}");
                return -1.0f;
        }
    }

    protected void SetTimingValue(float newValue)
    {
        if (MusicRoadManager == null)
        {
            Debug.LogError($"{MusicRoadManager} has not been defined");
            return;
        }

        switch (TimingToModify)
        {
            case TimingType.MusicStartDelay:
                MusicRoadManager.MusicStartDelayTime = newValue;
                break;

            case TimingType.GameStartDelay:
                MusicRoadManager.GameStartDelayTime = newValue;
                break;

            case TimingType.SecondsPerScreenLength:
                MusicRoadManager.SecondsPerScreenLength = newValue;
                break;

            case TimingType.MidiTempo:
                MusicRoadManager.DesiredMidiTempo = newValue;
                break;

            default:
                Debug.LogError($"{TimingToModify} has not been defined in {nameof(GetCurrentTimingValue)}");
                return;
        }
    }

    protected void SetTimingText(float newValue)
    {
        if (TimingTextDisplay == null)
        {
            Debug.LogError($"{nameof(TimingTextDisplay)} is not defined.");
            return;
        }

        string rHex = ((int)(TimingTextHighlightColour.r * 255.0f)).ToString("X2");
        string gHex = ((int)(TimingTextHighlightColour.g * 255.0f)).ToString("X2");
        string bHex = ((int)(TimingTextHighlightColour.b * 255.0f)).ToString("X2");

        string timingType;
        switch (TimingToModify)
        {
            case TimingType.MusicStartDelay:
            case TimingType.GameStartDelay:
            case TimingType.SecondsPerScreenLength:
                timingType = "Seconds";
                break;

            case TimingType.MidiTempo:
                timingType = "BPM";
                break;

            default:
                timingType = "Undefined";
                break;
        }

        TimingTextDisplay.text = $"<color=#{rHex}{gHex}{bHex}>{newValue.ToString("0.00")}</color> {timingType}";
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        float currentTimingValue = GetCurrentTimingValue();
        SetTimingText(currentTimingValue);
    }
}

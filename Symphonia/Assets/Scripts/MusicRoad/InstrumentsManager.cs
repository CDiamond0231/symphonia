using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstrumentsManager : MonoBehaviour
{
    public MusicRoadManager MusicRoadManager;
    public Button_InstrumentToggle[] InstrumentButtons = new Button_InstrumentToggle[MidiFileConstants.NumberOfChannels];

    protected MidiFileReader m_midiFileReader = null;

    public void OnInstrumentToggled(Button_InstrumentToggle pressedButton)
    {
        if (m_midiFileReader == null)
        {
            return;
        }
        if (m_midiFileReader.IsActiveChannel(pressedButton.ChannelID) == false)
        {
            // Can not make any changes
            return;
        }
        if (MusicRoadManager == null)
        {
            return;
        }

        pressedButton.WillShowInstrument = !pressedButton.WillShowInstrument;
        UpdateButtonColour(pressedButton);

        MusicRoadManager.ToggleChannelNotes(pressedButton.ChannelID, pressedButton.WillShowInstrument);
    }

    public void OnMusicRoadGenerated(MidiFileReader midiReader)
    {
        m_midiFileReader = midiReader;
        if (m_midiFileReader == null)
        {
            return;
        }

        for (int channelID = 0; channelID < MidiFileConstants.NumberOfChannels; ++channelID)
        {
            Button_InstrumentToggle instrumentButton = InstrumentButtons[channelID];
            if (instrumentButton == null)
            {
                continue;
            }

            instrumentButton.WillShowInstrument = MusicRoadManager.CheckIfChannelIsActive(channelID); ;
            UpdateButtonColour(instrumentButton);

            instrumentButton.TextLabel = m_midiFileReader.GetChannelName(channelID);
        }
    }

    protected void UpdateButtonColour(Button_InstrumentToggle instrumentButton)
    {
        if (m_midiFileReader == null)
        {
            return;
        }
        if (MusicRoadManager == null)
        {
            return;
        }

        SpriteRenderer sprRend = instrumentButton.SprRenderer;
        if (sprRend == null)
        {
            return;
        }

        if (m_midiFileReader.IsActiveChannel(instrumentButton.ChannelID) == false)
        {
            Color colour = Color.black;
            colour.a = 1.0f;
            sprRend.color = colour;
            return;
        }

        Color channelColour = MusicRoadManager.ChannelNotesColours[instrumentButton.ChannelID];
        if (instrumentButton.WillShowInstrument == false)
        {
            channelColour.a = 0.5f;
        }
        else
        {
            channelColour.a = 1.0f;
        }
        sprRend.color = channelColour;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

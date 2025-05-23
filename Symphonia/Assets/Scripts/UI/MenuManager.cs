using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public SceneFadeEffect AudioSelectionOptions;
    public SceneFadeEffect TimingSelectionOptions;
    public SceneFadeEffect InstrumentSelectionOptions;

    public enum TabWindows
    {
        AudioSelectionOptions,
        TimingSelectionOptions,
        InstrumentSelectionOptions,

        COUNT
    }

    public TabWindows CurrentlyOpenedTab { get; protected set; } = TabWindows.COUNT;


    public TabWindows OnTabWindowButtonClicked(TabWindows windowToToggle)
    {
        SceneFadeEffect oldWindow = GetTabWindowFadeEffect(CurrentlyOpenedTab);
        if (oldWindow != null)
        {
            oldWindow.InitiateFadeOut();
        }

        if (CurrentlyOpenedTab == windowToToggle)
        {
            // Close currently opened tab.
            CurrentlyOpenedTab = TabWindows.COUNT;
        }
        else
        {
            CurrentlyOpenedTab = windowToToggle;
            SceneFadeEffect newWindow = GetTabWindowFadeEffect(windowToToggle);

            if (newWindow != null)
            {
                newWindow.InitiateFadeIn();
            }
        }

        return CurrentlyOpenedTab;
    }

    protected SceneFadeEffect GetTabWindowFadeEffect(TabWindows tabWindow)
    {
        switch (tabWindow)
        {
            case TabWindows.AudioSelectionOptions:
                return AudioSelectionOptions;

            case TabWindows.TimingSelectionOptions:
                return TimingSelectionOptions;

            case TabWindows.InstrumentSelectionOptions:
                return InstrumentSelectionOptions;

            default:
                return null;
        }
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

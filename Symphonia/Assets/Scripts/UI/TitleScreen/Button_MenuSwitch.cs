using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_MenuSwitch : Button_Base
{
    public MenuManager MenuManager;
    public MenuManager.TabWindows TabWindowType = MenuManager.TabWindows.AudioSelectionOptions;

    protected override void OnTrigger()
    {
        base.OnTrigger();

        if (MenuManager == null)
        {
            return;
        }

        MenuManager.TabWindows newWindow = MenuManager.OnTabWindowButtonClicked(TabWindowType);
        if (newWindow == TabWindowType)
        {
            // This means we have just opened our tab. So Highlight the button to reflect this.
            ShowPressedSprite();
        }
        else
        {
            // Otherwise we have closed the tab. So set the button back to normal.
            ShowUnpressedSprite();
        }
    }
}

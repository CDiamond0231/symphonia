using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_ExitApplication : Button_Base
{
    protected override void OnTrigger()
    {
        base.OnTrigger();
        Application.Quit();
    }
}

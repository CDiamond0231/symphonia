using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_InstrumentToggle : Button_Base
{
    public InstrumentsManager InstrumentsManager;
    public int ChannelID = 0;

    public bool WillShowInstrument { get; set; } = true;

    protected override void OnTrigger()
    {
        base.OnTrigger();
        InstrumentsManager.OnInstrumentToggled(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
}

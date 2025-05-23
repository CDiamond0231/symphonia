using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MidiKeyboardInputHandler : MonoBehaviour
{
    //// Static Variables /////////////////////////////////////////////////

    //// DLL Import /////////////////////////////////////////////////
    [DllImport("KeyboardInputReader", EntryPoint = "OpenMidiInput", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_InitialiseDeviceInput();

    [DllImport("KeyboardInputReader", EntryPoint = "CloseMidiInput", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_CloseMidiInput();

    [DllImport("KeyboardInputReader", EntryPoint = "IsDeviceConnected", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_IsDeviceConnected();

    [DllImport("KeyboardInputReader", EntryPoint = "GetDeviceName", CallingConvention = CallingConvention.StdCall)]
    private static extern void _Dll_GetDeviceName(byte[] buf, int bufSize);

    [DllImport("KeyboardInputReader", EntryPoint = "Update", CallingConvention = CallingConvention.StdCall)]
    private static extern bool _Dll_Update();

    [DllImport("KeyboardInputReader", EntryPoint = "GetNextTriggeredKey", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetNextTriggeredKey();

    [DllImport("KeyboardInputReader", EntryPoint = "GetNextReleasedKey", CallingConvention = CallingConvention.StdCall)]
    private static extern int _Dll_GetNextReleasedKey();
    /////////////////////////////////////////////////////////////////


    //////////////////// Static Functions /////////////////////////////////////
    public static bool IsDeviceConnected()
    {
        bool isConnected = _Dll_IsDeviceConnected();
        return isConnected;
    }

    public static string GetDeviceName()
    {
        // Device name can only go up to 32 characters. No need to allocate any more than that.
        const int bufSize = 32; 
        byte[] buf = new byte[bufSize];
        _Dll_GetDeviceName(buf, bufSize);
        string deviceName = System.Text.Encoding.ASCII.GetString(buf);
        return deviceName;
    }

    public static int GetNextTriggeredKey()
    {
        int nextKey = _Dll_GetNextTriggeredKey();
        return nextKey;
    }

    public static int GetNextReleasedKey()
    {
        int nextKey = _Dll_GetNextReleasedKey();
        return nextKey;
    }

    //////////////////// Instance Functions /////////////////////////////////////
    MidiKeyboardInputHandler() : base()
    {
        // It's okay to call the 'initialise' function more than once. The DLL ignores it if requested a second time.
        bool result = _Dll_InitialiseDeviceInput();
    }

    protected void Update()
    {
        bool result = _Dll_Update();
    }

    protected void OnApplicationQuit()
    {
        bool result = _Dll_CloseMidiInput();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;


public static class WindowsOpenFileDialog
{
    //// Declarations /////////////////////////////////////////////////
    public const int OFN_READONLY                = 0x00000001;
    public const int OFN_OVERWRITEPROMPT         = 0x00000002;
    public const int OFN_HIDEREADONLY            = 0x00000004;
    public const int OFN_NOCHANGEDIR             = 0x00000008;
    public const int OFN_SHOWHELP                = 0x00000010;
    public const int OFN_ENABLEHOOK              = 0x00000020;
    public const int OFN_ENABLETEMPLATE          = 0x00000040;
    public const int OFN_ENABLETEMPLATEHANDLE    = 0x00000080;
    public const int OFN_NOVALIDATE              = 0x00000100;
    public const int OFN_ALLOWMULTISELECT        = 0x00000200;
    public const int OFN_EXTENSIONDIFFERENT      = 0x00000400;
    public const int OFN_PATHMUSTEXIST           = 0x00000800;
    public const int OFN_FILEMUSTEXIST           = 0x00001000;
    public const int OFN_CREATEPROMPT            = 0x00002000;
    public const int OFN_SHAREAWARE              = 0x00004000;
    public const int OFN_NOREADONLYRETURN        = 0x00008000;
    public const int OFN_NOTESTFILECREATE        = 0x00010000;
    public const int OFN_NONETWORKBUTTON         = 0x00020000;
    public const int OFN_EXPLORER                = 0x00080000;    // new look commdlg
    public const int OFN_NODEREFERENCELINKS      = 0x00100000;
    public const int OFN_LONGNAMES               = 0x00200000;    // force long names for 3.x modules

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    //// Static Variables /////////////////////////////////////////////////
    private static string s_previousMidiFileLocation = null;
    private static string s_previousOverlayFileLocation = null;
    private static string s_previousVideoFileLocation = null;


    //// DLL Import /////////////////////////////////////////////////
    [DllImport("Comdlg32.dll", EntryPoint = "GetOpenFileName", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool _DLL_OpenFileDialogBox([In, Out] OpenFileName ofn);
    /////////////////////////////////////////////////////////////////

    //////////////////// Static Functions /////////////////////////////////////
    public static bool OpenFileDialogBox([In, Out] OpenFileName ofn)
    {
        bool result = _DLL_OpenFileDialogBox(ofn);
        return result;
    }

    public static string OpenFileDialogBox(string title, string filter = "All Files\0*.*\0\0", string initialDir = null)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = filter;
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = initialDir;
        ofn.title = title;
        ofn.flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR;

        if (OpenFileDialogBox(ofn) == false)
        {
            Debug.Log($"{nameof(OpenFileDialogBox)} failed");
            return null;
        }

        return ofn.file;
    }

    public static string OpenMidiFileDialogBox()
    {
        string fileExts = string.Empty;
        foreach (string fileExt in HelperFunctions.AcceptedMidiFileExts)
        {
            if (string.IsNullOrEmpty(fileExts))
            {
                fileExts += $"*{fileExt}";
            }
            else
            {
                fileExts += $";*{fileExt}";
            }
        }

        string initialDir = s_previousMidiFileLocation ?? UnityEngine.Application.dataPath;
        string midiFileLocation = OpenFileDialogBox("Open Midi File", $"Midi Files ({fileExts})\0{fileExts}\0\0", initialDir);
        if (string.IsNullOrEmpty(midiFileLocation))
        {
            return null;
        }

        // Ensuring we start from this same directory next time we browse for a file.
        s_previousMidiFileLocation = System.IO.Path.GetDirectoryName(midiFileLocation);

        return midiFileLocation;
    }

    public static string OpenOverlayTrackFileDialogBox()
    {
        string fileExts = string.Empty;
        foreach (string fileExt in HelperFunctions.AcceptedOverlayFileExts)
        {
            if (string.IsNullOrEmpty(fileExts))
            {
                fileExts += $"*{fileExt}";
            }
            else
            {
                fileExts += $";*{fileExt}";
            }
        }

        // Midi Files can also be an Overlay Track
        foreach (string fileExt in HelperFunctions.AcceptedMidiFileExts)
        {
            if (string.IsNullOrEmpty(fileExts))
            {
                fileExts += $"*{fileExt}";
            }
            else
            {
                fileExts += $";*{fileExt}";
            }
        }

        string initialDir = s_previousOverlayFileLocation ?? UnityEngine.Application.dataPath;
        string trackFileLocation = OpenFileDialogBox("Open Overlay Track", $"Music Files ({fileExts})\0{fileExts}\0\0", initialDir);
        if (string.IsNullOrEmpty(trackFileLocation))
        {
            return null;
        }

        // Ensuring we start from this same directory next time we browse for a file.
        s_previousOverlayFileLocation = System.IO.Path.GetDirectoryName(trackFileLocation);

        return trackFileLocation;
    }

    public static string OpenVideoFileDialogBox()
    {
        string fileExts = string.Empty;
        foreach (string fileExt in HelperFunctions.AcceptedMovieFileExts)
        {
            if (string.IsNullOrEmpty(fileExts))
            {
                fileExts += $"*{fileExt}";
            }
            else
            {
                fileExts += $";*{fileExt}";
            }
        }

        string initialDir = s_previousVideoFileLocation ?? UnityEngine.Application.dataPath;
        string videoFileLocation = OpenFileDialogBox("Open Video File", $"Video Files ({fileExts})\0{fileExts}\0\0", initialDir);
        if (string.IsNullOrEmpty(videoFileLocation))
        {
            return null;
        }

        // Ensuring we start from this same directory next time we browse for a file.
        s_previousVideoFileLocation = System.IO.Path.GetDirectoryName(videoFileLocation);

        return videoFileLocation;
    }
}

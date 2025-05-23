using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFunctions
{
    public static string DefaultAudioLocation = Application.dataPath + "/StreamingAssets/Audio/";

    public static readonly List<string> AcceptedMidiFileExts = new List<string>() { ".mid", ".midi" };
    public static readonly List<string> AcceptedOverlayFileExts = new List<string>() { ".mp3", ".ogg", ".wav", ".aiff", ".flac" };
    public static readonly List<string> AcceptedMovieFileExts = new List<string>() { ".mp4", ".m4v", ".mov", ".dv", ".mpg", ".mpeg" };

    public static string GetRandomMidiFileFromDefaultLocation()
    {
        string[] filesInDirecotry = System.IO.Directory.GetFiles(DefaultAudioLocation);
        List<string> usableMidiFiles = new List<string>();

        foreach (string filename in filesInDirecotry)
        {
            string fileExt = System.IO.Path.GetExtension(filename).ToLower();
            if (AcceptedMidiFileExts.Contains(fileExt))
            {
                usableMidiFiles.Add(filename);
            }
        }

        if (usableMidiFiles.Count == 0)
        {
            return null;
        }

        int r = Random.Range(0, usableMidiFiles.Count);
        return usableMidiFiles[r];
    }

    public static string GetAssociatedOverlayTrack(string absMidiFilePath)
    {
        string overlayFilePath = GetAssociatedFileForMidi(absMidiFilePath, AcceptedOverlayFileExts);
        if (string.IsNullOrEmpty(overlayFilePath))
        {
            // The Midi file itself will be the ovelray since we couln't find another file to do it
            return absMidiFilePath;
        }

        return overlayFilePath;
    }

    public static string GetAssociatedMovieFile(string absMidiFilePath)
    {
        string moveFilePath = GetAssociatedFileForMidi(absMidiFilePath, AcceptedMovieFileExts);
        return moveFilePath;
    }

    protected static string GetAssociatedFileForMidi(string absMidiFilePath, List<string> extsToLookFor)
    {
        string directory = System.IO.Path.GetDirectoryName(absMidiFilePath);
        string[] filesInDirecotry = System.IO.Directory.GetFiles(directory);
        string midiFilename = System.IO.Path.GetFileNameWithoutExtension(absMidiFilePath);

        foreach (string filePath in filesInDirecotry)
        {
            string filename = System.IO.Path.GetFileNameWithoutExtension(filePath);
            if (filename != midiFilename)
            {
                continue;
            }

            string fileExt = System.IO.Path.GetExtension(filePath).ToLower();
            if (extsToLookFor.Contains(fileExt))
            {
                return filePath;
            }
        }

        return null;
    }
}

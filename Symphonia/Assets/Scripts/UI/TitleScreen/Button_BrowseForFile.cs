using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_BrowseForFile : Button_Base
{
    public enum FileType
    {
        Midi,
        OverlayTrack,
        BackgroundVideo,
    }

    public FileType BrowsingForFileType = FileType.Midi;
    public Text SelectedMidiFileTextRenderer;
    public Text SelectedOverlayTrackFileTextRenderer;
    public MusicRoadManager MusicRoadManager;


    protected override void OnTrigger()
    {
        base.OnTrigger();

        if (BrowsingForFileType == FileType.Midi)
        {
            BrowseForMidiFile();
        }
        else if (BrowsingForFileType == FileType.OverlayTrack)
        {
            BrowseForOverlayTrackFile();
        }
        else
        {
            BrowseForBackgroundVideoFile();
        }
    }

    protected void BrowseForMidiFile()
    {
        string midiFilePath = WindowsOpenFileDialog.OpenMidiFileDialogBox();
        if (string.IsNullOrEmpty(midiFilePath))
        {
            return;
        }

        if (MusicRoadManager == null)
        {
            return;
        }

        string filename = System.IO.Path.GetFileName(midiFilePath);
        if (SelectedMidiFileTextRenderer != null)
        {
            SelectedMidiFileTextRenderer.text = filename;
        }
        if (SelectedOverlayTrackFileTextRenderer != null)
        {
            SelectedOverlayTrackFileTextRenderer.text = filename;
        }

        MusicRoadAudioHandler audioHandler = MusicRoadManager.MusicRoadAudioHandler;
        if (audioHandler != null)
        {
            string overlayTrackPath = HelperFunctions.GetAssociatedOverlayTrack(midiFilePath);
            audioHandler.StopBGM();
            audioHandler.LoadOverlayTrack(overlayTrackPath);
        }

        MusicRoadVideoHandler videoHandler = MusicRoadManager.MusicRoadVideoHandler;
        if (videoHandler != null)
        {
            string videoPath = HelperFunctions.GetAssociatedMovieFile(midiFilePath);
            videoHandler.Stop(true);

            if (string.IsNullOrEmpty(videoPath) == false)
            {
                videoHandler.SetTargetVideoFile(videoPath);
            }
        }

        MusicRoadManager.GenerateMusicRoad("file:///" + midiFilePath);
    }

    protected void BrowseForOverlayTrackFile()
    {
        string overlayTrackFile = WindowsOpenFileDialog.OpenOverlayTrackFileDialogBox();
        if (string.IsNullOrEmpty(overlayTrackFile))
        {
            return;
        }

        if (MusicRoadManager == null)
        {
            return;
        }

        if (SelectedOverlayTrackFileTextRenderer != null)
        {
            string filename = System.IO.Path.GetFileName(overlayTrackFile);
            SelectedOverlayTrackFileTextRenderer.text = filename;
        }

        MusicRoadAudioHandler audioHandler = MusicRoadManager.MusicRoadAudioHandler;
        if (audioHandler != null)
        {
            audioHandler.StopBGM();
            audioHandler.LoadOverlayTrack(overlayTrackFile);
        }

        MusicRoadManager.ResetMusicRoad();
    }

    protected void BrowseForBackgroundVideoFile()
    {
        string videoFilePath = WindowsOpenFileDialog.OpenVideoFileDialogBox();
        if (string.IsNullOrEmpty(videoFilePath))
        {
            return;
        }

        if (MusicRoadManager == null)
        {
            return;
        }

        if (SelectedOverlayTrackFileTextRenderer != null)
        {
            string filename = System.IO.Path.GetFileName(videoFilePath);
            SelectedOverlayTrackFileTextRenderer.text = filename;
        }

        MusicRoadVideoHandler videoHandler = MusicRoadManager.MusicRoadVideoHandler;
        if (videoHandler != null)
        {
            videoHandler.Stop(false);
            videoHandler.SetTargetVideoFile(videoFilePath);
        }
    }
}

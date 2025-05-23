using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MusicRoadVideoHandler : MonoBehaviour
{
    public bool IsPlaying
    {
        get
        {
            return VideoPlayer.isPlaying;
        }
    }

    public MusicRoadManager MusicRoadManager;
    public UnityEngine.Video.VideoPlayer VideoPlayer;

    public bool HasVideoSource { get; protected set; } = false;


    public void Play(double playbackStartTime = 0.0)
    {
        if (HasVideoSource == false)
        {
            return;
        }

        if (VideoPlayer.isPaused || VideoPlayer.isPlaying == false)
        {
            VideoPlayer.Play();
        }

        VideoPlayer.time = playbackStartTime;
    }

    public void Stop(bool removeMovieFile = false)
    {
        VideoPlayer.Stop(); 

        if (removeMovieFile)
        {
            HasVideoSource = false;
        }
    }

    public void SetTargetVideoFile(string absPath)
    {
        VideoPlayer.source = UnityEngine.Video.VideoSource.Url;
        VideoPlayer.url = "file:///" + absPath;

        HasVideoSource = true;
        VideoPlayer.Play();
        VideoPlayer.Pause();
    }
}

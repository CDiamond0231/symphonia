using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_PlayGame : Button_Base
{
    public MusicRoadManager MusicRoadManager;
    public FadeToGameEffect FadeToGameEffect;
    public SceneFadeEffect TitleScreenFadeEffect;
    public MusicRoadManager.PlaybackType GamePlaybackType = MusicRoadManager.PlaybackType.PlayerControlled;

    protected override void OnTrigger()
    {
        base.OnTrigger();

        if (FadeToGameEffect != null)
        {
            FadeToGameEffect.PerformFadeEffect(OnFadeInEffectFinished, OnFadeOutEffectFinished);
        }
    }

    private void OnFadeInEffectFinished()
    {
        if (MusicRoadManager == null)
        {
            return;
        }

        if (TitleScreenFadeEffect != null)
        {
            TitleScreenFadeEffect.InitiateFadeOut();
        }

        MusicRoadManager.IsPlayingGame = true;
        MusicRoadManager.ResetMusicRoad();
        MusicRoadManager.MusicRoadAudioHandler.FadeoutSound();
        MusicRoadManager.MusicRoadVideoHandler.Stop();

        if (GamePlaybackType == MusicRoadManager.PlaybackType.Automatic)
        {
            MusicRoadManager.ShouldGamePlayAutomatically = true;
        }
        else
        {
            MusicRoadManager.ShouldGamePlayAutomatically = false;
        }
    }

    private void OnFadeOutEffectFinished()
    {
        if (MusicRoadManager == null)
        {
            return;
        }

        //Musicroadm
    }
}

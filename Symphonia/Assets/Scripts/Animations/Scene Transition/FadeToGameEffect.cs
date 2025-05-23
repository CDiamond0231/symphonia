using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeToGameEffect : MonoBehaviour
{
    public delegate void OnFadeEffectFinished();

    public class LineEffect
    {
        public LineRenderer lineRenderer;
        public Vector3 positionPoint1;
        public Vector3 positionPoint2;

        public TimeTracker timeTracker { get; set; } = new TimeTracker(1.0f);

        public Vector3 startPos { get; set; }
        public Vector3 endPos { get; set; }

        public Color StartColour { get; set; }
        public Color EndColour { get; set; }
    }

    protected enum TransitionState
    {
        WipeIn,
        Wait,
        WipeOut,
        Idle,
    }

    public float MinLineEffectDuration = 0.01f;
    public float MaxLineEffectDuration = 0.15f;
    public float LoadWaitDuration = 0.3f;
    public MusicRoadManager MusicRoadManager;
    public LineRenderer[] LineRenderers;
    
    protected List<LineEffect> m_lineEffects = new List<LineEffect>();
    protected int m_currentLineID = 0;
    protected TransitionState m_currentState = TransitionState.Idle;

    protected TimeTracker m_waitTimer = new TimeTracker(1.0f);

    protected OnFadeEffectFinished m_onFadeInEffectFinished;
    protected OnFadeEffectFinished m_onFadeOutEffectFinished;

    public void PerformFadeEffect(OnFadeEffectFinished onFadeInEffectFinished, OnFadeEffectFinished onFadeOutEffectFinished)
    {
        m_onFadeInEffectFinished = onFadeInEffectFinished;
        m_onFadeOutEffectFinished = onFadeOutEffectFinished;

        m_currentLineID = 0;
        m_currentState = TransitionState.WipeIn;

        RandomiseLineEffects();
        SetupLineEffects();

        foreach (LineRenderer lineRenderer in LineRenderers)
        {
            lineRenderer.enabled = true;
            lineRenderer.startColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            lineRenderer.endColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }

    protected void RandomiseLineEffects()
    {
        // Randomising Transition Effect
        for (int i = m_lineEffects.Count - 1; i > 0; --i)
        {
            int r = Random.Range(0, i);
            LineEffect temp = m_lineEffects[r];
            m_lineEffects[r] = m_lineEffects[i];
            m_lineEffects[i] = temp;
        }
    }

    protected void SetupLineEffects()
    {
        if (MusicRoadManager == null)
        {
            Debug.LogError($"{nameof(MusicRoadManager)} is not assigned");
            return;
        }

        foreach (LineEffect lineEffect in m_lineEffects)
        {
            int randNum = Random.Range(0, 2);
            if (randNum == 0)
            {
                lineEffect.startPos = lineEffect.positionPoint1;
                lineEffect.endPos = lineEffect.positionPoint2;
            }
            else
            {
                lineEffect.startPos = lineEffect.positionPoint2;
                lineEffect.endPos = lineEffect.positionPoint1;
            }

            lineEffect.timeTracker.FinishTime = Random.Range(MinLineEffectDuration, MaxLineEffectDuration);
            lineEffect.timeTracker.Reset();

            randNum = Random.Range(0, MusicRoadManager.ChannelNotesColours.Length);
            lineEffect.StartColour = MusicRoadManager.ChannelNotesColours[randNum];

            randNum = Random.Range(0, MusicRoadManager.ChannelNotesColours.Length);
            lineEffect.EndColour = MusicRoadManager.ChannelNotesColours[randNum];
        }
    }

    protected void Awake()
    {
        m_waitTimer.FinishTime = LoadWaitDuration;

        if (LineRenderers == null)
        {
            return;
        }

        foreach (LineRenderer lineRenderer in LineRenderers)
        {
            LineEffect lineEffect = new LineEffect();
            lineEffect.lineRenderer = lineRenderer;
            lineEffect.positionPoint1 = lineRenderer.GetPosition(0);
            lineEffect.positionPoint2 = lineRenderer.GetPosition(1);

            m_lineEffects.Add(lineEffect);
        }
    }

    protected void Update()
    {
        if (m_currentState == TransitionState.Idle)
        {
            return;
        }
        if (m_currentState == TransitionState.Wait)
        {
            if (m_waitTimer.Update())
            {
                m_waitTimer.Reset();
                m_currentState = TransitionState.WipeOut;
            }
            return;
        }

        LineEffect lineEffect = m_lineEffects[m_currentLineID];
        bool hasEffectFinished = lineEffect.timeTracker.Update();

        Vector3 endLinePos;
        if (m_currentState == TransitionState.WipeIn)
        {
            endLinePos = Vector3.Lerp(lineEffect.startPos, lineEffect.endPos, lineEffect.timeTracker.GetCompletionPercentage());
        }
        else
        {
            float t = 1.0f - lineEffect.timeTracker.GetCompletionPercentage();
            endLinePos = Vector3.Lerp(lineEffect.startPos, lineEffect.endPos, t);
        }

        lineEffect.lineRenderer.SetPositions(new Vector3[]
        {
            lineEffect.startPos,
            endLinePos
        });

        Color startColour = lineEffect.StartColour;
        startColour.a = 1.0f;

        Color endColour = lineEffect.EndColour;
        endColour.a = 1.0f;

        lineEffect.lineRenderer.startColor = startColour;
        lineEffect.lineRenderer.endColor = endColour;

        if (hasEffectFinished)
        {
            ++m_currentLineID;

            lineEffect.timeTracker.Reset();

            if (m_currentState == TransitionState.WipeOut)
            {
                lineEffect.lineRenderer.enabled = false;
            }

            if (m_currentLineID >= m_lineEffects.Count)
            {
                if (m_currentState == TransitionState.WipeIn)
                {
                    m_currentState = TransitionState.Wait;
                    if (m_onFadeInEffectFinished != null)
                    {
                        m_onFadeInEffectFinished.Invoke();
                        m_onFadeInEffectFinished = null;
                    }

                    m_currentLineID = 0;
                    RandomiseLineEffects();
                }
                else
                {
                    m_currentState = TransitionState.Idle;
                    if (m_onFadeOutEffectFinished != null)
                    {
                        m_onFadeOutEffectFinished.Invoke();
                        m_onFadeOutEffectFinished = null;
                    }
                }
            }
        }
    }
}

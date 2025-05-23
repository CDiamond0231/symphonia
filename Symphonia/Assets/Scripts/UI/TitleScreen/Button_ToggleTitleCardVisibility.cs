using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_ToggleTitleCardVisibility : Button_Base
{
    protected Vector3 TransitionStartPosition
    {
        get
        {
            if (m_isCurrentlyVisible)
            {
                return VisibleTitleCardPosition;
            }
            return HiddenTitleCardPosition;
        }
    }

    protected Vector3 TransitionEndPosition
    {
        get
        {
            if (m_isCurrentlyVisible)
            {
                return HiddenTitleCardPosition;
            }
            return VisibleTitleCardPosition;
        }
    }

    public Vector3 HiddenTitleCardPosition = new Vector3(-1189.0f, 37.0f, 0.0f);
    public Vector3 VisibleTitleCardPosition = new Vector3(-703.0f, 37.0f, 0.0f);
    public float TransitionDuration = 0.3f;

    public Rect HiddenTitleCardCollisionData = new Rect(0.0f, 0.0f, 11.28f, 7.74f);
    public Rect VisibleTitleCardCollisionData = new Rect(0.0f, 0.0f, 11.28f, 7.74f);

    protected bool m_isCurrentlyVisible = false;
    protected bool m_isTransitioning = false;
    protected TimeTracker m_transitionTimeTracker = new TimeTracker(0.3f);

    protected override void OnTrigger()
    {
        base.OnTrigger();

        if (m_isTransitioning)
        {
            return;
        }

        m_isTransitioning = true;
        m_transitionTimeTracker.FinishTime = TransitionDuration;
        m_transitionTimeTracker.Reset();
    }

    protected override void Update()
    {
        base.Update();

        if (m_isTransitioning)
        {
            bool isFinished = m_transitionTimeTracker.Update();

            float f = m_transitionTimeTracker.GetCompletionPercentage();
            gameObject.transform.localPosition = Vector3.Lerp(TransitionStartPosition, TransitionEndPosition, f);

            if (isFinished)
            {
                m_isTransitioning = false;
                m_isCurrentlyVisible = m_isCurrentlyVisible == false;
            }
        }
    }
}

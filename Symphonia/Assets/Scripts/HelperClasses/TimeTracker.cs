//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Time Tracker
//             Author: Christopher Allport
//             Date: August 1, 2013
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This Script contains a time tracking class which you can use as a timer.
//	  The idea is that you can use this script as a way to shorten any code
//	  that requires a wait timer in actual seconds.
//
//-------------------------------------------------------------------------------
//	Instructions:
//
//	~	Simply create a new instance of this class and call the 'Update()' method 
//		as needed.
//
//	~	Check whether or not Time is Up with either the 'TimeUp()' method or the
//		returning boolean when calling the 'Update()' method which will also 
//		return time_up?
//
//	~	Once time is up, the timer can be reset by calling the 'Reset()' method.
//			This will reset the time back to zero and begin tracking back up to the
//			specified finish time via calling the 'Update()' method.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections;

public class TimeTracker
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private float m_fCurrentTime	= 0.0f;
	private float m_fWaitTimer		= 5.0f;
	private bool m_bTimeUp			= false;



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+- Attr_Accessors
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public float CurrentTime
	{
		get 
		{ 
			return m_fCurrentTime; 
		}
		set 
		{ 
			m_fCurrentTime = value;
			if (CheckIfTimeIsUpExact())
				ForceTimerFinish();
			else
				m_bTimeUp = false;
		}
	}

	public float FinishTime
	{
		get
		{
			return m_fWaitTimer;
		}
		set
		{
			m_fWaitTimer = value;
			if (CheckIfTimeIsUpExact())
				ForceTimerFinish();
			else
				m_bTimeUp = false;
		}
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	** Constructor
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public TimeTracker(float TotalWaitTime)
	{
		m_fWaitTimer = TotalWaitTime;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*~ Deconstructor
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	~TimeTracker()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Update Timer
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool Update()
	{
		if (!TimeUp())
		{
			m_fCurrentTime += Time.deltaTime;

			if (CheckIfTimeIsUp())
			{
				m_bTimeUp = true;
			}
		}

        return TimeUp();
	}

	public bool Update(float TimeMultiplier)
	{
		if (!TimeUp())
		{
			m_fCurrentTime += Time.deltaTime * TimeMultiplier;

			if (CheckIfTimeIsUp())
			{
				m_bTimeUp = true;
			}
		}

		return TimeUp();
	}

	public bool UpdateWithRealtime()
	{
		if (!TimeUp())
		{
			m_fCurrentTime += (Time.deltaTime * (1.0f / Time.timeScale));

			if (CheckIfTimeIsUp())
			{
				m_bTimeUp = true;
			}
		}

		return TimeUp();
	}

	public bool UpdateWithRealtime(float TimeMultiplier)
	{
		if (!TimeUp())
		{
			m_fCurrentTime += (Time.deltaTime * (1.0f / Time.timeScale)) * TimeMultiplier;

			if (CheckIfTimeIsUp())
			{
				m_bTimeUp = true;
			}
		}

		return TimeUp();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Methods: Check if Time Is Up
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private bool CheckIfTimeIsUp()
	{
		return (m_fCurrentTime > m_fWaitTimer);
	}

	private bool CheckIfTimeIsUpExact()
	{
		return (m_fCurrentTime >= m_fWaitTimer);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is Time Up?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool TimeUp()
	{
		return m_bTimeUp;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Get Completion Percentage
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public float GetCompletionPercentage()
	{
		return (Mathf.Clamp(m_fCurrentTime, 0.0f, m_fWaitTimer) / m_fWaitTimer);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Set Completion Percentage
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void SetTimerCompletionPercentage(float Percent)
	{
		float f = (m_fWaitTimer * Percent);
		m_fCurrentTime = Mathf.Max( 0.0f,  Mathf.Min(f, m_fWaitTimer)  );
		m_bTimeUp = CheckIfTimeIsUpExact();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Force The Timer to Finish
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ForceTimerFinish()
	{
		m_fCurrentTime = m_fWaitTimer;
		m_bTimeUp = true;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Reset Timer
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void Reset()
	{
		m_fCurrentTime = 0.0f;
		m_bTimeUp = false;
	}
}

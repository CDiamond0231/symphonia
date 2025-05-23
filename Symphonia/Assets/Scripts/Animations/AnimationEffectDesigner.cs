//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Animation Effect Designer
//             Version: 1.0
//             Author: Christopher Allport
//             Date: November 24, 2014
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script is used as a manager for an array of Animation_Effects'.
//	  ~ It includes a custom editor to help assist with that.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections;

public class AnimationEffectDesigner : MonoBehaviour
{
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public AnimationEffect[] m_aAnimationEffect = new AnimationEffect[1];

	public SpriteRenderer m_rAdditionalSpriteRendererComponent;
	public UnityEngine.UI.Text m_rAdditionalTextComponent;
	public UnityEngine.UI.Image m_rAdditionalImageComponent;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*. Protected Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected int m_iCurrentElement = 0;



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Start
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void Start()
	{
		SetupAnimation();
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected virtual void Update()
	{
		if (m_aAnimationEffect[m_iCurrentElement].UpdateAnimation())
		{
			m_iCurrentElement += 1;

			// If at the end of our animation frames, restart from the first frame.
			if (m_iCurrentElement == m_aAnimationEffect.Length)
			{
				m_iCurrentElement = 0;
				m_aAnimationEffect[m_iCurrentElement].Reset();
			}

			// otherwise, reset the next animation frame and keep going
			else
			{
				m_aAnimationEffect[m_iCurrentElement].Reset();
			}
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Setup Animations
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void SetupAnimation()
	{
		for(int i = 0; i < m_aAnimationEffect.Length; ++i)
		{
			m_aAnimationEffect[i].Setup(this.transform, m_rAdditionalSpriteRendererComponent, m_rAdditionalTextComponent, m_rAdditionalImageComponent);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Reset Animation
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void ResetAnimation()
	{
		m_iCurrentElement = 0;
		if(m_aAnimationEffect != null && m_aAnimationEffect.Length > 0)
		{
			m_aAnimationEffect[m_iCurrentElement].Reset();
			m_aAnimationEffect[m_iCurrentElement].ShowFirstFrame();
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: On Enable
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	protected void OnEnable()
	{
		SetupAnimation();
        ResetAnimation();
    }
}

//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Text Animation Effect
//             Author: Christopher Allport
//             Date: December 22, 2015
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This class animates the text on display. Default presets allow you to show
//		text one character/line at a time; fading in the next sequence as needed.
//	  Or you can define a custom selection using an AnimationEffect.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TextAnimationEffect : MonoBehaviour 
{
	private const int TOTAL_HIGHLIGHTED_CHARACTERS = 3;
	private const int INVALID_CHARACTER_ID = -100000;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Public Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public UnityEngine.UI.Text m_rTextRenderer;
	public float m_fTextHighlightFadeTime = 0.02f;
	public float m_fHighlightEffectWaitTime = 2.0f;

	public FadeinAnimationType m_eDesiredFadeinAnimationType = FadeinAnimationType.SINGLE_LINE_CHARACTERS_FADEIN;
	public IdleAnimationType m_eDesiredIdleAnimationType = IdleAnimationType.ALL_LINES_CHARACTER_HIGHLIGHT;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*- Private Instance Variables
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private string m_sFullDisplayText = "";
	private string m_sDefaultTextColourCode = "FFFFFF";
	private string[] m_asLinedSplitText;
	private AnimationState m_eCurrentAnimationState = AnimationState.FADE_IN_ANIMATION;
	private LinkedList<ColourPoints> m_lColouredAreas = new LinkedList<ColourPoints>();

	private TimeTracker m_ttHighlightEffectWaitTimer;					// How long will the highlight effect wait in between completed runs before displaying again?
	private CharacterHighlightEffect[] m_aCharacterHighlightEffect;
	private LinkedList<CharacterHighlightEffect>[] m_alOrderedCharacters;

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*+ Attr_Accessors
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public bool IsFadeingin		{ get { return m_eCurrentAnimationState == AnimationState.FADE_IN_ANIMATION;	} }
	public bool IsFadeingout	{ get { return m_eCurrentAnimationState == AnimationState.FADE_OUT_ANIMATION;	} }
	public bool IsIdleAnimation { get { return m_eCurrentAnimationState == AnimationState.IDLE_ANIMATION;		} }
	public bool IsAnimating		{ get { return (IsIdleAnimation || IsFadeingin || IsFadeingout);				} }

	private LinkedList<CharacterHighlightEffect> SingleLineOrderedCharactersList { get { return m_alOrderedCharacters[0]; } }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	*{} Class Declarations
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private struct ColourPoints
	{
		public int from, to;		// From Index in String, To Index in String. (e.g: FROM Index 10 - TO Index 15 ~ Colour text in Green).
		public string colourCode;
	}

	private class CharacterHighlightEffect
	{
		public int characterID = INVALID_CHARACTER_ID;
		public TimeTracker fadeinTimer;
	}

	public enum FadeinAnimationType
	{
		SINGLE_LINE_CHARACTERS_FADEIN,		// Fadein one character at a time
		ALL_LINES_CHARACTER_FADEIN,			// Fadein one character at a time on all lines at once
		INSTANT,							// Don't fadein, just show instantly~!
		CUSTOM,								// Define a custom fadein type using AnimationEffects
	}


	public enum IdleAnimationType
	{
		SINGLE_LINE_CHARACTER_HIGHLIGHT,
		ALL_LINES_CHARACTER_HIGHLIGHT,
		NONE,
	}

	private enum AnimationState
	{
		FADE_IN_ANIMATION,
		IDLE_ANIMATION,
		FADE_OUT_ANIMATION,
		NONE,
	}



	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Start
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	void Start()
	{
		m_ttHighlightEffectWaitTimer = new TimeTracker(m_fHighlightEffectWaitTime);
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* Derived Method: Update
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	void Update() 
	{
		switch (m_eCurrentAnimationState)
		{
			case AnimationState.FADE_IN_ANIMATION:
				UpdateFadeinAnimation();
				break;
			case AnimationState.FADE_OUT_ANIMATION:
				UpdateFadeoutAnimation();
				break;
			default:
				UpdateIdleAnimation();
				break;
		}
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Fadein Animation
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateFadeinAnimation()
	{
		switch (m_eDesiredFadeinAnimationType)
		{
			case FadeinAnimationType.SINGLE_LINE_CHARACTERS_FADEIN:
				UpdateSingleLineFadeinEffect();
				break;
			case FadeinAnimationType.ALL_LINES_CHARACTER_FADEIN:
				UpdateMultiLineFadeinEffect();
                break;
			case FadeinAnimationType.CUSTOM:
				break;
			default:
				break;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Idle Animation
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateIdleAnimation()
	{
		switch(m_eDesiredIdleAnimationType)
		{
			case IdleAnimationType.SINGLE_LINE_CHARACTER_HIGHLIGHT:
				UpdateSingleLineHighlightEffect();
                break;
			case IdleAnimationType.ALL_LINES_CHARACTER_HIGHLIGHT:
				UpdateMultiLineHighlightEffect();
				break;
			default:
				break;
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Fadeout Animation
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateFadeoutAnimation()
	{
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Single Line Character Fadein Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateSingleLineFadeinEffect()
	{
		if(IsEmptyHighlight())
		{
			m_eCurrentAnimationState = AnimationState.IDLE_ANIMATION;
		}
		else
		{
			m_rTextRenderer.text = GetSingleLineCharacterFadeinEffect(m_alOrderedCharacters[0], 0, m_sFullDisplayText.Length);
        }
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Multi Line Character Fadein Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateMultiLineFadeinEffect()
	{
		if(IsEmptyHighlight())
		{
			m_eCurrentAnimationState = AnimationState.IDLE_ANIMATION;
		}
		else
		{
			int iCurrentStartingTextID = 0;
			m_rTextRenderer.text = "";
			for (int i = 0; i < m_alOrderedCharacters.Length; ++i)
			{
				m_rTextRenderer.text += GetSingleLineCharacterFadeinEffect(m_alOrderedCharacters[i], iCurrentStartingTextID, m_asLinedSplitText[i].Length) + (i < (m_alOrderedCharacters.Length - 1) ? "\n" : "");
                iCurrentStartingTextID += m_asLinedSplitText[i].Length + 1;
            }
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Single Line Character Highlight Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateSingleLineHighlightEffect()
	{
		// No Highlight happening? Great, wait for a little while then set everything up to make it happen.
		if (IsEmptyHighlight())
		{
			if (m_ttHighlightEffectWaitTimer.Update())
			{
				m_ttHighlightEffectWaitTimer.Reset();
				SetupCharacterHighlightEffects(false);
            }
		}

		// Update Hightlight Effect, obviously
		else
		{
			m_rTextRenderer.text = GetSingleLineCharacterHighlightEffect(m_alOrderedCharacters[0], 0, m_sFullDisplayText.Length);
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Multi Line Character Highlight Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateMultiLineHighlightEffect()
	{
		// No Highlight happening? Great, wait for a little while then set everything up to make it happen.
		if (IsEmptyHighlight())
		{
			if (m_ttHighlightEffectWaitTimer.Update())
			{
				m_ttHighlightEffectWaitTimer.Reset();
				SetupCharacterHighlightEffects();
            }
		}

		// Update Hightlight Effect, obviously
		else
		{
			int iCurrentStartingTextID = 0;
			m_rTextRenderer.text = "";
			for (int i = 0; i < m_alOrderedCharacters.Length; ++i)
			{
				m_rTextRenderer.text += GetSingleLineCharacterHighlightEffect(m_alOrderedCharacters[i], iCurrentStartingTextID, m_asLinedSplitText[i].Length) + (i < (m_alOrderedCharacters.Length - 1) ? "\n" : "");
                iCurrentStartingTextID += m_asLinedSplitText[i].Length + 1;
            }
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Update Highlight Effect Characters
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void UpdateHighlightEffectCharacters(LinkedList<CharacterHighlightEffect> a_lCharacterHighlightEffect, int a_iStartIndex, int a_iLength)
	{
		foreach(CharacterHighlightEffect che in a_lCharacterHighlightEffect)
		{
			if(che.characterID != INVALID_CHARACTER_ID)
			{
				if(che.fadeinTimer.Update())
				{
					che.fadeinTimer.FinishTime = m_fTextHighlightFadeTime;
					che.fadeinTimer.Reset();

					if(che.characterID < 0)
					{
						che.characterID = Mathf.Abs(che.characterID);
					}
					else
					{
						che.characterID += TOTAL_HIGHLIGHTED_CHARACTERS;
					}

					if(che.characterID >= (a_iStartIndex + a_iLength))
					{
						che.characterID = INVALID_CHARACTER_ID;
					}
				}
			}
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Get Single Line Character Fadein Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private string GetSingleLineCharacterFadeinEffect(LinkedList<CharacterHighlightEffect> a_lCharacterFadeinEffect, int a_iStartIndex, int a_iLength)
	{
		UpdateHighlightEffectCharacters(a_lCharacterFadeinEffect, a_iStartIndex, a_iLength);

		// Get the CharacterID of whatever has currently been the furthermost rendered character. If it remains "INVALID" Just render all text as normal
		int iLowestCharacterLength = (a_iStartIndex + a_iLength);
		int iHighestCharacterID = INVALID_CHARACTER_ID;
		foreach(CharacterHighlightEffect che in a_lCharacterFadeinEffect)
		{
			if(che.characterID >= a_iStartIndex && che.characterID < iLowestCharacterLength)
				iLowestCharacterLength = che.characterID;
			if(che.characterID > iHighestCharacterID)
				iHighestCharacterID = che.characterID;
		}
		if(iLowestCharacterLength == INVALID_CHARACTER_ID)
		{
			iLowestCharacterLength = (a_iStartIndex + a_iLength);
		}
		if(iHighestCharacterID < iLowestCharacterLength)
			iHighestCharacterID = iLowestCharacterLength;

		string sReturnVal = "";
		for(int i = a_iStartIndex; i < iLowestCharacterLength; ++i)
			sReturnVal += "<color=#" + GetCharacterColourCode(i) + ">" + m_sFullDisplayText[i] + "</color>";

		for(int i = iLowestCharacterLength; i < iHighestCharacterID; ++i)
		{
			if(IsValidCharacter(m_sFullDisplayText[i]))
			{
				CharacterHighlightEffect highlightInfo = MatchCharacterIDToHighlightEffect(i);
				if(highlightInfo != null)
					sReturnVal += "<color=#" + GetCharacterColourCode(i) + ">" + m_sFullDisplayText[i] + "</color>";
				else
					sReturnVal += "<color=#FFFFFF00>" + m_sFullDisplayText[i] + "</color>";
			}
			else
			{
				sReturnVal += m_sFullDisplayText[i];
			}
        }

		if(iHighestCharacterID < (a_iStartIndex + a_iLength))
		{
			sReturnVal += "<color=#FFFFFF00>";
			for(int i = iHighestCharacterID; (i < (a_iStartIndex + a_iLength)); ++i)
			{
				sReturnVal += m_sFullDisplayText[i];
            }
			sReturnVal += "</color>";
        }

		return sReturnVal;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Get Single Line Character Highlight Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private string GetSingleLineCharacterHighlightEffect(LinkedList<CharacterHighlightEffect> a_lCharacterHighlightEffect, int a_iStartIndex, int a_iLength)
	{
		UpdateHighlightEffectCharacters(a_lCharacterHighlightEffect, a_iStartIndex, a_iLength);

		string sReturnVal = "";
		for(int i = a_iStartIndex; (i < (a_iStartIndex + a_iLength)); ++i)
		{
			if(IsValidCharacter(m_sFullDisplayText[i]))
				sReturnVal += "<color=#" + GetCharacterColourCode(i) + ">" + m_sFullDisplayText[i] + "</color>";
			else
				sReturnVal += m_sFullDisplayText[i];
		}

		return sReturnVal;
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Setup Chracter Highlight Effects
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void SetupCharacterHighlightEffects(bool bMultilined = true)
	{
		// This convoluted mess essentially sets every highlight object to the correct character ID. Many of them will be set to the inverse of their actual characterID (eg. -10)
		//	=> This is for visual purposes. They later get set to the correct character. So as to avoid highlighting multiple characters at the exact same time. Trust me, it looks weird.
		if(bMultilined)
		{
			int iCurrentStartingTextID = 0;
			for(int i = 0; i < m_alOrderedCharacters.Length; ++i)
			{
				int iCurrentTextIndex = 0;
				for(int j = (i * TOTAL_HIGHLIGHTED_CHARACTERS); j < ((i + 1) * TOTAL_HIGHLIGHTED_CHARACTERS); ++j, ++iCurrentTextIndex)
				{
					m_aCharacterHighlightEffect[j].characterID = (iCurrentTextIndex > 0 ? -(iCurrentTextIndex + iCurrentStartingTextID) : (iCurrentTextIndex + iCurrentStartingTextID));
					m_aCharacterHighlightEffect[j].fadeinTimer.Reset();
					if(m_aCharacterHighlightEffect[j].characterID < m_sFullDisplayText.Length)
					{
						m_aCharacterHighlightEffect[j].fadeinTimer.FinishTime = (m_fTextHighlightFadeTime * (1.0f + (0.25f * iCurrentTextIndex)));
					}
				}
				iCurrentStartingTextID += m_asLinedSplitText[i].Length + 1;
			}
		}
		else
		{
			for(int i = 0; i < TOTAL_HIGHLIGHTED_CHARACTERS; ++i)
			{
				m_aCharacterHighlightEffect[i].characterID = (i > 0 ? -i : i);
				m_aCharacterHighlightEffect[i].fadeinTimer.Reset();
				if(m_aCharacterHighlightEffect[i].characterID < m_sFullDisplayText.Length)
				{
					m_aCharacterHighlightEffect[i].fadeinTimer.FinishTime = (m_fTextHighlightFadeTime * (1.0f + (0.25f * i)));
				}
			}
		}
	}	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Set Text
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void SetText(string text, FadeinAnimationType startAnimation = FadeinAnimationType.SINGLE_LINE_CHARACTERS_FADEIN)
	{
		DiscoverColouredAreas(text);
		SetDefaultTextColourCode();
		m_sFullDisplayText = RemoveColourTags(text);
		
		m_asLinedSplitText = m_sFullDisplayText.Split(new char[] { '\n' });
		m_aCharacterHighlightEffect = new CharacterHighlightEffect[TOTAL_HIGHLIGHTED_CHARACTERS * m_asLinedSplitText.Length];
		for (int i = 0; i < m_aCharacterHighlightEffect.Length; ++i)
		{
			m_aCharacterHighlightEffect[i] = new CharacterHighlightEffect();
			m_aCharacterHighlightEffect[i].fadeinTimer = new TimeTracker(m_fTextHighlightFadeTime);
		}

		m_alOrderedCharacters = new LinkedList<CharacterHighlightEffect>[m_asLinedSplitText.Length];
		for (int i = 0; i < m_alOrderedCharacters.Length; ++i)
		{
			m_alOrderedCharacters[i] = new LinkedList<CharacterHighlightEffect>();
			for(int j = 0; j < TOTAL_HIGHLIGHTED_CHARACTERS; ++j)
			{
				m_alOrderedCharacters[i].AddLast( m_aCharacterHighlightEffect[ ((i * TOTAL_HIGHLIGHTED_CHARACTERS) + j) ] );
            }
		}



		if(startAnimation == FadeinAnimationType.INSTANT)
		{
			m_eCurrentAnimationState = AnimationState.IDLE_ANIMATION;
		}
		else
		{
			m_eCurrentAnimationState = AnimationState.FADE_IN_ANIMATION;
			if(m_eDesiredFadeinAnimationType == FadeinAnimationType.SINGLE_LINE_CHARACTERS_FADEIN)
				SetupCharacterHighlightEffects(false);
			else if(m_eDesiredFadeinAnimationType == FadeinAnimationType.ALL_LINES_CHARACTER_FADEIN)
				SetupCharacterHighlightEffects(true);
        }
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is Valid Character?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private bool IsValidCharacter(char character)
	{
		return !(character == ' ' || character == '\n' || character == '\r');
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Get Character Colour Code
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private string GetCharacterColourCode(int a_iCharacterID)
	{
		CharacterHighlightEffect highlightInfo = MatchCharacterIDToHighlightEffect(a_iCharacterID);
		if(highlightInfo != null)
			return "FFFFFF" + ((int)(highlightInfo.fadeinTimer.GetCompletionPercentage() * 255.0f)).ToString("X2");

		foreach(ColourPoints colourPoint in m_lColouredAreas)
		{
			if(a_iCharacterID >= colourPoint.from && a_iCharacterID <= colourPoint.to)
				return colourPoint.colourCode;
        }
		return m_sDefaultTextColourCode;
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Match Character ID To Highlight Effect
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private CharacterHighlightEffect MatchCharacterIDToHighlightEffect(int a_iCharacterID)
	{
		foreach(CharacterHighlightEffect che in m_aCharacterHighlightEffect)
		{
			if(a_iCharacterID == che.characterID)
				return che;
		}
		return null;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Is Empty Highlight?
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private bool IsEmptyHighlight()
	{
		for(int i = 0; i < m_aCharacterHighlightEffect.Length; ++i)
		{
			if(m_aCharacterHighlightEffect[i].characterID != INVALID_CHARACTER_ID)
				return false;
		}
		return true;
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Set Default Text Colour Code
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void SetDefaultTextColourCode()
	{
		Color textColour = m_rTextRenderer.color;
		int r = Mathf.CeilToInt(textColour.r * 255.0f);
		int g = Mathf.CeilToInt(textColour.g * 255.0f);
		int b = Mathf.CeilToInt(textColour.b * 255.0f);
		m_sDefaultTextColourCode = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Discover Coloured Areas
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private void DiscoverColouredAreas(string a_sText)
	{
		const int BEGIN_COLOUR_TAG_LENGTH	= 15;	// => "<color=#FFFFFF>".Length;
		const int COLOUR_DECLARATION_LENGTH = 8;	// => "<color=#".Length
		const int END_COLOUR_TAG_LENGTH		= 8;	// => "</color>".Length;

		m_lColouredAreas.Clear();
		int iFinalTextIndexOffset = 0;
        for(int i = 0; i < a_sText.Length; ++i)
		{
			if(a_sText[i] == '<')
			{
				int iRemainingLength = (a_sText.Length - i);
                if(iRemainingLength >= BEGIN_COLOUR_TAG_LENGTH && a_sText.Substring(i, COLOUR_DECLARATION_LENGTH) == "<color=#")
                {
                    Match match = Regex.Match(a_sText.Substring(i, iRemainingLength), @"<color=#([0-9a-zA-Z]{6})[0-9a-zA-Z]?[0-9a-zA-Z]?>", RegexOptions.IgnoreCase);
					if(match.Success)
					{
                        string sColourCode = match.Groups[1].Value;
						int iColourTagLength = match.Groups[0].Value.Length;
                        for(int j = (i + iColourTagLength); j < a_sText.Length; ++j)
						{
							if(a_sText[j] == '<')
							{
								iRemainingLength = (a_sText.Length - j);
								if(iRemainingLength >= END_COLOUR_TAG_LENGTH)
								{
									match = Regex.Match(a_sText.Substring(j, iRemainingLength), @"</color>", RegexOptions.IgnoreCase);
									if(match.Success)
									{
										ColourPoints newColourPoint = new ColourPoints();
										newColourPoint.from = (i - iFinalTextIndexOffset);
										iFinalTextIndexOffset += iColourTagLength;
										newColourPoint.to = (j - iFinalTextIndexOffset);
										newColourPoint.colourCode = sColourCode;
										m_lColouredAreas.AddLast(newColourPoint);

										iFinalTextIndexOffset += END_COLOUR_TAG_LENGTH;
										break;
									}
								}
							}
						} // 'for' loop
					}
                }
			}
		}
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: Remove Colour Tags
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	private string RemoveColourTags(string text)
	{
		string noColourDeclaration = Regex.Replace(text, @"<color=#([0-9a-zA-Z]{6})[0-9a-zA-Z]?[0-9a-zA-Z]?>", "", RegexOptions.IgnoreCase);
        return Regex.Replace(noColourDeclaration, @"</color>", "", RegexOptions.IgnoreCase);
    }
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	* New Method: On Enable
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	void OnEnable()
	{
		if(GetComponent<UnityEngine.UI.Text>() != null)
		{
			SetText(GetComponent<UnityEngine.UI.Text>().text, m_eDesiredFadeinAnimationType);
        }
	}
}

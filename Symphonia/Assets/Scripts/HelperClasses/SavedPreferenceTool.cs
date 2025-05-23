//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
//             Saved Preference Tool
//             Author: Christopher Allport
//             Date: September 30, 2015
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//  Description:
//
//    This script is simply a wrapper for the PlayerPrefs tool.
//
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using System.Collections;

public class SavedPreferenceTool : MonoBehaviour
{
	public static bool GetBool(string savedPrefName, bool defaultValue = false)
	{
		if(PlayerPrefs.HasKey(savedPrefName))
			return PlayerPrefs.GetInt(savedPrefName) == 1;
		return defaultValue;
	}

	public static int GetInt(string savedPrefName, int defaultValue = 0)
	{
		if(PlayerPrefs.HasKey(savedPrefName))
			return PlayerPrefs.GetInt(savedPrefName);
		return defaultValue;
	}

	public static float GetFloat(string savedPrefName, float defaultValue = 0.0f)
	{
		if(PlayerPrefs.HasKey(savedPrefName))
			return PlayerPrefs.GetFloat(savedPrefName);
		return defaultValue;
	}

	public static string GetString(string savedPrefName, string defaultValue = "")
	{
		if(PlayerPrefs.HasKey(savedPrefName))
			return PlayerPrefs.GetString(savedPrefName);
		return defaultValue;
	}



	public static void SaveBool(string savedPrefName, bool bValue)
	{
		PlayerPrefs.SetInt(savedPrefName, bValue ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SaveInt(string savedPrefName, int iValue)
	{
		PlayerPrefs.SetInt(savedPrefName, iValue);
		PlayerPrefs.Save();
	}

	public static void SaveFloat(string savedPrefName, float fValue)
	{
		PlayerPrefs.SetFloat(savedPrefName, fValue);
		PlayerPrefs.Save();
	}

	public static void SaveString(string savedPrefName, string sValue)
	{
		PlayerPrefs.SetString(savedPrefName, sValue);
		PlayerPrefs.Save();
	}
}

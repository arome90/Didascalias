using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* It extends the Player prefs to set the boolean values */
public class BoolPrefs{
	// It sets a bool value in the Player preferences
	public static void SetBool(string key, bool value){
		PlayerPrefs.SetInt(key, value?1:0);
	}
	// It returns a boolean value for a key
	public static bool GetBool(string key){
		if (PlayerPrefs.GetInt (key) > 0)
			return true;
		else
			return false;
	}
	// It returns whether there is any value set to a key
	public static bool HasKey(string key){
		return PlayerPrefs.HasKey(key);
	}
}

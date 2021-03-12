using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class stores the options of the user in the Player Prefs. It uses
static methods as all the preferences are stored uniquely in the PlayerPrefs */
public class PosePrefs{
	// Default values
	private static bool defaultDraggingKnees =false;
	private static bool defaultDraggingElbows=false;
	private static bool defaultNaturalRotation=true;
	// Strings for the player prefs
	private static string draggingKnees="draggingKnees";
	private static string draggingElbows="draggingElbows";
	private static string naturalRotation="naturalRotation";
	// It determines whether the knees are draggable
	public static bool IsDraggingKnees(){
		if (!BoolPrefs.HasKey (draggingKnees))
			BoolPrefs.SetBool (draggingKnees, defaultDraggingKnees);
		return BoolPrefs.GetBool(draggingKnees);
	}
	// It determines whether the elbows are draggable
	public static bool IsDraggingElbows(){
		if (!BoolPrefs.HasKey (draggingElbows))
			BoolPrefs.SetBool (draggingElbows, defaultDraggingElbows);
		return BoolPrefs.GetBool(draggingElbows);
	}
	// It determines whether the camera rotates in the natural way
	public static bool IsNaturalRotation(){
		if (!BoolPrefs.HasKey (naturalRotation))
			BoolPrefs.SetBool (naturalRotation, defaultNaturalRotation);
		return BoolPrefs.GetBool(naturalRotation);
	}
	// Set whether to drag Knees
	public static void SetDraggingKnees(bool isDraggingKnees){
		BoolPrefs.SetBool (draggingKnees, isDraggingKnees);

	}
	// Set whether to drag elbows
	public static void SetDraggingElbows(bool isDraggingElbows){
		BoolPrefs.SetBool (draggingElbows, isDraggingElbows);
	}
	// Set wheter to use natural rotation
	public static void SetNaturalRotation(bool isNaturalRotation){
		BoolPrefs.SetBool (naturalRotation, isNaturalRotation);
	}
}

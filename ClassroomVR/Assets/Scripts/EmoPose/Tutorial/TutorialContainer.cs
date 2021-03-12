using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This class contains several tutorials. The tutorial are only shown once. 
 * If the user explicitly requests to see the tutorial agains, this
 * class resets the tutorials so they act as the first time. */
public class TutorialContainer : MonoBehaviour {
	/** Container of tutorial handlers */
	public TutorialHandler[] tutorials;
	/* Reset all the tutorials so they are shown again for the first time */
	public void ResetTutorials(){
		foreach(TutorialHandler tutorial in tutorials){
			tutorial.Reset ();
		}
	}
}

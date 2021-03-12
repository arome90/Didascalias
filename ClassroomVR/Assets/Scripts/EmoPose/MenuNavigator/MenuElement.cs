using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** It represents the scene objects of a group that controlled by a Menu Manager.
 * Each menu item associated with an object has a reference to the menu manager.
 * In this way, they can go to any ojbect (normally representing a screen) disabling
 * all the others. The reference to the menu manager must bu set in the Unity
* editor. */
public class MenuElement : MonoBehaviour {
	// Reference to the menu manager
	public MenuManager menuManager; 
	// Reference to the previous menu item (i.e. screen) so that the back/escape button
	// works properly. If not any, the application exits.
	public MenuElement precedes;
	// It supports that several menus elements share some common elements defined in
	// this refrerred parentMenuElement. For now, this is only implemented up to one level.
	// Normally from one item yo go the partcilar item without explicitly going through
	// the parent element.
	public MenuElement parentMenuElement;
	// In the initialization, it registers itself in the menu manager.
	void Awake () {
		menuManager.register (this);		
	}
	// It goes to a specific item by calling the same function on the menu manager.
	// It uses the object name as the menu item identifier
	public void GoTo(string name){
		menuManager.GoTo (name);
	}
}


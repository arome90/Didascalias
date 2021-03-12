using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This script represents a generic menu manager. It is mainly aimed at 
representing the navigation through menus, but it can also be used
to navigate through screens (even if there is not any menu behind). 
The different screens register on this manager. This manager can navigate
by disabling all registered elements and enabling one particular item.
 There must be only one instance of this class so that there are not
 conflicts in the udpates. If several menu elements share some objects,
 they can have some parent common item to maintin this elements activated
 in all these. For now, this is only implemented up to one level. */
public class MenuManager : MonoBehaviour {
	// It indicates the initial menu item to start, from the editor
	public string initialMenuElement;
	/* The items of the menu that are registered in the Awake method */ 
	private LinkedList<MenuElement> items = new LinkedList<MenuElement> ();
	// It indicates the current menu
	private MenuElement currentMenuItem;

	/** It registers the game object of a menu item in the list of items */
	public void register (MenuElement item){
		items.AddLast (item);
	}
	/** It navigates to certain screen. Basically it enables the object
	 * with the corresponding name and its parent if different from null.
	 * It disables all the other menu elements. */
	public void GoTo(string name){
		// Find the menu element
		bool found = false;
		foreach (MenuElement item in items) {
			if (item.gameObject.name.Equals (name)) {
				ActivateMenuElement(item);
				found = true;
				currentMenuItem = item;
			}else {
				item.gameObject.SetActive(false);
			}
		}
		// Find the parent and activate
		if(found && (currentMenuItem.parentMenuElement!=null))
			ActivateMenuElement(currentMenuItem.parentMenuElement);
		// Report possible error
		if (!found && !name.Equals(""))
			Debug.Log ("Error while going to the menu item " + name);
	}
	// It activates a menu element
	private void ActivateMenuElement(MenuElement item){
		item.enabled=true;
		item.gameObject.SetActive(true);
	}
	// It goes back to the previous menu item or the app exits if there is not any 
	// any preceeding item defined
	public void GoBack(){
		MenuElement precedingItem = currentMenuItem.precedes;
		if (precedingItem == null) {
			Application.Quit ();
		} else {
			GoTo (precedingItem.gameObject.name);
		}
	}
	// At the beginning, it goes to the initial menu item
	void Start (){
		GoTo (initialMenuElement);
	}
	// When pressing the back/escape button, it goes to the menu item that precedes
	// the current one
	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			GoBack();
		}
	}

}

// ------- NOT USED
/** This script represents a generic menu manager. It is mainly aimed at 
representing the navigation through menus, but it can also be used
to navigate through screens (even if there is not any menu behind). 
The different screens register on this manager. This manager can navigate
by disabling all registered elements and enabling one particular item. 
public class MenuManager : MonoBehaviour {
	// It indicates the initial menu item to start, from the editor
	public string initialMenuItem;
	// The items of the menu that are registered in the Awake method  
	private LinkedList<MenuItem> items = new LinkedList<MenuItem> ();
	// It indicates the current menu
	private MenuItem currentMenuItem;

	// It registers the game object of a menu item in the list of items 
	public void register (MenuItem item){
		items.AddLast (item);
	}
	// It navigates to certain screen. Basically it enables the object
	// with the corresponding name and disables all the others 
	public void GoTo(string name){
		bool found = false;
		foreach (MenuItem item in items) {
			//Debug.Log ("GoTo, Object name = "+item.name);
			if (item.gameObject.name.Equals (name)) {
				item.gameObject.SetActive(true);
				found = true;
				currentMenuItem = item;
			} else {
				item.gameObject.SetActive(false);
			}
		}
		if (!found)
			Debug.Log ("Error while going to the menu item " + name);
	}
	// At the beginning, it goes to the initial menu item
	void Start (){
		GoTo (initialMenuItem);
	}
	// When pressing the back/escape button, it goes to the menu item that precedes
	// the current one
	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			MenuItem precedingItem = currentMenuItem.precedes;
			if (precedingItem == null) {
				Application.Quit ();
			} else {
				GoTo (precedingItem.gameObject.name);
			}
		}
	}

}
*/



/* It uses a non-static singleton pattern to (1) only have one single instance 
	 of the list items, and (2) to make it ready in the first call. Remind that we
	could not do it in the Awake, because we do not know for sure it will be called
	prior other Awake calls for registering the menu items. 
	public List<MenuItem> SingleItems(){
		if (items == null) {
			items = new LinkedList<MenuItem> ();
		}
		return items;
	}*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements a queue with the player prefs of unity. The object always refer
to the same queue for the corresponding name, and it does not erase when closing
the app. It is implemented as a circular queue so it never overpass the integer capacity*/
// FALTARÍA calcular el módulo en los incrementos (quizás con función aparte), si se cree
// que se puede desbordar int
public class QueuePrefs{
	// Name of the queue used in the player prefs, which will be used as prefix of all the
	// key, so that the user can have differente queues
	private string name;
	// Key for the number of elements of the queue
	public static string count="Count";
	// Key for the position of the first element if not empty
	public static string init= "Init";
	// Key for the next empty position;
	public static string end="End";
	// Maximum capacity
	public static int capacity=10000;
	// Contructor with a name in the prefs
	public QueuePrefs(string name){
		this.name = name;
		if (!PlayerPrefs.HasKey (name + "Count") || !PlayerPrefs.HasKey (name + "Init") || !PlayerPrefs.HasKey (name + "End")) {
			PlayerPrefs.SetInt (name + "Count", 0);
			PlayerPrefs.SetInt (name + "Init", 0);
			PlayerPrefs.SetInt (name + "End", 0);
		}
	}
	// This method gets a parameter returning 0 if not ever assigned
	private int GetInt(string key){
		if(PlayerPrefs.HasKey(name+key))
			return PlayerPrefs.GetInt(name+key);
		else
			return 0;
	}
	// Increment an integer value from the key of the prefs, considering the capacity of
	// the circular queue
	private void Increment(string key){
		SumToKey (key, 1);
	}
	// Generic function to implement both increment and decrement of the value of key
	private void SumToKey(string key, int summedValue){
		int value = GetInt (key);
		value = (value + summedValue) % capacity;
		PlayerPrefs.SetInt (name + key, value);
	}
	// Decrement an integer value from the key of the preferences considering the capacity
	// of the circular queue
	private void Decrement(string key){
		SumToKey (key, -1);
	}
	// It adds an element at the end of the queue
	public void Add(string element){
		PlayerPrefs.SetString (name + GetInt(end), element);
		Increment (end);
		Increment (count);
		//PlayerPrefs.SetInt (name + "End", PlayerPrefs.GetInt (name + "End") + 1);
		//PlayerPrefs.SetInt (name + "Count", PlayerPrefs.GetInt (name + "Count") + 1);
	}
	public bool IsEmpty(){
		return GetInt(count) == 0;
	}
	public string First(){
		if (!IsEmpty())
			return PlayerPrefs.GetString (name + GetInt (init));
		else
			return "none";
	}
	public void RemoveFirst(){
		if(!IsEmpty()){
			Increment(init);
			Decrement(count);
		}
		//PlayerPrefs.SetInt (name + "Init", PlayerPrefs.GetInt (name + "Init") + 1);
		//PlayerPrefs.SetInt (name + "Count", Mathf.Min(0,PlayerPrefs.GetInt (name + "Count") - 1));
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* It represents a generic database manager, that is able to safely insert
 * information into any table */
public class GenericDBManager:MonoBehaviour{
	// Url of the "InsertGenericData.php" file
	private string url;
	// special characters
	public static string SPACE = "%20";
	public static string COLON = "%3A";
	// Queue with the messages for sending data to the DB
	private QueuePrefs queue;
	// Name of the queue
	private string nameQueue = "messages";
	// Create the queue before starting the script
	void Awake(){
		// Create the queue from the prefs
		queue = new QueuePrefs (nameQueue);
	}
	// Start the script
	void Start(){
		// Checks whether there were unsent messages
		if (!queue.IsEmpty ()) {
			string urlRequest = queue.First ();
			WWW www = new WWW (urlRequest);
			StartCoroutine (WaitForRequest (www));
		}
	}
	public void SetUrl(string url){
		this.url = url;
	}
	// Insert any data in a table with a name. The values parameter
	//  is any string with the values separated by commas.
	// param tablename: a string with the name of table
	// param values: a string with all the values. Use simple quote for the string
	//			values.
	public void InsertData(string tablename, string values){
		// Create the url with the propers parameters and request it
		string newValues = ReplaceSpecialChars(values);
		string urlRequest = url+"?tablename="+tablename+"&values="+newValues;
		//WWW www = new WWW (urlRequest);
		queue.Add (urlRequest);
		string urlRequestRetrieved = queue.First ();
		WWW www = new WWW (urlRequestRetrieved);
		StartCoroutine(WaitForRequest(www));
	}

	// It manages asyncrous response of the web request. It recursively manages
	// all the remaining requests in the request
	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null)
		{
			queue.RemoveFirst ();
			if (!queue.IsEmpty ()) {
				string urlRequest = queue.First ();
				WWW wwwOther = new WWW (urlRequest);
				StartCoroutine(WaitForRequest(wwwOther));
			}
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
	// It returns the date in a string in the proper format 
	public string GetUserDatetime(){
		// Example '2017-01-31 23:59:59'
		//return "'2017-01-31"+SPACE+"23"+COLON+"59"+COLON+"59'";
		DateTime now = DateTime.Now;
		string userDatetime = "'"+now.Year+"-"+now.Month+"-"+now.Day+" "+now.Hour+":"+
			now.Minute+":"+now.Second+"'";
		return userDatetime;
	}

	// Replace special characters in URL
	private string ReplaceSpecialChars(string str){
		return str.Replace ("(", "%28").Replace(")","%29").Replace("-","%2D")
			.Replace (".", "%2E").Replace (";", "%3B").Replace(":","%3A")
			.Replace(" ", "%20");
	}

	// It saves the device info, including the model name and the user name. Since the 
	// the primary key is the deviceID, this is only stored in the first use of the device
	// The prefix param is added to the beginning of the tablename
	public void SaveDeviceInfo(string prefix){
		InsertData(prefix+"DeviceInfo",
			"'"+SystemInfo.deviceUniqueIdentifier+"'"+","+
			"'"+SystemInfo.deviceModel+"'"+","+
			"'"+SystemInfo.deviceName+"'"+","+
			GetUserDatetime()+","+
			Screen.width+","+
			Screen.height);
	}
}


/////////////////////////// NOT USED
/// 

//public GenericDBManager(string url){
//	this.url = url;
//}

// Pruebas
/*
		Debug.Log("Inicio Prueba Queue");
		QueuePrefs queuePrueba = new QueuePrefs ("prueba");
		queuePrueba.Add ("1");
		queuePrueba.Add ("2");
		queuePrueba.Add ("3");
		while (!queuePrueba.IsEmpty ()) {
			Debug.Log (queuePrueba.First ());
			queuePrueba.RemoveFirst ();
		}
		Debug.Log("Fin Prueba Queue");
		*/


// Url of the "InsertGenericDataPost.php" file
//public string urlPost;

// Insert any data in a table with a name. The values parameter
//  is any string with the values separated by commas.
// param tablename: a string with the name of table
// param values: a string with all the values. Use simple quote for the string
//			values.
/*public void InsertDataPost(string tablename, string values){
		WWWForm form = new WWWForm ();
		form.AddField ("tablename", tablename);
		form.AddField ("values", values);
		WebRequest.Post(url, form);
		yield return www.Send();

		if(www.isError) {
			Debug.Log(www.error);
		}
		else {
			Debug.Log("Form upload complete!");
		}
		// Create the url with the propers parameters and request it
		string urlRequest = url+"?tablename="+tablename+"&values="+values;
		WWW www = new WWW (urlRequest);
		Debug.Log ("urlRequest:" + urlRequest);
	}
	*/

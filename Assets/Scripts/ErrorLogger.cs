using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorLogger : ConsoleLogger {

	public override void Log(string toLog){
		Debug.Log(toLog);
	}

}

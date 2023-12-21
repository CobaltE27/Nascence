using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger
{
    public Dictionary<string, bool> loggableSystems;

    public DebugLogger()
    {
		loggableSystems = new Dictionary<string, bool>();
	}

    public void Log(string message, string systemName)
    {
        if (loggableSystems[systemName])
            Debug.Log(message);
    }
}

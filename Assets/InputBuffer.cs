using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to track what one-time inputs are currently buffered.
/// </summary>
public class InputBuffer : MonoBehaviour
{

    private Dictionary<string, int> downBuffer = new Dictionary<string, int>();
    private Dictionary<string, int> upBuffer = new Dictionary<string, int>();
    public int BUFFER_FRAMES = 5;
    private string[] validKeyDownInputs;
    private string[] validKeyUpInputs;
    private string[] validMouseDownInputs;
    private string[] validMouseUpInputs;

    public InputBuffer(/*maybe later this can take arguments determining which strings the valid lists are initialized with*/)
	{
        validKeyDownInputs = new string[] {"space"};
        validKeyUpInputs = new string[] {"space"};
        validMouseDownInputs = new string[] { };
        validMouseUpInputs = new string[] {"mouse0"};

        foreach (string key in validKeyDownInputs)
        {
            downBuffer.Add(key, 0);
        }

        foreach (string key in validKeyUpInputs)
        {
            upBuffer.Add(key, 0);
        }

        foreach (string button in validMouseDownInputs)
        {
            downBuffer.Add(button, 0);
        }

        foreach (string button in validMouseUpInputs)
        {
            upBuffer.Add(button, 0);
        }
    }

    void Update()
    {
         foreach (string key in validKeyDownInputs)
		{
            if (Input.GetKeyDown(key))
                downBuffer[key] = BUFFER_FRAMES;
		}

        foreach (string key in validKeyUpInputs)
        {
            if (Input.GetKeyUp(key))
                upBuffer[key] = BUFFER_FRAMES;
        }

        foreach (string button in validMouseDownInputs)
        {
            int buttonNum = (int) char.GetNumericValue(button[5]); // since the fifth character in a "mouseX" string is the number of the mouse button
            if (Input.GetMouseButtonDown(buttonNum))
                downBuffer[button] = BUFFER_FRAMES;
        }

        foreach (string button in validMouseUpInputs)
        {
            int buttonNum = (int) char.GetNumericValue(button[5]); // since the fifth character in a "mouseX" string is the number of the mouse button
            if (Input.GetMouseButtonUp(buttonNum))
                upBuffer[button] = BUFFER_FRAMES;
        }
    }

    void FixedUpdate()
	{
        foreach (string key in validKeyDownInputs)
            if (downBuffer[key] > 0)
                downBuffer[key] -= 1;
        foreach (string key in validKeyUpInputs)
            if (upBuffer[key] > 0)
                upBuffer[key] -= 1;
        foreach (string button in validMouseDownInputs)
            if (downBuffer[button] > 0)
                downBuffer[button] -= 1;
        foreach (string button in validMouseUpInputs)
            if (upBuffer[button] > 0)
                upBuffer[button] -= 1;
    }

    public bool GetInputDown(bool precondition, string input)
	{
        if (precondition && downBuffer[input] > 0)
		{
            downBuffer[input] = 0;
            return true;
		}

        return false;
	}

    public bool GetInputUp(bool precondition, string input)
    {
        if (precondition && upBuffer[input] > 0)
        {
            upBuffer[input] = 0;
            return true;
        }

        return false;
    }
}

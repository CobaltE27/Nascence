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
    private Dictionary<string, int> keyDownFor = new Dictionary<string, int>();
	private Dictionary<string, int> keyUpFor = new Dictionary<string, int>();
	public int BUFFER_FRAMES = 10;
    private string[] validKeyInputs;
    private string[] validMouseDownInputs;
    private string[] validMouseUpInputs;
    public Vector2 SwingDir { get; private set; }
    private enum SwingDirModes { MIRROR_MOVEMENT, SECOND_DIRECTION, MOUSE_POS}
    private SwingDirModes swingDirMode;
    public enum MovementControls { UP, DOWN, RIGHT, LEFT, FASTFALL, SWING, PLAT_PROJECTILE, JUMP, SHIFT, CTRL}; //change names of SHIFT and CTRL once there are more abilities
	private Dictionary<MovementControls, KeyCode> keyBindings;

	public InputBuffer()
	{
        keyBindings = new Dictionary<MovementControls, KeyCode> { //need to fetch these from some saved location in the future
            {MovementControls.UP, KeyCode.W},
		    {MovementControls.DOWN, KeyCode.S},
		    {MovementControls.LEFT, KeyCode.A},
		    {MovementControls.RIGHT, KeyCode.D},
		    //{MovementControls.FASTFALL, },
		    {MovementControls.SWING, KeyCode.Mouse0},
		    {MovementControls.PLAT_PROJECTILE, KeyCode.Mouse1},
		    {MovementControls.JUMP, KeyCode.Space},
		    {MovementControls.SHIFT, KeyCode.LeftShift},
		    {MovementControls.CTRL, KeyCode.LeftControl}
		};
        swingDirMode = SwingDirModes.MIRROR_MOVEMENT;

        validKeyInputs = new string[] {"space", "w", "a", "s", "d"};
        validMouseDownInputs = new string[] { };
        validMouseUpInputs = new string[] {"mouse0"};

        foreach (string key in validKeyInputs)
        {
            downBuffer.Add(key, 0);
            upBuffer.Add(key, 0);
			keyDownFor.Add(key, 0);
			keyUpFor.Add(key, 0);
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
        foreach (string key in validKeyInputs)
		{
            if (Input.GetKeyDown(key))
                downBuffer[key] = BUFFER_FRAMES;

            if (Input.GetKey(key))
            {
                keyDownFor[key]++;
                keyUpFor[key] = 0;
            }
            else
            {
                keyDownFor[key] = 0;
                keyUpFor[key]++;
            }

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

        UpdateSwingDir();
    }

    void FixedUpdate()
    {
        foreach (string key in validKeyInputs)
        { 
            if (downBuffer[key] > 0)
                downBuffer[key] -= 1;
            if (upBuffer[key] > 0)
                upBuffer[key] -= 1;
        }
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

    private void UpdateSwingDir()
    {
        Vector2 newSwingDir = Vector2.zero;
		switch (swingDirMode)
		{
			case SwingDirModes.MIRROR_MOVEMENT:
				{
                    if (keyUpFor["w"] <= 1)
						newSwingDir += Vector2.up;
					if (keyUpFor["a"] <= 1)
						newSwingDir += Vector2.left;
					if (keyUpFor["s"] <= 1)
						newSwingDir += Vector2.down;
					if (keyUpFor["d"] <= 1)
						newSwingDir += Vector2.right;

                    if (keyUpFor["w"] <= 1 && keyUpFor["s"] <= 1)
                    {
                        if (keyDownFor["w"] < keyDownFor["s"])
                            newSwingDir.y = 1;
                        else
                            newSwingDir.y = -1;
                    }
					if (keyUpFor["a"] <= 1 && keyUpFor["d"] <= 1)
					{
						if (keyDownFor["a"] < keyDownFor["d"])
							newSwingDir.x = -1;
						else
							newSwingDir.x = 1;
					}

					newSwingDir.Normalize();

					if (newSwingDir != Vector2.zero)
                        SwingDir = newSwingDir;
					break;
				}
			case SwingDirModes.SECOND_DIRECTION:
				{
                    //use arrow keys or stick
					break;
				}
			case SwingDirModes.MOUSE_POS:
				{
					SwingDir = Vector2.up;
					break;
				}
		}
	}
}

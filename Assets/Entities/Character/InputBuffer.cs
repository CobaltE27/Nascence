using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to track what one-time inputs are currently buffered.
/// </summary>
public class InputBuffer : MonoBehaviour
{
    private Dictionary<Controls, int> downBuffer = new Dictionary<Controls, int>();
    private Dictionary<Controls, int> upBuffer = new Dictionary<Controls, int>();
    private Dictionary<Controls, int> keyDownFor = new Dictionary<Controls, int>();
	private Dictionary<Controls, int> keyUpFor = new Dictionary<Controls, int>();
	public int BUFFER_FRAMES = 10;
    private KeyCode[] validKeyInputs;
    public Vector2 SwingDir { get; private set; }
    private enum SwingDirModes { MIRROR_MOVEMENT, SECOND_DIRECTION, MOUSE_POS}
    private SwingDirModes swingDirMode;
    public enum Controls { UP, DOWN, RIGHT, LEFT, FASTFALL, SWING, PLAT_PROJECTILE, JUMP, SHIFT, CTRL}; //change names of SHIFT and CTRL once there are more abilities
	public Dictionary<KeyCode, Controls> keyBindings;

	public InputBuffer()
	{
        keyBindings = new Dictionary<KeyCode, Controls> { //need to fetch these from some saved location in the future
            {KeyCode.W, Controls.UP},
		    {KeyCode.S, Controls.DOWN},
		    {KeyCode.A , Controls.LEFT},
		    {KeyCode.D , Controls.RIGHT},
		    //{MovementControls.FASTFALL, },
		    {KeyCode.Mouse0 , Controls.SWING},
		    {KeyCode.Mouse1 , Controls.PLAT_PROJECTILE},
		    {KeyCode.Space , Controls.JUMP},
		    {KeyCode.LeftShift , Controls.SHIFT},
		    {KeyCode.LeftControl , Controls.CTRL}
		};
        swingDirMode = SwingDirModes.MIRROR_MOVEMENT;

        validKeyInputs = keyBindings.Keys.ToArray();

        foreach (KeyCode key in validKeyInputs)
        {
            downBuffer.Add(keyBindings[key], 0);
            upBuffer.Add(keyBindings[key], 0);
			keyDownFor.Add(keyBindings[key], 0);
			keyUpFor.Add(keyBindings[key], 0);
		}
    }

    void Update()
    {
        foreach (KeyCode key in validKeyInputs)
		{
			Controls control = keyBindings[key];
			if (Input.GetKeyDown(key))
                downBuffer[control] = BUFFER_FRAMES;

			if (Input.GetKeyUp(key))
                upBuffer[control] = BUFFER_FRAMES;
        }

        UpdateSwingDir();
    }

    void FixedUpdate()
    {
        foreach (KeyCode key in validKeyInputs)
        {
            Controls control = keyBindings[key];
			if (downBuffer[control] > 0)
                downBuffer[control] -= 1;
            if (upBuffer[control] > 0)
                upBuffer[control] -= 1;

			if (Input.GetKey(key))
			{
				keyDownFor[control]++;
				keyUpFor[control] = 0;
			}
			else
			{
				keyDownFor[control] = 0;
				keyUpFor[control]++;
			}
		}
    }

    public bool GetBufferedInputDown(bool precondition, Controls controlInput)
	{
        if (precondition && downBuffer[controlInput] > 0)
		{
            downBuffer[controlInput] = 0;
            return true;
		}

        return false;
	}

    public bool GetBufferedInputUp(bool precondition, Controls controlInput)
    {
        if (precondition && upBuffer[controlInput] > 0)
        {
            upBuffer[controlInput] = 0;
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
                    if (keyUpFor[Controls.UP] <= 1)
						newSwingDir += Vector2.up;
					if (keyUpFor[Controls.LEFT] <= 1)
						newSwingDir += Vector2.left;
					if (keyUpFor[Controls.DOWN] <= 1)
						newSwingDir += Vector2.down;
					if (keyUpFor[Controls.RIGHT] <= 1)
						newSwingDir += Vector2.right;

                    if (keyUpFor[Controls.UP] <= 1 && keyUpFor[Controls.DOWN] <= 1)
                    {
                        if (keyDownFor[Controls.UP] < keyDownFor[Controls.DOWN])
                            newSwingDir.y = 1;
                        else
                            newSwingDir.y = -1;
                    }
					if (keyUpFor[Controls.LEFT] <= 1 && keyUpFor[Controls.RIGHT] <= 1)
					{
						if (keyDownFor[Controls.LEFT] < keyDownFor[Controls.RIGHT])
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

    public bool GetInput(Controls input)
    {
        return keyDownFor[input] > 0;
    }

	public bool GetInputDown(Controls controlInput)
	{
		return keyDownFor[controlInput] == 1;
	}

	public bool GetInputUp(Controls controlInput)
	{
		return keyUpFor[controlInput] == 1;
	}
}

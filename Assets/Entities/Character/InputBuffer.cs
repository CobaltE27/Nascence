using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to track what one-time inputs are currently buffered.
/// </summary>
public class InputBuffer : MonoBehaviour
{
    private Dictionary<KeyCode, int> downBuffer = new Dictionary<KeyCode, int>();
    private Dictionary<KeyCode, int> upBuffer = new Dictionary<KeyCode, int>();
    private Dictionary<KeyCode, int> keyDownFor = new Dictionary<KeyCode, int>();
	private Dictionary<KeyCode, int> keyUpFor = new Dictionary<KeyCode, int>();
	public int BUFFER_FRAMES = 10;
    private KeyCode[] validKeyInputs;
    public Vector2 SwingDir { get; private set; }
    private enum SwingDirModes { MIRROR_MOVEMENT, SECOND_DIRECTION, MOUSE_POS}
    private SwingDirModes swingDirMode;
    public enum MovementControls { UP, DOWN, RIGHT, LEFT, FASTFALL, SWING, PLAT_PROJECTILE, JUMP, SHIFT, CTRL}; //change names of SHIFT and CTRL once there are more abilities
	public Dictionary<MovementControls, KeyCode> keyBindings;

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

        validKeyInputs = keyBindings.Values.ToArray();

        foreach (KeyCode key in validKeyInputs)
        {
            downBuffer.Add(key, 0);
            upBuffer.Add(key, 0);
			keyDownFor.Add(key, 0);
			keyUpFor.Add(key, 0);
		}
    }

    void Update()
    {
        foreach (KeyCode key in validKeyInputs)
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

        UpdateSwingDir();
    }

    void FixedUpdate()
    {
        foreach (KeyCode key in validKeyInputs)
        { 
            if (downBuffer[key] > 0)
                downBuffer[key] -= 1;
            if (upBuffer[key] > 0)
                upBuffer[key] -= 1;
        }
    }

    public bool GetInputDown(bool precondition, KeyCode input)
	{
        if (precondition && downBuffer[input] > 0)
		{
            downBuffer[input] = 0;
            return true;
		}

        return false;
	}

    public bool GetInputUp(bool precondition, KeyCode input)
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
                    if (keyUpFor[KeyCode.W] <= 1)
						newSwingDir += Vector2.up;
					if (keyUpFor[KeyCode.A] <= 1)
						newSwingDir += Vector2.left;
					if (keyUpFor[KeyCode.S] <= 1)
						newSwingDir += Vector2.down;
					if (keyUpFor[KeyCode.D] <= 1)
						newSwingDir += Vector2.right;

                    if (keyUpFor[KeyCode.W] <= 1 && keyUpFor[KeyCode.S] <= 1)
                    {
                        if (keyDownFor[KeyCode.W] < keyDownFor[KeyCode.S])
                            newSwingDir.y = 1;
                        else
                            newSwingDir.y = -1;
                    }
					if (keyUpFor[KeyCode.A] <= 1 && keyUpFor[KeyCode.D] <= 1)
					{
						if (keyDownFor[KeyCode.A] < keyDownFor[KeyCode.D])
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumConstants : object
{
	public enum Layer : int
	{
		PLAYER = 8,
		ENVIRONMENT = 9,
		SOLID_ENTITY = 10,
		INCORPOREAL_ENTITY = 11,
		SWING = 12,
		DEFAULT = 1
	}
}

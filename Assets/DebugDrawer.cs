using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugDrawer
{
    static Color defaultDrawColor = Color.white;
    public static void DrawPoint(Vector2 pos, Color? color = null, float duration = 0.0f, float size = 0.25f)
    {
        Color drawColor = color ?? defaultDrawColor;
		Vector2 topRight = pos + new Vector2(size, size);
		Vector2 topLeft = pos + new Vector2(-size, size);
		Vector2 bottomRight = pos + new Vector2(size, -size);
		Vector2 bottomLeft = pos + new Vector2(-size, -size);
        Debug.DrawLine(topLeft, bottomRight, drawColor, duration);
		Debug.DrawLine(topRight, bottomLeft, drawColor, duration);
	}
	public static void DrawBox(Vector2 pos, Vector2 size, Color? color = null, float duration = 0.0f)
	{
		Color drawColor = color ?? defaultDrawColor;
		Vector2 extents = size / 2;
		Vector2 topRight = pos + new Vector2(extents.x, extents.y);
		Vector2 topLeft = pos + new Vector2(-extents.x, extents.y);
		Vector2 bottomRight = pos + new Vector2(extents.x, -extents.y);
		Vector2 bottomLeft = pos + new Vector2(-extents.x, -extents.y);
		Debug.DrawLine(topLeft, topRight, drawColor, duration);
		Debug.DrawLine(topRight, bottomRight, drawColor, duration);
		Debug.DrawLine(bottomRight, bottomLeft, drawColor, duration);
		Debug.DrawLine(bottomLeft, topLeft, drawColor, duration);
	}
}

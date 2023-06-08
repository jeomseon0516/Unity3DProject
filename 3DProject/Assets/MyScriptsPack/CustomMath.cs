using UnityEngine;

static public class CustomMath
{
    public static string GetRemoveSelectString(string str, string selectStr)
    {
        int index = str.IndexOf(selectStr);
        return index > 0 ? str.Substring(0, index) : str;
    }
    public static float GetDistance(Vector2 p1, Vector2 p2)
    {
        float x = p1.x - p2.x;
        float y = p1.y - p2.y;

        return Mathf.Sqrt(x * x + y * y);
    }
    public static Vector2 GetFromPostionToDirection(Vector2 p1, Vector2 p2)
    {
        float radian = GetFromPositionToRadian(p1, p2);
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public static float GetToConvertRotationToAngle(Vector3 targetPoint, Vector3 pivotPoint, float angle) // Angle값
    {
        return ConvertFromRadianToAngle(GetFromPositionToRadian(targetPoint, pivotPoint)) + angle;
    }
    public static float GetToConvertRotationToAngle(float targetY, float pivotY, float targetX, float pivotX, float angle) // Angle값
    {
        return ConvertFromRadianToAngle(GetFromPositionToRadian(targetY, pivotY, targetX, pivotX)) + angle;
    }
    public static float GetToConvertRotationToRadian(Vector3 targetPoint, Vector3 pivotPoint, float angle)
    {
        return ConvertFromAngleToRadian(GetToConvertRotationToAngle(targetPoint, pivotPoint, angle));
    }
    public static float GetToConvertRotationToRadian(float targetY, float pivotY, float targetX, float pivotX, float angle) // Angle값
    {
        return ConvertFromAngleToRadian(GetToConvertRotationToAngle(targetY, pivotY, targetX, pivotX, angle));
    }
    public static int GetIntParseString(string number) { return int.TryParse(number, out int num) ? num : 0; }
    public static float GetFloatParseString(string number) { return float.TryParse(number, out float num) ? num : 0.0f; }
    public static float GetFromPositionToRadian(Vector2 p1, Vector2 p2) { return Mathf.Atan2(p1.y - p2.y, p1.x - p2.x); }
    public static float GetFromPositionToRadian(float p1Y, float p2Y, float p1X, float p2X) { return Mathf.Atan2(p1Y - p2Y, p1X - p2X); }
    public static float ConvertFromRadianToAngle(float radian) { return radian * Mathf.Rad2Deg; }
    public static float ConvertFromAngleToRadian(float angle) { return angle * Mathf.Deg2Rad; }
}

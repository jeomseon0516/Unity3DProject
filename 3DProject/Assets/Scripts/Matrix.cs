using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix : MonoBehaviour
{
    public static Matrix4x4 Translate(Vector3 position)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m03 = position.x;
        m.m13 = position.y;
        m.m23 = position.z;

        return m;
    }
    public static Matrix4x4 Scale(Vector3 _scale)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = _scale.x;
        m.m11 = _scale.y;
        m.m22 = _scale.z;

        return m;
    }
    public static Matrix4x4 Rotate(Vector3 _rotate)
    {
        Vector3 rad = _rotate * Mathf.Deg2Rad;

        return RotateX(rad.x) * RotateY(rad.y) * RotateZ(rad.z);
    }
    public static Matrix4x4 RotateX(float _x)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m11 = m.m22 = Mathf.Cos(_x);
        m.m21 = Mathf.Sin(_x);
        m.m12 = -m.m21;

        return m;
    }
    public static Matrix4x4 RotateY(float _y)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = m.m22 = Mathf.Cos(_y);
        m.m02 = Mathf.Sin(_y);
        m.m20 = -m.m02;

        return m;
    }
    public static Matrix4x4 RotateZ(float _z)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = m.m11 = Mathf.Cos(_z);
        m.m10 = Mathf.Sin(_z);
        m.m01 = -m.m10;

        return m;
    }
}

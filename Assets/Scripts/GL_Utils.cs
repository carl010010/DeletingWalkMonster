﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GL_Utils
{
    public static void DrawCrossNormal(Vector3 pos, Vector3 normal, Color color, float size = 0.1f)
    {
        DrawCross(pos, normal, color, size);
    }

    public static void DrawCrossVertical(Vector3 pos, Color color, float size = 0.1f)
    {
        DrawCross(pos, Vector3.up, color, size);
    }

    public static void DrawCross(Vector3 pos, Vector3 normal, Color color, float size)
    {
        var p0 = new Vector3(-size / 2, 0, size / 2);
        var p1 = new Vector3(size / 2, 0, -size / 2);
        var p2 = new Vector3(-size / 2, 0, -size / 2);
        var p3 = new Vector3(size / 2, 0, size / 2);


        if (normal != Vector3.up)
        {
            // Matrix magic
            Quaternion YY = Quaternion.FromToRotation(Vector3.forward, Vector3.up);

            Matrix4x4 m = Matrix4x4.Rotate(Quaternion.LookRotation(normal) * YY);

            // Matrix dotproduct X4
            p0 = m.MultiplyPoint3x4(p0);
            p1 = m.MultiplyPoint3x4(p1);
            p2 = m.MultiplyPoint3x4(p2);
            p3 = m.MultiplyPoint3x4(p3);
        }

        p0 += pos;
        p1 += pos;
        p2 += pos;
        p3 += pos;

        DrawLine(p0, p1, color);
        DrawLine(p2, p3, color);
    }


    public static void DrawLine(Vector3 start, Vector3 end, Color sColor)
    {
        DrawLine(start, end, sColor, sColor);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color sColor, Color eColor)
    {
        GL.Color(sColor);

        GL.Vertex(start);
        GL.Color(eColor);
        GL.Vertex(end);
    }
}

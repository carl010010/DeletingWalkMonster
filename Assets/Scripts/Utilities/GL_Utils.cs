using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GL_Utils
{
    public static Material CreateLineMaterial()
    {
        Material lineMaterial;

        // Unity has a built-in shader that is useful for drawing simple colored things
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);

        return lineMaterial;
    }


    public static void DrawCrossNormal(Vector3 pos, Vector3 normal, Color color, float size = 0.1f)
    {
        DrawCross(pos, normal, color, size);
    }

    public static void DrawCrossVertical(Vector3 pos, Color color, float size = 0.1f)
    {
        DrawCross(pos, Vector3.up, color, size);
    }

    static void DrawCross(Vector3 pos, Vector3 normal, Color color, float size)
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


    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        DrawLine(start, end, color, color);
    }

    internal static void DrawLine(Vector3 start, Vector3 direction, float rayLength, Color color)
    {
        DrawLine(start, start + direction * rayLength, color, color);
    }


    public static void DrawLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
    {
        GL.Color(startColor);

        GL.Vertex(start);
        GL.Color(endColor);
        GL.Vertex(end);
    }

    public static void DrawCircle(Vector3 center, float radius, Vector3 normal, Color color)
    {
        GL.Color(color);

        Vector3 ci;

        normal = normal.normalized;
        Vector3 forward = normal == Vector3.up ?
            Vector3.ProjectOnPlane(Vector3.forward, normal).normalized :
            Vector3.ProjectOnPlane(Vector3.up, normal);
        Vector3 right = Vector3.Cross(normal, forward);

        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += 0.4f)
        {
            ci = center + forward * Mathf.Cos(theta) * radius + right * Mathf.Sin(theta) * radius;
            GL.Vertex(ci);

            if (theta != 0)
                GL.Vertex(ci);
        }

        ci = center + forward * Mathf.Cos(0) * radius + right * Mathf.Sin(0) * radius;
        GL.Vertex(ci);
    }
}

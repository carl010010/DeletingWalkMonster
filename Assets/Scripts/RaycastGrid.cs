using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class RaycastGrid : MonoBehaviour
{
    public float SqrSize = 1;
    public int lineCount = 25;
    public float playerHeight = 2;


    WalkGrid walkGrid;

    public bool drawRay = false;
    public bool drawCross = false;


    Material lineMaterial;

    void Awake()
    {
        // must be called before trying to draw lines..
        CreateLineMaterial();
    }

    void CreateLineMaterial()
    {
        // Unity has a built-in shader that is useful for drawing simple colored things
        var shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
    }


    private void OnRenderObject()
    {
        GL.PushMatrix();

        if (lineMaterial == null)
            CreateLineMaterial();

        if (walkGrid == null)
            walkGrid = new WalkGrid();

            lineMaterial.SetPass(0);


        // Draw lines
        GL.Begin(GL.LINES);

        walkGrid.drawRay = drawRay;
        walkGrid.drawCross = drawCross;
        walkGrid.GenerateWalkGrid(SqrSize, lineCount, playerHeight, transform.position);

        walkGrid.DisplayGrid();

        GL.End();
        GL.PopMatrix();
        //TestGLPoints();
    }

    private void TestGLPoints()
    {
        GL.PushMatrix();

        if (lineMaterial == null)
            CreateLineMaterial();

        lineMaterial.SetPass(0);


        // Draw lines
        GL.Begin(GL.LINES);

        Vector3 pos = this.transform.position;

        float count = Mathf.Sqrt(lineCount);
        float spacing = (1f / (count - 1)) * SqrSize;

        for (int i = 0; i < count; i++)
        {
            for (int b = 0; b < count; b++)
            {
                Vector3 point = pos + new Vector3(0 - (SqrSize / 2) + i * spacing, 0, 0 - (SqrSize / 2) + b * spacing);


                RaycastHit[] hits = Physics.RaycastAll(point, Vector3.down);

                hits = hits.OrderBy(v => v.point.y).Reverse().ToArray();

                if (hits.Length != 0)
                {
                    List<Collider> blockerCollider = new List<Collider>();

                    for (int h = 0; h < hits.Length; h++)
                    {
                        Color crossColor = Color.white;

                        RaycastHit hit = hits[h];
                        RaycastHit hit2;

                        foreach (var c in blockerCollider)
                        {
                            if (IsPointInCollider(c, hit.point))
                                crossColor = Color.red;
                        }


                        if (hit.collider.gameObject.layer == 8)
                        {
                            blockerCollider.Add(hit.collider);
                        }

                        if (h != 0
                            && hit.point.y + playerHeight > hits[h - 1].point.y
                            || Physics.Raycast(hit.point, Vector3.up, out hit2, playerHeight))  //TODO (Carl) can significantly reduce the amount of raycasts
                                                                                                // if we assume hits[0] is the top most point
                                                                                                // in the world
                        {
                            crossColor = Color.red;
                        }
                        //else //Shows all extra raycasts that are needed
                        //{
                        //    if (Physics.Raycast(hit.point, Vector3.up, out hit2, playerHeight))
                        //    {
                        //        crossColor = Color.red;
                        //        DrawLine(hit.point, hit2.point, Color.red);
                        //    }
                        //    else
                        //    {
                        //        DrawLine(hit.point, hit.point + Vector3.up * 10, Color.blue);
                        //    }
                        //}

                        GL_Utils.DrawCrossNormal(hit.point + Vector3.up * 0.01f, hit.normal, crossColor);
                    }
                    if (drawRay)
                        GL_Utils.DrawLine(point, hits[hits.Length - 1].point, Color.green);
                }
            }
        }

        //RaycastHit[] hits = Physics.RaycastAll(this.transform.position, Vector3.down);

        //if(hits.Length != 0)
        //{
        //    foreach (RaycastHit hit in hits)
        //    {
        //        DrawCrossNormal(hit.point, hit.normal, Color.red);
        //    }

        //    DrawLine(this.transform.position, hits[Mathf.Max(0, hits.Length - 1)].point, Color.green);
        //}            


        //DrawLine(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.red);
        //DrawLine(new Vector3(1, 0, 0), new Vector3(1, 0.1f, 1), Color.red);

        //DrawCrossVertical(new Vector3(0,0.1f,0), Color.white);
        //DrawCrossNormal(new Vector3(1,0.1f,0.5f), new Vector3(0,0,0), Color.red, 0.1f);

        //DrawPlusMark

        GL.End();
        GL.PopMatrix();
    }

    private static bool IsPointInCollider(Collider c, Vector3 point)
    {
        Vector3 closest = c.ClosestPoint(point);
        // Because closest=point if point is inside - not clear from docs I feel
        return closest == point;
    }
}

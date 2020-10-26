using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SkeletonLine : MonoBehaviour
{
    private Transform start;
    private Transform end;
    private LineRenderer lr;

    void Update()
    {
        if (!(start is null) && !(end is null))
        {
            lr.SetPosition(0, start.position);
            lr.SetPosition(1, end.position);
        }
    }

    public void init()
    {
        lr = this.gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetJoints(Transform _start, Transform _end)
    {
        start = _start;
        end = _end;
    }

    public void SetColor(Color color)
    {
        lr.startColor = color;
        lr.endColor = color;
    }

    public void SetWidth(float f)
    {
        lr.startWidth = f;
        lr.endWidth = f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationPoint : MonoBehaviour
{
    public int annotNumber = -1;

    public void Init(int num, Color color)
    {
        SetNumber(num);
        this.GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetNumber(int num)
    {
        annotNumber = num;
    }
}

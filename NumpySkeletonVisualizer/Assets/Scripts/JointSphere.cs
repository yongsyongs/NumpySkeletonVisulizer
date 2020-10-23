using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointSphere : MonoBehaviour
{
    private int jointNumber;

    public int JointNumber
    {
        get
        {
            return jointNumber;
        }
    }

    public void SetScale(float scale)
    {
        this.transform.localScale = Vector3.one * scale;
    }

    public void SetJointIndex(int idx)
    {
        jointNumber = idx;
    }
}

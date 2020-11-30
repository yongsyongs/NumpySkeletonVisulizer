using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCenterCoord
{
    public static Vector3 ToBodyCentered(Vector3 posToTrans, Vector3[] urf)
    {
        Vector3 transPos = new Vector3();
        transPos.x = Vector3.Dot(urf[1], posToTrans);
        transPos.y = Vector3.Dot(urf[0], posToTrans);
        transPos.z = Vector3.Dot(urf[2], posToTrans);

        return transPos;
    }
    public static Vector3 ToBodyCentered(Vector3 posToTrans, Vector3[] urf, Vector3 origin)
    {
        Vector3 transPos = new Vector3();
        transPos.x = Vector3.Dot(urf[1], posToTrans - origin);
        transPos.y = Vector3.Dot(urf[0], posToTrans - origin);
        transPos.z = Vector3.Dot(urf[2], posToTrans - origin);

        return transPos;
    }

    public static Vector3[] GetBasis(Vector3 root, Vector3 leftShoulder, Vector3 rightShoulder)
    {
        Vector3 up = (leftShoulder + rightShoulder) / 2f - root;
        Vector3 forward = Vector3.Cross((leftShoulder - root), (rightShoulder - root));
        Vector3 right = Vector3.Cross(forward, up);

        return new Vector3[3] { up.normalized, right.normalized, forward.normalized };
    }
}

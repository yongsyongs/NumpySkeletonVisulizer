using UnityEditor;
using UnityEngine;
using NumSharp;
using System;

public static class NPTools
{
    public static NDArray LoadNpyFile(string path)
    {
        Debug.Log("path: " + path);
        NDArray data = np.load(path);

        int[] dims = data.shape;
        string shapeString = "(";
        foreach (int d in dims)
            shapeString += d.ToString() + ", ";
        shapeString += ")";
        Debug.Log("data shape: " + shapeString);
        
        if(dims.Length != 3)
        {
            EditorUtility.DisplayDialog("Shape Error", "array shape must be (T, J, 2 or 3).", "ok");
            return null;
        }

        return data;
    }

}
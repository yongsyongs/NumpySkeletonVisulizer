using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NumSharp;

public class DataManager : MonoBehaviour
{
    [SerializeField] private GameObject jointPrefab;
    [SerializeField] private float scaleFactor = 30f;

    private NDArray skeletonArray;
    private List<GameObject> jointList;
    private List<Transform> jointTransformList;

    private int frameCount;
    private int currentFrame = 0;
    private bool is2D = false;
    private bool pause = false;

    void Start()
    {

    }

    void Update()
    {
        if (!(pause || skeletonArray is null)) 
        {
            if (currentFrame >= frameCount) 
                currentFrame = 0;
            NDArray arr = skeletonArray[currentFrame++.ToString()];
            for (int i = 0; i < jointList.Count; i++)
            {
                if (is2D)
                    jointTransformList[i].position = new Vector3(arr[i, 0], arr[i, 1]);
                else
                    jointTransformList[i].position = new Vector3(arr[i, 0], arr[i, 2], arr[i, 1]);
            }
        }
    }

    void GenerateJoints()
    {
        if(jointList is null)
        {
            jointList = new List<GameObject>();
            jointTransformList = new List<Transform>();

            int[] dims = skeletonArray.shape;
            int jointNum = dims[1];
            frameCount = dims[0];
            is2D = dims[2] == 2;

            for (int i = 0; i < jointNum; i++)
            {
                jointList.Add(Instantiate(jointPrefab));
                jointList[i].AddComponent<Joint>();
                jointList[i].GetComponent<Joint>().SetJointIndex(i);
                jointTransformList.Add(jointList[i].transform);
            }

            foreach(var g in jointList)
                g.AddComponent<Joint>();
        }
        else
        {
            foreach (GameObject g in jointList)
                Destroy(g);
            jointList = null;
            jointTransformList = null;
        }
    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
    }

    public void OpenSkeletonFile()
    {
        string path = EditorUtility.OpenFilePanel("Open Skeleton File", "", "npy");
        if(path.Length != 0)
        {
            skeletonArray = NPTools.LoadNpyFile(path) * scaleFactor;
            GenerateJoints();
        }
    }
}

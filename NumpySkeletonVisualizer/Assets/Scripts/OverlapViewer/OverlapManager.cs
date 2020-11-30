using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using NumSharp;
using TMPro;
using System;

public class OverlapManager : MonoBehaviour
{
    [SerializeField] private bool bodyCenter = false;
    [SerializeField] private GameObject jointPrefab;
    [SerializeField] private float scaleFactor = 30f;
    [SerializeField] private float detachDistance = 100f;

    [SerializeField] private int[] offsets;
    [SerializeField] private Text frameText;

    private NDArray skeletonArray;
    private NDArray adjMat;
    private List<List<GameObject>> skeletonList;
    private List<Vector3[]> urfs;

    private int frameCount;
    private int currentFrame = 0;
    private bool is2D = false;
    private bool pause = false;
    private bool detaching = false;


    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        urfs = new List<Vector3[]>();
    }

    void Update()
    {

        if(!(skeletonList is null))
        {
            if (!pause)
            {
                NextFrame();
            }
        }
    }

    void RenderSkeleton(List<GameObject> jointList)
    {
        for (int i = 0; i < adjMat.shape[0]; i++)
        {
            for(int j = i; j < adjMat.shape[1]; j++)
            {
                if(adjMat[i, j])
                {
                    DrawLine(jointList[i].transform, jointList[j].transform, Color.red);
                }
            }
        }
    }

    void NextFrame()
    {
        if (currentFrame >= frameCount)
            currentFrame = 0;
        for (int i = 0; i < skeletonList.Count; i++)
        {
            NDArray arr = skeletonArray[i, currentFrame];
            List<GameObject> jointList = skeletonList[i];
            Vector3 root = new Vector3(arr[0, 0], arr[0, 1], arr[0, 2]);

            for (int j = 0; j < jointList.Count; j++)
            {
                if (is2D)
                    jointList[j].transform.position = new Vector2(arr[j, 0] + detachDistance * (i - 1), arr[j, 1]);
                else
                {
                    Vector3 v = new Vector3(arr[j, 0], arr[j, 2], arr[j, 1]);
                    if (bodyCenter)
                    {
                        Vector3 transed = BodyCenterCoord.ToBodyCentered(v, urfs[i], root);
                        transed.x += detaching ? detachDistance * (i - 1) : 0f;
                        jointList[j].transform.position = transed; 
                    }
                    else
                    {
                        v.x += detaching ? detachDistance * (i - 1) : 0f;
                        jointList[j].transform.position = v;
                    }
                }
            }
        }
        frameText.text = currentFrame++.ToString();
    }

    void DrawLine(Transform start, Transform end, Color color)
    {
        GameObject sk = new GameObject();
        sk.transform.SetParent(start);

        SkeletonLine skline = sk.AddComponent<SkeletonLine>();
        skline.init();
        skline.SetJoints(start, end);
        skline.SetColor(color);
        skline.SetWidth(0.1f);
    }

    void GenerateJoints()
    {
        List<GameObject> jointList = new List<GameObject>();

        int[] dims = skeletonArray.shape;
        int jointNum = dims[2];
        frameCount = dims[1];
        is2D = dims[3] == 2;

        for (int i = 0; i < jointNum; i++)
        {
            GameObject joint = Instantiate(jointPrefab);
            JointSphere js = joint.AddComponent<JointSphere>();
            js.SetJointIndex(i);
            jointList.Add(joint);
        }
        skeletonList.Add(jointList);
        RenderSkeleton(jointList);
    }

    public void Detach()
    {
        detaching = detaching ? false : true;
    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
    }

    public void LoadAdjMatrix()
    {
        string path = EditorUtility.OpenFilePanel("Open Adjacency Matrix File", "", "npy");
        if (path.Length != 0)
        {
            adjMat = np.load(path);
        }
    }

    public void OpenSkeletonFile()
    {
        string path = EditorUtility.OpenFilePanel("Open Skeleton File", "", "npy");
        if(path.Length != 0)
        {
            // read npy file that has (T, J, 2 or 3) shape.
            skeletonArray = np.load(path) * scaleFactor;
            
            Debug.Assert(skeletonArray.shape.Length == 4, "skeleton array shape must be (N, T, J, 2 or 3) but got " + skeletonArray.Shape);
            
            int jdim = skeletonArray.shape[2];
            adjMat = np.full<bool>(false, new int[] { jdim, jdim });
            LoadAdjMatrix();
            
            if(!(skeletonList is null))
            {
                foreach(List<GameObject> jointList in skeletonList)
                {
                    foreach(GameObject g in jointList)
                        Destroy(g);
                }
                skeletonList = null;
            }
            skeletonList = new List<List<GameObject>>();

            int ndim = skeletonArray.shape[0];
            
            for (int i = 0; i < ndim; i++)
            {
                if (bodyCenter)
                {
                    NDArray arr = skeletonArray[i, offsets[i]];
                    Vector3[] urf = BodyCenterCoord.GetBasis(
                        new Vector3(arr[0, 0], arr[0, 1], arr[0, 2]),
                        new Vector3(arr[14, 0], arr[14, 1], arr[14, 2]),
                        new Vector3(arr[11, 0], arr[11, 1], arr[11, 2])
                    );
                    urfs.Add(urf);
                }
                GenerateJoints();
            }
        }
    }
}
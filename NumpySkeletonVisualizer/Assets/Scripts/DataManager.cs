using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using NumSharp;
using TMPro;
using System;

public class DataManager : MonoBehaviour
{
    [SerializeField] private GameObject jointPrefab;
    [SerializeField] private float scaleFactor = 30f;

    private NDArray skeletonArray;
    private NDArray adjMat;
    private List<GameObject> jointList;

    private int frameCount;
    private int currentFrame = 0;
    private bool is2D = false;
    private bool pause = false;
    private bool connectingMode = false;
    private bool isConnecting = false;

    private int start = -1, end = -1;

    void Start()
    {

    }

    void Update()
    {
        if(!(skeletonArray is null))
        {
            if (!pause)
                NextFrame();
            if (!(adjMat is null))
                RenderSkeleton();
            if (connectingMode)
                SetConnectionJoints();
        }
    }

    void RenderSkeleton()
    {
        for (int i = 0; i < adjMat.shape[0]; i++)
        {
            for(int j = i; j < adjMat.shape[1]; j++)
            {
                if(adjMat[i, j])
                {
                    DrawLine(jointList[i].transform.position, jointList[j].transform.position, Color.red);
                }
            }
        }
    }

    void NextFrame()
    {
        if (currentFrame >= frameCount)
            currentFrame = 0;
        NDArray arr = skeletonArray[currentFrame++.ToString()];
        for (int i = 0; i < jointList.Count; i++)
        {
            if (is2D)
                jointList[i].transform.position = new Vector3(arr[i, 0], arr[i, 1]);
            else
                jointList[i].transform.position = new Vector3(arr[i, 0], arr[i, 2], arr[i, 1]);
        }
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(myLine, 2f * Time.deltaTime);
    }

    void SetConnectionJoints()
    {
        if (isConnecting)
        {
            if (Input.GetMouseButtonUp(0))
            {
                var target = GetTarget();
                JointSphere jsc;
                if (!(target is null) && target.TryGetComponent<JointSphere>(out jsc))
                {
                    end = jsc.JointNumber;
                    Debug.Log("end was set to " + end);
                    if (end > 0)
                    {
                        adjMat[start, end] = true;
                        adjMat[end, start] = true;
                    }
                }
                isConnecting = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                var target = GetTarget();
                JointSphere jsc;
                if(!(target is null) && target.TryGetComponent<JointSphere>(out jsc))
                {
                    start = jsc.JointNumber;
                    Debug.Log("start was set to " + start);
                    if (start > 0)
                        isConnecting = true;
                }
            }
        }
    }


    Transform GetTarget()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
            return hit.transform;
        else
            return null;
    }

    void GenerateJoints()
    {
        if(jointList is null)
        {
            jointList = new List<GameObject>();

            int[] dims = skeletonArray.shape;
            int jointNum = dims[1];
            frameCount = dims[0];
            is2D = dims[2] == 2;

            for (int i = 0; i < jointNum; i++)
            {
                jointList.Add(Instantiate(jointPrefab));
                jointList[i].AddComponent<JointSphere>();
                jointList[i].GetComponent<JointSphere>().SetJointIndex(i);
            }

            foreach(var g in jointList)
                g.AddComponent<JointSphere>();
        }
        else
        {
            foreach (GameObject g in jointList)
                Destroy(g);
            jointList = null;
        }
    }

    public void SwitchConnectMode()
    {
        if (connectingMode)
            EndConnectMode();
        else
            StartConnectMode();
    }

    public void StartConnectMode()
    {
        this.Pause();
        connectingMode = true;
        Text text = GameObject.Find("Btn_ConnectMode").transform.GetChild(0).GetComponent<Text>();
        text.text = "Done";
    }

    public void EndConnectMode()
    {
        this.Resume();
        connectingMode = false;
        Text text = GameObject.Find("Btn_ConnectMode").transform.GetChild(0).GetComponent<Text>();
        text.text = "Connect Joints";
    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
    }

    public void SaveAdjMatrix()
    {
        if(!(adjMat is null))
        {
            np.save("Assets/Data/adjmat.npy", adjMat);
        }
    }

    public void LoadAdjMatrix()
    {
        string path = EditorUtility.OpenFilePanel("Open Adjacency Matrix File", "", "npy");
        if (path.Length != 0)
        {
            adjMat = NPTools.LoadNpyFile(path);
        }
    }

    public void OpenSkeletonFile()
    {
        string path = EditorUtility.OpenFilePanel("Open Skeleton File", "", "npy");
        if(path.Length != 0)
        {
            // read npy file that has (T, J, 2 or 3) shape.
            skeletonArray = NPTools.LoadNpyFile(path) * scaleFactor;
            int jdim = skeletonArray.shape[1];
            adjMat = np.full<bool>(false, new int[] { jdim, jdim });
            GenerateJoints();
        }
    }
}

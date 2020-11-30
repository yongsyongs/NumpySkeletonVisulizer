using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using NumSharp;
using System;

public class AnnotationManager : MonoBehaviour
{
    [SerializeField] private Text modeChangeButtonText;
    [SerializeField] private GameObject annotPrefab;
    [SerializeField] private float dragginObjectMoveSpeed = 0.001f;

    private VideoManager vm;
    private Vector2 invalidVector = new Vector2(-1f, -1f);
    private GameObject draggingObject;
    private GameObject[] annotObjects;

    private NDArray records;

    private bool annotMode = false;
    private bool dragging = false;


    void Start()
    {
        records = np.empty();
        vm = GetComponent<VideoManager>();
    }

    void Update()
    {
        if (annotMode)
        {
            Vector2 pos = ScreenToImage(Input.mousePosition);
            if (pos != invalidVector)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ClickImage();
                }
                else if (Input.GetMouseButton(0))
                {
                    if (dragging)
                    {
                        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        newPos.z = 0f;
                        draggingObject.transform.position = newPos;
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (dragging)
                    {
                        dragging = false;
                        //draggingObject = null;
                    }
                }
            }
        }

        if (records.shape.Length > 0 && !(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
            SyncObjToArray();

        if (draggingObject != null)
        {
            Vector3 v = draggingObject.transform.position;
            if (Input.GetKey(KeyCode.LeftArrow))
                v.x -= dragginObjectMoveSpeed;
            if (Input.GetKey(KeyCode.RightArrow))
                v.x += dragginObjectMoveSpeed;
            if (Input.GetKey(KeyCode.DownArrow))
                v.y -= dragginObjectMoveSpeed;
            if (Input.GetKey(KeyCode.UpArrow))
                v.y += dragginObjectMoveSpeed;
        }
    }

    void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            if (np.all(records[vm.CurrentFrameIdx] == -1f))
                records[vm.CurrentFrameIdx] = records[vm.CurrentFrameIdx - 1];
            else
                SyncArrayToObj();
        }
    }

    private void ClickImage()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Annotation")
            {
                dragging = true;
                draggingObject = hit.collider.gameObject;
            }
        }
    }

    private Vector2 ScreenToImage(Vector2 screenPos)
    {
        Vector2 res = vm.Resolution;
        if (res == Vector2.zero)
        {
            Debug.Log("Cannot load video resolution. Probably video is not loaded");
            return invalidVector;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float aspectImage = res.x / res.y;
        float aspectScreen = screenSize.x / screenSize.y;

        Vector2 imagePos = new Vector2();

        if (aspectImage >= aspectScreen)
        {
            Vector2 imageSize = new Vector2(screenSize.x, screenSize.x / aspectImage);
            float rangeStart = screenSize.y / 2f - imageSize.y / 2f;
            float rangeEnd = screenSize.y / 2f + imageSize.y / 2f;
            if (screenPos.y <= rangeStart || screenPos.y >= rangeEnd)
                return invalidVector;

            imagePos.x = screenPos.x / imageSize.x;
            imagePos.y = (screenPos.y - rangeStart) / imageSize.y;
        }
        else
        {
            Vector2 imageSize = new Vector2(screenSize.y * aspectImage, screenSize.y);
            float rangeStart = screenSize.x / 2f - imageSize.x / 2f;
            float rangeEnd = screenSize.x / 2f + imageSize.x / 2f;
            if (screenPos.x <= rangeStart || screenPos.x >= rangeEnd)
                return invalidVector;

            imagePos.x = (screenPos.x - rangeStart) / imageSize.x;
            imagePos.y = screenPos.y / imageSize.y;
        }

        return imagePos;
    }

    private Vector2 WorldToImage(Vector3 worldPos)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        return ScreenToImage(screenPos);
    }

    private Vector2 ImageToScreen(Vector2 imagePos)
    {
        Vector2 res = vm.Resolution;
        if (res == Vector2.zero)
        {
            Debug.Log("Cannot load video resolution. Probably video is not loaded");
            return invalidVector;
        }

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float aspectImage = res.x / res.y;
        float aspectScreen = screenSize.x / screenSize.y;

        Vector2 screenPos = new Vector2();

        if (aspectImage >= aspectScreen)
        {
            Vector2 imageSize = new Vector2(screenSize.x, screenSize.x / aspectImage);
            float rangeStart = screenSize.y / 2f - imageSize.y / 2f;

            screenPos.x = imagePos.x * imageSize.x;
            screenPos.y = imagePos.y * imageSize.y + rangeStart;
        }
        else
        {
            Vector2 imageSize = new Vector2(screenSize.y * aspectImage, screenSize.y);
            float rangeStart = screenSize.x / 2f - imageSize.x / 2f;

            screenPos.x = imagePos.x * imageSize.x + rangeStart;
            screenPos.y = imagePos.y * imageSize.y;
        }

        return screenPos;
    }

    private Vector3 ImageToWorld(Vector2 imagePos)
    {
        Vector2 screenPos = ImageToScreen(imagePos);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        return worldPos;
    }

    private Color[] GetSequentialColor(int num)
    {
        Color[] colors = new Color[num];
        Vector4 start = new Vector4(0f, 0f, 1f, 63f / 255f); // Blue
        Vector4 end = new Vector4(0f, 1f, 0f, 63f / 255f); // Green

        for (int i = 0; i < num; i++)
            colors[i] = Vector4.Lerp(start, end, (float)i / (float)num);
        return colors;
    }

    public void ChangeMode()
    {
        annotMode = !annotMode;
        modeChangeButtonText.text = annotMode ? "End" : "Annot Mode";
    }

    public void GenerateDefaultAnnotForm(int num)
    {
        GenerateDefaultAnnotForm(np.zeros(new Shape(num, num)));
    }

    public void GenerateDefaultAnnotForm(NDArray adjMat)
    {
        int num = adjMat.shape[0]; // (N, N)
        Vector3 origin = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2f, Screen.height / 2f));
        float r = 3;
        Vector3[] circlePos = new Vector3[num];
        for (int i = 0; i < num; i++)
        {
            float angle = (float)i / (float)num * 2f * Mathf.PI;
            Vector3 newPos = new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle), 0f) + origin;
            newPos.z = 0f;
            circlePos[i] = newPos;
        }

        GenerateDefaultAnnotForm(adjMat, circlePos);
    }

    public void GenerateDefaultAnnotForm(NDArray adjMat, Vector3[] initPos)
    {
        int num = adjMat.shape[0]; // (N, N)
        GameObject[] annots = new GameObject[num];
        Color[] colors = GetSequentialColor(num);
        for (int i = 0; i < num; i++)
        {
            annots[i] = Instantiate(annotPrefab, initPos[i], Quaternion.identity);
            AnnotationPoint ap = annots[i].AddComponent<AnnotationPoint>();
            ap.Init(i, colors[i]);
        }

        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (adjMat[i, j] == 1)
                {
                    DrawLine(annots[i].transform, annots[j].transform, Color.red);
                }
            }
        }
    }

    public void OpenAdjFile()
    {
        NDArray adjmat = null;
        NDArray initpos = null;

        string path = EditorUtility.OpenFilePanel("Open Adjacency Matrix File", "", "npy");
        if (path.Length != 0)
        {
            adjmat = np.load(path).astype(NPTypeCode.Int32);
            Debug.Assert(adjmat.shape.Length == 2 && adjmat.shape[0] == adjmat.shape[1]);

            records = np.empty(new Shape(vm.AnnotFrames.Length, adjmat.shape[0], 2), NPTypeCode.Float);
            records[":"] = -1;

            path = EditorUtility.OpenFilePanel("Open Initial Position File", ".", "npy");
            if (path.Length != 0)
            {
                initpos = np.load(path).astype(NPTypeCode.Float);
                Debug.Assert(adjmat.shape[0] == initpos.shape[0] && initpos.shape[1] == 2);
                records[0] = initpos;
            }
        }


        if (adjmat == null)
            GenerateDefaultAnnotForm(20);
        else
        {
            if (initpos == null)
                GenerateDefaultAnnotForm(adjmat);
            else
            {
                Vector3[] initPos = new Vector3[adjmat.shape[0]];
                for (int i = 0; i < adjmat.shape[0]; i++)
                {
                    initPos[i].x = initpos[i, 0];
                    initPos[i].y = initpos[i, 1];
                    initPos[i].z = 0f;
                }
                GenerateDefaultAnnotForm(adjmat, initPos);
            }
        }
        annotObjects = GameObject.FindGameObjectsWithTag("Annotation");
    }

    public void SaveAnnotationFile()
    {
        if (records == null)
            Debug.LogError("No Annotation Recorded");

        string path = EditorUtility.SaveFilePanel("Save Annotation File", "", vm.VideoFileName, "npy");
        Debug.Log(path);
        List<string> dirs = path.Split('/').ToList();
        string basePath = path.Remove(path.Length - 4);
        np.save(basePath + "_annot.npy", records);
        np.save(basePath + "_index.npy", new NDArray(vm.AnnotFrames));
        Debug.Log("Saved Annotation File at " + basePath + "_[annot/index].npy");
    }

    public void SyncObjToArray()
    {
        if (annotObjects.Length > 0)
        {
            NDArray annots = np.zeros(new Shape(annotObjects.Length, 2), NPTypeCode.Float);
            foreach (GameObject ao in annotObjects)
            {
                int an = ao.GetComponent<AnnotationPoint>().annotNumber;
                Vector2 v = WorldToImage(ao.transform.position);
                annots[an, 0] = v.x;
                annots[an, 1] = v.y;
            }

            records[vm.CurrentFrameIdx] = annots;
        }
    }

    public void SyncArrayToObj()
    {
        if (annotObjects.Length > 0)
        {
            foreach (GameObject ao in annotObjects)
            {
                int an = ao.GetComponent<AnnotationPoint>().annotNumber;
                Vector3 v = ImageToWorld(new Vector2(records[vm.CurrentFrameIdx, an, 0], records[vm.CurrentFrameIdx, an, 1]));
                ao.transform.position = v;
            }
        }
    }

    void DrawLine(Transform start, Transform end, Color color)
    {
        GameObject sk = new GameObject();
        sk.transform.SetParent(start);

        SkeletonLine skline = sk.AddComponent<SkeletonLine>();
        skline.init();
        skline.SetJoints(start, end);
        skline.SetColor(color);
        skline.SetWidth(0.01f);
    }
}
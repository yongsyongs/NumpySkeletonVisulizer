using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoManager : MonoBehaviour
{
    [SerializeField] private int annotFrameCount = 100;
    [SerializeField] private Text frameIdxText;

    private VideoPlayer player;
    private int currentFrameIdx = 0;
    private int maxFrame;
    private Vector2 resolution;
    private int[] annotFrames;

    private bool isFirstReady = true;
    private bool loaded = false;

    public int CurrentFrame
    {
        get { return AnnotFrames[currentFrameIdx]; }
    }
    public int CurrentFrameIdx
    {
        get { return currentFrameIdx; }
    }
    public int AnnotFrameCount
    {
        get { return annotFrameCount; }
    }
    public int[] AnnotFrames
    {
        get
        { return annotFrames; }
    }
    public Vector2 Resolution
    {
        get 
        {
            if (resolution == null)
                return Vector2.zero;
            else
                return resolution;
        }
    }

    public string VideoFileDirectory;
    public string VideoFileName;

    void Start()
    {
        player = GetComponent<VideoPlayer>();
        resolution = Vector2.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (loaded)
            {
                if (currentFrameIdx <= 0)
                    currentFrameIdx = 0;
                else
                    currentFrameIdx--;
                player.frame = annotFrames[currentFrameIdx];
            }
            else
                Debug.Log("Video is not loaded");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if(loaded)
            {
                if (currentFrameIdx >= annotFrameCount - 1)
                    currentFrameIdx = annotFrameCount - 1;
                else
                    currentFrameIdx++;
                player.frame = annotFrames[currentFrameIdx];
            }
            else
                Debug.Log("Video is not loaded");
        }
        frameIdxText.text = "Frame: " + currentFrameIdx.ToString();
    }

    public void OpenVideoFile()
    {
        string path = EditorUtility.OpenFilePanel("Open Video File", "", "*");
        if (path.Length != 0)
        {
            isFirstReady = true;
            loaded = false;
            List<string> dirs = path.Split('/').ToList();
            VideoFileDirectory = string.Join("/", dirs.GetRange(0, dirs.Count - 1));
            VideoFileName = dirs[dirs.Count - 1];
            PlayVideo("file://" + path);
        }
    }

    private void PlayVideo(string path)
    {
        player.source = VideoSource.Url;
        player.url = path;
        player.audioOutputMode = VideoAudioOutputMode.None;
        player.EnableAudioTrack(0, false);
        player.waitForFirstFrame = true;
        player.sendFrameReadyEvents = true;

        player.frameReady += OnFrameReady;

        player.Prepare();
        player.Play();
        //StartCoroutine(initPause());
    }
    
    void OnFrameReady(VideoPlayer source, long frameIdx)
    {
        if (isFirstReady)
        {
            //if(GameObject.FindGameObjectsWithTag("Annotation").Length != 0)
            //{
            //    GetComponent<AnnotationManager>().GenerateDefaultAnnotForm(20);
            //}
            player.frame = 0;
            player.Pause();
            maxFrame = (int)player.frameCount - 1;
            annotFrames = new int[annotFrameCount];
            annotFrames[0] = 0;
            for (int i = 1; i < annotFrameCount; i++)
                annotFrames[i] = (int)((float)annotFrames[i - 1] + (float)maxFrame / (float)annotFrameCount);
            currentFrameIdx = 0;
            resolution = new Vector2(player.width, player.height);
            isFirstReady = false;
            loaded = true;
            Debug.Log("Video Loaded! maxFrame: " + maxFrame.ToString());
        }
    }
}
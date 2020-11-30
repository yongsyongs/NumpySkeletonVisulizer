using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField] private uint frameInterval = 5;

    private VideoPlayer player;
    private uint currentFrame = 0;
    private uint maxFrame;
    private Vector2 resolution;

    private bool isFirstReady = true;
    private bool loaded = false;

    public uint CurrentFrame
    {
        get { return currentFrame; }
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
                if (currentFrame <= frameInterval)
                    currentFrame = 0;
                else
                    currentFrame -= frameInterval;
                player.frame = currentFrame;
            }
            else
                Debug.Log("Video is not loaded");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if(loaded)
            {
                if (currentFrame >= maxFrame)
                    currentFrame = maxFrame;
                else
                    currentFrame += frameInterval;
                player.frame = currentFrame;
            }
            else
                Debug.Log("Video is not loaded");
        }
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
            maxFrame = (uint)player.frameCount - 1;
            currentFrame = 0;
            resolution = new Vector2(player.width, player.height);
            isFirstReady = false;
            loaded = true;
            Debug.Log("Video Loaded! maxFrame: " + maxFrame.ToString());
        }
        else
        {
        }
            Debug.Log("Ready Frame: " + frameIdx.ToString());
    }
}
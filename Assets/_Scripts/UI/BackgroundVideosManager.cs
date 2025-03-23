using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideosManager : MonoBehaviour
{
    public VideoPlayer player;
    public VideoClip loopClip;

    private void OnEnable() { player.loopPointReached += StartLoopVideo; }
    private void OnDisable() { player.loopPointReached -= StartLoopVideo; }

    private void StartLoopVideo(VideoPlayer vp)
    {
        player.clip = loopClip;
        player.loopPointReached -= StartLoopVideo;
    }
}

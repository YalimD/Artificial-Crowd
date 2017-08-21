// Examples of VideoPlayer function

using UnityEngine;

public class MyVideoPlayer : MonoBehaviour
{
    UnityEngine.Video.VideoPlayer videoPlayer;
    string videoName = "video1.mp4";
    private bool videoPlaying;

    public bool VideoPlaying { get { return videoPlaying; } }

    //Resume, pause and stop the video
    public void resumeVideo()
    {
        videoPlaying = true;
        Time.timeScale = 1;
    }

    public void pauseVideo()
    {
        Time.timeScale = 0;
        videoPlaying = false;
    }

    public void stopVideo()
    {
        RVO.AgentBehaviour.Instance.restart();
        videoPlaying = false;
        videoPlayer.frame = 0;
        Time.timeScale = 0;
    }

    public string getVideoName()
    {
        return videoName;
    }

    //Initialize the video player
	public void StartVideo(string videoName)
	{
        this.videoName = videoName + ".mp4";
		GameObject camera = GameObject.Find("BackGroundCamera");

		videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
		videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;
		videoPlayer.targetCameraAlpha = 1F;
		videoPlayer.url = "Assets/Video/" + this.videoName;

		videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;

        videoPlayer.Pause();
        videoPlaying = true;
		Debug.Log(videoPlayer.frameRate);
        
	}

    float deltaTime = 0.0f;

    public void UpdateFrame()
	{
        
        videoPlaying = (videoPlayer.frame < (long)videoPlayer.frameCount);
        //Call this from behaviour class
        if (videoPlaying)
        {
            videoPlayer.StepForward();
        }

	}

}
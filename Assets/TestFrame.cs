// Examples of VideoPlayer function

using UnityEngine;

public class TestFrame : MonoBehaviour
{
	UnityEngine.Video.VideoPlayer videoPlayer;
	void Start()
	{
		// Will attach a VideoPlayer to the main camera.
		GameObject camera = GameObject.Find("MainCamera");

		// VideoPlayer automatically targets the camera backplane when it is added
		// to a camera object, no need to change videoPlayer.targetCamera.
		videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

		// By default, VideoPlayers added to a camera will use the far plane.
		// Let's target the near plane instead.
		videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;

		// This will cause our scene to be visible through the video being played.
		videoPlayer.targetCameraAlpha = 1F;

		// Set the video to play. URL supports local absolute or relative paths.
		// Here, using absolute.
		videoPlayer.url = "D:/VideoRVO/UnityProject/3D-Reconstuction-From-Video-/Assets/Video/a.mp4";

		// Skip the first 100 frames.
		//videoPlayer.frame = 500;

		// Restart from beginning when done.
		videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
		// Each time we reach the end, we slow down the playback by a factor of 10.
		//videoPlayer.loopPointReached += EndReached;

		// Start playback. This means the VideoPlayer may have to prepare (reserve
		// resources, pre-load a few frames, etc.). To better control the delays
		// associated with this preparation one can use videoPlayer.Prepare() along with
		// its prepareCompleted event.
		//videoPlayer.Play();
        videoPlayer.Pause();
		Debug.Log(videoPlayer.frameRate);
        
	}

    float deltaTime = 0.0f;

    public void UpdateFrame()
	{
     //   deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
     //   float fps = (1.0f / (deltaTime * 1000.0f));
      //  videoPlayer.playbackSpeed = 30f / videoPlayer.frameRate;
       // videoPlayer.playbackSpeed = (fps * 1000f) / (31.75f);

        //videoPlayer.Prepare();
        //Call this from behaviour class
        videoPlayer.StepForward();
       // Debug.Log(videoPlayer.frame);

       // RVO.PedestrianProjection.Instance.nextFrame();
        //RVO.PedestrianProjection.Instance.frameNumber++;
        //

	}

	/*void EndReached(UnityEngine.Video.VideoPlayer vp)
	{
		vp.playbackSpeed = vp.playbackSpeed / 10.0F;
	}*/
}
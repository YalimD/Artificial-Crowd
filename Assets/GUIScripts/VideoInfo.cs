using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Insprired from http://wiki.unity3d.com/index.php?title=FramesPerSecond by Dave Hampson
 * 
 * This code calculates the frames per second currently on Unity and displays it on the related
 * text on screen.
 * 
 * Writes the name of the video on the screen.
 * 
 * Also displays the current time of the video.
 */ 

public class VideoInfo: MonoBehaviour {

    private Text fpsCount, name;
    private float deltaTime;

    void Start()
    {
        deltaTime = 0.0f;
        //Reference to parent panel

        //Reference to related children of the parent panel
        fpsCount = transform.FindChild("FPSCount").gameObject.GetComponent<Text>();
        name = transform.FindChild("VideoName").gameObject.GetComponent<Text>();

        name.text = "Video Name: " + GameObject.Find("Video").GetComponent<RawImage>().mainTexture.name;
    }

    //Update the delta time for fps and time of the video
	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        fpsCount.text = string.Format("FPS: {0:0.}",fps);
	}
 /*
	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}*/
}


using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {
    
	float timeRemaining = 0;
    int frameNum = 0;
    private readonly int MAXFRAME = 100;
	private readonly int MAXCAMERAS = 3;
	int camNum;
	public Camera classCamera,libCamera,marCamera;
	public Camera[] cameras;
	
	void Start()
	{
		cameras = new Camera[MAXCAMERAS];
		
		cameras[0] = classCamera;
		cameras[1] = libCamera;
		cameras[2] = marCamera;
		
		foreach(Camera c in cameras)
		{
			c.enabled = false;
		}
	}

	// Update is called once per frame
	void Update () {
		
        timeRemaining -= Time.deltaTime;
		
       // transform.Rotate(new Vector3(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * Time.deltaTime * 5.0f);
        if (timeRemaining <= 0 && frameNum != MAXFRAME) 
        {
            //transform.Translate(Vector3.left*25 * Time.deltaTime, Space.Self);

           /* transform.position = new Vector3((Mathf.Cos(frameNum * Mathf.Deg2Rad*2) * -20 + -105), 1050,
                (Mathf.Sin(frameNum * Mathf.Deg2Rad*2) * 20 + -43));

            transform.LookAt(GameObject.Find("target").transform.position);*/
		
			for(int i = 0;i < cameras.Length;i++)
			{
				cameras[i].enabled = true;
				Application.CaptureScreenshot("C:\\Users\\Rakosi\\Desktop\\VideoRVO\\Graphic\\Graphic Videos\\Marmara3CamerasStatic\\Screenshot" + (frameNum  + (i * MAXFRAME)) +  ".png");
				
				cameras[i].enabled = false;
				
			}

            timeRemaining += 0.001f;
            frameNum++;
        }

	}
}

using UnityEngine;

public class CameraScript : MonoBehaviour {

	void Start () {
	//demo
	}
    float TimeRemaining = 0.0;
    int frameNum = 0;
	// Update is called once per frame
	void Update () {
        timeRemaining -= Time.deltaTime;
       // transform.Rotate(new Vector3(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * Time.deltaTime * 5.0f);
        if (timeRemaining <= 0 && frameNum != 100) 
        {
            //transform.Translate(Vector3.left*25 * Time.deltaTime, Space.Self);

            transform.position = new Vector3((Mathf.Cos(frameNum * Mathf.Deg2Rad*2) * -20 + -105), 1050,
                (Mathf.Sin(frameNum * Mathf.Deg2Rad*2) * 20 + -43));

            transform.LookAt(GameObject.Find("target").transform.position);
            Application.CaptureScreenshot("C:\\Users\\Rakosi\\Desktop\\VideoRVO\\Graphic\\Graphic Videos\\MarmaraCircularRotate\\Screenshot" + frameNum + ".png");
            timeRemaining += 0.001f;
            frameNum++;
        }

	}
}

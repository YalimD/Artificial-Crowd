using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

/*
 * Written by Yalım Doğan
 * This code projects the pedestrians from given video feed onto the provided plan of the area
 * Version 0.1
 *  + Poles and other unnecessary obstacles are removed.
 *  - Testing for raytracing and its coordinate system
 *  - Adapting the coordinate system of given output to ray tracing
 *  - Using the given human model to simulate the behaviour of real agents
 *  - Working on meanshift etc for navigable area generation and debugging using the provided walkableDebug mesh
 *  - Generating the mesh from walkable areas (polygons)
 *  - Adapting the mesh to mutiple layers for ARA*
 */ 

public class PedestrianProjection : MonoBehaviour {

    //As the process is only 100 frames long, we need longer duration in order to fully project the recorded
    //behaviour
    //If not provided, we can safely assume that they preserve their velocities

    string[] frames; //Frames containing the pedestrian information
    int frameNumber; //Current frame index
    List<ProjectedAgent> realAgents; //ArrayList of real agents, we don't know their size
    public GameObject ball;
    float distance = 5000.0F; //Creating distance from camera
    int resX, resY;

        
	// Load the given output file
	void Start () {
        frameNumber = 1;
       
        frames = System.IO.File.ReadAllLines(@"Output.txt");
        Debug.Log("Number of frames loaded: " + frames.Length);

        //Read the image resolution from the given output file
        //I HAVE EDITED THE OUTPUT FILE TO CONTAIN THE FRAME RESOLUTION INFORMATION
        string[] info = frames[0].Split(' ');
        resX = int.Parse(info[1]);
        resY = int.Parse(info[2]);

        Debug.Log("Adjust the aspect ratio of the screen to:" + resX + "x" + resY);
	}


	// Update is called once per frame
    //Starting from first frame, on each frame passed in game, update the agents 
    //Possible problem: Constantly changing the velocity of agents is necessary; but would it cause serious problems ?
	void Update () {

        Debug.Log(frameNumber);
        //If there are still frames to process
        if (frameNumber < frames.Length)
        {
            string[] output = frames[frameNumber].Split(',');

            //-1 as last space is also counted as a string
            for (int index = 1; index < output.Length-1; index = index + 7)
            {
                //Create a ray to the feet of the detected pedestrian (pos.y - scale.y/2)/3
                //It is necessary to convert the axis of pedestrian coordinates of output file (upper left) to our axis (lower left)
                //For this, resolution.y - (coordinate.y/3) and 
                Ray ray = Camera.main.ScreenPointToRay(new Vector2((float.Parse(output[index + 1], CultureInfo.InvariantCulture)) / 3, (float) resY - ((float.Parse(output[index + 2], CultureInfo.InvariantCulture) - float.Parse(output[index + 6], CultureInfo.InvariantCulture) / 2) / 3)));
               // Debug.Log(new Vector2((int.Parse(output[index + 1]) - int.Parse(output[index + 5]) / 2) / 3, (int.Parse(output[index + 2]) - int.Parse(output[index + 6]) / 2) / 3));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, distance))
                {
                    //Rearrange this prefab with id so we can track it
                    //If in next frame, same detected id has discolated more than velocity (+ an error margin)
                    //than the pedestrian's control is taken by artificial agents


                    Instantiate(ball, hit.point, new Quaternion());
                    Debug.Log(hit);
                }
            }
        }
        frameNumber++;
	}

}

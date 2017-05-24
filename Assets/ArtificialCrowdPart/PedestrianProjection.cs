using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

/*
 * Written by Yalım Doğan
 * This code projects the pedestrians from given video feed onto the provided plan of the area
 * Version 0.8
 *  + Poles and other unnecessary obstacles are removed.
 *  + Testing for raytracing and its coordinate system
 *  + Adapting the coordinate system of given output to ray tracing
 *  + Ray shouldn't consider agents (Done by IgnoreRayCast layer)
 *  + Positioning is made more adaptive to resolution, output file format
 *  + Velocity is also projected for each agent
 *  + Agents within certain angle to camera are ignored, to fight false positives. But angles are given manually
 *  + Velocities are updated for each agent, each frame
 *  + As the process is only few frames long (not a live stream yet), we need longer duration in order to fully project the recorded behaviour
 *  + If not provided, we can safely assume that they preserve their velocities. This can be also done by giving the control of those
 *    projected agents to artificial agent class (Therefore, wandering in the area with random objectives)
 *    
 *  TODOS:
 *  - RVO is under construction (initial design is done)
 *  - Using the given human model to simulate the behaviour of real agents
 *  - Working on meanshift etc for navigable area generation and debugging using the provided walkableDebug mesh
 *  - Generating the mesh from walkable areas (polygons)
 *  - Adapting the mesh to mutiple layers for ARA*
 *  - Remove agents who left the navigable area, by deleting the id. It can be done by sudden change in position which is not prevPos + prevVel
 *  
 */

public sealed class PedestrianProjection : MonoBehaviour
{

    //Singleton 
    private static readonly PedestrianProjection instance = new PedestrianProjection();

    public static PedestrianProjection Instance
    {
        get
        {
            return instance;
        }
    }

    //CONSTANTS

    //Threshold for velocity, used for validating the detection. Subject to change
    private const int velocityDifferenceThreshold = 10;

    //VARIABLES

    //Determined in editor for which video output to be used
    public int videoFile = 0;

    private string[] frames; //Frames containing the pedestrian information

    //As serkan magnified the image for better detection, I need to divide my coordinates with
    //the its multiplier. This info is given in the first line of the output file
    private int magnificationMultiplier;
    private int frameNumber; //Current frame index
    private int resX, resY; //Resolution of given frames

    //Dictionary for agents, where the key is their id
    static Dictionary<int, GameObject> realAgents = new Dictionary<int, GameObject>(); 

    private GameObject model; //Dummy for agents
    private GameObject newAgent; //Reference to the created agent

    private const float distance = 5000.0F; //Creating distance from camera

    //Enum for video output files
    enum VideoOutputs
    {
        None,
        Output,
        Output2,
    };

    //The execution starts here
    void Start()
    {

        frameNumber = 1;

        //Read the output file
        string file = ((VideoOutputs)videoFile).ToString() + ".txt";
        frames = System.IO.File.ReadAllLines(@file);
        Debug.Log("Number of frames loaded: " + frames.Length);

        //Initialize the dictionary of agents
   //     realAgents = new Dictionary<int, GameObject>();

        //Read the image resolution from the given output file
        //I HAVE EDITED THE OUTPUT FILE TO CONTAIN THE FRAME RESOLUTION INFORMATION AND MAGNIFICATION MULTIPLIER IF IT HAS BEEN USED
        string[] info = frames[0].Split(' ');
        resX = int.Parse(info[1]);
        resY = int.Parse(info[2]);
        magnificationMultiplier = int.Parse(info[3]);

        //Load the pedestrian model
        model = Resources.Load("ProjectedAgent", typeof(GameObject)) as GameObject;

        Debug.Log("The aspect ratio of the output file is :" + resX + "x" + resY + " with multipication multiplier " + magnificationMultiplier);

    }

    //Reads and returns the float representation of the given string
    float floatFromText(string coord, bool convert)
    {
        return float.Parse(coord, CultureInfo.InvariantCulture) / (convert ? magnificationMultiplier : 1);
    }

    //Returns the feet location of given origin of the detector, also reverses the y coordinate system
    float feetAdjuster(string origin, string distance)
    {
        return (float)resY - (floatFromText(origin, false) - (floatFromText(distance, false) / 2)) / magnificationMultiplier;
    }

    //Generates the ray for pixel at x and y
    Ray rayGenerator(float x, float y)
    {
        return Camera.main.ScreenPointToRay(new Vector2(x / (resX) * Screen.width, y / (resY) * Screen.height));
    }

    //Generates the velocity from given position and velocity information 
    Vector3 velocityGenerator(string[] input, int index, Vector3 origin)
    {
        Vector3 vel = new Vector3(floatFromText(input[index + 3], true), 0, -floatFromText(input[index + 4], true));
        vel = vel + new Vector3(floatFromText(input[index + 1], true), 0, feetAdjuster(input[index + 2], input[index + 6]));

        //Project the vel + pos (which is proposed position)
        //Ray velRay = Camera.main.ScreenPointToRay(new Vector2(agentVelocity.x / (resX) * Screen.width, ((float)resY - ((agentVelocity.z - float.Parse(output[index + 6], CultureInfo.InvariantCulture) / 2) / magnificationMultiplier)) / resY * Screen.height));
        Ray velRay = rayGenerator(vel.x, vel.z);
        RaycastHit hit;
        Physics.Raycast(velRay, out hit, distance);

        //Get the projected velocity (projected endpoint - origin)
        vel = ((new Vector3(hit.point.x, hit.point.y/* + model.transform.lossyScale.y * 4*/, hit.point.z))) - (origin);

        return vel;
    }

    /*
     * This method verifies the size of the detector, by inversely relating it to its distance from camera.
     * The width of the detector is not important, but the height is used. The angle between the top of the detector and the camera
     * (which is the angle created by the ray that hits the plane) should be between certain angles 
     * TODO: Relate those certain angles to camera coordinates
     * 
     */

    const float minAngle = 15.0f;
    const float maxAngle = 60.0f;

    bool verifyDetectorSize(String height, RaycastHit hit)
    {
        //Find the angle ray makes with camera
        float angle = (float)(180 * Math.Asin(Math.Abs(transform.position.y - hit.point.y) / (Vector3.Distance(transform.position, hit.point))) / Math.PI);
        //Debug.Log("The angle is " + angle);

        //Angles are subject to change (I am thinking of relating them to the positioning of the camera)
        return (minAngle < angle) && (angle < maxAngle);
        //return (floatFromText(width, true) * floatFromText(height, true)) * hit.distance >= 250000; //Old size comparator
    }

    /* Update is called once per frame
     * Starting from first frame, on each frame passed in game, update the agents 
     * Each time:
     *  - Project pedestrians if they have not been added before
     *  - Check their location each frame, for any misdetections
     *  - Update their velocity according to given output file (NOT DONE YET)
     *  
     */
    void Update()
    {

        //If there are still frames to process
        if (frameNumber < frames.Length)
        {
            //Frame info: FRAMENUM, AGENTID, POSX, POSY, VELX, VELY, WIDTH, HEIGHT, (SPACE AT END)
            string[] output = frames[frameNumber].Split(',');

            //Debug.Log("Frame Number:" + frameNumber + " with " + output.Length +  " agent detector input read ( " + ((output.Length - 2) / 7)  +  ")");

            //-1 as last space is also counted as a string
            for (int index = 1; index < output.Length - 1; index = index + 7)
            {
                //Create a ray to the feet of the detected pedestrian (pos.y - scale.y/2) / magnificationMultiplier
                //It is necessary to convert the axis of pedestrian coordinates of output file (upper left) to our axis (lower left)
                //For this, resolution.y - (coordinate.y/ magnificationMultiplier) and 

                //We need to adjust the coordinates related to the resolution of the screen which is used.

                float rayX = floatFromText(output[index + 1], true);
                //    float rayY = (float)resY - (floatFromTextGenerator(output[index + 2]) - (floatFromTextGenerator(output[index + 6])) / 2) / magnificationMultiplier;
                float rayY = feetAdjuster(output[index + 2], output[index + 6]);
                // Ray ray = Camera.main.ScreenPointToRay(new Vector2(rayX / resX * Screen.width , rayY / resY * Screen.height));
                Ray ray = rayGenerator(rayX, rayY);

                // Debug.Log("Ray: " + new Vector2(rayX / resX * Screen.width, rayY / resY * Screen.height));

                RaycastHit hit;
                //   Debug.Log("Agent ID in file: " + int.Parse(output[index]) + " with loc " + float.Parse(output[index + 1], CultureInfo.InvariantCulture) + " - " + float.Parse(output[index + 2], CultureInfo.InvariantCulture));
                int readId = int.Parse(output[index]);
                //Debug.Log(readId);
                GameObject tryOutput; //Used for trygetvalue method

                if (Physics.Raycast(ray, out hit, distance) && !realAgents.TryGetValue(readId, out tryOutput) && verifyDetectorSize(output[index + 6], hit)) //New agent
                {
                    //   Debug.Log("Agent with " + readId + " created");

                    //Hold the reference to the newly created agent
                    Vector3 agentPos = new Vector3(hit.point.x, hit.point.y/* + model.transform.lossyScale.y * 4*/, hit.point.z);

                    /*
                     * OLD Velocity system
                 //   Vector3 agentVelocity = new Vector3(floatFromText(output[index + 3], true), 0, -floatFromText(output[index + 4], true));
                    Debug.Log("VEL: " + agentVelocity);
                    agentVelocity = agentVelocity + new Vector3(floatFromText(output[index + 1],true), 0, feetAdjuster(output[index + 2],output[index + 6]));
                    
                    
                    //Project the vel + pos (which is proposed position)
                    //Ray velRay = Camera.main.ScreenPointToRay(new Vector2(agentVelocity.x / (resX) * Screen.width, ((float)resY - ((agentVelocity.z - float.Parse(output[index + 6], CultureInfo.InvariantCulture) / 2) / magnificationMultiplier)) / resY * Screen.height));
                    Ray velRay = rayGenerator(agentVelocity.x, agentVelocity.z);

                    Physics.Raycast(velRay, out hit, distance);

                    //Get the projected velocity
                    agentVelocity = ((new Vector3(hit.point.x, hit.point.y/* + model.transform.lossyScale.y * 4, hit.point.z))) - (agentPos);
                 //   agentVelocity.z = -agentVelocity.z; 

                 //   Debug.Log("velocity of agent " + readId + " is " + Vector3.Distance(agentPos, (new Vector3(hit.point.x, hit.point.y + model.transform.lossyScale.y * 4, hit.point.z))));
                    */

                    Vector3 agentVelocity = velocityGenerator(output, index, agentPos);

                    newAgent = (GameObject)Instantiate(model, agentPos + Vector3.up * 5, new Quaternion());

                    //DEBUGING THE LOCATION ONLY 
                    // agentVelocity = Vector3.zero;

                    //Modify the agent's parameters for its management.
                    newAgent.GetComponent<ProjectedAgent>().createAgent(agentVelocity, readId);

                    realAgents.Add(readId, newAgent);
                    //Debug.Log(hit);
                }

                /* If the agent already exists in the dictionary, we need to assess its current state.
                 * The detector which is attached to the agent might not be valid, best way to detect that is to 
                 * check its velocity. It might change its position MUCH more than its initial velocity if it is invalid.This will indicate either
                 *      - Agent left the area but its detector is now attached to someone else using the same ID
                 *      - The detector made an error
                 *  If it is valid, update its velocity
                 */
                else if (Physics.Raycast(ray, out hit, distance) && realAgents.TryGetValue(readId, out tryOutput))
                {
                    Vector3 proposedPos = new Vector3(hit.point.x, hit.point.y /*+ model.transform.lossyScale.y * 4*/, hit.point.z);
                    GameObject checkedAgent = realAgents[readId];
                    if (checkedAgent != null)
                    {
                        if (Vector3.Distance(checkedAgent.GetComponent<ProjectedAgent>().Pos, proposedPos) > velocityDifferenceThreshold)
                        {
                            //Get rid of the agent

                            //      Debug.Log("Agent with id " + readId + " is destroyed");

                            removeAgent(readId, checkedAgent);
                        }
                        else //Update the velocity
                        {
                            checkedAgent.GetComponent<ProjectedAgent>().Velocity = velocityGenerator(output, index, proposedPos);
                        }
                    }
                }
            }
            frameNumber++;
        }

    }

    internal void removeAgent(int agentId, GameObject agent)
    {
        //TODO: Remove from simulation too
        realAgents.Remove(agentId);
        Destroy(agent);
    }
}

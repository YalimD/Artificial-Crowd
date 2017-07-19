using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

/*
 * Written by Yalım Doğan
 * This code projects the pedestrians from given video feed onto the provided plan of the area
 * Version 1.0
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
 *  
 */

namespace RVO
{

    public sealed class PedestrianProjection : MonoBehaviour
    {
       
        private static PedestrianProjection instance = new PedestrianProjection();

        public static PedestrianProjection Instance
        {
            get
            {
                return instance;
            }
        }

        //CONSTANTS

        //Threshold for velocity, used for validating the detection. Subject to change
        private const float velocityDifferenceThreshold = 50f;

        //VARIABLES

        private bool isRunning = false;
        public bool IsSimulationRunning { get { return PedestrianProjection.Instance.isRunning; } }

        //The camera viewport parameters are needed for placement of camera render on scene (as width and height are same, only one is enough)
        private float viewPortScale;

        //Determined in editor for which video output to be used
        private int videoFile = 5;
        private string[] frames; //Frames containing the pedestrian information

        //As serkan magnified the image for better detection, I need to divide my coordinates with
        //the its multiplier. This info is given in the first line of the output file
        private float magnificationMultiplier;
        public int frameNumber; //Current frame index
        private int resX, resY; //Resolution of given frames

        //Dictionary for agents, where the key is their id
        Dictionary<int, GameObject> realAgents = new Dictionary<int, GameObject>();

        public Dictionary<int, GameObject> RealAgents { get { return PedestrianProjection.Instance.realAgents; } }

        private GameObject model; //Dummy for agents
        private GameObject newAgent; //Reference to the created agent

        private const float distance = 5000.0F; //Creating distance from camera

        TestFrame video;

        //Are projected agents visible to user ?
        private bool visibility;
        public bool Visibility { set { PedestrianProjection.Instance.visibility = value; } get { return PedestrianProjection.Instance.visibility; } }

        //Enum for video output files
        enum VideoOutputs
        {
            None,
            Output,
            Output2,
            Output3,
            Output4,
            Output6
        };

        //The execution starts here
        public void InitiateProjection()
        {
            PedestrianProjection.Instance.isRunning = true;
            PedestrianProjection.Instance.frameNumber = 1;

            //Read the output file
            string file = ((VideoOutputs)videoFile).ToString() + ".txt";
            PedestrianProjection.Instance.frames = System.IO.File.ReadAllLines(@file);
            Debug.Log("Number of frames loaded: " + frames.Length);

            //Initialize the dictionary of agents
            //     realAgents = new Dictionary<int, GameObject>();

            //Read the image resolution from the given output file
            //I HAVE EDITED THE OUTPUT FILE TO CONTAIN THE FRAME RESOLUTION INFORMATION AND MAGNIFICATION MULTIPLIER IF IT HAS BEEN USED
            string[] info = frames[0].Split(' ');
            PedestrianProjection.Instance.resX = int.Parse(info[1]);
            PedestrianProjection.Instance.resY = int.Parse(info[2]);
            PedestrianProjection.Instance.magnificationMultiplier = float.Parse(info[3]);

            //Load the pedestrian model
            PedestrianProjection.Instance.model = Resources.Load("ProjectedAgent", typeof(GameObject)) as GameObject;

            PedestrianProjection.Instance.viewPortScale = GameObject.Find("MainCamera").transform.GetComponent<Camera>().rect.width;
            Debug.Log("The aspect ratio of the output file is :" + resX + "x" + resY + " with multipication multiplier " + magnificationMultiplier);

            PedestrianProjection.Instance.visibility = true;

            PedestrianProjection.Instance.video = GameObject.Find("MainCamera").GetComponent<TestFrame>();

        }

        //Reads and returns the float representation of the given string
        float floatFromText(string coord, bool convert)
        {
            return float.Parse(coord, CultureInfo.InvariantCulture) / (convert ? PedestrianProjection.Instance.magnificationMultiplier : 1);
        }

        //Returns the feet location of given origin of the detector, also reverses the y coordinate system
        float feetAdjuster(string origin, string distance)
        {
            return (float)PedestrianProjection.Instance.resY - ((floatFromText(origin, false) + (floatFromText(distance, false) / 2)) / PedestrianProjection.Instance.magnificationMultiplier);
        }

        //Generates the ray for pixel at x and y
        Ray rayGenerator(float x, float y)
        {
            return Camera.main.ScreenPointToRay(new Vector3(x / (PedestrianProjection.Instance.resX) * Screen.width * PedestrianProjection.Instance.viewPortScale, y / (PedestrianProjection.Instance.resY) * Screen.height * PedestrianProjection.Instance.viewPortScale, 0));
        }

        //Generates the velocity from given position and velocity information 
        Vector3 velocityGenerator(string[] input, int index, Vector3 origin)
        {
            //Was using velocity data from output file, which was wrong
          //  Vector3 vel = new Vector3(floatFromText(input[index + 3], true), 0, -floatFromText(input[index + 4], true));
            //vel = vel + new Vector3(floatFromText(input[index + 1], true), 0, feetAdjuster(input[index + 2], input[index + 6]));


            Vector3 vel = new Vector3(floatFromText(input[index + 1], true), 0, feetAdjuster(input[index + 2], input[index + 6]));
            

            //Project the vel + pos (which is proposed position)
            //Ray velRay = Camera.main.ScreenPointToRay(new Vector2(agentVelocity.x / (resX) * Screen.width, ((float)resY - ((agentVelocity.z - float.Parse(output[index + 6], CultureInfo.InvariantCulture) / 2) / magnificationMultiplier)) / resY * Screen.height));
            Ray velRay = rayGenerator(vel.x, vel.z);
            RaycastHit hit;
            Physics.Raycast(velRay, out hit, distance);

            //Get the projected velocity (projected endpoint - origin)
            vel = new Vector3(hit.point.x, hit.point.y/* + model.transform.lossyScale.y * 4*/, hit.point.z) - (origin);
           // vel = new Vector3(-vel.z, vel.y,vel.x);
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
        void FixedUpdate()
       // void Update()
        //public void nextFrame()
        {
            if (PedestrianProjection.Instance.isRunning)
            {
                
                //If there are still frames to process
                if (PedestrianProjection.Instance.frameNumber < PedestrianProjection.Instance.frames.Length)
                {
                    //Frame info: FRAMENUM, AGENTID, POSX, POSY, VELX, VELY, WIDTH, HEIGHT, (SPACE AT END)
                    string[] output = PedestrianProjection.Instance.frames[PedestrianProjection.Instance.frameNumber].Split(',');

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

                        if (Physics.Raycast(ray, out hit, distance) && !PedestrianProjection.Instance.realAgents.TryGetValue(readId, out tryOutput) && verifyDetectorSize(output[index + 6], hit)) //New agent
                        {
                            //   Debug.Log("Agent with " + readId + " created");

                            //Hold the reference to the newly created agent
                            Vector3 agentPos = new Vector3(hit.point.x, hit.point.y /*+ 3.5f * PedestrianProjection.Instance.model.transform.localScale.y*/, hit.point.z);

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
                           // Vector3 agentVelocity = Vector3.zero;

                            newAgent = (GameObject)Instantiate(PedestrianProjection.Instance.model, agentPos /*+ Vector3.up * 3.5f * PedestrianProjection.Instance.model.transform.localScale.y*/, new Quaternion());

                            //DEBUGING THE LOCATION ONLY 
                            // agentVelocity = Vector3.zero;
                            int agentId; //seperate from the readId, as that is used for tracking from the output file, while this is used for tracking in RVO
                            RVO.Vector2 origin = new Vector2(agentPos.x, agentPos.z);
                            RVO.Agent agentReference = Simulator.Instance.addAgent(origin, false, out agentId);

                            Simulator.Instance.setAgentPosition(agentId, origin);

                            //Modify the agent's parameters for its management.
                            newAgent.GetComponent<ProjectedAgent>().createAgent(agentVelocity, readId, agentId, agentReference);

                            PedestrianProjection.Instance.realAgents.Add(readId, newAgent);
                            newAgent.GetComponent<ProjectedAgent>().IsSync = true;


                         //   Debug.Log("pos of agent " + readId + " is " + agentPos);
                        }

                        /* If the agent already exists in the dictionary, we need to assess its current state.
                         * The detector which is attached to the agent might not be valid, best way to detect that is to 
                         * check its velocity. It might change its position MUCH more than its initial velocity if it is invalid.This will indicate either
                         *      - Agent left the area but its detector is now attached to someone else using the same ID
                         *      - The detector made an error
                         *  If it is valid, update its velocity
                         *  TODO: We also need to check where it is currently, so we can eliminate him from simulation if he goes out of navigable areas
                         */
                        else if (Physics.Raycast(ray, out hit, distance) && PedestrianProjection.Instance.realAgents.TryGetValue(readId, out tryOutput))
                        {
                            Vector3 proposedPos = new Vector3(hit.point.x, hit.point.y /*+ 3.5f * PedestrianProjection.Instance.model.transform.localScale.y*/, hit.point.z);
                            GameObject checkedAgent = PedestrianProjection.Instance.realAgents[readId];
                            Vector3 checkedVelocity = proposedPos - checkedAgent.GetComponent<ProjectedAgent>().Pos;
                            if (checkedAgent != null)
                            {

                                if (Vector3.Distance(checkedAgent.GetComponent<ProjectedAgent>().Pos, proposedPos) > velocityDifferenceThreshold
                                    /*checkedVelocity.magnitude > velocityDifferenceThreshold*/)
                                {
                                    //Get rid of the agent

                                    Debug.Log("Agent with id " + readId + " is destroyed");

                                    PedestrianProjection.Instance.removeAgent(readId, checkedAgent);
                                }
                                else //Update the velocity
                                {
                                    checkedAgent.GetComponent<ProjectedAgent>().Velocity = checkedVelocity;
                                    checkedAgent.GetComponent<ProjectedAgent>().IsSync = true;
                               }
                            }
                        }        
                

                    }
                  //  Debug.Log("Projecting frame" + PedestrianProjection.Instance.frameNumber);
                    PedestrianProjection.Instance.video.UpdateFrame();
                    PedestrianProjection.Instance.frameNumber++;
                }
                else
                {
                    PedestrianProjection.Instance.isRunning = false;
                }




            }



        }

        internal void removeAgent(int agentId, GameObject agent)
        {
            //TODO: Remove from simulation too
            RVO.Simulator.Instance.agents_.Remove(agent.GetComponent<ProjectedAgent>().AgentReference);
            PedestrianProjection.Instance.realAgents.Remove(agentId);
            Destroy(agent);
        }
    }
}
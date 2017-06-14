using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * Agent Behaviour code extends the simulation code provided by RVO for the
 * spawn and management of the agents. It will also consider the projected agents coming from
 * the Pedestrian Projection class. But they won't have the change in velocity, only their velocities 
 * will be modified
 * 
 * This class manages the rvo by having an instance for its simulation logic. It creates artificial agents (Each has an agent reference) 
 * unless there are certain number of them wandering on the screen already. TODO: THIS MUST BE AN OPTION, HOW CROWDED THE USER WANTS THE SCREEN
 * 
 * This class communicates with Pedestrian Projection class for the managemtn of ProjectedAgents (Each has an agent reference)
 * It will cue the Pedestrian Projection to spawn and move the projected agents. This way, both agent types
 * will be managed from a single source, meaning better control. TODO: This isn't really necessary
 * 
 * The associated Agent references are managed by their own objects, therefore a centralized control SHOULDN'T be required for RVO.
 * 
 * The option to define goal for each artificial agent is also done in their own respective class.
 * 
 */

namespace RVO
{
    class AgentBehaviour : MonoBehaviour
    {
        //temporary vector for holding the goal locations
        private List<Vector2> goals;

        //Singleton 
        /*
        private static readonly AgentBehaviour instance = new AgentBehaviour();

        public static AgentBehaviour Instance
        {
            get
            {
                return instance;
            }
        }*/

        public const float maxNumberOfAgents = 10;
       // Dictionary<int, GameObject> artificialAgents; 
        private GameObject agentModel; //Dummy model for the agents
        List<GameObject> artificialAgents;

        //OPTIONS FOR RVO ABOUT THE AGENTS
        float numOfAgents = maxNumberOfAgents; //Total number of agents in the simulation
        int K = 20; //Number of neigbours
        float neighbourRange = 15f; //The range of visible area
        float reactionSpeed = 1000f; //Keep this high, so that agents react to neighbors faster

        // Initialize the artificial agents on the area.Assign goals randomly at a radius around the center of the square
        void Start()
        {
          //  artificialAgents = new Dictionary<int, GameObject>();
            artificialAgents = new List<GameObject>();
            agentModel = Resources.Load("ArtificialAgent", typeof(GameObject)) as GameObject;
            instantiateSimulation();
        }

        /*Initializes the goals of the artificial agents
         */
        private void defineGoals()
        {

            goals = new List<Vector2>();
            goals.Add(new Vector2(-130, -46));
            goals.Add(new Vector2(-135, -20));
            goals.Add(new Vector2(-100, -60));
            goals.Add(new Vector2(-90, -54));
            goals.Add(new Vector2(-64, -24));

            //Closer to middle area

            goals.Add(new Vector2(-112, -28));
            goals.Add(new Vector2(-112, -33));
            goals.Add(new Vector2(-116, -31));
            goals.Add(new Vector2(-115, -50));
            goals.Add(new Vector2(-110, -49));


        }

        //Clear the simulation, add agents and define goals
        void instantiateSimulation()
        {
            defineGoals();
            Simulator.Instance.Clear();
            /* Specify the global time step of the simulation. */
            Simulator.Instance.setTimeStep(1f);

            //Initiate the agent properties, these will also help us modify the agent behavior using the RVO simulation
            Simulator.Instance.setAgentDefaults(neighbourRange, K, reactionSpeed, 10.0f, 2f, 1f, new Vector2(0.0f, 0.0f));

            //Create the initial crowd of agents, depending on the current unocupied locations of the projected agents
            //Actually, we don't need a complicated conversion, the z coordinate will be given to the RVO as y and y coordinate from RVO
            //will be considered as Z.

            for (int artificialAgentId = 0; artificialAgentId < maxNumberOfAgents; artificialAgentId++)
            {
                //Generate them at the main area first, with goal of a random exit point (given manualy for now)
                //Generate random points and goals later (which will be determined individually)

                // SPAWN THEM FROM ANY OF THE GOALS, THE GOAL OF THE AGENT SHOULD BE DIFFERENT FROM ITS ORIGIN
                Vector2 origin = goals[(int)Math.Floor(UnityEngine.Random.value * goals.Count)];

                origin = goals[artificialAgentId % goals.Count];

                GameObject newArtAgent = (GameObject)Instantiate(agentModel, new Vector3(origin.x_, 0f, origin.y_), new Quaternion());


                //Initialize the RVO part of the agent by connecting the reference to the
                //related new agent
                int agentId;
                RVO.Agent agentReference = Simulator.Instance.addAgent(origin, true, out agentId);
                Simulator.Instance.setAgentPosition(agentId, origin); //TODO: is this necessary ?

                newArtAgent.GetComponent<ArtificialAgent>().createAgent(agentId, agentReference);

              //  artificialAgents.Add(artificialAgentId, newArtAgent);
                artificialAgents.Add(newArtAgent);

                newArtAgent.GetComponent<NavMeshAgent>().SetDestination(new Vector3((float)goals[(artificialAgentId + 1) % goals.Count].x(), 0f, (float)goals[(artificialAgentId + 1) % goals.Count].y()));


            }

            PedestrianProjection.Instance.InitiateProjection();

        }

        // Update the RVO simulation 
        void Update()
        {
            
            //I stop and resume the navmeshagent part as it causes ossilation between navmesh velocity and rvo velocity
            foreach (GameObject ag in artificialAgents)
            {
                ag.GetComponent<ArtificialAgent>().setPreferred();
                ag.GetComponent<NavMeshAgent>().Stop();
           }
            //Apply the step for determining the required velocity for each agent on the move
            Simulator.Instance.doStep();

            foreach (GameObject ag in artificialAgents) {
                ag.GetComponent<ArtificialAgent>().updateVelo();
                ag.GetComponent<NavMeshAgent>().Resume(); //Resume so that the velocity of the path is recalculated
            }
        }
    }
}
/*
            IList<Vector2> goals;
        IList<GameObject> agentSpheres; //Contains the sphere objects of agents used for updating their positions

        RVOSim()
        {

            goals = new List<Vector2>();
            agentSpheres = new List<GameObject>();
        }


        //OPTIONS
        float numOfAgents = 60f; //Total number of agents in the simulation
        int K = 20; //Number of neigbours
        float neighbourRange = 40f; //The range of visible area
        int startAngle = -90; //-179 to 180 for close to original. 
        int endAngle = 90; //Should be positive
        int scenarioInput = 1; //Circle or grid order
        int scenarioOutput = 1; //Mirror position,over circle or CS text
        bool visibleArea = false;//Make this true to see the visible area of each agent in red on grid.
        float reactionSpeed = 100f; //Keep this high, so that agents react to neighbors faster

        void setupScenario()
        {
            Simulator.Instance.Clear();
            Specify the global time step of the simulation
            Simulator.Instance.setTimeStep(1f);



            //Limit the range of neighbours as more will cause leak from outside of grids
            Simulator.Instance.setAgentDefaults(neighbourRange, K, reactionSpeed, 10.0f, 4f, 2.0f, new Vector2(0.0f, 0.0f));
            grid = GameObject.Find("Grid").GetComponent<Grid>();
            Simulator.Instance.setGrid(grid.gridX, grid.gridY, grid.noOfCells);

            float radius = numOfAgents;
            for (int i = 0; i < (int)radius; ++i)
            {
                Vector2 startingPos = new Vector2((i * 24) % 480 - 250, (int)(i / 20) * 120 - 150);
                if(scenarioInput == 1)
                    startingPos = 200f * new Vector2((float)Math.Cos(i * 2.0f * Math.PI / radius), (float)Math.Sin(i * 2.0f * Math.PI / radius));
                
                Simulator.Instance.addAgent(startingPos);

                //Create the objects as points on the Unity scene where each will represent an agent
                GameObject agent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                agent.transform.position = new Vector3(startingPos.x(), startingPos.y(), 0);
                agent.transform.localScale = new Vector3(6f, 6f, 6f);

                //Add the scripts necesseary for agents to determine their neighbours and move the sphere 
                //that represents them
                //agent.AddComponent<FanExploration>();
                Simulator.Instance.agents_[i].startingAngle = startAngle;
                Simulator.Instance.agents_[i].endingAngle = endAngle;
                //Add the goal of the agent as a position in the space

                if (scenarioOutput == 1)
                    goals.Add(-Simulator.Instance.getAgentPosition(i));
                else if (scenarioOutput == 2)
                    goals.Add(200.0f * new Vector2((float)Math.Cos(i * 2.0f * Math.PI / radius), (float)Math.Sin(i * 2.0f * Math.PI / radius)));
                else
                    goals.Add(new Vector2(scenarioData1x[i], scenarioData1y[i]));

                // goals.Add(new Vector2(scenarioData1x[i], scenarioData1y[i]));
                //Add the sphere component of the agent to the rendered area
                agentSpheres.Add(agent);


                agent.GetComponent<Renderer>().material.color = HSVtoRGB((i / radius), 1, 1, 1);

            }



        }

        //Update visualization changes the positions of each agent according to their movement
#if RVO_OUTPUT_TIME_AND_POSITIONS
        void updateVisualization()
        {
            grid.initCellAgents();

             Update all positions of agents  
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                Vector2 newPos = Simulator.Instance.getAgentPosition(i);
                agentSpheres[i].transform.position = new Vector3(newPos.x(),
                                                                newPos.y(), 0f);
                assignToCell(Simulator.Instance.getAgent(i));
            }

   


        }
#endif

        //Assigns the agent position to grid cell so it can be used to find the agents in the view range.
        //While each tile is searched, this array's last version is used.
        void assignToCell(Agent agent)
        {
            float x = agent.position_.x();
            float y = agent.position_.y();

            int cell_i = (int)(x / Mathf.Abs(grid.gridX / grid.noOfCells)) + grid.noOfCells / 2;
            int cell_j = (int)(y / Mathf.Abs(grid.gridY / grid.noOfCells)) + grid.noOfCells / 2;

            Grid.cellAgents[cell_i][cell_j].Add(agent);
        }

        //The velocities are arrange according to the neighbour status of the agent
        void setPreferredVelocities()
        {

            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                Vector2 goalVector = goals[i] - Simulator.Instance.getAgentPosition(i);

                if (RVOMath.absSq(goalVector) > 1.0f)
                {
                    goalVector = RVOMath.normalize(goalVector);
                }

                Simulator.Instance.setAgentPrefVelocity(i, goalVector);
            }
        }

        //This is used for checking if the agent has arrived to its destination
        bool reachedGoal()
        {
            for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i)
            {
                if (RVOMath.absSq(Simulator.Instance.getAgentPosition(i) - goals[i]) > Simulator.Instance.getAgentRadius(i) * Simulator.Instance.getAgentRadius(i))
                {
                    return false;
                }
            }

            return true;
        }

        //Inıtiates the simulation and sets up scenarios
        RVOSim sim;
        public void Start()
        {
            grid = GameObject.Find("Grid").GetComponent<Grid>();
            sim = new RVOSim();
            sim.setupScenario();
        }


        Grid grid;
        public void FixedUpdate()
        {
            if (!sim.reachedGoal())
            {
#if RVO_OUTPUT_TIME_AND_POSITIONS
                sim.updateVisualization();
#endif
                sim.setPreferredVelocities();

                //Apply the step for determining the required velocity for each agent on the move
                FanExploration.clearVisual();
                Simulator.Instance.doStep();

            }
        }

}*/
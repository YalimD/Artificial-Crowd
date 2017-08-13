using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * 
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
 * will be managed from a single source, meaning better control. 
 * 
 * The associated Agent references are managed by their own objects, therefore a centralized control SHOULDN'T be required for RVO.
 * 
 * The option to define goal for each artificial agent is also done in their own respective class.
 * 
 */

namespace RVO
{
    public sealed class AgentBehaviour : MonoBehaviour
    {
        //temporary vector for holding the goal locations
        private List<Vector2> goals;

        //Singleton 
        private static AgentBehaviour instance;

        public static AgentBehaviour Instance
        {
            get
            {
                return instance;
            }
        }

        private List<GameObject> agentModels;
        //private GameObject agentModel; //Dummy models for agents
        private List<GameObject> artificialAgents;

        public List<GameObject> ArtificialAgents { get { return instance.artificialAgents; } }

        public float initialNumberOfAgents = 10;
        public int numOfAgents { get { return instance.artificialAgents.Count; } }

        //OPTIONS FOR RVO ABOUT THE AGENTS
        private int numOfNeighboursConsidered = 20; //Number of neigbours

        public int NumOfNeighboursConsidered
        {
            get { return numOfNeighboursConsidered; }
            set { numOfNeighboursConsidered = value; }
        }

        private float neighbourRange = 2f; //The range of visible area

        public float NeighbourRange
        {
            get { return neighbourRange; }
            set { neighbourRange = value; }
        }
        private float maxSpeed = 1f; //Max speed of agents

        public float MaxSpeed
        {
            get { return maxSpeed; }
            set { maxSpeed = value; }
        }

        private float reactionSpeed = 100000f; //Keep this high, so that agents react to neighbors faster

        public float ReactionSpeed
        {
            get { return reactionSpeed; }
            set { reactionSpeed = value; }
        }

        //Are artificial agents visible to user ?
        private bool visibility;
        public bool Visibility { set { instance.visibility = value; } get { return instance.visibility; } }

        private int AAcollision;
        public int getAACollision() { return AAcollision; }

        public void incrementArtificialCollision()
        {
            lock (instance)
            {
                instance.AAcollision++;
            }
        }

        private int APcollision;
        public int getAPCollision() { return APcollision; }

        public void incrementProjectedCollision()
        {
            lock (instance)
            {
                instance.APcollision++;
            }
        }


        // Initialize the artificial agents on the area.Assign goals randomly at a radius around the center of the square
        void Start()
        {
            instance = new AgentBehaviour();
            instance.instantiateSimulation();
        }


        public void restart()
        {
            //Delete all agents in the simulation
            while (instance.artificialAgents.Count > 0)
            {
                Debug.Log("Deleted agent");
                instance.removeAgent(instance.artificialAgents[0]);
            }
            instance = new AgentBehaviour();
            instance.instantiateSimulation();

        }

        /*Initializes the goals of the artificial agents
         * Needs to be random
         */
        private void defineGoals()
        {
            
            instance.goals = new List<Vector2>();
            instance.goals.Add(new Vector2(-130, -46));
            instance.goals.Add(new Vector2(-135, -20));
            instance.goals.Add(new Vector2(-100, -60));
            instance.goals.Add(new Vector2(-90, -54));
            instance.goals.Add(new Vector2(-64, -24));

            //Closer to middle area

            instance.goals.Add(new Vector2(-112, -28));
            instance.goals.Add(new Vector2(-112, -33));
            instance.goals.Add(new Vector2(-116, -31));
            instance.goals.Add(new Vector2(-115, -50));
            instance.goals.Add(new Vector2(-110, -49));
            

        }

        //Clear the simulation, add agents and define goals
        void instantiateSimulation()
        {
            //Initiate agent models
            instance.agentModels = new List<GameObject>();
            instance.agentModels.Add(Resources.Load("AgentAngelica", typeof(GameObject)) as GameObject);
            instance.agentModels.Add(Resources.Load("AgentChuan", typeof(GameObject)) as GameObject);
            instance.agentModels.Add(Resources.Load("AgentDavid", typeof(GameObject)) as GameObject);
            instance.agentModels.Add(Resources.Load("AgentJenna", typeof(GameObject)) as GameObject);

            
            instance.artificialAgents = new List<GameObject>();
            ArtificialAgent.selectedMat = Resources.Load("Selected", typeof(Material)) as Material;
            instance.visibility = true;

            instance.AAcollision = 0;
            instance.APcollision = 0;

            instance.defineGoals();
            Simulator.Instance.Clear();
            /* Specify the global time step of the simulation. */
            Simulator.Instance.setTimeStep(1f);

            //Initiate the agent properties, these will also help us modify the agent behavior using the RVO simulation
            Simulator.Instance.setAgentDefaults(instance.neighbourRange, instance.numOfNeighboursConsidered, instance.reactionSpeed, 10.0f, 1.5f, instance.maxSpeed, new Vector2(0.0f, 0.0f));

            //Create the initial crowd of agents, depending on the current unocupied locations of the projected agents
            //Actually, we don't need a complicated conversion, the z coordinate will be given to the RVO as y and y coordinate from RVO
            //will be considered as Z.

            for (int artificialAgentId = 0; artificialAgentId < initialNumberOfAgents; artificialAgentId++)
            {
                //Generate them at the main area first, with goal of a random exit point (given manualy for now)
                //Generate random points and goals later (which will be determined individually)

                Vector2 origin = instance.goals[(int)Math.Floor(UnityEngine.Random.value * instance.goals.Count)];

                origin = instance.goals[artificialAgentId % instance.goals.Count];

                GameObject newArtAgent = (GameObject)Instantiate(instance.agentModels[(int)Math.Floor(UnityEngine.Random.value * instance.agentModels.Count)], new Vector3(origin.x_, 0f, origin.y_), new Quaternion());

                //Initialize the RVO part of the agent by connecting the reference to the
                //related new agent
                RVO.Agent agentReference = Simulator.Instance.addResponsiveAgent(origin);
            //    Simulator.Instance.setAgentPosition(agentReference.id_, origin); //is this necessary ?

                newArtAgent.GetComponent<ArtificialAgent>().createAgent(agentReference.id_, agentReference);

              //  artificialAgents.Add(artificialAgentId, newArtAgent);
                instance.artificialAgents.Add(newArtAgent);

                newArtAgent.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(new Vector3((float)instance.goals[(artificialAgentId + 1) % instance.goals.Count].x(), 0f, (float)instance.goals[(artificialAgentId + 1) % instance.goals.Count].y()));


            }

            PedestrianProjection.Instance.InitiateProjection();

        }

        // Update the RVO simulation 
        void FixedUpdate()
        {
            
            //I stop and resume the navmeshagent part as it causes ossilation between navmesh velocity and rvo velocity
            foreach (GameObject ag in instance.artificialAgents)
            {
                ag.GetComponent<ArtificialAgent>().setPreferred();
                ag.GetComponent<UnityEngine.AI.NavMeshAgent>().Stop();
           }
            //Apply the step for determining the required velocity for each agent on the move
            Simulator.Instance.doStep();

            foreach (GameObject ag in instance.artificialAgents)
            {
                ag.GetComponent<ArtificialAgent>().updateVelo();
                ag.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = false;
         //       ag.GetComponent<UnityEngine.AI.NavMeshAgent>().Resume(); //Resume so that the velocity of the path is recalculated
            }
        }

        //Add the agent to given position in the space
        public void addAgent(Vector3 pos)
        { 

            Vector2 origin = new Vector2(pos.x, pos.z);

            GameObject newArtAgent = (GameObject)Instantiate(instance.agentModels[(int)Math.Floor(UnityEngine.Random.value * instance.agentModels.Count)], new Vector3(origin.x_, 0f, origin.y_), new Quaternion());

            //Initialize the RVO part of the agent by connecting the reference to the
            //related new agent
            int agentId;
            RVO.Agent agentReference = Simulator.Instance.addResponsiveAgent(origin);
            Simulator.Instance.setAgentPosition(agentReference.id_, origin);

            newArtAgent.GetComponent<ArtificialAgent>().createAgent(agentReference.id_, agentReference);

            //  artificialAgents.Add(artificialAgentId, newArtAgent);
            instance.artificialAgents.Add(newArtAgent);

        }

        internal void removeAgent(GameObject agent)
        {
            RVO.Simulator.Instance.agents_.Remove(agent.GetComponent<ArtificialAgent>().AgentReference);
            instance.artificialAgents.Remove(agent);
            Destroy(agent);
        }

        
        //internal void adjustNumberOfNeighbours(float value)
        //{
        //    instance.numOfNeighboursConsidered = (int)value;
        //    /*
        //    foreach (GameObject ag in instance.artificialAgents)
        //    {
        //        ag.GetComponent<ArtificialAgent>().AgentReference.maxNeighbors_ = (int)value;
        //    }*/
        //}

        //internal void adjustMaxSpeed(int value)
        //{
        //    instance.maxSpeed = value;
        //   /* foreach (GameObject ag in instance.artificialAgents)
        //    {
        //        ag.GetComponent<ArtificialAgent>().AgentReference.maxSpeed_ = (int)value;
        //    }*/
        //}

        //internal void adjustRange(float value)
        //{
        //    instance.neighbourRange = value;
        //   /* foreach (GameObject ag in instance.artificialAgents)
        //    {
        //        ag.GetComponent<ArtificialAgent>().AgentReference.neighborDist_ = (int)value;
        //    }*/
        //}

        //internal void adjustReactionSpeed(float value)
        //{
        //    instance.reactionSpeed = value;
        //    /* foreach (GameObject ag in instance.artificialAgents)
        //     {
        //         ag.GetComponent<ArtificialAgent>().AgentReference.neighborDist_ = (int)value;
        //     }*/
        //}
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
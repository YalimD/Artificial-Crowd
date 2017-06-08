using UnityEngine;
using System.Collections;

/*
 * Written by Yalım Doğan
 * 
 * A very basic agent model that has a stickman prefab and maintains a velocity starting from an initial spawn point.
 * Should include a reference to its Agent object from RVO where it determines its behaviour. 
 * 
 * The velocity of this agent is given by PedestrianProjection class, it cannot be modified by RVO (There is an option for that)
 * But the Agent reference inside should be updated with velocity read from file. There is no goal in RVO, just preferred velocity.
 * This enables wandering.
 * 
 * BUT if the simulation ends, the control should be given to ArtificialAgent (the simulation must end ?)
 * 
 */
namespace RVO
{
    public class ProjectedAgent : MonoBehaviour
    {

        //The RVO:Agent reference
        RVO.Agent agentReference;

        // Properties
        private int trackId; //This is the id which is given by the projection 
        private int rvoId;
        //  private Rigidbody rg;
        public Vector3 velocity;

        //Accessors mutators
        public Vector3 Velocity { set { velocity = value; } get { return velocity; } }
        public Vector3 Pos { get { return transform.position; } }
        public int TrackId { get; set; }
        public int RvoId { get; set; }
        public RVO.Agent AgentReference { get { return agentReference; } }

        //Constructor need 1 parameter id that is assigned to the agent
        public void createAgent(Vector3 initialVelocity, int trackid, int RVOId, RVO.Agent agentReference)
        {
            velocity = initialVelocity;
            this.trackId = trackid;
            rvoId = RVOId;
            this.agentReference = agentReference;
            //So that this agent ignores the neighboring agents in RVO. But participates the collision avoidance
        }

        void Update()
        {
            
            mag = velocity.magnitude;
            agentReference.prefVelocity_ = new Vector2(velocity.x , velocity.z );

            agentReference.position_ = new Vector2(transform.position.x , transform.position.z );

           // Debug.Log("velocity of agent " + trackId + " is " + velocity);
         //   transform.Translate(velocity);
            transform.LookAt(velocity); 
            

            if (velocity.magnitude > 0.5f)
            {
                PedestrianProjection.Instance.removeAgent(trackId, transform.gameObject);
            }

        }

        public float mag;

        /* Destroy agent if it gets out of mesh. This is checked by removing it
         * when its collider doesn't collide with the walkable mesh anymore
         */

        void OnCollisionExit(Collision collisionInfo)
        {
            if (collisionInfo.transform.name == "walkableDebug")
            {
              //  Debug.Log("Left");
                PedestrianProjection.Instance.removeAgent(trackId, transform.gameObject);
            }
        }
        
        //Ne*ed to implement RVO over these agents

        //Required methods from RVO to work
        //Reached goal
        //Change Velocity (setpreferredVelocities)
        //dostep

        //Intialize the agents as randomly over the area (Do we need to initialize the grid ?)
        //
        /*
        public void FixedUpdate()
        {
            if (!sim.reachedGoal())
            {
                sim.updateVisualization();
                sim.setPreferredVelocities();

                //Apply the step for determining the required velocity for each agent on the move
                FanExploration.clearVisual();
                Simulator.Instance.doStep();

            }
        }
         */
    }
}
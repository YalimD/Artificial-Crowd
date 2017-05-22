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

public class ProjectedAgent: MonoBehaviour{

    //The RVO:Agent reference
    RVO.Agent agentReference;

    // Properties
    private int agentId; //This is the id which is given by the projection TODO: Rename this
  //  private Rigidbody rg;
    private const float velocityMultiplier = 1.0f; //Used for speeding up the velocity, as it is not suitable for simulation.
    private Vector3 velocity;

    //Accessors mutators
    public Vector3 Velocity { set { velocity = value * velocityMultiplier; } get { return velocity; } }
    public Vector3 Pos { get { return transform.position; } }
    public int AgentId { get; set; }

    //Constructor need 1 parameter id that is assigned to the agent
    public void createAgent(Vector3 initialVelocity, int id)
    {
        velocity = initialVelocity * velocityMultiplier;
        agentId = id;
    }

    void Update()
    {
     
    //    Debug.Log("velocity of agent " + agentId + " is " + rg.velocity);
        transform.Translate(velocity * velocityMultiplier);

        //Destroy the agent if fallen below certain height
        if (transform.position.y < 2)
        {
            PedestrianProjection.Instance.removeAgent(agentId, transform.gameObject);  
        }

    }

    //Need to implement RVO over these agents

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

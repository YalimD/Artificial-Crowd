using UnityEngine;
using System.Collections;


/*
 * Written by Yalım Doğan
 * 
 * A very basic agent model that has a stickman prefab and maintains a velocity starting from an initial spawn point.
 * Should be coupled with the Agent class from RVO where it determines its behaviour
 * 
 */


public class ProjectedAgent: MonoBehaviour{

    // Properties
    public Vector3 velocity;
    private Vector3 pos;
    public int agentId;
    private Rigidbody rg;
    private const float velocityMultiplier = 5.0f; //Used for speeding up the velocity, as it is not suitable for simulation.

    //Constructor needs 3 parameters for position, velocity and id that is assigned to the agent
    public void createAgent(Vector3 initialPos, Vector3 initialVelocity, int id)
    {
        velocity = initialVelocity;
        pos = initialPos;
        agentId = id;
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public Vector3 getPosition()
    {
        return pos;
    }

    public int getId()
    {
        return agentId;
    }

    void Update()
    {
        rg = GetComponent<Rigidbody>();
      //  Debug.Log("velocity of agent " + agentId + " is " + velocity);
        rg.velocity = velocity * velocityMultiplier;
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

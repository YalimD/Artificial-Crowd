using UnityEngine;
using System.Collections;


/*
 * Written by Yalım Doğan
 * 
 * A very basic agent mode that has a stickman prefab and maintains a velocity starting from an initial spawn point
 * 
 */




public class ProjectedAgent: MonoBehaviour {

    // Properties
    private Vector3 velocity;
    private Vector3 pos;
    private int AgentId;

    //Constructor needs 3 parameters for position, velocity and id that is assigned to the agent
    ProjectedAgent(Vector3 initialPos, Vector3 initialVelocity, int id)
    {
        velocity = initialVelocity;
        pos = initialPos;
        AgentId = id;
    }

    //Need to implement RVO over these agents

}

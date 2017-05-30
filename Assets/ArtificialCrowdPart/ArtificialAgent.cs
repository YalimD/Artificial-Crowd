using UnityEngine;
using System;
using System.Collections;

/*
 *  TODO: This class is associated to artificial agent's PREFAB so it manages its behaviour 
 *  TODO: Each agent type should also have a different material associated with it
 *  
 *  For now, the navigational behavior of the artificial agents are determined by the built-in
 *  navigation mesh.
 * 
 */

namespace RVO
{

    public class ArtificialAgent : MonoBehaviour
    {

        //The RVO:Agent reference
        private RVO.Agent agentReference;
        private Vector3 goal;
        private NavMeshPath path;
        private int pathStatus;
        //Properties

        //Reference to navigation agent as it will determine this agent's preferred velocity
      //  NavMeshAgent navAgent;
        private int agentId;

        public int AgentId { get { return agentId; } set { agentId = value; } }
        public RVO.Agent AgentReference { get { return agentReference; } set { agentReference = value; } }

        // Use this for initialization
        void Start()
        {
           // path = new NavMeshPath();
            pathStatus = 0;
        }

        //Added the target of the agent
        public void createAgent(int id, RVO.Agent agentReference, Vector3 goal)
        {
            agentId = id;
            this.agentReference = agentReference;
            setDestination(goal);
           // navAgent = transform.GetComponent<NavMeshAgent>();
        }

        public void setDestination(Vector3 destination)
        {
            goal = destination;
            if(path == null)
                path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position,goal,NavMesh.AllAreas, path);
            Debug.Log(path.corners.Length);
        }


        // Update is called once per frame
        void Update()
        {
            // CUSTOM GOAL SELECTION OPTION
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    GetComponent<NavMeshAgent>().SetDestination(hit.point);
                }
            }
           
        }

        /* NAVMESHPATH version
         */ 

        private Vector3 velocity;
        public void setPreferred ()
        {
            transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //Debug.Log(path.corners.Length);
            Vector2 goalDirection = new Vector2(0.0f, 0.0f);
            if (pathStatus < path.corners.Length) { 
                goalDirection = new Vector2(path.corners[pathStatus].x,path.corners[pathStatus].z) - agentReference.position_;
                if (goalDirection.x() == 0.0f && goalDirection.y() == 0.0f)
                {
                    pathStatus++;
                    if (pathStatus < path.corners.Length)
                    {
                        goalDirection = new Vector2(path.corners[pathStatus].x, path.corners[pathStatus].z) - agentReference.position_;
                    }
                }


                if (RVOMath.absSq(goalDirection) > 0.1f)
                {
                    goalDirection = RVOMath.normalize(goalDirection) / 10;
                }

                
                Debug.Log("Velocity is:" + new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
            }
            agentReference.prefVelocity_ = goalDirection;

        }

        public void updateVelo()
        {
            /* Possible solution to desyncronized movement between RVO and NavmeshAgent
             * 
             * When the navmesh agent is gathering speed (acceleration) the RVO rounds the give velocity to 0 as it uses signle digit precision
             * Even if we move the agent to RVO loc, it stays still as navmeshagent never gains enough velocity to escape from rounding to zero!
             * 
             * So, until navmeshagent gets to an acceptable speed, it's velocity shouldn't be touched. AND the RVO agent should be located
             * with the same position of navmeshagent. As this happens in a split second, it shouldn't cause problems
             * 
             * After that, in order to make sure the navAgent and RVO is on the same location, we will locate the navmesh agent according to the result of
             * RVO, as RVO loses precision and that difference shouldn't be allowed to add up.
             */
            transform.LookAt(new Vector3(agentReference.position_.x(), transform.position.y,
                                                               agentReference.position_.y()));
            transform.position = new Vector3(agentReference.position_.x(),transform.position.y,
                                                               agentReference.position_.y());

           // transform.GetComponent<Rigidbody>().AddForce(new Vector3(agentReference.velocity_.x(),0.0f,
           //                                                    agentReference.velocity_.y()));
        //    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

           // agentReference.position_ = new Vector2(transform.position.x,transform.position.z);
          //  agentReference.velocity_ = new Vector2(navAgent.velocity.x, navAgent.velocity.z);


         //   if (!navAgent.hasPath)
         //       Debug.Log("I don't have a path!");

               // if (GetComponent<NavMeshAgent>().hasPath && !agentReference.velocity_.Equals(new Vector2(0f, 0f)) && transform.GetComponent<NavMeshAgent>().velocity.magnitude > 5){
                    //transform.GetComponent<NavMeshAgent>().velocity = new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y());
              //      transform.GetComponent<NavMeshAgent>().Warp(new Vector3(agentReference.position_.x(), 3.6f, agentReference.position_.y()));
                  //  transform.position = new Vector3(agentReference.position_.x(), 4f, agentReference.position_.y());
        //}
          //      else if (!GetComponent<NavMeshAgent>().hasPath)
            //        agentReference.velocity_ = new Vector2(0f, 0f);
            
               // transform.GetComponent<NavMeshAgent>().Move(new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()) -
                 //        transform.GetComponent<NavMeshAgent>().velocity);
                //  Debug.Log("Velocity is:" + new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
           //     Debug.Log("The Velocity of agent " + agentId + " is: "  + transform.GetComponent<NavMeshAgent>().velocity);
        }/*

        
            //Debug.Log(GetComponent<NavMeshAgent>().hasPath);
           // if (GetComponent<NavMeshAgent>().hasPath)
          //  {
                //Update the agent's velocity in the RVO simulation part through RVO.Agent Reference
                //But as the RVO simulation works on 2d in itself, the velocity is converted (z becomes y)

                //TODO: The precision of NavMesh and RVO doesn't match, therefore causing movement problems
                //  Vector3 nav = transform.GetComponent<NavMeshAgent>().velocity;
                // Debug.Log("Navigation Velocity of agent " + agentId + " is:" + nav);
                //transform.GetComponent<NavMeshAgent>().velocity = Vector3.zero;

                //  transform.GetComponent<NavMeshAgent>().velocity = new Vector3((float.Parse(string.Format("{0:+#;-#;0.0}", nav.x)) == 0.0f && nav.x != 0f) ? 1f : float.Parse(string.Format("{0:+#;-#;0.0}", nav.x)), 0, (float.Parse(string.Format("{0:+#;-#;0.0}", nav.z)) == 0.0f && nav.z != 0f) ? 1f : float.Parse(string.Format("{0:+#;-#;0.0}", nav.z)));
                // Debug.Log(float.Parse(string.Format("{0:+#;-#;0.0}", nav.x)));
                //agentReference.prefVelocity_ = vectorConverter(transform.GetComponent<NavMeshAgent>().velocity);
              //  transform.GetComponent<NavMeshAgent>().
                agentReference.prefVelocity_ =  new Vector2(transform.GetComponent<NavMeshAgent>().velocity.x, transform.GetComponent<NavMeshAgent>().velocity.z);
                Debug.Log("Preferred jghjhgjVelocity of agent " + agentId + " is:" + transform.GetComponent<NavMeshAgent>().velocity);
                Debug.Log("Preferred Velocity of agent " + agentId + " is:" + agentReference.prefVelocity_);

                //In between, we need to make sure the simulation does a step for calculation of the new velocity

                agentReference.update(); 
                agentReference.computeNewVelocity();
               // Debug.Log(agentReference.velocity_.Equals(new Vector2(0f, 0f)));
              //  transform.Translate(new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
                if (GetComponent<NavMeshAgent>().hasPath && !agentReference.velocity_.Equals(new Vector2(0f, 0f)) && transform.GetComponent<NavMeshAgent>().velocity.magnitude > 5)
                    transform.GetComponent<NavMeshAgent>().velocity = new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y());
                else if (!GetComponent<NavMeshAgent>().hasPath)
                    agentReference.velocity_ = new Vector2(0f, 0f);

               // transform.GetComponent<NavMeshAgent>().Move(new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()) -
                 //        transform.GetComponent<NavMeshAgent>().velocity);
                //  Debug.Log("Velocity is:" + new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
                Debug.Log("The Velocity of agent " + agentId + " is: "  + transform.GetComponent<NavMeshAgent>().velocity);
          //  }
        }*/
    }

}
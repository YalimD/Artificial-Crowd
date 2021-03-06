﻿using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

/* 
 * Written by Yalım Doğan
 * 
 * The logic of the artificial agents
 */

namespace RVO
{

    public class ArtificialAgent : MonoBehaviour
    {
        //Properties

        #region PROPERTY

        //The material to be set when user selects this agent
        public static Material selectedMat;

        private Material defaultMaterial = null;
        private Material defaultHairMaterial = null;

        //The RVO:Agent reference
        private RVO.Agent agentReference;
        private Animator anim;
        private bool selected; //Is this agent currently selected by user to modify its RVO properties

        //Reference to navigation agent as it will determine this agent's preferred velocity
        UnityEngine.AI.NavMeshAgent navAgent;
        private int agentId;

        public int AgentId { get { return agentId; } set { agentId = value; } }
        public RVO.Agent AgentReference { get { return agentReference; } set { agentReference = value; } }

        #endregion

        public void createAgent(int id, RVO.Agent agentReference)
        {
            agentId = id;
            this.agentReference = agentReference;
            navAgent = transform.GetComponent<UnityEngine.AI.NavMeshAgent>();
            anim = transform.GetComponent<Animator>();

            defaultMaterial = transform.Find("body").GetComponent<Renderer>().material;
            if (transform.Find("hair"))
                defaultHairMaterial = transform.Find("hair").GetComponent<Renderer>().material;

            //DENEME
            goal = transform.position;
            path = null;
            pathStatus = 0;
        }


        // Update is called once per physics step
        void FixedUpdate()
        {
            // Updating the animations
            anim.SetFloat("Velocity",RVOMath.abs(goalDirection)*30f);
            if (!AgentBehaviour.Instance.Visibility)
            {
                transform.Find("body").GetComponent<Renderer>().enabled = false;
                if (defaultHairMaterial)
                    transform.Find("hair").GetComponent<Renderer>().enabled = false;
            }
            else
            {
                transform.Find("body").GetComponent<Renderer>().enabled = true;
                if (defaultHairMaterial)
                    transform.Find("hair").GetComponent<Renderer>().enabled = true;
            }
                
        }

        //deneme

        private Vector3 goal;
        private NavMeshPath path;
        private int pathStatus;

        public void setDestination(Vector3 destination)
        {
            goal = destination;
            path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, goal, NavMesh.AllAreas, path);
        //    Debug.Log(path.corners.Length);
        }
        Vector2 goalDirection;
        public void setPreferred()
        {
           // transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //Debug.Log(path.corners.Length);
            goalDirection = new Vector2(0.0f, 0.0f);
            if (path != null && pathStatus < path.corners.Length)
            {
               // goalDirection = new Vector2(path.corners[pathStatus].x, path.corners[pathStatus].z) - agentReference.position_;
                goalDirection = new Vector2(path.corners[pathStatus].x, path.corners[pathStatus].z) - new Vector2(transform.position.x,transform.position.z);
              // Debug.Log("goal direction:" + goalDirection);

             /*   if (pathStatus < path.corners.Length - 1)
                {
                goalDirection
                }
*/
                if (RVOMath.absSq(goalDirection) < 0.1f)
                {
                    pathStatus++;
                    /*   if (pathStatus < path.corners.Length)
                       {
                           goalDirection = new Vector2(path.corners[pathStatus].x, path.corners[pathStatus].z) - agentReference.position_;
                       }*/
                }
                if (RVOMath.absSq(goalDirection) > 0.01f)
                {
                    //goalDirection = RVOMath.normalize(goalDirection) / 10f;
                    float speed = (float)((RVOMath.abs(RVOMath.normalize(goalDirection) / 30f) > 0.1) ? 0.1 : RVOMath.abs(RVOMath.normalize(goalDirection) / 30f));
                    goalDirection = new Vector2(RVOMath.normalize(goalDirection).x() * speed, RVOMath.normalize(goalDirection).y() * speed);

                }
 
            }
            else
            {
                Debug.Log("Staying");
                pathStatus = 0;
                path = null;
                setDestination(transform.position);
            }

            agentReference.prefVelocity_ = goalDirection * RVOMagnify.magnify;
        }

        public void updateVelo()
        {
           // Debug.Log("Given Velocity is:" + new Vector3(agentReference.prefVelocity_.x(), 0, agentReference.prefVelocity_.y()));
            //We need tomove
           // if (RVOMath.abs(agentReference.velocity_) == 0 && RVOMath.abs(goalDirection) > 0)
         //   {
          //      Debug.Log("MOVE");
         //   }

         //   else
         //   {
             //   Debug.Log("DAMN");
              //  transform.LookAt(new Vector3(agentReference.position_.x(), transform.position.y,
              //                                                     agentReference.position_.y()));
            goalDirection = new Vector2(agentReference.velocity_.x() / RVOMagnify.magnify, agentReference.velocity_.y() / RVOMagnify.magnify);
           // transform.Translate(new Vector3(goalDirection.x() , 0f, goalDirection.y()), Space.World);
            transform.position = new Vector3(agentReference.position_.x_ / RVOMagnify.magnify, 0f, agentReference.position_.y_ / RVOMagnify.magnify);
          //  transform.Translate(new Vector3(goalDirection.x(), 0f,goalDirection.y()),Space.World);

            if (new Vector3(goalDirection.x(), 0f, goalDirection.y()).magnitude != 0)
            {
                Quaternion rotation = Quaternion.LookRotation(new Vector3(goalDirection.x(), 0f, goalDirection.y()));
                rotation.x = 0;
                rotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }

         //   }
       //    Debug.Log("goal direction:" + goalDirection + "with length" + RVOMath.abs(goalDirection)+" Velocity is:" + new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
            //Debug.Log("Pos" + transform.position + "with RVO" + agentReference.position_ );

        }
        
        //public void setPreferred()
        //{
        //    agentReference.prefVelocity_ = new Vector2(navAgent.velocity.x, navAgent.velocity.z);
        //}
        //public void updateVelo()
        //{
        //    /* Possible solution to desyncronized movement between RVO and NavmeshAgent
        //     * 
        //     * When the navmesh agent is gathering speed (acceleration) the RVO rounds the give velocity to 0 as it uses signle digit precision
        //     * Even if we move the agent to RVO loc, it stays still as navmeshagent never gains enough velocity to escape from rounding to zero!
        //     * 
        //     * So, until navmeshagent gets to an acceptable speed, it's velocity shouldn't be touched. AND the RVO agent should be located
        //     * with the same position of navmeshagent. As this happens in a split second, it shouldn't cause problems
        //     * 
        //     * After that, in order to make sure the navAgent and RVO is on the same location, we will locate the navmesh agent according to the result of
        //     * RVO, as RVO loses precision and that difference shouldn't be allowed to add up.
        //     */
        //    agentReference.computeNewVelocity();

        //    if (navAgent.hasPath && (RVOMath.abs(agentReference.velocity_) <= navAgent.speed * 0.5f))
        //    {
        //        agentReference.position_ = new Vector2(transform.position.x, transform.position.z);
        //        agentReference.velocity_ = new Vector2(navAgent.velocity.x, navAgent.velocity.z);

        //          navAgent.acceleration = (navAgent.remainingDistance > 5) ? 10f:2f;

        //        Debug.Log("Under nav with nav velocity:" + navAgent.velocity + "but agent vel" + agentReference.velocity_);
        //    }
        //    else if (navAgent.remainingDistance <= 0.1f){
        //        Debug.Log("STOP");
        //        agentReference.velocity_ = new Vector2(0, 0);
        //        navAgent.ResetPath();
        //    }
        //    else
        //    {
        //        navAgent.velocity = new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y());
        //        Debug.Log("under RVO with agent velocity:" + agentReference.velocity_);

        //    }

        //    Rotate the agent to directly match its orientation
        //    /*
        //    Quaternion rotation = Quaternion.LookRotation(navAgent.velocity);
        //    rotation.x = 0;
        //    rotation.z = 0;
        //    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 1f);


        //    */

        //    /*
        //    Debug.Log(transform.GetComponent<Collider>().gameObject.name);
        //        if (GetComponent<NavMeshAgent>().hasPath && !agentReference.velocity_.Equals(new Vector2(0f, 0f)) && transform.GetComponent<NavMeshAgent>().velocity.magnitude > 5){
        //            transform.GetComponent<NavMeshAgent>().velocity = new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y());
        //            transform.GetComponent<NavMeshAgent>().Warp(new Vector3(agentReference.position_.x(), 3.6f, agentReference.position_.y()));
        //            transform.position = new Vector3(agentReference.position_.x(), 4f, agentReference.position_.y());
        //}
        //        else if (!GetComponent<NavMeshAgent>().hasPath)
        //            agentReference.velocity_ = new Vector2(0f, 0f);
            
        //        transform.GetComponent<NavMeshAgent>().Move(new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()) -
        //                 transform.GetComponent<NavMeshAgent>().velocity);
        //          Debug.Log("Velocity is:" + new Vector3(agentReference.velocity_.x(), 0, agentReference.velocity_.y()));
        //        Debug.Log("The Velocity of agent " + agentId + " is: "  + transform.GetComponent<NavMeshAgent>().velocity);
        //     */
        //}
        
        
        
        /*

        
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


        //Change the material's emission to highlight the agent
        internal void setSelected()
        {
            this.selected = true;
            transform.Find("body").GetComponent<Renderer>().material = selectedMat;
            if (defaultHairMaterial)
                transform.Find("hair").GetComponent<Renderer>().material = selectedMat;
        }

        internal void deSelect()
        {
            this.selected = false;
            transform.Find("body").GetComponent<Renderer>().material = defaultMaterial;
            if (defaultHairMaterial)
                transform.Find("hair").GetComponent<Renderer>().material = defaultHairMaterial;
        }

        internal bool isSelected()
        {
            return selected;
        }
        

    }

}
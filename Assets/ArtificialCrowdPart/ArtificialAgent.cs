using UnityEngine;
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

        //Properties

        //Reference to navigation agent as it will determine this agent's preferred velocity
        NavMeshAgent navAgent;
        private int agentId;

        public int AgentId { get; set; }

        // Use this for initialization
        void Start()
        {

        }

        //Temporary
        public void createAgent(int id, RVO.Agent agentReference)
        {
            agentId = id;
            this.agentReference = agentReference;
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
    }

}
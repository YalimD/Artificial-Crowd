using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
 * Written by Yalım Doğan
 * 
 * This class manages the GUI controls on RVO, selection of artificial agents etc.
 */

public class GUIController : MonoBehaviour
{
    Button add, remove;
    Slider neighbours;
    Transform agentCount;
    Text numOfSelected, totalArtificial, totalProjected, totalAgents;
    ColorBlock cbAdd, cbRemove;
    //Modes for adding/removing artificial agents
    /* 0 - idle
     * 1 - Add
     * 2- Remove
     */
    private int currentMode = 0;

    //Currently selected agents
    private List<RVO.ArtificialAgent> selectedAgents;

    //Add agent mode
    public void addAgentMode()
    {
        if (currentMode == 0)
        {
            currentMode = 1;
            cbAdd.normalColor = Color.yellow;
            cbAdd.highlightedColor = Color.yellow;
            add.colors = cbAdd;
            remove.interactable = false;
        }
        else
        {
            cbAdd.normalColor = Color.white;
            cbAdd.highlightedColor = Color.white;
            add.colors = cbAdd;
            remove.interactable = true;
            currentMode = 0;
        }
    }

    //Remove agent mode
    public void removeAgentMode()
    {
        if (currentMode == 0)
        {
            currentMode = 2;
            cbRemove.normalColor = Color.yellow;
            cbRemove.highlightedColor = Color.yellow;
            remove.colors = cbRemove;
            add.interactable = false;
        }
        else
        {
            cbRemove.normalColor = Color.white;
            cbRemove.highlightedColor = Color.white;
            remove.colors = cbRemove;
            add.interactable = true;
            currentMode = 0;
        }
    }


    /*
    public void toogleArtificialManagement(int mode)
    {
        if (currentMode == mode)
        {
            cbAdd.normalColor = Color.white;
            cbAdd.highlightedColor = Color.white;
            add.colors = cbAdd;
            add.interactable = true;

            cbRemove.normalColor = Color.white;
            cbRemove.highlightedColor = Color.white;
            remove.colors = cbRemove;
            remove.interactable = true;

            currentMode = 0;

        }
        else
        {
            currentMode = mode;
            switch (currentMode)
            { 
                case 1:
                    cbAdd = add.colors;
                    cbAdd.normalColor = Color.yellow;
                    cbAdd.highlightedColor = Color.yellow;
                    add.colors = cbAdd;
                    remove.interactable = false;
                    break;
                case 2:
                    cbRemove = remove.colors;
                    cbRemove.normalColor = Color.yellow;
                    cbRemove.highlightedColor = Color.yellow;
                    remove.colors = cbRemove;
                    add.interactable = false;
                    break;

            }
        }
        //Depending on the current mode of the program, change the color of the related button.
    }
    */


    Transform collisionCounter;
    Text numOfCollisions, artificialToArtificialCollisions, artificialToProjectedCollisions;

    void Start()
    {
        // Adding / Removing agents
        add = transform.Find("RVOControl").Find("AddArtificial").GetComponent<UnityEngine.UI.Button>();
        cbAdd = add.colors;
        remove = transform.Find("RVOControl").Find("RemoveArtificial").GetComponent<UnityEngine.UI.Button>();
        cbRemove = remove.colors;

        neighbours = transform.Find("RVOControl").Find("Neighbours").Find("Slider").GetComponent<Slider>();

        selectedAgents = new List<RVO.ArtificialAgent>();

        agentCount = transform.Find("AgentCounter"); //Root agentCount object

        numOfSelected =  agentCount.transform.Find("NumOfSelectedArt").Find("Num").GetComponent<Text>();
        totalProjected = agentCount.transform.Find("Show Projected").Find("Num").GetComponent<Text>();
        totalArtificial = agentCount.transform.Find("Show Artificial").Find("Num").GetComponent<Text>();
        totalAgents = agentCount.transform.Find("Total").Find("Num").GetComponent<Text>();

        collisionCounter = transform.Find("CollisionCounter");
        numOfCollisions = collisionCounter.transform.Find("NumOfCollisions").Find("Text").GetComponent<Text>();
        artificialToArtificialCollisions = collisionCounter.transform.Find("ArtArt").Find("Text").GetComponent<Text>();
        artificialToProjectedCollisions = collisionCounter.transform.Find("ArtPro").Find("Text").GetComponent<Text>();

    }


    //Change the visibility of the artificial Agents
    public void changeArtificialVisibility(bool visibility)
    {
        RVO.AgentBehaviour.Instance.Visibility = visibility;
    }

    //Change the visibility of the projected Agents
    public void changeProjectedVisibility(bool visibility)
    {
        RVO.PedestrianProjection.Instance.Visibility = visibility;
    }

    public void selectAllAgents()
    {
        foreach (GameObject obj in RVO.AgentBehaviour.Instance.ArtificialAgents)
        {
            if (!obj.GetComponent<RVO.ArtificialAgent>().isSelected())
            {
                obj.GetComponent<RVO.ArtificialAgent>().setSelected();
                selectedAgents.Add(obj.GetComponent<RVO.ArtificialAgent>());
            }
        }
    }

    public void deSelectAllAgents()
    {
        foreach (GameObject obj in RVO.AgentBehaviour.Instance.ArtificialAgents)
        {
            if (obj.GetComponent<RVO.ArtificialAgent>().isSelected())
            {
                obj.GetComponent<RVO.ArtificialAgent>().deSelect();
                selectedAgents.Remove(obj.GetComponent<RVO.ArtificialAgent>());
            }
        }
    }

    void Update()
    {
        //Agent selection counters are updated here
        numOfSelected.text = selectedAgents.Count.ToString();
        totalProjected.text = RVO.PedestrianProjection.Instance.RealAgents.Count.ToString();
        totalArtificial.text = RVO.AgentBehaviour.Instance.ArtificialAgents.Count.ToString();
        totalAgents.text = (RVO.AgentBehaviour.Instance.ArtificialAgents.Count + RVO.PedestrianProjection.Instance.RealAgents.Count).ToString();

        //Agent collision counters are updated here
        numOfCollisions.text = (RVO.AgentBehaviour.Instance.getAACollision() / 2 + RVO.AgentBehaviour.Instance.getAPCollision()).ToString();
        artificialToArtificialCollisions.text = (RVO.AgentBehaviour.Instance.getAACollision() / 2 ).ToString();
        artificialToProjectedCollisions.text = (RVO.AgentBehaviour.Instance.getAPCollision()).ToString();

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            switch (currentMode)
            {
                case 0: //Select agents
                    if (Physics.Raycast(ray, out hit, 100) && hit.collider.tag == "Agent")
                    {
                        Debug.Log("Hit:" + hit.collider.name);
                        RVO.ArtificialAgent art = hit.collider.transform.GetComponentInParent<RVO.ArtificialAgent>();
                        if (!art.isSelected())
                        {
                            art.setSelected();
                            selectedAgents.Add(art);
                        }

                    }
                    
                    break;
                case 1://Add agent


                    if (Physics.Raycast(ray, out hit, 100) && hit.collider.tag != "Agent")
                    {
                        RVO.AgentBehaviour.Instance.addAgent(hit.point);
                    }
                    neighbours.maxValue = RVO.AgentBehaviour.Instance.numOfAgents;
                    

                    break;
                case 2://Remove agent
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100) && hit.collider.tag == "Agent")
                    {
                        RVO.AgentBehaviour.Instance.removeAgent(hit.collider.gameObject);

                    }
                    neighbours.maxValue = RVO.AgentBehaviour.Instance.numOfAgents;
                    break;
                default:
                    break;
            }



        }
        else if (Input.GetMouseButtonUp(1)) //Right clicking will deselect the agent clicked (if it has been selected before)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (currentMode == 0 && Physics.Raycast(ray, out hit, 100) && hit.collider.tag == "Agent")
            {
                RVO.ArtificialAgent art = hit.transform.GetComponentInParent<RVO.ArtificialAgent>();
                if (hit.transform.GetComponentInParent<RVO.ArtificialAgent>().isSelected())
                {
                    art.deSelect();
                    selectedAgents.Remove(art);
                }
            }
            else if (currentMode == 0 && Physics.Raycast(ray, out hit, 100) && hit.collider.tag != "Agent")
            {
                foreach (RVO.ArtificialAgent agent in selectedAgents)
                    agent.transform.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(hit.point);
            }
        }

        //Debug.Log(selectedAgents.Count + "Selected agents");
    }

    public void changeConsideredNeighbours(float numOfNeighbours) { foreach (RVO.ArtificialAgent art in selectedAgents) art.AgentReference.maxNeighbors_ = (int)numOfNeighbours; }
    public void changeMaxSpeed(string maxSpeed) { foreach (RVO.ArtificialAgent art in selectedAgents) art.AgentReference.maxSpeed_ = int.Parse(maxSpeed); }
    public void changeRange(string range) { foreach (RVO.ArtificialAgent art in selectedAgents) art.AgentReference.neighborDist_ = int.Parse(range); }
    public void changeReactionSpeed(string reactionSpeed) { foreach (RVO.ArtificialAgent art in selectedAgents) art.AgentReference.timeHorizon_ = int.Parse(reactionSpeed); }

}

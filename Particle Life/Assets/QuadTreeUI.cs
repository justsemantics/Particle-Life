using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class QuadTreeUI : MonoBehaviour
{
    public QuadTree quadTree;

    [SerializeField]
    float layerHeight = 50;

    [SerializeField]
    RectTransform canvas;

    [SerializeField]
    LeafNodeUI LeafNodeUIPrefab;

    [SerializeField]
    InternalNodeUI InternalNodeUIPrefab;

    LeafNodeUI[] leafNodes;
    InternalNodeUI[] internalNodes;

    [SerializeField]
    DrawZCurve drawZCurve, drawZCurvePrefab;

    // Start is called before the first frame update
    void Start()
    {
        leafNodes = new LeafNodeUI[quadTree.leafNodes.Length];
        internalNodes = new InternalNodeUI[quadTree.internalNodes.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if(quadTree.rootNode != null)
        {
            foreach(NodeUI internalNode in internalNodes)
            {
                if(internalNode != null)
                internalNode.Visited = false;
            }

            DrawNode(quadTree.rootNode, null, false);
        }
    }

    public void Highlight(NodeUI nodeToHighlight)
    {
        if(drawZCurve == null)
        {
            drawZCurve = Instantiate<DrawZCurve>(drawZCurvePrefab);
            drawZCurve.resolution = 1024 / 10f;
            drawZCurve.transform.position = new Vector3(-5, 0, -5);
        }

        Vector2[] points = new Vector2[4];

        points[0] = nodeToHighlight.BoundingBox.min;
        points[1] = new Vector2(nodeToHighlight.BoundingBox.xMax, nodeToHighlight.BoundingBox.yMin);
        points[2] = nodeToHighlight.BoundingBox.max;
        points[3] = new Vector2(nodeToHighlight.BoundingBox.xMin, nodeToHighlight.BoundingBox.yMax);

        drawZCurve.lineRenderer.loop = true;

        drawZCurve.SetPoints(points, 0.5f);
    }


    /// <summary>
    /// To avoid creating and deleting NodeUIs, abstract process of retrieving one from the arrays
    /// </summary>
    /// <param name="node"></param>
    /// <param name="isInternalNode"></param>
    /// <returns></returns>
    NodeUI GetNodeUI(Node node, out bool isInternalNode)
    {
        if(node.GetType() == typeof(LeafNode))
        {
            isInternalNode = false;

            if (leafNodes[node.id] == null)
            {
                LeafNodeUI newNodeUI = Instantiate<LeafNodeUI>(LeafNodeUIPrefab);
                newNodeUI.transform.SetParent(canvas.transform, false);
                newNodeUI.quadTree = quadTree;
                leafNodes[node.id] = newNodeUI;
            }
            
            return leafNodes[node.id];
        }
        else
        {
            isInternalNode = true;

            if (internalNodes[node.id] == null)
            {
                InternalNodeUI newNodeUI = Instantiate<InternalNodeUI>(InternalNodeUIPrefab);
                newNodeUI.transform.SetParent(canvas.transform, false);
                newNodeUI.quadTreeUI = this;
                internalNodes[node.id] = newNodeUI;
            }

            return internalNodes[node.id];
        }
    }

    bool AlreadyVisitedInternalNode(Node node)
    {
        if (internalNodes[node.id] == null) return false;
        else if (internalNodes[node.id].Visited == false) return false;
        else return true;
    }

    bool AlreadyVisitedInternalNode(int id)
    {
        if (id < 0 || id >= internalNodes.Length) return true;
        else if (internalNodes[id] == null) return false;
        else if (internalNodes[id].Visited == false) return false;
        else return true;
    }

    Rect DrawNode(Node node, InternalNodeUI parent = null, bool ASide = false)
    {
        bool isInternalNode = false;
        NodeUI ui = GetNodeUI(node, out isInternalNode);
        
        ui.Node = node;
        ui.ASide = ASide;

        if(parent != null)
        {
            ui.rectTransform.SetParent(parent.Children);
        }

        if(isInternalNode)
        {
            ui.Visited = true;

            InternalNode internalNode = node as InternalNode;
            InternalNodeUI internalNodeUI = ui as InternalNodeUI;

            Node childA, childB;

            //if we have already created an internal node with the ID, this reference must be to the leaf with the same index instead
            if (AlreadyVisitedInternalNode(internalNode.childAID))
            {
                childA = quadTree.leafNodes[internalNode.childAID];
            }
            else
            {
                childA = quadTree.internalNodes[internalNode.childAID];
            }

            //if we have already created an internal node with the ID, this reference must be to the leaf with the same index instead
            if (AlreadyVisitedInternalNode(internalNode.childBID))
            {
                childB = quadTree.leafNodes[internalNode.childBID];
            }
            else
            {
                childB = quadTree.internalNodes[internalNode.childBID];
            }

            Rect childABoundingBox = DrawNode(childA, internalNodeUI, true);
            Rect childBBoundingBox = DrawNode(childB, internalNodeUI, false);

            float xMin, yMin, xMax, yMax;

            xMin = Mathf.Min(childABoundingBox.xMin, childBBoundingBox.xMin);
            xMax = Mathf.Max(childABoundingBox.xMax, childBBoundingBox.xMax);
            yMin = Mathf.Min(childABoundingBox.yMin, childBBoundingBox.yMin);
            yMax = Mathf.Max(childABoundingBox.yMax, childBBoundingBox.yMax);

            Vector2 min = new Vector2(xMin, yMin);
            Vector2 max = new Vector2(xMax, yMax);

            Rect internalNodeBoundingBox = new Rect(min, max - min);

            ui.BoundingBox = internalNodeBoundingBox;

            return internalNodeBoundingBox;
        }
        else
        {
            LeafNode leafNode = node as LeafNode;

            Rect leafNodeBoundingBox = new Rect(leafNode.agent.position, Vector2.zero);

            ui.BoundingBox = leafNodeBoundingBox;

            return leafNodeBoundingBox;
        }
    }
}

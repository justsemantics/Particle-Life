using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class QuadTreeUI : MonoBehaviour
{
    [SerializeField]
    int numAgents;

    [SerializeField]
    float unitHeight = 50;

    [SerializeField]
    float unitWidth = 50;

    [SerializeField]
    RectTransform canvas;

    [SerializeField]
    LeafNodeUI LeafNodeUIPrefab;

    [SerializeField]
    InternalNodeUI InternalNodeUIPrefab;

    LeafNodeUI[] leafNodes;
    InternalNodeUI[] internalNodes;

    InternalNodeData[] NodeData;
    Agent[] Agents;

    bool[] visitedInternalNode;

    [SerializeField]
    DrawZCurve drawZCurve, drawZCurvePrefab;

    // Start is called before the first frame update
    void Start()
    {
        leafNodes = new LeafNodeUI[numAgents];
        internalNodes = new InternalNodeUI[numAgents - 1];
        visitedInternalNode = new bool[numAgents - 1];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawQuadtree(InternalNodeData[] _internalNodeData, Agent[] _agents)
    {
        NodeData = _internalNodeData;
        Agents = _agents;

        visitedInternalNode = new bool[numAgents - 1];

        DrawNode(0);
    }

    bool VisitedNode(int index)
    {
        if (index < 0 || index >= numAgents - 1) return true;
        else return visitedInternalNode[index];
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

    InternalNodeUI GetInternalNodeUI(InternalNodeData nodeData)
    {
        if (internalNodes[nodeData.index] == null)
        {
            InternalNodeUI newNodeUI = Instantiate<InternalNodeUI>(InternalNodeUIPrefab);
            newNodeUI.transform.SetParent(canvas.transform, false);
            newNodeUI.rectTransform.anchoredPosition = Vector2.zero;
            internalNodes[nodeData.index] = newNodeUI;
        }

        return internalNodes[nodeData.index];
    }

    LeafNodeUI GetLeafNodeUI(int index)
    {
        if (leafNodes[index] == null)
        {
            LeafNodeUI newNodeUI = Instantiate<LeafNodeUI>(LeafNodeUIPrefab);
            newNodeUI.transform.SetParent(canvas.transform, false);
            leafNodes[index] = newNodeUI;
        }

        return leafNodes[index];
    }

    void DrawNode(int index, InternalNodeUI parent = null, bool ASide = false)
    {
        if (VisitedNode(index))
        {
            DrawChildNode(index, parent, ASide);
            return;
        }

        visitedInternalNode[index] = true;

        InternalNodeData nodeData = NodeData[index];

        InternalNodeUI ui = GetInternalNodeUI(nodeData);

        float width = unitWidth;
        float height = (nodeData.range.y + 1 - nodeData.range.x) * unitHeight;
        Vector2 position = Vector2.zero;

        if (parent != null)
        {
            position = parent.SplitPosition;
        }

        if (ASide)
        {
            position.y -= height;
            ui.IndexKnob.anchoredPosition = new Vector2(0, height - unitHeight);
        }
        else
        {
            ui.IndexKnob.anchoredPosition = Vector2.zero;
        }

        ui.ASide = ASide;

        ui.indexText.text = index.ToString();

        ui.rectTransform.sizeDelta = new Vector2(width, height);
        ui.rectTransform.anchoredPosition = position;

        ui.IndexKnob.sizeDelta = new Vector2(unitWidth, unitHeight);

        ui.SplitPosition = new Vector2(position.x + unitWidth, nodeData.childBIndex * unitHeight);

        DrawNode(nodeData.childAIndex, ui, true);
        DrawNode(nodeData.childBIndex, ui, false);
    }

    void DrawChildNode(int index, InternalNodeUI parent, bool ASide)
    {
        LeafNodeUI ui = GetLeafNodeUI(index);

        Vector2 position = Vector2.zero;

        position = parent.SplitPosition;

        if (ASide)
        {
            position.y -= unitHeight;
        }

        if (Agents[index].id == 32)
        {
            ui.BG.color = Color.red;
        }
        else
        {
            ui.BG.color = Color.white;
        }

        ui.indexText.text = index.ToString();

        ui.rectTransform.anchoredPosition = position;

        ui.mortonCode.text = MortonCodeToString(Agents[index].mortonCode);
    }

    string MortonCodeToString(uint mortonCode)
    {
        string mortonCodeString = Convert.ToString(mortonCode, 2);

        int leadingZeros = 32 - mortonCodeString.Length;

        string result = "";

        for (int i = 0; i < leadingZeros; i++)
        {
            result += "0";
        }

        result += mortonCodeString;

        return result;
    }
}

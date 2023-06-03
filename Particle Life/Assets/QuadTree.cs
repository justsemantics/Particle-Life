using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class QuadTree : MonoBehaviour
{
    public int resolution;
    public int positionMultiplier;

    public Node rootNode;

    public LeafNode[] leafNodes;
    public InternalNode[] internalNodes;

    int currentLeafNodeIndex, currentInternalNodeIndex;

    public QuadTree(int _resolution)
    {
        resolution = _resolution;
        positionMultiplier = (int)System.Math.Floor(ushort.MaxValue / (float)resolution);
    }

    public void Construct(Agent[] agents)
    {
        agents = SortAgents(agents);
        ConstructPseudoParallel(agents);

        //currentLeafNodeIndex = 0;
        //currentInternalNodeIndex = 0;

        //leafNodes = new LeafNode[agents.Length];
        //internalNodes = new InternalNode[agents.Length - 1];

        //rootNode = generateHierarchy(agents, 0, agents.Length - 1);
    }

    public void AddLeafNode(LeafNode leafNode)
    {
        leafNodes[currentLeafNodeIndex] = leafNode;
        currentLeafNodeIndex++;
    }

    public void AddInternalNode(InternalNode internalNode)
    {
        internalNodes[currentInternalNodeIndex] = internalNode;
        currentInternalNodeIndex++;
    }

    public Node generateHierarchy(Agent[] agents, int first, int last, bool ASide = false)
    {
        //end recursion when range narrows to a single Agent
        if (first == last)
        {
            LeafNode newLeafNode = new LeafNode(agents[first], first, ASide);
            AddLeafNode(newLeafNode);
            return newLeafNode;
        }

        //otherwise, determine where to divide the agents in the range
        int split = FindSplit(agents, first, last);

        Node childA = generateHierarchy(agents, first, split, ASide: true);
        Node childB = generateHierarchy(agents, split + 1, last, ASide: false);

        //a side nodes have the split to the right
        if (ASide)
        {
            InternalNode newInternalNode = new InternalNode(childA, childB, last, ASide);
            AddInternalNode(newInternalNode);
            return newInternalNode;
        }
        //b side nodes have the split to the left
        else
        {
            InternalNode newInternalNode = new InternalNode(childA, childB, first, ASide);
            AddInternalNode(newInternalNode);
            return newInternalNode;
        }
    }

    public Agent[] SortAgents(Agent[] agents)
    {
        for (int i = 0; i < agents.Length; i++)
        {
            AssignMortonCode(ref agents[i]);
        }

        SerialRadixSort sorter = new SerialRadixSort();

        agents = sorter.Sort(agents);

        for (int i = 0; i < agents.Length; i++)
        {
            //Debug.Log(string.Format("{0}, {1}", DecodeMorton2X(agents[i].mortonCode) / positionMultiplier, DecodeMorton2Y(agents[i].mortonCode) / positionMultiplier));
        }

        return agents;
    }

    void AssignMortonCode(ref Agent agent)
    {
        uint xPos = (uint)(agent.position.x * positionMultiplier);
        uint yPos = (uint)(agent.position.y * positionMultiplier);

        agent.mortonCode = EncodeMorton2(xPos, yPos);

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ConstructPseudoParallel(Agent[] agents)
    {
        leafNodes = new LeafNode[agents.Length];
        internalNodes = new InternalNode[agents.Length - 1];

        for(int i = 0; i < leafNodes.Length;i++)
        {
            leafNodes[i] = new LeafNode(agents[i], i);
        }

        for(int i = 0; i < internalNodes.Length; i++)
        {
            Vector2Int nodeRange = DetermineRange(agents, i);

            if(nodeRange.x == nodeRange.y)
            {
                continue;
            }

            int split = FindSplit(agents, nodeRange.x, nodeRange.y);

            internalNodes[i] = new InternalNode(null, null, i);
            internalNodes[i].childAID = split;
            internalNodes[i].childBID = split + 1;
        }

        rootNode = internalNodes[0];
    }


    public Vector2Int DetermineRange(Agent[] agents, int id)
    {
        int commonPrefixLength = 0;
        bool currentNodeIsASide = NodeIsASide(agents, id, out commonPrefixLength);

        uint firstCode = agents[id].mortonCode;

        int first = id;
        int last;
        int direction;

        if(currentNodeIsASide)
        {
            direction = -1;
            if(first == agents.Length - 1)
            {
                return new Vector2Int(0, agents.Length - 1);
            }
        }
        else
        {
            direction = 1;
            if (first == 0)
            {
                return new Vector2Int(0, agents.Length - 1);
            }

        }

        uint adjacentNodeCode = agents[first - direction].mortonCode;

        uint lastCode;

        //this is the shared prefix with the neighbor that is not in the range
        int sharedPrefixWithAdjacentNode = CountLeadingZeros(firstCode ^ adjacentNodeCode);

        //find the step size that the range is within
        int step = 1;
        do
        {
            //potential position for other end of range
            last = first + step * direction;

            //stop looping if we exit the array entirely
            if(last < 0 || last > agents.Length - 1)
            {
                break;
            }

            lastCode = agents[last].mortonCode;

            //double each time
            step = step << 1;
        } while (CountLeadingZeros(firstCode ^ lastCode) > sharedPrefixWithAdjacentNode);

        int range = 0;

        do
        {
            //half each time
            step = (step + 1) >> 1;
            last = first + range + step * direction;

            if(last < 0 || last > agents.Length - 1)
            {
                break;
            }

            lastCode = agents[last].mortonCode;
            if(CountLeadingZeros(firstCode ^ lastCode) > sharedPrefixWithAdjacentNode)
            {
                range += step * direction;
            }

        } while (step > 1);

        last = first + range;

        if (currentNodeIsASide)
        {
            return new Vector2Int(last, first);
        }
        else
        {
            return new Vector2Int(first, last);
        }
    }

    //A side nodes' ids are their last element, B side nodes' ids are the first element
    public bool NodeIsASide(Agent[] agents, int id, out int commonPrefixLength)
    {
        int min = 0;
        int max = agents.Length - 1;

        //checking adjacent morton codes to find direction of the split
        uint currentMortonCode = agents[id].mortonCode;
        uint nextMortonCode, prevMortonCode;

        //easy to check, and prevents access out of range in the following step
        if (id == min)
        {
            nextMortonCode = agents[id + 1].mortonCode;
            commonPrefixLength = CountLeadingZeros(currentMortonCode ^ nextMortonCode) - 1;
            return false;
        }
        else if(id == max)
        {
            prevMortonCode = agents[id - 1].mortonCode;
            commonPrefixLength = CountLeadingZeros(currentMortonCode ^ prevMortonCode) - 1;
            return true;
        }

        //checking adjacent morton codes to find direction of the split
        nextMortonCode = agents[id + 1].mortonCode;
        prevMortonCode = agents[id - 1].mortonCode;

        //whichever direction has a longer common prefix is in the same node
        int forwardPrefixLength = CountLeadingZeros(currentMortonCode ^ nextMortonCode);
        int backwardPrefixLength = CountLeadingZeros(currentMortonCode ^ prevMortonCode);


        if(forwardPrefixLength > backwardPrefixLength)
        {
            //nodes forward in the array share more bits, this is B side
            commonPrefixLength = forwardPrefixLength - 1;
            return false;
        }
        else
        {
            //nodes backward in the array share more bits, this is A side
            commonPrefixLength = backwardPrefixLength - 1;
            return true;
        }
    }

    /// <summary>
    /// Finds the index of the Agent where all following Agents have the next most significant bit set in their morton code
    /// </summary>
    /// <param name="agents"></param>
    /// <param name="first"></param>
    /// <param name="last"></param>
    /// <returns></returns>
    public int FindSplit(Agent[] agents, int first, int last)
    {
        uint firstCode = agents[first].mortonCode;
        uint lastCode = agents[last].mortonCode;

        //Debug.Log(string.Format("First Code: {0}, Last Code: {1}",
        //    System.Convert.ToString(firstCode, 2),
        //    System.Convert.ToString(lastCode, 2)));

        int commonPrefix = CountLeadingZeros(firstCode ^ lastCode);

        int split = first;
        int step = last - first;

        do
        {
            //increase by decreasingly large steps
            step = (step + 1) >> 1;

            //checking position if we step this far from the current acceptable step
            int newSplit = split + step;

            //don't step past the end of the range
            if (newSplit < last)
            {
                //lookup the Agent at the location of the split
                uint splitCode = agents[newSplit].mortonCode;

                //check if that Agent's code shares more bits (i.e. the XOR produces more leading zeros) than our last guess
                int splitPrefix = CountLeadingZeros(firstCode ^ splitCode);
                if (splitPrefix > commonPrefix)
                {
                    //cool, resume search from this point
                    split = newSplit;
                }
                //otherwise, keep stepping from the previous split
            }
        } while (step > 1); //iterate until we have tried stepping at every size

        return split;
    }

    public int CountLeadingZeros(uint x)
    {
        int zeros = 0;
        uint nextX = x << 1;
        while(nextX > x)
        {
            zeros++;
            x = (uint)nextX;
            nextX = x << 1;
        }

        return zeros;
    }



    //Morton code bit magic from https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/

    // "Insert" a 0 bit after each of the 16 low bits of x
    uint Part1By1(uint x)
    {
        x &= 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
        x = (x ^ (x << 8)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
        x = (x ^ (x << 4)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
        x = (x ^ (x << 2)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
        x = (x ^ (x << 1)) & 0x55555555; // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
        return x;
    }

    // Inverse of Part1By1 - "delete" all odd-indexed bits
    uint Compact1By1(uint x)
    {
        x &= 0x55555555; // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
        x = (x ^ (x >> 1)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
        x = (x ^ (x >> 2)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
        x = (x ^ (x >> 4)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
        x = (x ^ (x >> 8)) & 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
        return x;
    }

    uint EncodeMorton2(uint x, uint y)
    {
        return (Part1By1(y) << 1) + Part1By1(x);
    }

    uint DecodeMorton2X(uint code)
    {
        return Compact1By1(code >> 0);
    }

    uint DecodeMorton2Y(uint code)
    {
        return Compact1By1(code >> 1);
    }
}

public class Node
{
    public Node(int _id, bool _ASide = false)
    {
        id = _id;
        ASide = _ASide;
    }
    public int id;
    public bool ASide;
}

public class LeafNode : Node
{
    public LeafNode(Agent _agent, int _id, bool _ASide = false) : base(_id, _ASide)
    {
        agent = _agent;
    }

    public Agent agent;
}

public class InternalNode : Node
{
    public InternalNode(Node _ChildA, Node _ChildB, int _id, bool _ASide = false) : base(_id, _ASide)
    {
        ChildA = _ChildA;
        ChildB = _ChildB;
    }
    public Node ChildA;
    public Node ChildB;

    public int childAID;
    public int childBID;
}

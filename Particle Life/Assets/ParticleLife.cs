using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParticleLife : MonoBehaviour
{
    [SerializeField]
    int numAgents, numSpecies, resolution;

    [SerializeField]
    float maxDistance, minDistance;

    [SerializeField]
    ComputeShader computeShader;

    Agent[] agents;
    Species[] species;
    Rule[] rules;

    ComputeBuffer agentsBuffer, speciesBuffer, rulesBuffer, leafNodesBuffer, internalNodesBuffer;

    RenderTexture drawTexture;

    [SerializeField]
    DrawZCurve zCurvePrefab;

    DrawZCurve zCurve;

    DrawZCurve BeforeSplit, AfterSplit;

    QuadTree quadTree;

    float timer = 0;

    float height = 0.5f;

    float color = 0;

    [SerializeField]
    int SplitRegionStart, SplitRegionEnd;

    [SerializeField]
    QuadTreeUI quadTreeUI;

    bool Initialized = false;

    InternalNodeData[] internalNodesData;
    LeafNodeData[] leafNodeData;

    // Start is called before the first frame update
    void Start()
    {

        agents = CreateAgentsGrid((int)Mathf.Sqrt(numAgents));
        species = CreateSpecies(numSpecies);
        rules = CreateRules(species);

        quadTree = new QuadTree(resolution);
        agents = quadTree.SortAgents(agents);
        quadTree.Construct(agents);


        BeforeSplit = Instantiate<DrawZCurve>(zCurvePrefab);
        AfterSplit = Instantiate<DrawZCurve>(zCurvePrefab);

        BeforeSplit.resolution = resolution / 10f;
        BeforeSplit.transform.position = new Vector3(-5, 0, -5);
        AfterSplit.resolution = resolution / 10f;
        AfterSplit.transform.position = new Vector3(-5, 0, -5);

        BeforeSplit.lineRenderer.startColor = Color.red;
        BeforeSplit.lineRenderer.endColor = Color.red;
        AfterSplit.lineRenderer.startColor = Color.blue;
        AfterSplit.lineRenderer.endColor = Color.blue;



        /*
        zCurve = Instantiate<DrawZCurve>(zCurvePrefab);
        zCurve.lineRenderer.startColor = Color.HSVToRGB(color, 1, 1);
        zCurve.lineRenderer.endColor = Color.HSVToRGB(color, 1, 1);
        zCurve.resolution = resolution / 10f;
        zCurve.transform.position = new Vector3(-5, 0, -5);
        zCurve.SetPoints(agents, height);
        */
        
        agentsBuffer = new ComputeBuffer(numAgents, sizeof(int) * 2 + sizeof(float) * 5);
        speciesBuffer = new ComputeBuffer(numSpecies, sizeof(float) * 4);
        rulesBuffer = new ComputeBuffer(numSpecies * numSpecies, sizeof(float) * 2);
        leafNodesBuffer = new ComputeBuffer(numAgents, sizeof(int) * 2);
        internalNodesBuffer = new ComputeBuffer(numAgents - 1, sizeof(int) * 7 + sizeof(float) * 4);

        internalNodesData = new InternalNodeData[numAgents - 1];
        leafNodeData = new LeafNodeData[numAgents];

        agentsBuffer.SetData(agents);
        speciesBuffer.SetData(species);
        rulesBuffer.SetData(rules);
        leafNodesBuffer.SetData(leafNodeData);
        internalNodesBuffer.SetData(internalNodesData);



        computeShader.SetBuffer(0, "agents", agentsBuffer);
        computeShader.SetBuffer(0, "species", speciesBuffer);
        computeShader.SetBuffer(0, "rules", rulesBuffer);
        computeShader.SetBuffer(0, "leafNodes", leafNodesBuffer);
        computeShader.SetBuffer(0, "internalNodes", internalNodesBuffer);

        computeShader.SetBuffer(1, "agents", agentsBuffer);
        computeShader.SetBuffer(1, "leafNodes", leafNodesBuffer);
        computeShader.SetBuffer(1, "internalNodes", internalNodesBuffer);

        computeShader.SetBuffer(2, "agents", agentsBuffer);
        computeShader.SetBuffer(2, "leafNodes", leafNodesBuffer);
        computeShader.SetBuffer(2, "internalNodes", internalNodesBuffer);



        drawTexture = new RenderTexture(resolution, resolution, 0);
        drawTexture.enableRandomWrite = true;
        drawTexture.Create();

        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetInt("numSpecies", numSpecies);
        computeShader.SetInt("resolution", resolution);

        computeShader.SetFloat("minDistance", minDistance);
        computeShader.SetFloat("maxDistance", maxDistance);
        computeShader.SetFloat("repulsionForce", 10f);
        computeShader.SetTexture(0, "Result", drawTexture);

        Renderer renderer = GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", drawTexture);

        Initialized = true;
    }

    private void OnValidate()
    {
        if (Initialized)
        {

        }
    }

    string MortonCodeToString(uint mortonCode)
    {
        string mortonCodeString = Convert.ToString(mortonCode, 2);

        int leadingZeros = 32 - mortonCodeString.Length;

        string result = "";

        for(int i = 0; i < leadingZeros; i++)
        {
            result += "0";
        }

        result += mortonCodeString;

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        agentsBuffer.GetData(agents);
        for (int i = 0; i < agents.Length; i++)
        {
            Debug.Log(string.Format("Agent {0}: {1}",
                agents[i].id,
                MortonCodeToString(agents[i].mortonCode)));
        }
        agents = quadTree.SortAgents(agents);
        agentsBuffer.SetData(agents);
        computeShader.Dispatch(1, numAgents / 64, 1, 1);
        //computeShader.Dispatch(2, numAgents / 64, 1, 1);

        internalNodesBuffer.GetData(internalNodesData);
        quadTreeUI.DrawQuadtree(internalNodesData, agents);



        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(0, numAgents / 64, 1, 1);





        /*
        timer += Time.deltaTime;

        if(timer >= 3)
        {
            height += 0.2f;
            color += 0.05f;
            color = color % 1;
            timer = 0;

            zCurve = Instantiate<DrawZCurve>(zCurvePrefab);
            zCurve.lineRenderer.startColor = Color.HSVToRGB(color, 1, 1);
            zCurve.lineRenderer.endColor = Color.HSVToRGB(color, 1, 1);
            zCurve.resolution = resolution / 10f;
            zCurve.transform.position = new Vector3(-5, 0, -5);
            zCurve.SetPoints(agents, height);
        }
        */
    }

    Agent[] CreateAgents(int agentsToCreate)
    {
        Agent[] createdAgents = new Agent[agentsToCreate];

        for(int i = 0; i < agentsToCreate; i++)
        {
            int species = i % numSpecies;
            Vector2 position = (Random.insideUnitCircle + Vector2.one) / 2f * resolution;
            Vector2 velocity = (Random.insideUnitCircle);

            Agent a = new Agent(i, species, position, velocity);
            createdAgents[i] = a;
        }

        return createdAgents;
    }

    Agent[] CreateAgentsGrid(int gridDivisions)
    {
        Agent[] createdAgents = new Agent[gridDivisions * gridDivisions];

        for(int i = 0; i < gridDivisions; i++)
        {
            for(int j = 0; j < gridDivisions; j++)
            {
                int species = (i * gridDivisions + j) % numSpecies;
                float offsetX = (i + 0.5f) * ((float)resolution / gridDivisions);
                float offsetY = (j + 0.5f) * ((float)resolution / gridDivisions);
                Vector2 velocity = Random.insideUnitCircle;

                Agent a = new Agent(i * gridDivisions + j, species, new Vector2(offsetX, offsetY), velocity);
                createdAgents[i * gridDivisions + j] = a;
            }
        }

        return createdAgents;
    }

    Species[] CreateSpecies(int speciesToCreate)
    {
        Species[] createdSpecies = new Species[speciesToCreate];

        for(int i = 0; i < speciesToCreate; i++)
        {
            float hue = i / (float)speciesToCreate;
            Color color = Color.HSVToRGB(hue, 1, 1);

            Species s = new Species(color);
            createdSpecies[i] = s;
        }

        return createdSpecies;
    }

    Rule[] CreateRules(Species[] species)
    {
        Rule[] createdRules = new Rule[species.Length * species.Length];

        for(int i = 0; i < species.Length; i++)
        {
            for(int j = 0; j < species.Length; j++)
            {
                float forceAmount = Random.value * 1000 - 500;
                float forceDistance = maxDistance / Random.value * 10;

                Rule r = new Rule(new Vector2(forceDistance, forceAmount));

                createdRules[j + i * species.Length] = r;
            }
        }

        return createdRules;
    }
}

public struct Agent
{
    public Agent(int _id, int _species, Vector2 _position, Vector2 _velocity, uint _mortonCode = 0)
    {
        id = _id;
        species = _species;
        position = _position;
        velocity = _velocity;
        mortonCode = _mortonCode;
    }

    public int id;
    public int species;
    public Vector2 position;
    public Vector2 velocity;
    public uint mortonCode;
}

public struct Species
{
    public Species(Color _color)
    {
        color = _color;
    }

    Color color;
}

public struct Rule
{
    public Rule(Vector2 _force)
    {
        force = _force;
    }

    Vector2 force;
}

public struct LeafNodeData
{
    public int index;
    public int parentIndex;
}

public struct InternalNodeData
{
    public int index;
    public Vector2Int range;
    public Vector4 boundingBox;
    public int parentIndex;
    public int childAIndex;
    public int childBIndex;
    public int visited;
}



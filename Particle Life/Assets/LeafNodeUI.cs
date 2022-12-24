using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeafNodeUI : NodeUI
{
    public TextMeshProUGUI mortonCode;

    public QuadTree quadTree;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mortonCode.text = System.Convert.ToString(quadTree.leafNodes[Node.id].agent.mortonCode, 2);
    }
}

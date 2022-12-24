using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI indexText;

    public RectTransform rectTransform;

    [SerializeField]
    protected Image BG;

    public Rect BoundingBox;

    virtual public bool ASide
    {
        get
        {
            return aSide;
        }

        set
        {
            aSide = value;
        }
    }

    private bool aSide = false;
    
    public bool Visited = false;

    virtual public Node Node
    {
        get
        {
            return node;
        }

        set
        {
            node = value;

            indexText.text = node.id.ToString();
        }
    }

    private Node node;
}

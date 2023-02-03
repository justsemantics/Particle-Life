using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InternalNodeUI : NodeUI
{
    public RectTransform IndexKnob;
    public Vector2 SplitOffset = Vector2.zero;

    private bool aSide;
    public bool ASide 
    {
        get => aSide;
        set
        {
            aSide = value;

            if (ASide)
            {
                BG.color = new Color(1, 0, 0, 0.5f);
            }
            else
            {
                BG.color = new Color(0, 0, 1, 0.5f);
            }
        } 
    }

    void Highlight()
    {
        OnHighlight(this);
    }

    public Action<InternalNodeUI> OnHighlight = (node) => { };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

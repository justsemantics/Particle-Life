using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalNodeUI : NodeUI
{
    public RectTransform Children;

    public RectTransform IndexKnob;

    public QuadTreeUI quadTreeUI;



    public override bool ASide 
    { 
        get => base.ASide;
        set
        {
            base.ASide = value;

            if (ASide)
            {
                IndexKnob.SetSiblingIndex(1);
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
        quadTreeUI.Highlight(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

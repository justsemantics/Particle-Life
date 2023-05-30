using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeafNodeUI : NodeUI
{
    public TextMeshProUGUI mortonCode;


    public void Highlight()
    {
        OnHighlight(this);
    }

    public Action<LeafNodeUI> OnHighlight = (node) => { };
}

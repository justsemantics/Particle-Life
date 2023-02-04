using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI indexText;

    public RectTransform rectTransform;

    [SerializeField]
    public Image BG;

    public Rect BoundingBox;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonTextUpdater : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonText = null;
    int x = 20, y = 20;
    public void SetMapSizeX(float n) {
        x = (int)n;
        UpdateText();
    }
    public void SetMapSizeY(float n) {
        y = (int)n;
        UpdateText();
    }
    void UpdateText() {
        buttonText.text = $"Generate {x}x{y} Maze";
    }
}

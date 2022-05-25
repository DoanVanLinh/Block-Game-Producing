using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gem : MonoBehaviour
{
    [SerializeField]private int type;
    
    private GamePlay gamePlayScript;

    public int Type { get{return type;} private set{}}
    public int X { get; set; }
    public int Y { get; set; }
    
    public void SetPosition(int x, int y, GamePlay gamePlayScript)
    {
        this.X = x;
        this.Y = y;
        this.gamePlayScript = gamePlayScript;
    }
    
    private void OnMouseDown() {
        StartCoroutine(gamePlayScript.AddOrRemoveGemChosen(this));
    }
}

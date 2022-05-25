using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region  Singleton
    public static GameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion

    private int pointPerGem = 10;

    public bool IsEnd { get; set; }
    public bool IsPause{ get; set; }
    public int Point { get; set; }

    private void Start()
    {
        IsEnd = false;
        IsPause = false;
        Point = 0;
    }

    public void CalculatorPoint(int countGem)
    {
        switch (countGem)
        {
            case 4:
                Point += 4 * pointPerGem * 2;
                UIManager.Instance.ExtraPoints();
                break;
            case 5:
                Point += 5 * pointPerGem * 3;
                UIManager.Instance.ExtraPoints();
                break;
            case 6:
                Point += 6 * pointPerGem * 4;
                UIManager.Instance.ExtraPoints();
                break;
            case 7:
                Point += 7 * pointPerGem * 5;
                UIManager.Instance.ExtraPoints();
                break;
            case 8:
                Point += 8 * pointPerGem * 6;
                UIManager.Instance.ExtraPoints();
                break;
            default:
                Point += 3 * pointPerGem;
                break;
        }
    }
}

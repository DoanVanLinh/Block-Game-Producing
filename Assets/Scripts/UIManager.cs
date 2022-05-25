using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region  Singleton
    public static UIManager Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion

    [SerializeField] private Animator resetPanel;
    [SerializeField] private Animator extraPoints;
    [SerializeField] private TextMeshProUGUI pointText;

    private void Start() {
        UpdatePoint();
    }
    public void Pause()
    {
        GameManager.Instance.IsPause = !GameManager.Instance.IsPause;
        resetPanel.SetTrigger("Show");
    }

    public void Restart()
    {
        GameManager.Instance.IsEnd = false;
        SceneManager.LoadScene(0);
    }
    public void UpdatePoint()
    {
        pointText.text = GameManager.Instance.Point.ToString();
    }
    public void ExtraPoints()
    {
        extraPoints.SetTrigger("Show");
    }
}

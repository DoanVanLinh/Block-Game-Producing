using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    #region Singleton
    public static ControlManager Instance{ get; set; }
    private void Awake() {
        if(Instance==null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion
    

}

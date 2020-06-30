using UnityEngine;
using System.Collections.Generic;

public class ButtonSelection : MonoBehaviour {
    public GameObject activePanel;
    public void ShowMyActivePanel() {    
        activePanel.SetActive(true);
    }
}
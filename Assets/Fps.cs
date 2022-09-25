using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;

public class Fps : MonoBehaviour {

    //private Text fpsText;
    
    private String fpsFile = "fps.txt";
    private List<double> fpsList;

    void Start() {
        //fpsText = GetComponent<Text>();
        fpsList = new List<double>();
        InvokeRepeating("WriteFps", 5, 5);
    }

    void Update() {
        double fps = Math.Round(1.0f / Time.smoothDeltaTime, 1);
        //fpsText.text = fps.ToString();
        fpsList.Add(fps);
    }

    private void WriteFps() {
        File.AppendAllText(fpsFile, "time 5\n");
        List<string> fpsStringList = fpsList.Select(fps => fps.ToString()).ToList();
        File.AppendAllLines(fpsFile, fpsStringList);
        fpsList.Clear();
    }

}

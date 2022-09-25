using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAgentBody : MonoBehaviour {

    public GameObject HumanAgent;

    void Update() {
        transform.position = HumanAgent.GetComponent<HumanAgent>().Position + new Vector3(0, 0.5f, 0);
    }
}

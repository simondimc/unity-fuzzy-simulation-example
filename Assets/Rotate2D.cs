using UnityEngine;

public class Rotate2D : MonoBehaviour {

    public GameObject BirdAgent;

    void Update() {
        transform.position = BirdAgent.GetComponent<BirdAgent2D>().Position;
        
        Vector3 euler = transform.rotation.eulerAngles;
        Vector3 sphereDir = BirdAgent.GetComponent<BirdAgent2D>().Direction;
        euler.y = Mathf.Atan2(sphereDir.x, sphereDir.z) * Mathf.Rad2Deg + 90;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler.x, euler.y, euler.z), Time.deltaTime * 2f);
    }
}

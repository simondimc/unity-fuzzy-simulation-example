using UnityEngine;

public class Rotate3D : MonoBehaviour {

    public GameObject BirdAgent;

    void Update() {
        transform.position = BirdAgent.GetComponent<BirdAgent3D>().Position;

        Vector3 sphereDir = BirdAgent.GetComponent<BirdAgent3D>().Direction;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, sphereDir, Time.deltaTime * 2f, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}

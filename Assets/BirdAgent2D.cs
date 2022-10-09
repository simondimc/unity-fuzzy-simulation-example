using UnityEngine;

public class BirdAgent2D : Agent {

    [Header("Bird Agent")]
    public float WallPerceptionRadius = 6;
    public GameObject Sphere;
    public float MinSpeed;
    public float MaxSpeed;
    private Rigidbody sphereRigidbody;
    private GameObject[] walls;

    public override Vector3 Position {
        get {
            return this.Sphere.transform.position;
        }
        set {
            this.Sphere.transform.position = value;
        }
    }

    public override Vector3 Direction { get; set; }

    public override float Speed { get; set; }

    private void Start() {
        this.sphereRigidbody = Sphere.GetComponent<Rigidbody>();
        this.Direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        this.Speed = Random.Range(this.MinSpeed, this.MaxSpeed);
        this.walls = GameObject.FindGameObjectsWithTag("Wall");
    }

    private void FixedUpdate() {
        this.sphereRigidbody.AddForce(this.Direction * this.Speed);
    }

    private void Update() {

        if (this.Neighbors != null && this.Neighbors.Count > 0) {
            
            for (int i = 0; i < this.Neighbors.Count; i++) {
                Vector3 neighborPosition = this.Neighbors[i].Position;
                Vector3 neighborDirection = this.Neighbors[i].Direction;
                float neighborSpeed = this.Neighbors[i].Speed;

                float distance = (Vector3.Distance(this.Position, neighborPosition) / this.PerceptionRadius) * 100;
                if (distance > 100) distance = 100;
                if (distance < 0) distance = 0;
                this.GetFuzzyController().SetValue(i, "distance", distance);

                float position = Vector3.SignedAngle(
                    this.Direction, 
                    neighborPosition - this.Position, 
                    Vector3.up
                );
                this.GetFuzzyController().SetValue(i, "position", position);

                float direction = Vector3.SignedAngle(
                    this.Direction, 
                    neighborDirection, 
                    Vector3.up
                );
                this.GetFuzzyController().SetValue(i, "direction", direction);

                float speed = (((neighborSpeed - this.Speed) - this.MinSpeed) / (this.MaxSpeed - this.MinSpeed)) * 100;
                if (speed > 100) speed = 100;
                if (speed < -100) speed = -100;
                this.GetFuzzyController().SetValue(i, "speed", speed);
            }
        }

        float? min_wall_distance = null;
        Vector2? min_wall_position = null;

        foreach(GameObject wall in this.walls) {
            float d = wall.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            float sx = wall.transform.localScale.x;
            Vector3 p1 = wall.transform.position + new Vector3(Mathf.Cos(d) * sx / 2, 0, Mathf.Sin(d) * sx / 2);
            Vector3 p2 = wall.transform.position + new Vector3(Mathf.Cos(d) * -sx / 2, 0, Mathf.Sin(d) * -sx / 2);

            Vector3 pointDistance = Utils.PointDistanceLinePoint(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(this.Position.x, this.Position.z));

            if (min_wall_distance == null || pointDistance.z < min_wall_distance) {
                min_wall_distance = pointDistance.z;
                min_wall_position = new Vector2(pointDistance.x, pointDistance.y);
            }
        }

        if (min_wall_position != null) {

            Vector3 wall_position = new Vector3(min_wall_position.Value.x, 0, min_wall_position.Value.y);
            Vector3 transform_position = new Vector3(this.Position.x, 0, this.Position.z);

            float distanceW = (Vector3.Distance(transform_position, wall_position) / WallPerceptionRadius) * 100;
            if (distanceW > 100) distanceW = 100;
            if (distanceW < 0) distanceW = 0;

            float positionW = Vector3.SignedAngle(
                this.Direction, 
                wall_position - transform_position, 
                Vector3.up
            );

            this.GetFuzzyController().SetValue(-1, "distance_wall", distanceW);
            this.GetFuzzyController().SetValue(-1, "position_wall", positionW); 
        }
        
        this.GetFuzzyController().Step();

        float? flightDirection = this.GetFuzzyController().GetValue("flight_direction");
        float? flightSpeed = this.GetFuzzyController().GetValue("flight_speed");

        if (flightDirection != null) this.Direction = (Quaternion.AngleAxis(flightDirection.Value, Vector3.up) * this.Direction).normalized;
        if (flightSpeed != null) this.Speed += (flightSpeed.Value / 100) * (this.MaxSpeed - this.MinSpeed) + this.MinSpeed;

        if (this.Speed > this.MaxSpeed) this.Speed = this.MaxSpeed;
        if (this.Speed < this.MinSpeed) this.Speed = this.MinSpeed;
    }

}

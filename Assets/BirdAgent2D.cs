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
            
            float distanceP = 0;
            float positionP = 0;
            float directionP = 0;
            float speedP = 0;

            Vector3 observed_position = new Vector3(0, 0, 0);
            Vector3 observed_direction = new Vector3(0, 0, 0);
            float observed_speed = 0;
            foreach(Agent a in this.Neighbors) {
                observed_position += a.Position;
                observed_direction += a.Direction;
                observed_speed += a.Speed;
            }
            observed_position /= this.Neighbors.Count;
            observed_direction /= this.Neighbors.Count;
            observed_direction = observed_direction.normalized;
            observed_speed /= this.Neighbors.Count;

            distanceP = (Vector3.Distance(Position, observed_position) / this.PerceptionRadius) * 100;
            if (distanceP > 100) distanceP = 100;
            if (distanceP < 0) distanceP = 0;

            positionP = Vector3.SignedAngle(
                this.Direction, 
                observed_position - Position, 
                Vector3.up
            );

            directionP = Vector3.SignedAngle(
                this.Direction, 
                observed_direction, 
                Vector3.up
            );

            speedP = (((observed_speed - this.Speed) - this.MinSpeed) / (this.MaxSpeed - this.MinSpeed)) * 100;
            if (speedP > 100) speedP = 100;
            if (speedP < -100) speedP = -100;

            this.GetFuzzyController().SetValue("distance", distanceP);
            this.GetFuzzyController().SetValue("position", positionP);
            this.GetFuzzyController().SetValue("direction", directionP);
            this.GetFuzzyController().SetValue("speed", speedP);
        }

        float? min_wall_distance = null;
        Vector2? min_wall_position = null;

        foreach(GameObject wall in this.walls) {
            float d = wall.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            float sx = wall.transform.localScale.x;
            Vector3 p1 = wall.transform.position + new Vector3(Mathf.Cos(d) * sx / 2, 0, Mathf.Sin(d) * sx / 2);
            Vector3 p2 = wall.transform.position + new Vector3(Mathf.Cos(d) * -sx / 2, 0, Mathf.Sin(d) * -sx / 2);

            Vector3 pointDistance = Utils.PointDistanceLinePoint(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(Position.x, Position.z));

            if (min_wall_distance == null || pointDistance.z < min_wall_distance) {
                min_wall_distance = pointDistance.z;
                min_wall_position = new Vector2(pointDistance.x, pointDistance.y);
            }
        }

        if (min_wall_position != null) {

            float distanceW = 0;
            float positionW = 0;

            Vector3 wall_position = new Vector3(min_wall_position.Value.x, 0, min_wall_position.Value.y);
            Vector3 transform_position = new Vector3(Position.x, 0, Position.z);

            distanceW = (Vector3.Distance(transform_position, wall_position) / WallPerceptionRadius) * 100;
            if (distanceW > 100) distanceW = 100;
            if (distanceW < 0) distanceW = 0;


            positionW = Vector3.SignedAngle(this.Direction, wall_position - transform_position, Vector3.up);

            this.GetFuzzyController().SetValue("distance_wall", distanceW);
            this.GetFuzzyController().SetValue("position_wall", positionW);
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

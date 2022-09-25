using UnityEngine;

public class BirdAgent3D : Agent {

    [Header("Bird Agent")]
    public float WallPerceptionRadius = 6;
    public GameObject Sphere;
    public float MinSpeed;
    public float MaxSpeed;
    private Rigidbody sphereRigidbody;
    private Vector4[] walls;

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
        this.Direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        this.Speed = Random.Range(this.MinSpeed, this.MaxSpeed);
        this.walls = new Vector4[] {
            new Vector4(1, 0, 0, 0),
            new Vector4(1, 0, 0, 100),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 1, 0, 100),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 1, 100),
        };
    }

    private void FixedUpdate() {
        this.sphereRigidbody.AddForce(this.Direction * this.Speed);
    }

    private void Update() {

        Vector3 r = Vector3.Cross(Vector3.up, this.Direction).normalized;
        Vector3 u = Vector3.Cross(this.Direction, r).normalized;
        
        if (this.Neighbors != null && this.Neighbors.Count > 0) {
            
            float distanceP = 0;
            float hPositionP = 0;
            float vPositionP = 0;
            float hDirectionP = 0;
            float vDirectionP = 0;
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

            distanceP = (Vector3.Distance(this.Position, observed_position) / this.PerceptionRadius) * 100;
            if (distanceP > 100) distanceP = 100;
            if (distanceP < 0) distanceP = 0;

            hPositionP = Vector3.SignedAngle(
                this.Direction, 
                Vector3.ProjectOnPlane(observed_position - Position, u),
                u
            );

            vPositionP = Vector3.SignedAngle(
                this.Direction, 
                Vector3.ProjectOnPlane(observed_position - Position, r), 
                r
            );

            hDirectionP = Vector3.SignedAngle(
                this.Direction, 
                Vector3.ProjectOnPlane(observed_direction, u),
                u
            );

            vDirectionP = Vector3.SignedAngle(
                this.Direction, 
                Vector3.ProjectOnPlane(observed_direction, r),
                r
            );

            speedP = (((observed_speed - this.Speed) - this.MinSpeed) / (this.MaxSpeed - this.MinSpeed)) * 100;
            if (speedP > 100) speedP = 100;
            if (speedP < -100) speedP = -100;

            this.GetFuzzyController().SetValue("distance", distanceP);
            this.GetFuzzyController().SetValue("h_position", hPositionP);
            this.GetFuzzyController().SetValue("v_position", vPositionP);
            this.GetFuzzyController().SetValue("h_direction", hDirectionP);
            this.GetFuzzyController().SetValue("v_direction", vDirectionP);
            this.GetFuzzyController().SetValue("speed", speedP);
        }

        float? min_wall_distance = null;
        Vector3? min_wall_position = null;

        foreach(Vector4 wall in this.walls) {
            float d = Utils.DistancePointPlane(this.Position, wall);
            Vector3 p = Utils.PointPointPlane(this.Position, wall);

            if (min_wall_distance == null || d < min_wall_distance) {
                min_wall_distance = d;
                min_wall_position = p;
            }
        }

        if (min_wall_position != null) {

            float distanceW = 0;
            float hPositionW = 0;
            float vPositionW = 0;

            distanceW = (min_wall_distance.Value / WallPerceptionRadius) * 100;
            if (distanceW > 100) distanceW = 100;
            if (distanceW < 0) distanceW = 0;

            Vector3 wall_dir = (min_wall_position.Value - this.Position);

            Vector3 projh = Vector3.ProjectOnPlane(wall_dir, u);

            Vector3 projv = Vector3.ProjectOnPlane(wall_dir, r);

            hPositionW = Vector3.SignedAngle(
                this.Direction, 
                projh, 
                u
            );

            vPositionW = Vector3.SignedAngle(
                this.Direction, 
                projv, 
                r
            );

            this.GetFuzzyController().SetValue("distance_wall", distanceW);
            this.GetFuzzyController().SetValue("h_position_wall", hPositionW);
            this.GetFuzzyController().SetValue("v_position_wall", vPositionW);
        }

        this.GetFuzzyController().Step();

        float? hFlightDirection = this.GetFuzzyController().GetValue("h_flight_direction");
        float? vFlightDirection = this.GetFuzzyController().GetValue("v_flight_direction");
        float? flightSpeed = this.GetFuzzyController().GetValue("flight_speed");

        if (hFlightDirection != null && vFlightDirection != null) {
            this.Direction = (Quaternion.AngleAxis(vFlightDirection.Value, r) * this.Direction).normalized;
            this.Direction = (Quaternion.AngleAxis(hFlightDirection.Value, u) * this.Direction).normalized;
        }

        if (flightSpeed != null) this.Speed += (flightSpeed.Value / 100) * (this.MaxSpeed - this.MinSpeed) + this.MinSpeed;

        if (this.Speed > this.MaxSpeed) this.Speed = this.MaxSpeed;
        if (this.Speed < this.MinSpeed) this.Speed = this.MinSpeed;

    }
}

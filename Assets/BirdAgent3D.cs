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

            for (int i = 0; i < this.Neighbors.Count; i++) {
                Vector3 neighborPosition = this.Neighbors[i].Position;
                Vector3 neighborDirection = this.Neighbors[i].Direction;
                float neighborSpeed = this.Neighbors[i].Speed;

                float distance = (Vector3.Distance(this.Position, neighborPosition) / this.PerceptionRadius) * 100;
                if (distance > 100) distance = 100;
                if (distance < 0) distance = 0;
                this.GetFuzzyController().SetValue(i, "distance", distance);

                float hPosition = Vector3.SignedAngle(
                    this.Direction, 
                    Vector3.ProjectOnPlane(neighborPosition - this.Position, u),
                    u
                );
                this.GetFuzzyController().SetValue(i, "h_position", hPosition);

                float vPosition = Vector3.SignedAngle(
                    this.Direction, 
                    Vector3.ProjectOnPlane(neighborPosition - this.Position, r), 
                    r
                );
                this.GetFuzzyController().SetValue(i, "v_position", vPosition);

                float hDirection = Vector3.SignedAngle(
                    this.Direction, 
                    Vector3.ProjectOnPlane(neighborDirection, u),
                    u
                );
                this.GetFuzzyController().SetValue(i, "h_direction", hDirection);

                float vDirection = Vector3.SignedAngle(
                    this.Direction, 
                    Vector3.ProjectOnPlane(neighborDirection, r),
                    r
                );
                this.GetFuzzyController().SetValue(i, "v_direction", vDirection);

                float speed = (((neighborSpeed - this.Speed) - this.MinSpeed) / (this.MaxSpeed - this.MinSpeed)) * 100;
                if (speed > 100) speed = 100;
                if (speed < -100) speed = -100;
                this.GetFuzzyController().SetValue(i, "speed", speed);
            }
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

            this.GetFuzzyController().SetValue(-1, "distance_wall", distanceW);
            this.GetFuzzyController().SetValue(-1, "h_position_wall", hPositionW);
            this.GetFuzzyController().SetValue(-1, "v_position_wall", vPositionW);
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

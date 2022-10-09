using UnityEngine;

public class HumanAgent : Agent {

    [Header("Human Agent")]
    public float WallPerceptionRadius = 6;
    public GameObject Sphere;
    public float MinSpeed;
    public float MaxSpeed;
    private Rigidbody sphereRigidbody;
    private GameObject[] walls;
    private GameObject[] obstacles;

    private GameObject target;
    private GameObject grid;

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
        this.obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        this.target = GameObject.FindGameObjectWithTag("Target");
        this.grid = GameObject.FindGameObjectWithTag("Grid");
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

        foreach(GameObject obstacle in this.obstacles) {
            Vector2 pos = new Vector2(obstacle.transform.position.x, obstacle.transform.position.z);
            float width = obstacle.transform.localScale.z;
            float height = obstacle.transform.localScale.x;

            Vector2 tl = new Vector2(pos.x - width / 2, pos.y + height / 2);
            Vector2 tr = new Vector2(pos.x + width / 2, pos.y + height / 2);
            Vector2 bl = new Vector2(pos.x - width / 2, pos.y - height / 2);
            Vector2 br = new Vector2(pos.x + width / 2, pos.y - height / 2);

            Vector3 td = Utils.PointDistanceLinePoint(tl, tr, new Vector2(this.Position.x, this.Position.z));
            if (min_wall_distance == null || td.z < min_wall_distance) {
                min_wall_distance = td.z;
                min_wall_position = new Vector2(td.x, td.y);
            }

            Vector3 bd = Utils.PointDistanceLinePoint(bl, br, new Vector2(this.Position.x, this.Position.z));
            if (min_wall_distance == null || bd.z < min_wall_distance) {
                min_wall_distance = bd.z;
                min_wall_position = new Vector2(bd.x, bd.y);
            }

            Vector3 ld = Utils.PointDistanceLinePoint(bl, tl, new Vector2(this.Position.x, this.Position.z));
            if (min_wall_distance == null || ld.z < min_wall_distance) {
                min_wall_distance = ld.z;
                min_wall_position = new Vector2(ld.x, ld.y);
            }

            Vector3 rd = Utils.PointDistanceLinePoint(br, tr, new Vector2(this.Position.x, this.Position.z));
            if (min_wall_distance == null || rd.z < min_wall_distance) {
                min_wall_distance = rd.z;
                min_wall_position = new Vector2(rd.x, rd.y);
            }
        }

        if (min_wall_position != null) {

            float distanceW = 0;
            float positionW = 0;

            Vector3 wall_position = new Vector3(min_wall_position.Value.x, 0, min_wall_position.Value.y);
            Vector3 transform_position = new Vector3(this.Position.x, 0, this.Position.z);

            distanceW = (Vector3.Distance(transform_position, wall_position) / WallPerceptionRadius) * 100;
            if (distanceW > 100) distanceW = 100;
            if (distanceW < 0) distanceW = 0;


            positionW = Vector3.SignedAngle(this.Direction, wall_position - transform_position, Vector3.up);

            this.GetFuzzyController().SetValue(-1, "distance_wall", distanceW);
            this.GetFuzzyController().SetValue(-1, "position_wall", positionW);
        }

        if (this.target != null && this.grid != null) {
            Grid grid_script = grid.GetComponent<Grid>();
            Vector2 local_pos2 = grid_script.NextPos(new Vector2(this.Position.x, this.Position.z));
            Vector3 local_pos = new Vector3(local_pos2.x, 0, local_pos2.y);

            float distanceT = (Vector3.Distance(this.target.transform.position, this.Position) / this.PerceptionRadius) * 100;
            if (distanceT > 100) distanceT = 100;
            if (distanceT < 0) distanceT = 0;

            float positionT = Vector3.SignedAngle(this.Direction, local_pos - this.Position, Vector3.up);

            this.GetFuzzyController().SetValue(-2, "target_distance", distanceT);
            this.GetFuzzyController().SetValue(-2, "target_position", positionT);
        }

        this.GetFuzzyController().Step();

        float? walkDirection = this.GetFuzzyController().GetValue("walk_direction");
        float? walkSpeed = this.GetFuzzyController().GetValue("walk_speed");
        
        if (walkDirection != null) this.Direction = (Quaternion.AngleAxis(walkDirection.Value, Vector3.up) * this.Direction).normalized;
        if (walkSpeed != null) this.Speed += (walkSpeed.Value / 100) * (this.MaxSpeed - this.MinSpeed) + this.MinSpeed;

        if (this.Speed > this.MaxSpeed) this.Speed = this.MaxSpeed;
        if (this.Speed < this.MinSpeed) this.Speed = this.MinSpeed;
    }

}
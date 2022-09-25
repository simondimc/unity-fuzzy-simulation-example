using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class Grid : MonoBehaviour {

    public int width_cells = 20;
    public int height_cells = 10;

    public Vector2 corner1;
    public Vector2 corner2;

    private GameObject[] obstacles;
    private GameObject target;

    private bool[,] grid;

    private float[,] dist;
    private Vector2[,] prev;

    void Start() {
        this.obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        this.target = GameObject.FindGameObjectWithTag("Target");
        this.grid = new bool[width_cells, height_cells];

        this.dist = new float[width_cells, height_cells];
        this.prev = new Vector2[width_cells, height_cells];

        for (int w = 0; w < width_cells; w++) {
            for (int h = 0; h < height_cells; h++) {
                this.grid[w, h] = false;
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

            float by = bl.y;
            float ty = tl.y;
            float lx = bl.x;
            float rx = br.x;

            for (int w = 0; w < width_cells; w++) {
                for (int h = 0; h < height_cells; h++) {
                    Vector2 p = this.GridToPosition(new Vector2(w, h));

                    if (p.y >= by && p.y <= ty && p.x >= lx && p.x <= rx) {
                        this.grid[w, h] = true;
                    }
                }
            }
        }

        this.Dijkstra();

        //Vector2 target_cell = this.PositionToGrid(new Vector2(target.transform.position.x, target.transform.position.z));
        //this.grid[(int)target_cell.x, (int)target_cell.y] = true;
    }

    /*
    void OnDrawGizmosSelected(){
        if(Application.isPlaying) {
            for (int w = 0; w < width_cells; w++) {
                for (int h = 0; h < height_cells; h++) {

                    if (this.grid[w, h])
                        Gizmos.color = Color.magenta;
                    else
                        Gizmos.color = Color.cyan;
                    
                    Vector2 p = this.GridToPosition(new Vector2(w, h));
                    Gizmos.DrawSphere(new Vector3(p.x, 0, p.y), 1);


                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.black; 

                    Handles.Label(new Vector3(p.x, 3, p.y), this.prev[w, h].ToString(), style);

                    Handles.Label(new Vector3(p.x, 4, p.y), new Vector2(w, h).ToString(), style);
                }
            }
        }
    }
    */

    private void Dijkstra() {

        List<Vector2> q = new List<Vector2>();

        for (int w = 0; w < width_cells; w++) {
            for (int h = 0; h < height_cells; h++) {
                this.dist[w, h] = float.MaxValue;
                this.prev[w, h] = Vector2.negativeInfinity;

                if (!this.grid[w, h]) {
                    q.Add(new Vector2(w, h));
                }
            }
        }

        Vector2 target_cell = this.PositionToGrid(new Vector2(target.transform.position.x, target.transform.position.z));
        dist[(int)target_cell.x, (int)target_cell.y] = 0;

        while (q.Count > 0) {

            float min_v = float.MaxValue;
            Vector2 min_cell = new Vector2();

            foreach (Vector2 cell in q) {
                if (dist[(int)cell.x, (int)cell.y] < min_v) {
                    min_v = dist[(int)cell.x, (int)cell.y];
                    min_cell = cell;
                }
            }

            q.Remove(min_cell);

            Vector2 tl = new Vector2(min_cell.x - 1, min_cell.y + 1);
            if (q.Contains(tl)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)tl.x, (int)tl.y]) {
                    dist[(int)tl.x, (int)tl.y] = alt;
                    prev[(int)tl.x, (int)tl.y] = min_cell;
                }
            }

            Vector2 t = new Vector2(min_cell.x, min_cell.y + 1);
            if (q.Contains(t)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)t.x, (int)t.y]) {
                    dist[(int)t.x, (int)t.y] = alt;
                    prev[(int)t.x, (int)t.y] = min_cell;
                }
            }

            Vector2 tr = new Vector2(min_cell.x + 1, min_cell.y + 1);
            if (q.Contains(tr)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)tr.x, (int)tr.y]) {
                    dist[(int)tr.x, (int)tr.y] = alt;
                    prev[(int)tr.x, (int)tr.y] = min_cell;
                }
            }

            Vector2 l = new Vector2(min_cell.x - 1, min_cell.y);
            if (q.Contains(l)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)l.x, (int)l.y]) {
                    dist[(int)l.x, (int)l.y] = alt;
                    prev[(int)l.x, (int)l.y] = min_cell;
                }
            }

            Vector2 r = new Vector2(min_cell.x + 1, min_cell.y);
            if (q.Contains(r)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)r.x, (int)r.y]) {
                    dist[(int)r.x, (int)r.y] = alt;
                    prev[(int)r.x, (int)r.y] = min_cell;
                }
            }

            Vector2 bl = new Vector2(min_cell.x - 1, min_cell.y - 1);
            if (q.Contains(bl)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)bl.x, (int)bl.y]) {
                    dist[(int)bl.x, (int)bl.y] = alt;
                    prev[(int)bl.x, (int)bl.y] = min_cell;
                }
            }

            Vector2 b = new Vector2(min_cell.x, min_cell.y - 1);
            if (q.Contains(b)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)b.x, (int)b.y]) {
                    dist[(int)b.x, (int)b.y] = alt;
                    prev[(int)b.x, (int)b.y] = min_cell;
                }
            }

            Vector2 br = new Vector2(min_cell.x + 1, min_cell.y - 1);
            if (q.Contains(br)) {
                float alt = dist[(int)min_cell.x, (int)min_cell.y] + 1;
                if (alt < dist[(int)br.x, (int)br.y]) {
                    dist[(int)br.x, (int)br.y] = alt;
                    prev[(int)br.x, (int)br.y] = min_cell;
                }
            }

        }

    }

    void Update() {
        
    }

    public Vector2 PositionToGrid(Vector2 pos) {
        float width = corner2.x - corner1.x;
        float height = corner2.y - corner1.y;

        return new Vector2(
            Mathf.FloorToInt(((pos.x - corner1.x) / width) * width_cells),
            Mathf.FloorToInt(((pos.y - corner1.y) / height) * height_cells)
        );
    }

    public Vector2 GridToPosition(Vector2 grid) {
        float width = corner2.x - corner1.x;
        float height = corner2.y - corner1.y;

        float cell_width = width / width_cells;
        float cell_height = height / height_cells;

        return new Vector2(
            (grid.x * cell_width) + corner1.x + cell_width / 2,
            (grid.y * cell_height) + corner1.y + cell_height / 2
        );
    }

    public Vector2 NextPos(Vector2 pos) {
        Vector2 grid_pos = this.PositionToGrid(pos);
        Vector2 next_grid_pos = this.prev[(int)grid_pos.x, (int)grid_pos.y];
        Vector2 next_pos = this.GridToPosition(next_grid_pos);
        return next_pos;
    }

}

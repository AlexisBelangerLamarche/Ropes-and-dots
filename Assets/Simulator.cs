using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Simulator : MonoBehaviour
{
    public Vector2 lineFirst;
    public Vector2 lineSecond;
    public int counterP = 0;
    public int counterS = 0;
    public bool simulate;
    public bool RENDERPOINTS;
    public bool RENDERLINES;
    public float gravity;
    public int numIterations = 5;
    public Sprite PointTexture;
    public Material LineTexture;
    public Gradient LineColor;
    List<Point> points = new List<Point>();
    List<Stick> sticks = new List<Stick>();
    List<GameObject> PointRender = new List<GameObject>();
    List<GameObject> LineRender = new List<GameObject>();

    private Point MakeNewPoint(float x, float y, bool locked)
    {
        Point p = new Point(new Vector2(x, y), locked);
        points.Add(p);
        GameObject point = new GameObject("Point " + counterP, typeof(SpriteRenderer));
        point.GetComponent<SpriteRenderer>().sprite = PointTexture;
        point.transform.position = p.position;
        PointRender.Add(point);
        counterP += 1;
        return p;
    }

    private void MakeNewStick(Point pointA, Point pointB, float lenght)
    {
        Stick s = new Stick(pointA, pointB, lenght);
        sticks.Add(s);
        GameObject stick = new GameObject("Stick " + counterS, typeof(LineRenderer));
        LineRenderer l = stick.GetComponent<LineRenderer>();
        List<Vector3> pos = new List<Vector3>();
        pos.Add(pointA.position);
        pos.Add(pointB.position);
        l.startWidth = 0.1f;
        l.endWidth = 0.1f;
        l.SetPositions(pos.ToArray());
        l.useWorldSpace = true;
        LineRender.Add(stick);
        counterS += 1;
    }

    private void Start()
    {
        MakeSquare(3.5f, 3.5f, 0.5f);
    }

    public void MakeSquare(float sizeX, float sizeY, float distanceBetween)
    {
        for (float y = sizeY; y > -sizeY; y -= distanceBetween)
        {

            for (float x = sizeX; x > -sizeX; x -= distanceBetween)
            {
                if (y == sizeY)
                {
                    MakeNewPoint(x, y, true);
                }
                else
                {
                    MakeNewPoint(x, y, false);
                }
            }
        }

        for (int i = 0; i < points.ToArray().Length; i++)
        {
            foreach (Point p in points)
            {
                if (p.position.x == points[i].position.x && p.position.y == points[i].position.y)
                    continue;

                if ((p.position.x == points[i].position.x) && (p.position.y == (points[i].position.y + distanceBetween)))
                {
                    MakeNewStick(p, points[i], Vector2.Distance(p.position, points[i].position));
                }

                if ((p.position.x == (points[i].position.x + distanceBetween)) && (p.position.y == points[i].position.y))
                {
                    MakeNewStick(p, points[i], Vector2.Distance(p.position, points[i].position));
                }
            }
        }
    }

    private Point makingPointA;
    private Point makingPointB;
    private void Update()
    {

        if (simulate)
        {
            Simulate();

            if (Input.GetKey(KeyCode.Mouse0))
            {
                Debug.Log("Pressing");

                DeleteClosestStick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MakeNewPoint(position.x, position.y, false);
                Debug.Log("New point at: x" + position.x + " y" + position.y);
            }

            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MakeNewPoint(position.x, position.y, true);
                Debug.Log("New point at: x" + position.x + " y" + position.y);
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                lineFirst = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                makingPointA = GetClosestPoint(lineFirst);
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                lineSecond = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                makingPointB = GetClosestPoint(lineSecond);
                MakeNewStick(makingPointA, makingPointB, Vector2.Distance(lineFirst, lineSecond));
            }
        }

        u_RenderEverything();
    }

    public void u_RenderEverything()
    {

        if (RENDERLINES)
        {
            for (int i = 0; i < sticks.ToArray().Length; i++)
            {
                LineRender[i].SetActive(true);
                List<Vector3> pos = new List<Vector3>();
                pos.Add(sticks[i].PointA.position);
                pos.Add(sticks[i].PointB.position);
                LineRenderer l = LineRender[i].GetComponent<LineRenderer>();
                l.startWidth = 0.1f;
                l.endWidth = 0.1f;
                l.SetPositions(pos.ToArray());
                l.material = LineTexture;
                l.colorGradient = LineColor;
                l.useWorldSpace = true;
            }
        }
        else
        {
            foreach (GameObject s in LineRender)
            {
                    s.SetActive(false);
            }
        }

        if (RENDERPOINTS)
        {
            for (int i = 0; i < points.ToArray().Length; i++)
            {
                PointRender[i].SetActive(true);
                PointRender[i].transform.position = new Vector3(points[i].position.x, points[i].position.y, -0.1f);
            }
        }
        else
        {
            foreach (GameObject p in PointRender)
            {
                p.SetActive(false);
            }
        }
    }

    public void DeleteClosestStick(Vector2 position)
    {
        if (LineRender.Count == 0)
            return;

        float Distance;
        float bestDistance = -1;
        int iterator = 0;
        for (int i = 0; i < sticks.ToArray().Length; i++)
        {
            Vector3 middle = (LineRender[i].GetComponent<LineRenderer>().GetPosition(0) + LineRender[i].GetComponent<LineRenderer>().GetPosition(1)) / 2;
            Distance = Vector2.Distance(position, middle);

            if (bestDistance == -1)
            {
                bestDistance = Distance;
                continue;
            }

            if (Distance < bestDistance)
            {
                bestDistance = Distance;
                iterator = i;
            }
        }

        Debug.Log(bestDistance);

        if (bestDistance > 0.3f)
            return;


        GameObject.Destroy(LineRender[iterator]);
        LineRender.RemoveAt(iterator);
        sticks.RemoveAt(iterator);


    }

    public Point GetClosestPoint(Vector2 position)
    {
        float Distance;
        float bestDistance = -1;
        Point bestPoint = null;
        foreach (Point p in points)
        {
            Distance = Vector2.Distance(position, p.position);

            if (bestPoint == null)
            {
                bestDistance = Distance;
                bestPoint = p;
                continue;
            }

            if (Distance < bestDistance)
            {
                bestDistance = Distance;
                bestPoint = p;
            }
        }

        return bestPoint;
    }

    public void StartSimulate()
    {
        simulate = !simulate;
    }

    public void Simulate()
    {
        foreach (Point p in points)
        {
            if (!p.locked)
            {
                Vector2 positionBeforeUpdate = p.position;
                p.position += p.position - p.prevPosition;
                p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                p.prevPosition = positionBeforeUpdate;
            }
        }

        for (int i = 0; i < numIterations; i++)
        {
            foreach (Stick stick in sticks)
            {
                Vector2 stickCentre = (stick.PointA.position + stick.PointB.position) / 2;
                Vector2 stickDir = (stick.PointA.position - stick.PointB.position).normalized;
                if (!stick.PointA.locked)
                    stick.PointA.position = stickCentre + stickDir * stick.lenght / 2;
                if (!stick.PointB.locked)
                    stick.PointB.position = stickCentre - stickDir * stick.lenght / 2;
            }
        }
        
    }

    [System.Serializable]
    public class Point
    {
        public Vector2 position, prevPosition;
        public bool locked;
        private GameObject oldRender;

        public Point(Vector2 _position, bool _locked)
        {
            position = _position;
            prevPosition.y = position.y;
            prevPosition.x = position.x;
            locked = _locked;
        }
    }

    [System.Serializable]
    public class Stick
    {
        public Point PointA, PointB;
        public float lenght;

        public Stick(Point _pointA, Point _pointB, float _lenght)
        {
            PointA = _pointA;
            PointB = _pointB;
            lenght = _lenght;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Point p in points)
        {
            Gizmos.DrawSphere(p.position, 0.1f);
        }

        foreach (Stick stick in sticks)
        {
            Gizmos.DrawLine(stick.PointA.position, stick.PointB.position);
        }
    }
}

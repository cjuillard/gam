using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO continue around 7:12 - https://www.youtube.com/watch?v=pBMivz4rIJY&ab_channel=CodingMath
public class VerletIntegration : MonoBehaviour
{
    [Serializable]
    public class Point 
    {
        public Vector3 pos;
        public Vector3 oldPos;
        public GameObject visual;
    }

    public class Stick
    {
        public Point p0,p1;
        public float length;
        public LineRenderer visual;
    }

    public List<Point> points = new List<Point>();
    public List<Stick> sticks = new List<Stick>();
    public Bounds bounds = new Bounds(new Vector3(0,0,0), new Vector3(5,5,5));
    public GameObject pointPrefab;
    public LineRenderer stickPrefab;
    public float bounce = .9f;
    public Vector3 gravity = new Vector3(0, -0.5f, 0);
    public float friction = 0.999f;
    void Start()
    {
        InitPoints();
    }

    void Update()
    {
        
    }

    void FixedUpdate() 
    {
        UpdatePoints();
        UpdateSticks();
        UpdateRenderPos();
    }

    public void InitPoints() 
    {
        float speed = 1;
        float stepSize = speed * Time.fixedDeltaTime;
        AddPoint(new Vector3(0,2.75f,0));
        AddPoint(new Vector3(1,2f,0));

        AddStick(0,1);
    }

    private void AddPoint(Vector3 pos) {
        points.Add(new Point() {
            pos = pos,
            oldPos = pos,
            visual = Instantiate(pointPrefab, transform),
        });
    }

    private void AddStick(int index0, int index1) 
    {
        AddStick(points[index0], points[index1]);
    } 

    private void AddStick(Point p0, Point p1) 
    {
        sticks.Add(new Stick() {
            p0 = p0,
            p1 = p1,
            length = (p0.pos - p1.pos).magnitude,
            visual = Instantiate(stickPrefab, transform),
        });
    }

    public void UpdatePoints() 
    {
        foreach(Point p in points)
        {
            Vector3 v = (p.pos - p.oldPos) * friction;
            p.oldPos = p.pos;
            p.pos += v;
            p.pos += gravity * Time.fixedDeltaTime;
            if(p.pos.x > bounds.max.x) 
            {
                p.pos.x = bounds.max.x;
                p.oldPos.x = p.pos.x + v.x * bounce;
            } 
            else if(p.pos.x < bounds.min.x) 
            {
                p.pos.x = bounds.min.x;
                p.oldPos.x = p.pos.x + v.x * bounce;
            }
            
            if(p.pos.y > bounds.max.y) 
            {
                p.pos.y = bounds.max.y;
                p.oldPos.y = p.pos.y + v.y * bounce;
            } 
            else if(p.pos.y < bounds.min.y) 
            {
                p.pos.y = bounds.min.y;
                p.oldPos.y = p.pos.y + v.y * bounce;
            }
            
            if(p.pos.z > bounds.max.z) 
            {
                p.pos.z = bounds.max.z;
                p.oldPos.z = p.pos.z + v.z * bounce;
            } 
            else if(p.pos.z < bounds.min.z) 
            {
                p.pos.z = bounds.min.z;
                p.oldPos.z = p.pos.z + v.z * bounce;
            }
            
        }
    }

    public void UpdateSticks() 
    {
        foreach(Stick stick in sticks)
        {
            Vector3 delta = stick.p1.pos - stick.p0.pos;
            float distance = delta.magnitude;
            float diff = stick.length - distance;
            float percent = diff / distance / 2f;
            Vector3 offset = delta * percent;

            stick.p0.pos -= offset;
            stick.p1.pos += offset;

        }
    }

    public void UpdateRenderPos() 
    {
        foreach(Point p in points)
        {
            p.visual.transform.position = p.pos;
        }

        foreach(Stick s in sticks) 
        {
            var r = s.visual;
            r.positionCount = 2;
            r.SetPositions(new Vector3[] {
                s.p0.pos,
                s.p1.pos,
            });
        }
    }
}

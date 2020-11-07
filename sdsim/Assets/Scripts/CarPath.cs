﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathNode
{
  public Vector3 pos;
  public string activity;
}

public class CarPath
{
  public List<PathNode> nodes;
  public List<PathNode> centerNodes;
  public NavMeshPath navMeshPath;
  public int iActiveSpan = 0;


  public CarPath()
  {
    nodes = new List<PathNode>();
    centerNodes = new List<PathNode>();
    navMeshPath = new NavMeshPath();
    ResetActiveSpan();
  }

  public void ResetActiveSpan()
  {
    iActiveSpan = 0;
  }

  public PathNode GetActiveNode()
  {
    if (iActiveSpan < nodes.Count)
      return nodes[iActiveSpan];

    return null;
  }

  public void SmoothPath(float factor = 0.5f)
  {
    LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();

    for (int iN = 1; iN < nodes.Count - 2; iN++)
    {
      PathNode p = nodes[iN - 1];
      PathNode c = nodes[iN];
      PathNode n = nodes[iN + 1];

      LineSeg3d seg = new LineSeg3d(ref p.pos, ref n.pos);
      Vector3 closestP = seg.ClosestPointOnSegmentTo(ref c.pos, ref segRes);
      Vector3 dIntersect = closestP - c.pos;
      c.pos += dIntersect.normalized * factor;
    }
  }
  public double getDistance(Vector3 currentPosition, Vector3 target)
  {
    double distance = 0.0;
    if (NavMesh.CalculatePath(currentPosition, target, NavMesh.AllAreas, this.navMeshPath))
    {
      if (this.navMeshPath.corners.Length > 5)
      {
        //ignore the first corners -> more stable
        distance += (this.navMeshPath.corners[2] - currentPosition).magnitude;
        for (int i = 3; i < this.navMeshPath.corners.Length; i++)
        {
          Vector3 start = this.navMeshPath.corners[i - 1];
          Vector3 end = this.navMeshPath.corners[i];
          distance += (end - start).magnitude;
        }
      }
    }
    return distance;
  }

  public bool GetCrossTrackErr(Vector3 pos, ref float err)
  {
    if (iActiveSpan >= nodes.Count - 2)
      return false;

    PathNode a = nodes[iActiveSpan];
    PathNode b = nodes[iActiveSpan + 1];

    //2d path.
    pos.y = a.pos.y;

    LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref b.pos);

    pathSeg.Draw(Color.green);

    LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();

    Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);

    Debug.DrawLine(a.pos, closePt, Color.blue);

    if (segRes == LineSeg3d.SegResult.GreaterThanEnd)
    {
      iActiveSpan++;
    }
    else if (segRes == LineSeg3d.SegResult.LessThanOrigin)
    {
      if (iActiveSpan > 0)
        iActiveSpan--;
    }

    Vector3 errVec = pathSeg.ClosestVectorTo(ref pos);

    Debug.DrawRay(closePt, errVec, Color.white);

    float sign = 1.0f;

    Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

    if (cp.y > 0.0f)
      sign = -1f;

    err = errVec.magnitude * sign;
    return true;
  }

}
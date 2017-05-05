﻿//MIT, 2017, WinterDev
using System;
using System.Numerics;

namespace Typography.Rendering
{

    /// <summary>
    /// link between  (GlyphBoneJoint and Joint) or (GlyphBoneJoint and tipEdge)
    /// </summary>
    public class GlyphBone
    {
        public readonly GlyphBoneJoint JointA;
        public readonly GlyphBoneJoint JointB;
        public readonly EdgeLine TipEdge;




        double _len;
#if DEBUG 
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public GlyphBone(GlyphBoneJoint a, GlyphBoneJoint b)
        {
#if DEBUG 
            if (a == b)
            {
                throw new NotSupportedException();
            }
#endif

            JointA = a;
            JointB = b;
            Vector2 bpos = b.Position;
            _len = Math.Sqrt(a.CalculateSqrDistance(bpos));
            EvaluteSlope(a.Position, bpos);

            a.AddAssociateGlyphBoneToEndPoint(this);
            b.AddAssociateGlyphBoneToEndPoint(this);
        }
        public GlyphBone(GlyphBoneJoint a, EdgeLine tipEdge)
        {

            JointA = a;
            TipEdge = tipEdge;
            Vector2 midPoint = tipEdge.GetMidPoint();
            _len = Math.Sqrt(a.CalculateSqrDistance(midPoint));
            EvaluteSlope(a.Position, midPoint);

            //--------------------
            a.AddAssociateGlyphBoneToEndPoint(this);
            tipEdge.AddAssociateGlyphBoneToEndPoint(this);
        }
        public Vector2 GetVector()
        {
            if (this.JointB != null)
            {
                return JointB.Position - JointA.Position;
            }
            else if (this.TipEdge != null)
            {
                return TipEdge.GetMidPoint() - JointA.Position;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public bool IsTipBone
        {
            get { return this.TipEdge != null; }
        }


        void EvaluteSlope(Vector2 p, Vector2 q)
        {

            double x0 = p.X;
            double y0 = p.Y;
            //q
            double x1 = q.X;
            double y1 = q.Y;

            SlopeAngleNoDirection = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
            if (x1 == x0)
            {
                this.SlopeKind = LineSlopeKind.Vertical;
            }
            else
            {
                if (SlopeAngleNoDirection > MyMath._85degreeToRad)
                {
                    SlopeKind = LineSlopeKind.Vertical;
                }
                else if (SlopeAngleNoDirection < MyMath._03degreeToRad) //_15degreeToRad
                {
                    SlopeKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopeKind = LineSlopeKind.Other;
                }
            }
        }
        internal double SlopeAngleNoDirection { get; set; }
        public LineSlopeKind SlopeKind { get; set; }
        internal double Length
        {
            get
            {
                return _len;
            }
        }
        public bool IsLongBone { get; internal set; }

        //--------
        public float LeftMostPoint()
        {
            if (JointB != null)
            {
                //compare joint A and B 
                if (JointA.Position.X < JointB.Position.X)
                {
                    return JointA.GetLeftMostRib();
                }
                else
                {
                    return JointB.GetLeftMostRib();
                }
            }
            else
            {
                return JointA.GetLeftMostRib();
            }
        }


#if DEBUG
        public override string ToString()
        {
            if (TipEdge != null)
            {
                return dbugId + ":" + JointA.ToString() + "->" + TipEdge.GetMidPoint().ToString();
            }
            else
            {
                return dbugId + ":" + JointA.ToString() + "->" + JointB.ToString();
            }
        }
#endif
    }


    public static class GlyphBoneExtensions
    {

        //utils for glyph bones
        public static Vector2 GetMidPoint(this GlyphBone bone)
        {
            if (bone.JointB != null)
            {
                return (bone.JointA.Position + bone.JointB.Position) / 2;
            }
            else if (bone.TipEdge != null)
            {
                Vector2 edge = bone.TipEdge.GetMidPoint();
                return (edge + bone.JointA.Position) / 2;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        public static Vector2 GetBoneVector(this GlyphBone bone)
        {
            //if (bone.JointB != null)
            //{
            //    var b_pos = bone.JointB.Position;
            //    var a_pos = bone.JointA.Position;
            //    return new Vector2(
            //            Math.Abs(b_pos.X - a_pos.X),
            //            Math.Abs(b_pos.Y - a_pos.Y));
            //}
            //else if (bone.TipEdge != null)
            //{
            //    var b_pos = bone.TipEdge.GetMidPoint();
            //    var a_pos = bone.JointA.Position;
            //    return new Vector2(
            //            Math.Abs(b_pos.X - a_pos.X),
            //             Math.Abs(b_pos.Y - a_pos.Y));
            //    return bone.TipEdge.GetMidPoint() - bone.JointA.Position;
            //}
            //else
            //{
            //    return Vector2.Zero;
            //}

            if (bone.JointB != null)
            {
                var b_pos = bone.JointB.Position;
                var a_pos = bone.JointA.Position;
                return new Vector2(
                        b_pos.X - a_pos.X,
                        b_pos.Y - a_pos.Y);
            }
            else if (bone.TipEdge != null)
            {
                var b_pos = bone.TipEdge.GetMidPoint();
                var a_pos = bone.JointA.Position;
                return new Vector2(
                      b_pos.X - a_pos.X,
                      b_pos.Y - a_pos.Y);
            }
            else
            {
                return Vector2.Zero;
            }
        }

        /// <summary>
        /// find all outside edge a
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="outsideEdges"></param>
        /// <returns></returns>
        public static void CollectOutsideEdge(this GlyphBone bone, LineSlopeKind edgeSlopeKind, System.Collections.Generic.List<EdgeLine> outsideEdges)
        {
            if (bone.JointB != null)
            {
                GlyphTriangle commonTri = FindCommonTriangle(bone.JointA, bone.JointB);
                if (commonTri != null)
                { 
                    if (commonTri.e0.IsOutside && commonTri.e0.SlopeKind == edgeSlopeKind) { outsideEdges.Add(commonTri.e0); }
                    if (commonTri.e1.IsOutside && commonTri.e1.SlopeKind == edgeSlopeKind) { outsideEdges.Add(commonTri.e1); }
                    if (commonTri.e2.IsOutside && commonTri.e2.SlopeKind == edgeSlopeKind) { outsideEdges.Add(commonTri.e2); }
                }
            }
            else if (bone.TipEdge != null)
            {
                outsideEdges.Add(bone.TipEdge);
                EdgeLine found;
                if (ContainsEdge(bone.JointA.P_Tri, bone.TipEdge) &&
                    (found = FindAnotherOutsideEdge(bone.JointA.P_Tri, bone.TipEdge)) != null &&
                    (found.SlopeKind == edgeSlopeKind))
                {
                    outsideEdges.Add(found);
                }
                else if (ContainsEdge(bone.JointA.Q_Tri, bone.TipEdge) &&
                    (found = FindAnotherOutsideEdge(bone.JointA.Q_Tri, bone.TipEdge)) != null)
                {
                    outsideEdges.Add(found);
                }
            }
        }
        static EdgeLine FindAnotherOutsideEdge(GlyphTriangle tri, EdgeLine knownOutsideEdge)
        {
            if (tri.e0.IsOutside && tri.e0 != knownOutsideEdge) { return tri.e0; }
            if (tri.e1.IsOutside && tri.e1 != knownOutsideEdge) { return tri.e1; }
            if (tri.e2.IsOutside && tri.e2 != knownOutsideEdge) { return tri.e2; }
            return null;
        }

        static bool ContainsEdge(GlyphTriangle tri, EdgeLine edge)
        {
            return tri.e0 == edge || tri.e1 == edge || tri.e2 == edge;
        }
        static GlyphTriangle FindCommonTriangle(GlyphBoneJoint a, GlyphBoneJoint b)
        {

            if (a.P_Tri == b.P_Tri || a.P_Tri == b.Q_Tri)
            {
                return a.P_Tri;
            }
            else if (a.Q_Tri == b.P_Tri || a.Q_Tri == b.Q_Tri)
            {
                return a.Q_Tri;
            }
            else
            {
                return null;
            }
        }



    }

}
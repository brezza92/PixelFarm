﻿//MIT, 2017, WinterDev
using System;
using System.Numerics;

namespace Typography.Rendering
{
    public enum BoneDirection : byte
    {
        /// <summary>
        /// 0 degree direction (horizontal left to right)
        /// </summary>
        D0,
        D45,
        D90,
        D135,
        D180,
        D225,
        D270,
        D315
    }

    /// <summary>
    /// line link between 2 centroid contact point
    /// </summary>
    public class GlyphVirtualBone
    {
        //TODO: rename to glyph bone


    }

    public class GlyphEdgeContactSite
    {
        public EdgeLine _p_contact_edge;
        public EdgeLine _q_contact_edge;
        GlyphCentroidLine _owner;


        public GlyphEdgeContactSite(GlyphCentroidLine owner,
            EdgeLine p_contact_edge,
            EdgeLine q_contact_edge)
        {
            this._p_contact_edge = p_contact_edge;
            this._q_contact_edge = q_contact_edge;
            this._owner = owner;
        }
        public Vector2 GetContactPoint()
        {
            //mid point of the edge line
            return _p_contact_edge.GetMidPoint();
        }
        public GlyphCentroidLine OwnerCentroidBone
        {
            get { return _owner; }
        }
        public double GetSqrDistance(Vector2 v)
        {
            //get distance^2 from contact point 
            //to speicific point
            Vector2 contactPoint = this.GetContactPoint();
            float xdiff = contactPoint.X - v.X;
            float ydiff = contactPoint.Y - v.Y;

            return (xdiff * xdiff) + (ydiff * ydiff);
        }


        short _selEdgePointCount;
        Vector2 _selectedEdgePoint_A, _selectedEdgePoint_B, _tip;

        public void AddSelectedEdgePoint(Vector2 vec)
        {
            switch (_selEdgePointCount)
            {
                //not more thar2
                default: throw new NotSupportedException();
                case 0:
                    _selectedEdgePoint_A = vec;
                    break;
                case 1:
                    _selectedEdgePoint_B = vec;
                    break;
            }
            _selEdgePointCount++;
        }
        public short SelectedEdgePointCount { get { return _selEdgePointCount; } }
        public Vector2 SelectedEdgeA { get { return _selectedEdgePoint_A; } }
        public Vector2 SelectedEdgeB { get { return _selectedEdgePoint_B; } }
        public Vector2 TipPoint { get { return _tip; } set { _tip = value; } }
    }


    /// <summary>
    /// a line that connects between centroid of 2 GlyphTriangle(p => q)
    /// </summary>
    public class GlyphCentroidLine
    {

        public readonly GlyphTriangle p, q;
        public readonly double boneLength;


        GlyphEdgeContactSite _contactSite; //1L1


        public GlyphCentroidLine(GlyphTriangle p, GlyphTriangle q)
        {
            this.p = p;
            this.q = q;

            double dy = q.CentroidY - p.CentroidY;
            double dx = q.CentroidX - p.CentroidX;
            this.boneLength = Math.Sqrt(
                (dy * dy) + (dx * dx)
                );
        }

        public GlyphEdgeContactSite ContactSite { get { return _contactSite; } }

        public double SlopAngle { get; private set; }
        public bool IsLongBone { get; set; }

        public LineSlopeKind SlopKind { get; private set; }


        internal void Analyze()
        {

            //
            //p => (x0,y0)
            //q => (x1,y1)
            //line move from p to q 
            //...
            //tasks:
            //1. find slop angle
            //2. find slope kind
            //3. mark edge info


            //check if q is upper or lower when compare with p
            //check if q is on left side or right side of p
            //then we know the direction
            //....
            //p
            double x0 = p.CentroidX;
            double y0 = p.CentroidY;
            //q
            double x1 = q.CentroidX;
            double y1 = q.CentroidY;

            if (x1 == x0)
            {
                this.SlopKind = LineSlopeKind.Vertical;
                SlopAngle = 1;
            }
            else
            {
                SlopAngle = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
                if (SlopAngle > _85degreeToRad)
                {
                    SlopKind = LineSlopeKind.Vertical;
                }
                else if (SlopAngle < _03degreeToRad) //_15degreeToRad
                {
                    SlopKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopKind = LineSlopeKind.Other;
                }
            }
            //--------------------------------------
            //for p and q, count number of outside edge
            //if outsideEdgeCount of triangle >=2 -> this triangle is tip part

            int p_outsideEdgeCount = OutSideEdgeCount(p);
            int q_outsideEdgeCount = OutSideEdgeCount(q);
            //bool p_isTip = false;
            //bool q_isTip = false;

            //if (p_outsideEdgeCount >= 2)
            //{
            //    //tip bone
            //    p_isTip = true;
            //}
            //if (q_outsideEdgeCount >= 2)
            //{
            //    //tipbone
            //    q_isTip = true;
            //}

            //-------------------------------------- 
            //p_isTip && q_isTip is possible eg. dot or dot of  i etc.
            //-------------------------------------- 
            //find matching side:
            //the bone connects between triangle p and q (via centroid)
            //

            MarkEdgeSides(p.e0, q);
            MarkEdgeSides(p.e1, q);
            MarkEdgeSides(p.e2, q);
            //
            MarkEdgeSides(q.e0, p);
            MarkEdgeSides(q.e1, p);
            MarkEdgeSides(q.e2, p);
            //-------------------------------------- 


            if (_contactSite != null)
            {
                //add more information
                //find proper 'outside' edge
                MarkProperOpositeEdge(p, _contactSite, _contactSite._p_contact_edge);
                MarkProperOpositeEdge(q, _contactSite, _contactSite._q_contact_edge);
            }
        }


        static int AssignResult(EdgeLine result, ref EdgeLine edgeA, ref EdgeLine edgeB)
        {
            if (edgeA == null)
            {
                edgeA = result;
                return 1;
            }
            else
            {
                edgeB = result;
                return 2;
            }
        }

        void MarkProperOpositeEdge(GlyphTriangle triangle, GlyphEdgeContactSite contactSite, EdgeLine edge)
        {
            //find shortest part from contactsite to edge or to corner
            //draw perpendicular line to outside edge
            //and to the  corner of current edge 
            EdgeLine edgeA = null;
            EdgeLine edgeB = null;
            int count = 0;
            if (triangle.e0 != edge && triangle.e0.IsOutside)
            {
                count = AssignResult(triangle.e0, ref edgeA, ref edgeB);
            }
            if (triangle.e1 != edge && triangle.e1.IsOutside)
            {
                count = AssignResult(triangle.e1, ref edgeA, ref edgeB);
            }
            if (triangle.e2 != edge && triangle.e2.IsOutside)
            {
                count = AssignResult(triangle.e2, ref edgeA, ref edgeB);
            }
            //-------------------------------------------------------------------------------------
            switch (count)
            {
                default: throw new NotSupportedException();
                case 0:
                    break;
                case 1:
                    {
                        Vector2 perpend_A = MyMath.FindPerpendicularCutPoint(
                             new Vector2((float)edgeA.x0, (float)edgeA.y0),
                             new Vector2((float)edgeA.x1, (float)edgeA.y1),
                             contactSite.GetContactPoint());
                        Vector2 corner = new Vector2((float)edge.p.X, (float)edge.p.Y);
                        //find distance from contactSite to specific point 
                        double sqDistanceToEdgeA = contactSite.GetSqrDistance(perpend_A);
                        double sqDistanceTo_P = contactSite.GetSqrDistance(corner);

                        if (sqDistanceToEdgeA < sqDistanceTo_P)
                        {
                            contactSite.AddSelectedEdgePoint(perpend_A);
                        }
                        else
                        {
                            contactSite.AddSelectedEdgePoint(corner);
                        }
                    }
                    break;
                case 2:
                    {

                        Vector2 perpend_A = MyMath.FindPerpendicularCutPoint(
                           new Vector2((float)edgeA.x0, (float)edgeA.y0),
                           new Vector2((float)edgeA.x1, (float)edgeA.y1),
                           contactSite.GetContactPoint());
                        Vector2 perpend_B = MyMath.FindPerpendicularCutPoint(
                          new Vector2((float)edgeB.x0, (float)edgeB.y0),
                          new Vector2((float)edgeB.x1, (float)edgeB.y1),
                          contactSite.GetContactPoint());

                        Vector2 corner = new Vector2((float)edge.p.X, (float)edge.p.Y);
                        //find distance from contactSite to specific point 
                        double sqDistanceToEdgeA = contactSite.GetSqrDistance(perpend_A);
                        double sqDistanceToEdgeB = contactSite.GetSqrDistance(perpend_B);
                        double sqDistanceTo_P = contactSite.GetSqrDistance(corner);

                        int minAt = MyMath.Min(sqDistanceToEdgeA, sqDistanceToEdgeB, sqDistanceTo_P);
                        switch (minAt)
                        {
                            default: throw new NotSupportedException();
                            case 0:
                                contactSite.AddSelectedEdgePoint(perpend_A);
                                //check if B side is tip part
                                contactSite.TipPoint = edgeB.GetMidPoint();
                                break;
                            case 1:
                                contactSite.AddSelectedEdgePoint(perpend_B);
                                contactSite.TipPoint = edgeA.GetMidPoint();
                                break;
                            case 2:
                                contactSite.AddSelectedEdgePoint(corner);
                                break;
                        }
                        //-------
                        //find which edge is end edge
                        GlyphCentroidLine owner = contactSite.OwnerCentroidBone;

                    }
                    break;
            }
        }

        void MarkEdgeSides(EdgeLine edgeLine, GlyphTriangle anotherTriangle)
        {
            if (edgeLine.IsOutside)
            {
                MarkMatchingOutsideEdge(edgeLine, anotherTriangle);
            }
            else
            {
                //inside
                if (_contactSite == null)
                {
                    if (MarkMatchingInsideEdge(edgeLine, anotherTriangle))
                    {
                        _contactSite = new GlyphEdgeContactSite(
                            this,
                            edgeLine,
                            edgeLine.contactToEdge);
                    }
                }
            }
        }
        static bool MarkMatchingInsideEdge(EdgeLine insideEdge, GlyphTriangle another)
        {

            //side-by-side  
            if (another.e0.IsInside && MarkMatchingInsideEdge(insideEdge, another.e0))
            {
                //inside                 
                return true;
            }
            //
            if (another.e1.IsInside && MarkMatchingInsideEdge(insideEdge, another.e1))
            {
                return true;
            }
            //
            if (another.e2.IsInside && MarkMatchingInsideEdge(insideEdge, another.e2))
            {
                //check matching slope and coord?
                return true;
            }
            return false;
        }
        static bool MarkMatchingInsideEdge(EdgeLine p_edge, EdgeLine q_edge)
        {


            if (p_edge.x0 == q_edge.x0 && p_edge.x1 == q_edge.x1)
            {

            }
            else if (p_edge.x0 == q_edge.x1 && p_edge.x1 == q_edge.x0)
            {

            }
            else
            {
                return false; //no match in x-axis
            }
            //--------------------------------
            //y_axis
            if (p_edge.y0 == q_edge.y0 && p_edge.y1 == q_edge.y1)
            {
                p_edge.contactToEdge = q_edge;
            }
            else if (p_edge.y0 == q_edge.y1 && p_edge.y1 == q_edge.y0)
            {
                p_edge.contactToEdge = q_edge;
            }
            else
            {
                return false; //no match in y-axis
            }
            //--------------------------------
            return true;
        }
        static void MarkMatchingOutsideEdge(EdgeLine targetEdge, GlyphTriangle q)
        {

            EdgeLine matchingEdgeLine;
            int matchingEdgeSideNo;
            if (FindMatchingOuterSide(targetEdge, q, out matchingEdgeLine, out matchingEdgeSideNo))
            {
                //assign matching edge line   
                //mid point of each edge
                //p-triangle's edge midX,midY

                var pe = targetEdge.GetMidPoint();
                double pe_midX = pe.X, pe_midY = pe.Y;

                //q-triangle's edge midX,midY
                var qe = matchingEdgeLine.GetMidPoint();
                double qe_midX = qe.X, qe_midY = qe.Y;


                if (targetEdge.SlopKind == LineSlopeKind.Vertical)
                {
                    //TODO: review same side edge (Fan shape)
                    if (pe_midX < qe_midX)
                    {
                        targetEdge.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Vertical)
                        {
                            targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                    else
                    {
                        //matchingEdgeLine.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Vertical)
                        {
                            targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                }
                else if (targetEdge.SlopKind == LineSlopeKind.Horizontal)
                {
                    //TODO: review same side edge (Fan shape)

                    if (pe_midY > qe_midY)
                    {
                        //p side is upper , q side is lower
                        if (targetEdge.SlopKind == LineSlopeKind.Horizontal)
                        {
                            targetEdge.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                            {
                                targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                    else
                    {
                        if (matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                        {
                            // matchingEdgeLine.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                            {
                                targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                }
            }
        }




        static bool FindMatchingOuterSide(EdgeLine compareEdge,
            GlyphTriangle another,
            out EdgeLine result,
            out int edgeIndex)
        {
            //compare by radian of edge line
            double compareSlope = Math.Abs(compareEdge.SlopAngle);
            double diff0 = double.MaxValue;
            double diff1 = double.MaxValue;
            double diff2 = double.MaxValue;

            diff0 = Math.Abs(Math.Abs(another.e0.SlopAngle) - compareSlope);

            diff1 = Math.Abs(Math.Abs(another.e1.SlopAngle) - compareSlope);

            diff2 = Math.Abs(Math.Abs(another.e2.SlopAngle) - compareSlope);

            //find min
            int minDiffSide = FindMinIndex(diff0, diff1, diff2);
            if (minDiffSide > -1)
            {
                edgeIndex = minDiffSide;
                switch (minDiffSide)
                {
                    default: throw new NotSupportedException();
                    case 0:
                        result = another.e0;
                        break;
                    case 1:
                        result = another.e1;
                        break;
                    case 2:
                        result = another.e2;
                        break;
                }
                return true;
            }
            else
            {
                edgeIndex = -1;
                result = null;
                return false;
            }
        }
        static int FindMinIndex(double d0, double d1, double d2)
        {
            unsafe
            {
                double* tmpArr = stackalloc double[3];
                tmpArr[0] = d0;
                tmpArr[1] = d1;
                tmpArr[2] = d2;

                int minAt = -1;
                double currentMin = double.MaxValue;
                for (int i = 0; i < 3; ++i)
                {
                    double d = tmpArr[i];
                    if (d < currentMin)
                    {
                        currentMin = d;
                        minAt = i;
                    }
                }
                return minAt;
            }
        }
        /// <summary>
        /// count number of outside edge
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static int OutSideEdgeCount(GlyphTriangle t)
        {
            int n = 0;
            n += t.e0.IsOutside ? 1 : 0;
            n += t.e1.IsOutside ? 1 : 0;
            n += t.e2.IsOutside ? 1 : 0;
            return n;
        }
        static readonly double _85degreeToRad = MyMath.DegreesToRadians(85);
        static readonly double _15degreeToRad = MyMath.DegreesToRadians(15);
        static readonly double _03degreeToRad = MyMath.DegreesToRadians(3);
        static readonly double _90degreeToRad = MyMath.DegreesToRadians(90);
        public override string ToString()
        {
            return p + " -> " + q;
        }
    }
}
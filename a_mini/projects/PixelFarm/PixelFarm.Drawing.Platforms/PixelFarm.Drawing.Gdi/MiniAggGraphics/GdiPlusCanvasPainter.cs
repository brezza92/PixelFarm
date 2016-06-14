﻿//2016 MIT, WinterDev

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using PixelFarm.Agg;
using PixelFarm.Agg.Transform;
namespace PixelFarm.Drawing.WinGdi
{
    public class GdiPlusCanvasPainter : CanvasPainter
    {
        CanvasGraphics2dGdi _gfx;
        RectInt _clipBox;
        ColorRGBA _fillColor;
        int _width, _height;
        Agg.Fonts.Font _font;
        ColorRGBA _strokeColor;
        double _strokeWidth;
        bool _useSubPixelRendering;
        Graphics _internalGfx;
        public GdiPlusCanvasPainter(CanvasGraphics2dGdi gfx)
        {
            _gfx = gfx;
            _internalGfx = gfx.InternalGraphics;
            _width = 800;
            _height = 600;
        }
        public override RectInt ClipBox
        {
            get
            {
                return _clipBox;
            }
            set
            {
                _clipBox = value;
            }
        }

        public override Agg.Fonts.Font CurrentFont
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
            }
        }
        public override ColorRGBA FillColor
        {
            get
            {
                return _fillColor;
            }
            set
            {
                _fillColor = value;
            }
        }

        public override Graphics2D Graphics
        {
            get
            {
                return _gfx;
            }
        }

        public override int Height
        {
            get
            {
                return _height;
            }
        }

        public override ColorRGBA StrokeColor
        {
            get
            {
                return _strokeColor;
            }
            set
            {
                _strokeColor = value;
            }
        }
        public override double StrokeWidth
        {
            get
            {
                return _strokeWidth;
            }
            set
            {
                _strokeWidth = value;
            }
        }

        public override bool UseSubPixelRendering
        {
            get
            {
                return _useSubPixelRendering;
            }
            set
            {
                _useSubPixelRendering = value;
            }
        }

        public override int Width
        {
            get
            {
                return _width;
            }
        }

        public override void Clear(ColorRGBA color)
        {
            _gfx.Clear(color);
        }
        public override void DoFilterBlurRecursive(RectInt area, int r)
        {
            throw new NotImplementedException();
        }

        public override void DoFilterBlurStack(RectInt area, int r)
        {
            throw new NotImplementedException();
        }

        public override void Draw(VertexStore vxs)
        {
            VxsHelper.DrawVxsSnap(_internalGfx, new VertexStoreSnap(vxs), _fillColor);
        }

        public override void DrawBezierCurve(float startX, float startY, float endX, float endY, float controlX1, float controlY1, float controlX2, float controlY2)
        {
            throw new NotImplementedException();
        }

        public override void DrawEllipse()
        {
            throw new NotImplementedException();
        }

        public override void DrawImage(ActualImage actualImage, params AffinePlan[] affinePlans)
        {
            throw new NotImplementedException();
        }

        public override void DrawImage(ActualImage actualImage, double x, double y)
        {
            throw new NotImplementedException();
        }

        public override void DrawRoundRect(double left, double bottom, double right, double top, double radius)
        {
            throw new NotImplementedException();
        }

        public override void DrawString(string text, double x, double y)
        {
            throw new NotImplementedException();
        }

        public override void Fill(VertexStore vxs)
        {
            VxsHelper.DrawVxsSnap(_internalGfx, new VertexStoreSnap(vxs), _fillColor);
        }

        public override void Fill(VertexStoreSnap snap)
        {
            VxsHelper.DrawVxsSnap(_internalGfx, snap, _fillColor);
        }

        public override void Fill(VertexStore vxs, ISpanGenerator spanGen)
        {
            throw new NotImplementedException();
        }

        public override void FillCircle(double x, double y, double radius)
        {
            throw new NotImplementedException();
        }

        public override void FillCircle(double x, double y, double radius, ColorRGBA color)
        {
            throw new NotImplementedException();
        }

        public override void FillEllipse(double left, double bottom, double right, double top, int nsteps)
        {
            throw new NotImplementedException();
        }

        public override void FillRectangle(double left, double bottom, double right, double top)
        {
            throw new NotImplementedException();
        }

        public override void FillRectangle(double left, double bottom, double right, double top, ColorRGBA fillColor)
        {
            throw new NotImplementedException();
        }

        public override void FillRectLBWH(double left, double bottom, double width, double height)
        {
            throw new NotImplementedException();
        }

        public override void FillRoundRectangle(double left, double bottom, double right, double top, double radius)
        {
            throw new NotImplementedException();
        }

        public override VertexStore FlattenCurves(VertexStore srcVxs)
        {
            throw new NotImplementedException();
        }

        public override void Line(double x1, double y1, double x2, double y2)
        {
            throw new NotImplementedException();
        }

        public override void Line(double x1, double y1, double x2, double y2, ColorRGBA color)
        {
            throw new NotImplementedException();
        }

        public override void PaintSeries(VertexStore vxs, ColorRGBA[] colors, int[] pathIndexs, int numPath)
        {
            throw new NotImplementedException();
        }

        public override void Rectangle(double left, double bottom, double right, double top)
        {
            throw new NotImplementedException();
        }

        public override void Rectangle(double left, double bottom, double right, double top, ColorRGBA color)
        {
            throw new NotImplementedException();
        }

        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
        }
    }
}
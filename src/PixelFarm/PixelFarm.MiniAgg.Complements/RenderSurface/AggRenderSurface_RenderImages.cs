﻿//MIT, 2014-present, WinterDev

//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
using PixelFarm.Agg.Imaging;
using PixelFarm.Agg.Transform;
using PixelFarm.VectorMath;
using PixelFarm.Drawing;
namespace PixelFarm.Agg
{
    partial class AggRenderSurface
    {
        public bool UseSubPixelRendering
        {
            get { return this.sclineRasToBmp.ScanlineRenderMode == ScanlineRenderMode.SubPixelLcdEffect; }
            set { this.sclineRasToBmp.ScanlineRenderMode = value ? ScanlineRenderMode.SubPixelLcdEffect : ScanlineRenderMode.Default; }
        }
        static Affine BuildImageBoundsPath(
            int srcW, int srcH,
            double destX, double destY,
            double hotspotOffsetX, double hotSpotOffsetY,
            double scaleX, double scaleY,
            double angleRad,
            VertexStore outputDestImgRect)
        {

            AffinePlan[] plans = new AffinePlan[4];
            int i = 0;
            if (hotspotOffsetX != 0.0f || hotSpotOffsetY != 0.0f)
            {
                plans[i] = AffinePlan.Translate(-hotspotOffsetX, -hotSpotOffsetY);
                i++;
            }

            if (scaleX != 1 || scaleY != 1)
            {
                plans[i] = AffinePlan.Scale(scaleX, scaleY);
                i++;
            }

            if (angleRad != 0)
            {
                plans[i] = AffinePlan.Rotate(angleRad);
                i++;
            }

            if (destX != 0 || destY != 0)
            {
                plans[i] = AffinePlan.Translate(destX, destY);
                i++;
            }

            outputDestImgRect.Clear();
            outputDestImgRect.AddMoveTo(0, 0);
            outputDestImgRect.AddLineTo(srcW, 0);
            outputDestImgRect.AddLineTo(srcW, srcH);
            outputDestImgRect.AddLineTo(0, srcH);
            outputDestImgRect.AddCloseFigure();
            return Affine.NewMatix(plans);
        }
        static Affine BuildImageBoundsPath(
            int srcW, int srcH,
            double destX, double destY, VertexStore outputDestImgRect)
        {
            AffinePlan plan = new AffinePlan();
            if (destX != 0 || destY != 0)
            {
                plan = AffinePlan.Translate(destX, destY);
            }

            outputDestImgRect.Clear();
            outputDestImgRect.AddMoveTo(0, 0);
            outputDestImgRect.AddLineTo(srcW, 0);
            outputDestImgRect.AddLineTo(srcW, srcH);
            outputDestImgRect.AddLineTo(0, srcH);
            outputDestImgRect.AddCloseFigure();
            return Affine.NewMatix(plan);
        }
        static Affine BuildImageBoundsPath(int srcW, int srcH,
           AffinePlan[] affPlans,
           VertexStore outputDestImgRect)
        {
            outputDestImgRect.Clear();
            outputDestImgRect.AddMoveTo(0, 0);
            outputDestImgRect.AddLineTo(srcW, 0);
            outputDestImgRect.AddLineTo(srcW, srcH);
            outputDestImgRect.AddLineTo(0, srcH);
            outputDestImgRect.AddCloseFigure();
            return Affine.NewMatix(affPlans);
        }
        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
        void Render(VertexStore vxs, ISpanGenerator spanGen)
        {
            sclineRas.AddPath(vxs);
            sclineRasToBmp.RenderWithSpan(
                destImageReaderWriter,
                sclineRas,
                sclinePack8,
                spanGen);
        }

        public void Render(IBitmapSrc source,
            double destX, double destY,
            double angleRadians,
            double inScaleX, double inScaleY)
        {
            {   // exit early if the dest and source bounds don't touch.
                // TODO: <BUG> make this do rotation and scalling
                RectInt sourceBounds = source.GetBounds();
                RectInt destBounds = this.destImageReaderWriter.GetBounds();
                sourceBounds.Offset((int)destX, (int)destY);
                if (!RectInt.DoIntersect(sourceBounds, destBounds))
                {
                    if (inScaleX != 1 || inScaleY != 1 || angleRadians != 0)
                    {
                        throw new NotImplementedException();
                    }
                    return;
                }
            }

            double scaleX = inScaleX;
            double scaleY = inScaleY;
            Affine graphicsTransform = this.CurrentTransformMatrix;
            if (!graphicsTransform.IsIdentity())
            {
                if (scaleX != 1 || scaleY != 1 || angleRadians != 0)
                {
                    throw new NotImplementedException();
                }
                graphicsTransform.Transform(ref destX, ref destY);
            }

#if false // this is an optomization that eliminates the drawing of images that have their alpha set to all 0 (happens with generated images like explosions).
	        MaxAlphaFrameProperty maxAlphaFrameProperty = MaxAlphaFrameProperty::GetMaxAlphaFrameProperty(source);

	        if((maxAlphaFrameProperty.GetMaxAlpha() * color.A_Byte) / 256 <= ALPHA_CHANNEL_BITS_DIVISOR)
	        {
		        m_OutFinalBlitBounds.SetRect(0,0,0,0);
	        }
#endif
            bool isScale = (scaleX != 1 || scaleY != 1);
            bool isRotated = true;
            if (Math.Abs(angleRadians) < (0.1 * MathHelper.Tau / 360))
            {
                isRotated = false;
                angleRadians = 0;
            }

            //bool IsMipped = false;
            //double ox, oy;
            //source.GetOriginOffset(out ox, out oy);

            bool canUseMipMaps = isScale;
            if (scaleX > 0.5 || scaleY > 0.5)
            {
                canUseMipMaps = false;
            }

            bool renderRequriesSourceSampling = isScale || isRotated || destX != (int)destX || destY != (int)destY;

            VectorToolBox.GetFreeVxs(out VertexStore imgBoundsPath);

            // this is the fast drawing path
            if (renderRequriesSourceSampling)
            {
                // if the scalling is small enough the results can be improved by using mip maps
                //if(CanUseMipMaps)
                //{
                //    CMipMapFrameProperty* pMipMapFrameProperty = CMipMapFrameProperty::GetMipMapFrameProperty(source);
                //    double OldScaleX = scaleX;
                //    double OldScaleY = scaleY;
                //    const CFrameInterface* pMippedFrame = pMipMapFrameProperty.GetMipMapFrame(ref scaleX, ref scaleY);
                //    if(pMippedFrame != source)
                //    {
                //        IsMipped = true;
                //        source = pMippedFrame;
                //        sourceOriginOffsetX *= (OldScaleX / scaleX);
                //        sourceOriginOffsetY *= (OldScaleY / scaleY);
                //    }

                //    HotspotOffsetX *= (inScaleX / scaleX);
                //    HotspotOffsetY *= (inScaleY / scaleY);
                //}

                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                    destX, destY, ox, oy, scaleX, scaleY, angleRadians, imgBoundsPath);

                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();

                var interpolator = new SpanInterpolatorLinear();
                interpolator.Transformer = sourceRectTransform;
                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(source, Drawing.Color.Black, interpolator);

                VectorToolBox.GetFreeVxs(out var v1);
                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);
                // this is some debug you can enable to visualize the dest bounding box
                //LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
                //LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);

            }
            else // TODO: this can be even faster if we do not use an intermediat buffer
            {
                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, destX, destY, imgBoundsPath);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var interpolator = new SpanInterpolatorLinear();
                interpolator.Transformer = sourceRectTransform;
                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    //case 24:
                    //    imgSpanGen = new ImgSpanGenRGB_NNStepXby1(source, interpolator);
                    //    break;
                    case 8:
                        imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                        break;
                    default:
                        throw new NotImplementedException();
                }


                VectorToolBox.GetFreeVxs(out var v1);

                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);
                unchecked { destImageChanged++; };
            }
            VectorToolBox.ReleaseVxs(ref imgBoundsPath);
        }

        int destImageChanged = 0;
        public void Render(IBitmapSrc source, AffinePlan[] affinePlans)
        {

            VectorToolBox.GetFreeVxs(out var v1, out var v2);

            Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height, affinePlans, v1);
            // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
            Affine sourceRectTransform = destRectTransform.CreateInvert();

            var spanInterpolator = new SpanInterpolatorLinear();
            spanInterpolator.Transformer = sourceRectTransform;

            var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                source,
                Drawing.Color.Transparent,
                spanInterpolator);

            destRectTransform.TransformToVxs(v1, v2);
            Render(v2, imgSpanGen);
            //

            VectorToolBox.ReleaseVxs(ref v1, ref v2);

        }
        public void Render(IBitmapSrc source, double destX, double destY)
        {
            int inScaleX = 1;
            int inScaleY = 1;
            int angleRadians = 0;
            // exit early if the dest and source bounds don't touch.
            // TODO: <BUG> make this do rotation and scalling
            RectInt sourceBounds = source.GetBounds();
            sourceBounds.Offset((int)destX, (int)destY);

            RectInt destBounds = this.destImageReaderWriter.GetBounds();
            if (!RectInt.DoIntersect(sourceBounds, destBounds))
            {
                //if (inScaleX != 1 || inScaleY != 1 || angleRadians != 0)
                //{
                //    throw new NotImplementedException();
                //}
                return;
            }

            double scaleX = inScaleX;
            double scaleY = inScaleY;
            Affine graphicsTransform = this.CurrentTransformMatrix;
            if (!graphicsTransform.IsIdentity())
            {
                if (scaleX != 1 || scaleY != 1 || angleRadians != 0)
                {
                    throw new NotImplementedException();
                }
                graphicsTransform.Transform(ref destX, ref destY);
            }


#if false // this is an optomization that eliminates the drawing of images that have their alpha set to all 0 (happens with generated images like explosions).
	        MaxAlphaFrameProperty maxAlphaFrameProperty = MaxAlphaFrameProperty::GetMaxAlphaFrameProperty(source);

	        if((maxAlphaFrameProperty.GetMaxAlpha() * color.A_Byte) / 256 <= ALPHA_CHANNEL_BITS_DIVISOR)
	        {
		        m_OutFinalBlitBounds.SetRect(0,0,0,0);
	        }
#endif
            bool isScale = (scaleX != 1 || scaleY != 1);
            bool isRotated = false;
            if (angleRadians != 0 && Math.Abs(angleRadians) >= (0.1 * MathHelper.Tau / 360))
            {
                isRotated = true;
            }
            else
            {
                angleRadians = 0;//reset very small angle to 0

            }


            //bool IsMipped = false;
            //double ox, oy;
            //source.GetOriginOffset(out ox, out oy);

            bool canUseMipMaps = isScale;
            if (scaleX > 0.5 || scaleY > 0.5)
            {
                canUseMipMaps = false;
            }

            bool needSourceResampling = isScale || isRotated || destX != (int)destX || destY != (int)destY;



            VectorToolBox.GetFreeVxs(out VertexStore imgBoundsPath);
            // this is the fast drawing path
            if (needSourceResampling)
            {
#if false // if the scalling is small enough the results can be improved by using mip maps
	        if(CanUseMipMaps)
	        {
		        CMipMapFrameProperty* pMipMapFrameProperty = CMipMapFrameProperty::GetMipMapFrameProperty(source);
		        double OldScaleX = scaleX;
		        double OldScaleY = scaleY;
		        const CFrameInterface* pMippedFrame = pMipMapFrameProperty.GetMipMapFrame(ref scaleX, ref scaleY);
		        if(pMippedFrame != source)
		        {
			        IsMipped = true;
			        source = pMippedFrame;
			        sourceOriginOffsetX *= (OldScaleX / scaleX);
			        sourceOriginOffsetY *= (OldScaleY / scaleY);
		        }

			    HotspotOffsetX *= (inScaleX / scaleX);
			    HotspotOffsetY *= (inScaleY / scaleY);
	        }
#endif


                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                    destX, destY, ox, oy, scaleX, scaleY, angleRadians, imgBoundsPath);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();

                var spanInterpolator = new SpanInterpolatorLinear();
                spanInterpolator.Transformer = sourceRectTransform;
                var imgSpanGen = new ImgSpanGenRGBA_BilinearClip(
                    source,
                    Drawing.Color.Black,
                    spanInterpolator);

                VectorToolBox.GetFreeVxs(out VertexStore v1);
                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);
                VectorToolBox.ReleaseVxs(ref v1);
#if false // this is some debug you can enable to visualize the dest bounding box
		        LineFloat(BoundingRect.left, BoundingRect.top, BoundingRect.right, BoundingRect.top, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.top, BoundingRect.right, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.right, BoundingRect.bottom, BoundingRect.left, BoundingRect.bottom, WHITE);
		        LineFloat(BoundingRect.left, BoundingRect.bottom, BoundingRect.left, BoundingRect.top, WHITE);
#endif
            }
            else // TODO: this can be even faster if we do not use an intermediat buffer
            {
                Affine destRectTransform = BuildImageBoundsPath(source.Width, source.Height,
                    destX, destY, imgBoundsPath);
                // We invert it because it is the transform to make the image go to the same position as the polygon. LBB [2/24/2004]
                Affine sourceRectTransform = destRectTransform.CreateInvert();
                var interpolator = new SpanInterpolatorLinear();
                interpolator.Transformer = sourceRectTransform;
                ImgSpanGen imgSpanGen = null;
                switch (source.BitDepth)
                {
                    case 32:
                        imgSpanGen = new ImgSpanGenRGBA_NN_StepXBy1(source, interpolator);
                        break;
                    //case 24:
                    //    imgSpanGen = new ImgSpanGenRGB_NNStepXby1(source, interpolator);
                    //    break;
                    case 8:
                        imgSpanGen = new ImgSpanGenGray_NNStepXby1(source, interpolator);
                        break;
                    default:
                        throw new NotImplementedException();
                }


                VectorToolBox.GetFreeVxs(out VertexStore v1);

                destRectTransform.TransformToVxs(imgBoundsPath, v1);
                Render(v1, imgSpanGen);

                VectorToolBox.ReleaseVxs(ref v1);
                unchecked { destImageChanged++; };
            }
            VectorToolBox.ReleaseVxs(ref imgBoundsPath);
        }
    }
}
﻿//BSD, 2014-2018, WinterDev
//MatterHackers

#define USE_CLIPPING_ALPHA_MASK

using System;
using PixelFarm.Agg.Transform;
using PixelFarm.Agg.Imaging;
using Mini;
using PixelFarm.Drawing.WinGdi;
using PixelFarm.Drawing;

namespace PixelFarm.Agg.Sample_LionAlphaMask
{
    [Info(OrderCode = "05")]
    [Info(DemoCategory.Bitmap, "Clipping to multiple rectangle regions")]
    public class LionAlphaMask2 : DemoBase
    {
        int maskAlphaSliderValue = 100;
        ActualBitmap alphaBitmap;
        SpriteShape lionShape;
        double angle = 0;
        double lionScale = 1.0;
        double skewX = 0;
        double skewY = 0;
        bool isMaskSliderValueChanged = true;
        SubBitmapBlender alphaMaskImageBuffer;
        //IAlphaMask alphaMask;
        System.Drawing.Bitmap a_alphaBmp;
        ActualBitmap lionImg;
        public LionAlphaMask2()
        {

            string imgFileName = "Data/lion1.png";
            if (System.IO.File.Exists(imgFileName))
            {
                lionImg = DemoHelper.LoadImage(imgFileName);
            }


            lionShape = new SpriteShape(SvgRenderVxLoader.CreateSvgRenderVxFromFile("Samples/arrow2.svg"));
            this.Width = 800;
            this.Height = 600;
            //AnchorAll();
            //alphaMaskImageBuffer = new ReferenceImage();

#if USE_CLIPPING_ALPHA_MASK
            //alphaMask = new AlphaMaskByteClipped(alphaMaskImageBuffer, 1, 0);
#else
            //alphaMask = new AlphaMaskByteUnclipped(alphaMaskImageBuffer, 1, 0);
#endif

            //numMasksSlider = new UI.Slider(5, 5, 150, 12);
            //sliderValue = 0.0;
            //AddChild(numMasksSlider);
            //numMasksSlider.SetRange(5, 100);
            //numMasksSlider.Value = 10;
            //numMasksSlider.Text = "N={0:F3}";
            //numMasksSlider.OriginRelativeParent = Vector2.Zero;

        }

        public override void Init()
        {
        }
        void GenerateMaskWithWinGdiPlus(int w, int h)
        {

        }
        void GenAlphaMask(ScanlineRasToDestBitmapRenderer sclineRasToBmp, ScanlinePacked8 sclnPack, ScanlineRasterizer rasterizer, int width, int height)
        {

            alphaBitmap = new ActualBitmap(width, height);
            alphaMaskImageBuffer = new SubBitmapBlender(alphaBitmap, new PixelBlenderGrey());
            //
            ClipProxyImage clippingProxy = new ClipProxyImage(alphaMaskImageBuffer);
            clippingProxy.Clear(Drawing.Color.Black);

            System.Random randGenerator = new Random(1432);
            int i;
            int num = (int)maskAlphaSliderValue;
            num = 50;

            int elliseFlattenStep = 64;
            VectorToolBox.GetFreeVxs(out var v1);
            VertexSource.Ellipse ellipseForMask = new PixelFarm.Agg.VertexSource.Ellipse();

            for (i = 0; i < num; i++)
            {

                if (i == num - 1)
                {
                    ////for the last one

                    ellipseForMask.Reset(Width / 2, (Height / 2) - 90, 110, 110, elliseFlattenStep);
                    rasterizer.Reset();
                    rasterizer.AddPath(ellipseForMask.MakeVertexSnap(v1));
                    v1.Clear();
                    sclineRasToBmp.RenderWithColor(clippingProxy, rasterizer, sclnPack, new Color(255, 255, 255, 0));

                    ellipseForMask.Reset(ellipseForMask.originX, ellipseForMask.originY, ellipseForMask.radiusX - 10, ellipseForMask.radiusY - 10, elliseFlattenStep);
                    rasterizer.Reset();
                    rasterizer.AddPath(ellipseForMask.MakeVertexSnap(v1));
                    v1.Clear();
                    sclineRasToBmp.RenderWithColor(clippingProxy, rasterizer, sclnPack, new Color(255, 255, 0, 0));
                }
                else
                {
                    ellipseForMask.Reset(randGenerator.Next() % width,
                             randGenerator.Next() % height,
                             randGenerator.Next() % 100 + 20,
                             randGenerator.Next() % 100 + 20,
                             elliseFlattenStep);
                    // ellipseForMask.Reset(Width / 2, Height / 2, 150, 150, 100);

                    // set the color to draw into the alpha channel.
                    // there is not very much reason to set the alpha as you will get the amount of 
                    // transparency based on the color you draw.  (you might want some type of different edeg effect but it will be minor).

                    rasterizer.Reset();
                    rasterizer.AddPath(ellipseForMask.MakeVxs(v1));
                    v1.Clear();
                    sclineRasToBmp.RenderWithColor(clippingProxy, rasterizer, sclnPack, new Color(255, 255, 0, 0));
                    //ColorEx.Make((int)((float)i / (float)num * 255), 0, 0, 255));
                }
            }
            VectorToolBox.ReleaseVxs(ref v1);
        }


        [DemoConfig(MinValue = 0, MaxValue = 255)]
        public int MaskAlphaSliderValue
        {
            get
            {
                return maskAlphaSliderValue;
            }
            set
            {
                this.maskAlphaSliderValue = value;
                isMaskSliderValueChanged = true;
            }
        }
        static System.Drawing.Bitmap CreateBackgroundBmp(int w, int h)
        {
            //----------------------------------------------------
            //1. create background bitmap
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h);
            //2. create graphics from bmp
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            // draw a background to show how the mask is working better
            g.Clear(System.Drawing.Color.White);
            int rect_w = 30;

            var v1 = new VertexStore();//todo; use pool
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    if ((i + j) % 2 != 0)
                    {
                        VertexSource.RoundedRect rect = new VertexSource.RoundedRect(i * rect_w, j * rect_w, (i + 1) * rect_w, (j + 1) * rect_w, 0);
                        rect.NormalizeRadius();
                        // Drawing as an outline
                        VxsHelper.FillVxsSnap(g, new VertexStoreSnap(rect.MakeVxs(v1)), ColorEx.Make(.9f, .9f, .9f));
                        v1.Clear();
                    }
                }
            }

            //----------------------------------------------------
            return bmp;
        }
        void DrawWithWinGdi(GdiPlusPainter p)
        {
            int w = 800, h = 600;
            p.Clear(Drawing.Color.White);
            p.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            if (isMaskSliderValueChanged)
            {
                GenerateMaskWithWinGdiPlus(w, h);
            }

            using (System.Drawing.Bitmap background = CreateBackgroundBmp(w, h))
            {
                p.DrawImage(background, 0, 0);
            }

            //draw lion on background
            Affine transform = Affine.NewMatix(
              AffinePlan.Translate(-lionShape.Center.x, -lionShape.Center.y),
              AffinePlan.Scale(lionScale, lionScale),
              AffinePlan.Rotate(angle + Math.PI),
              AffinePlan.Skew(skewX / 1000.0, skewY / 1000.0),
              AffinePlan.Translate(w / 2, h / 2));
            using (System.Drawing.Bitmap lionBmp = new System.Drawing.Bitmap(w, h))
            using (System.Drawing.Graphics lionGfx = System.Drawing.Graphics.FromImage(lionBmp))
            {
                //lionGfx.Clear(System.Drawing.Color.White);
                //int n = lionShape.NumPaths;
                //int[] indexList = lionShape.PathIndexList;


                //TODO: review here again
                throw new NotSupportedException();

                //Color[] colors = lionShape.Colors;
                ////var lionVxs = lionShape.Path.Vxs;// transform.TransformToVxs(lionShape.Path.Vxs);

                //var lionVxs = new VertexStore();
                //transform.TransformToVxs(lionShape.Vxs, lionVxs);
                //for (int i = 0; i < n; ++i)
                //{
                //    VxsHelper.FillVxsSnap(lionGfx,
                //        new VertexStoreSnap(lionVxs, indexList[i]),
                //        colors[i]);
                //}
                //using (var mergeBmp = MergeAlphaChannel(lionBmp, a_alphaBmp))
                //{
                //    //gx.InternalGraphics.DrawImage(this.a_alphaBmp, new System.Drawing.PointF(0, 0));
                //    //gx.InternalGraphics.DrawImage(bmp, new System.Drawing.PointF(0, 0));                      
                //    p.DrawImage(mergeBmp, 0, 0);
                //}
            }
        }
        static System.Drawing.Bitmap MergeAlphaChannel(System.Drawing.Bitmap original, System.Drawing.Bitmap alphaChannelBmp)
        {
            int w = original.Width;
            int h = original.Height;
            System.Drawing.Bitmap resultBmp = new System.Drawing.Bitmap(original);
            System.Drawing.Imaging.BitmapData resultBmpData = resultBmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            System.Drawing.Imaging.BitmapData alphaBmpData = alphaChannelBmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, alphaChannelBmp.PixelFormat);
            IntPtr resultScan0 = resultBmpData.Scan0;
            IntPtr alphaScan0 = alphaBmpData.Scan0;
            int resultStride = resultBmpData.Stride;
            int lineByteCount = resultStride;
            int totalByteCount = lineByteCount * h;
            unsafe
            {
                byte* dest = (byte*)resultScan0;
                byte* src = (byte*)alphaScan0;
                for (int i = 0; i < totalByteCount;)
                {
                    // *(dest + 3) = 150;
                    //replace  alpha channel with data from alphaBmpData
                    byte oldAlpha = *(dest + 3);
                    byte src_B = *(src); //b
                    byte src_G = *(src + 1); //g
                    byte src_R = *(src + 2); //r
                                             //convert rgb to gray scale: from  this equation...
                    int y = (src_R * 77) + (src_G * 151) + (src_B * 28);
                    *(dest + 3) = (byte)(y >> 8);
                    //*(dest + 3) = (byte)((new_B + new_G + new_R) / 3);

                    dest += 4;
                    src += 4;
                    i += 4;
                }
            }
            alphaChannelBmp.UnlockBits(alphaBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }

        PixelBlenderWithMask maskPixelBlender = new PixelBlenderWithMask();
        public override void Draw(Painter p)
        {
            if (p is GdiPlusPainter)
            {
                //DrawWithWinGdi((GdiPlusPainter)p);
                return;
            }
            AggPainter p2 = (AggPainter)p;
            p2.Clear(Color.White);
            AggRenderSurface aggsx = p2.RenderSurface;
            BitmapBlenderBase widgetsSubImage = (BitmapBlenderBase)aggsx.DestImage;
            ScanlinePacked8 scline = aggsx.ScanlinePacked8;
            int width = (int)widgetsSubImage.Width;
            int height = (int)widgetsSubImage.Height;
            //change value ***
            if (isMaskSliderValueChanged)
            {
                GenAlphaMask(aggsx.ScanlineRasToDestBitmap, aggsx.ScanlinePacked8, aggsx.ScanlineRasterizer, width, height);
                this.isMaskSliderValueChanged = false;
                maskPixelBlender.SetMaskImage(alphaBitmap);

            }

            //1. alpha mask...
            //p2.DrawImage(alphaBitmap, 0, 0); 
            PixelBlender32 blender = widgetsSubImage.OutputPixelBlender; //save
            widgetsSubImage.OutputPixelBlender = maskPixelBlender;
            //
            //2. 
            p2.FillColor = Color.Blue;
            p2.FillCircle(300, 300, 100);

            p2.DrawImage(lionImg, 20, 20);

            widgetsSubImage.OutputPixelBlender = blender; //restore


            //var rasterizer = aggsx.ScanlineRasterizer;
            //rasterizer.SetClipBox(0, 0, width, height);
            ////alphaMaskImageBuffer.AttachBuffer(alphaByteArray, 0, width, height, width, 8, 1);

            //PixelFarm.Agg.Imaging.AlphaMaskAdaptor imageAlphaMaskAdaptor = new PixelFarm.Agg.Imaging.AlphaMaskAdaptor(widgetsSubImage, alphaMask);
            //ClipProxyImage alphaMaskClippingProxy = new ClipProxyImage(imageAlphaMaskAdaptor);
            //ClipProxyImage clippingProxy = new ClipProxyImage(widgetsSubImage);
            //////Affine transform = Affine.NewIdentity();
            //////transform *= Affine.NewTranslation(-lionShape.Center.x, -lionShape.Center.y);
            //////transform *= Affine.NewScaling(lionScale, lionScale);
            //////transform *= Affine.NewRotation(angle + Math.PI);
            //////transform *= Affine.NewSkewing(skewX / 1000.0, skewY / 1000.0);
            //////transform *= Affine.NewTranslation(Width / 2, Height / 2);
            //Affine transform = Affine.NewMatix(
            //        AffinePlan.Translate(-lionShape.Center.x, -lionShape.Center.y),
            //        AffinePlan.Scale(lionScale, lionScale),
            //        AffinePlan.Rotate(angle + Math.PI),
            //        AffinePlan.Skew(skewX / 1000.0, skewY / 1000.0),
            //        AffinePlan.Translate(width / 2, height / 2));
            //clippingProxy.Clear(Drawing.Color.White);
            //ScanlineRasToDestBitmapRenderer sclineRasToBmp = aggsx.ScanlineRasToDestBitmap;
            //// draw a background to show how the mask is working better
            //int rect_w = 30;

            //VectorToolBox.GetFreeVxs(out var v1);
            //for (int i = 0; i < 40; i++)
            //{
            //    for (int j = 0; j < 40; j++)
            //    {
            //        if ((i + j) % 2 != 0)
            //        {
            //            VertexSource.RoundedRect rect = new VertexSource.RoundedRect(i * rect_w, j * rect_w, (i + 1) * rect_w, (j + 1) * rect_w, 0);
            //            rect.NormalizeRadius(); 
            //            rasterizer.AddPath(rect.MakeVxs(v1));
            //            v1.Clear();
            //            sclineRasToBmp.RenderWithColor(clippingProxy, rasterizer, scline, ColorEx.Make(.9f, .9f, .9f));
            //        }
            //    }
            //}
            //VectorToolBox.ReleaseVxs(ref v1);

            ////int x, y; 
            //// Render the lion
            ////VertexSourceApplyTransform trans = new VertexSourceApplyTransform(lionShape.Path, transform);

            ////var vxlist = new System.Collections.Generic.List<VertexData>();
            ////trans.DoTransform(vxlist); 

            //var tmpVxs1 = new VertexStore();
            //lionShape.ApplyTransform(transform);


            //for (int i = 0; i < num_paths; ++i)
            //{
            //    rasterizer.Reset();
            //    rasterizer.AddPath(new VertexStoreSnap(vxs, path_id[i]));
            //    sclineRasToBmp.RenderWithColor(destImage, sclineRas, scline, colors[i]);
            //}

            //sclineRasToBmp.RenderSolidAllPaths(alphaMaskClippingProxy,
            //       rasterizer,
            //       scline,
            //       tmpVxs1,
            //       lionShape.Colors,
            //       lionShape.PathIndexList,
            //       lionShape.NumPaths);

            ///*
            //// Render random Bresenham lines and markers
            //agg::renderer_markers<amask_ren_type> m(r);
            //for(i = 0; i < 50; i++)
            //{
            //    m.line_color(agg::rgba8(randGenerator.Next() & 0x7F, 
            //                            randGenerator.Next() & 0x7F, 
            //                            randGenerator.Next() & 0x7F, 
            //                            (randGenerator.Next() & 0x7F) + 0x7F)); 
            //    m.fill_color(agg::rgba8(randGenerator.Next() & 0x7F, 
            //                            randGenerator.Next() & 0x7F, 
            //                            randGenerator.Next() & 0x7F, 
            //                            (randGenerator.Next() & 0x7F) + 0x7F));

            //    m.line(m.coord(randGenerator.Next() % width), m.coord(randGenerator.Next() % height), 
            //           m.coord(randGenerator.Next() % width), m.coord(randGenerator.Next() % height));

            //    m.marker(randGenerator.Next() % width, randGenerator.Next() % height, randGenerator.Next() % 10 + 5,
            //             agg::marker_e(randGenerator.Next() % agg::end_of_markers));
            //}


            //// Render random anti-aliased lines
            //double w = 5.0;
            //agg::line_profile_aa profile;
            //profile.width(w);

            //typedef agg::renderer_outline_aa<amask_ren_type> renderer_type;
            //renderer_type ren(r, profile);

            //typedef agg::rasterizer_outline_aa<renderer_type> rasterizer_type;
            //rasterizer_type ras(ren);
            //ras.round_cap(true);

            //for(i = 0; i < 50; i++)
            //{
            //    ren.Color = agg::rgba8(randGenerator.Next() & 0x7F, 
            //                         randGenerator.Next() & 0x7F, 
            //                         randGenerator.Next() & 0x7F, 
            //                         //255));
            //                         (randGenerator.Next() & 0x7F) + 0x7F); 
            //    ras.move_to_d(randGenerator.Next() % width, randGenerator.Next() % height);
            //    ras.line_to_d(randGenerator.Next() % width, randGenerator.Next() % height);
            //    ras.render(false);
            //}


            //// Render random circles with gradient
            //typedef agg::gradient_linear_color<color_type> grad_color;
            //typedef agg::gradient_circle grad_func;
            //typedef agg::span_interpolator_linear<> interpolator_type;
            //typedef agg::span_gradient<color_type, 
            //                          interpolator_type, 
            //                          grad_func, 
            //                          grad_color> span_grad_type;

            //agg::trans_affine grm;
            //grad_func grf;
            //grad_color grc(agg::rgba8(0,0,0), agg::rgba8(0,0,0));
            //agg::ellipse ell;
            //agg::span_allocator<color_type> sa;
            //interpolator_type inter(grm);
            //span_grad_type sg(inter, grf, grc, 0, 10);
            //agg::renderer_scanline_aa<amask_ren_type, 
            //                          agg::span_allocator<color_type>,
            //                          span_grad_type> rg(r, sa, sg);
            //for(i = 0; i < 50; i++)
            //{
            //    x = randGenerator.Next() % width;
            //    y = randGenerator.Next() % height;
            //    double r = randGenerator.Next() % 10 + 5;
            //    grm.reset();
            //    grm *= agg::trans_affine_scaling(r / 10.0);
            //    grm *= agg::trans_affine_translation(x, y);
            //    grm.invert();
            //    grc.colors(agg::rgba8(255, 255, 255, 0),
            //               agg::rgba8(randGenerator.Next() & 0x7F, 
            //                          randGenerator.Next() & 0x7F, 
            //                          randGenerator.Next() & 0x7F, 
            //                          255));
            //    sg.color_function(grc);
            //    ell.init(x, y, r, r, 32);
            //    g_rasterizer.add_path(ell);
            //    agg::render_scanlines(g_rasterizer, g_scanline, rg);
            //}
            // */
            ////m_num_cb.Render(g_rasterizer, g_scanline, clippingProxy); 
        }
        public override void MouseDown(int x, int y, bool isRightButton)
        {
            doTransform(this.Width, this.Height, x, y);
        }
        public override void MouseDrag(int x, int y)
        {
            doTransform(this.Width, this.Height, x, y);
            base.MouseDrag(x, y);
        }

        void doTransform(double width, double height, double x, double y)
        {
            x -= width / 2;
            y -= height / 2;
            angle = Math.Atan2(y, x);
            lionScale = Math.Sqrt(y * y + x * x) / 100.0;
        }
    }


    [Info(OrderCode = "05")]
    [Info(DemoCategory.Bitmap, "Clipping to multiple rectangle regions")]
    public class LionAlphaMask3 : DemoBase
    {
        int maskAlphaSliderValue = 100;
        ActualBitmap alphaBitmap;
        SpriteShape lionShape;
        double angle = 0;
        double lionScale = 1.0;
        double skewX = 0;
        double skewY = 0;
        bool isMaskSliderValueChanged = true;

        ActualBitmap lionImg;
        AggPainter alphaPainter;

        public LionAlphaMask3()
        {

            string imgFileName = "Data/lion1.png";
            if (System.IO.File.Exists(imgFileName))
            {
                lionImg = DemoHelper.LoadImage(imgFileName);
            }


            lionShape = new SpriteShape(SvgRenderVxLoader.CreateSvgRenderVxFromFile("Samples/arrow2.svg"));
            this.Width = 800;
            this.Height = 600;
            //AnchorAll();
            //alphaMaskImageBuffer = new ReferenceImage();

#if USE_CLIPPING_ALPHA_MASK
            //alphaMask = new AlphaMaskByteClipped(alphaMaskImageBuffer, 1, 0);
#else
            //alphaMask = new AlphaMaskByteUnclipped(alphaMaskImageBuffer, 1, 0);
#endif

            //numMasksSlider = new UI.Slider(5, 5, 150, 12);
            //sliderValue = 0.0;
            //AddChild(numMasksSlider);
            //numMasksSlider.SetRange(5, 100);
            //numMasksSlider.Value = 10;
            //numMasksSlider.Text = "N={0:F3}";
            //numMasksSlider.OriginRelativeParent = Vector2.Zero;

        }

        public override void Init()
        {
        }

        void SetupMaskPixelBlender(int width, int height)
        {
            //----------
            //same size
            alphaBitmap = new ActualBitmap(width, height);
            alphaPainter = AggPainter.Create(alphaBitmap);


            AggRenderSurface alphaRenderSx = new AggRenderSurface(alphaBitmap);
            alphaRenderSx.PixelBlender = new PixelBlenderGrey();
            alphaPainter = new AggPainter(alphaRenderSx);
            alphaPainter.Clear(Color.Black);
            //------------ 

            System.Random randGenerator = new Random(1432);
            int i;
            int num = (int)maskAlphaSliderValue;
            num = 50;

            int elliseFlattenStep = 64;
            VectorToolBox.GetFreeVxs(out var v1);
            VertexSource.Ellipse ellipseForMask = new PixelFarm.Agg.VertexSource.Ellipse();

            for (i = 0; i < num; i++)
            {

                if (i == num - 1)
                {
                    ////for the last one 
                    ellipseForMask.Reset(Width / 2, (Height / 2) - 90, 110, 110, elliseFlattenStep);
                    ellipseForMask.MakeVertexSnap(v1);
                    alphaPainter.FillColor = new Color(255, 255, 255, 0);
                    alphaPainter.Fill(v1);
                    v1.Clear();
                    //
                    ellipseForMask.Reset(ellipseForMask.originX, ellipseForMask.originY, ellipseForMask.radiusX - 10, ellipseForMask.radiusY - 10, elliseFlattenStep);
                    ellipseForMask.MakeVertexSnap(v1);
                    alphaPainter.FillColor = new Color(255, 255, 0, 0);
                    alphaPainter.Fill(v1);
                    v1.Clear();
                    //
                }
                else
                {
                    ellipseForMask.Reset(randGenerator.Next() % width,
                             randGenerator.Next() % height,
                             randGenerator.Next() % 100 + 20,
                             randGenerator.Next() % 100 + 20,
                             elliseFlattenStep);
                    ellipseForMask.MakeVertexSnap(v1);
                    alphaPainter.FillColor = new Color(255, 255, 0, 0);
                    alphaPainter.Fill(v1);
                    v1.Clear();
                }
            }
            VectorToolBox.ReleaseVxs(ref v1);


            maskPixelBlender.SetMaskImage(alphaBitmap);
        }


        [DemoConfig(MinValue = 0, MaxValue = 255)]
        public int MaskAlphaSliderValue
        {
            get
            {
                return maskAlphaSliderValue;
            }
            set
            {
                this.maskAlphaSliderValue = value;
                isMaskSliderValueChanged = true;
            }
        }

        PixelBlenderWithMask maskPixelBlender = new PixelBlenderWithMask();
        public override void Draw(Painter p)
        {
            if (p is GdiPlusPainter)
            {
                return;
            }

            //
            AggPainter painter = (AggPainter)p;
            painter.Clear(Color.White);

            int width = painter.Width;
            int height = painter.Height;
            //change value ***
            if (isMaskSliderValueChanged)
            {
                SetupMaskPixelBlender(width, height);
                this.isMaskSliderValueChanged = false;
                //
                painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlender; //change to new blender
            }
            //1. alpha mask...
            //p2.DrawImage(alphaBitmap, 0, 0);   
            // 
            painter.FillColor = Color.Blue;
            painter.FillCircle(300, 300, 100);
            painter.DrawImage(lionImg, 20, 20);

        }
        public override void MouseDown(int x, int y, bool isRightButton)
        {
            doTransform(this.Width, this.Height, x, y);
        }
        public override void MouseDrag(int x, int y)
        {
            doTransform(this.Width, this.Height, x, y);
            base.MouseDrag(x, y);
        }

        void doTransform(double width, double height, double x, double y)
        {
            x -= width / 2;
            y -= height / 2;
            angle = Math.Atan2(y, x);
            lionScale = Math.Sqrt(y * y + x * x) / 100.0;
        }
    }


}

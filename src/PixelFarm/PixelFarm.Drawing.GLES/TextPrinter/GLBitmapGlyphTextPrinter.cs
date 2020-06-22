﻿//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
//
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.BitmapAtlas;
using PixelFarm.Drawing;

using Typography.TextLayout;
using Typography.OpenFont;
using Typography.TextBreak;
using Typography.FontManagement;

namespace PixelFarm.DrawingGL
{
    /// <summary>
    /// texture-based render vx
    /// </summary>
    public class GLRenderVxFormattedString : PixelFarm.Drawing.RenderVxFormattedString
    {
        DrawingGL.VertexBufferObject _vbo;
        internal GLRenderVxFormattedString()
        {
        }

        //--------
        public float[] VertexCoords { get; set; }
        public ushort[] IndexArray { get; set; }
        public int IndexArrayCount { get; set; }
        public ushort WordPlateLeft { get; set; }
        public ushort WordPlateTop { get; set; }

        internal RequestFont RequestFont { get; set; }
        internal WordPlate OwnerPlate { get; set; }
        internal bool Delay { get; set; }
        internal bool UseWithWordPlate { get; set; }

        internal void ClearOwnerPlate()
        {
            OwnerPlate = null;
            //State = VxState.NoTicket;
        }

        internal DrawingGL.VertexBufferObject GetVbo()
        {
            if (_vbo != null)
            {
                return _vbo;
            }
            _vbo = new VertexBufferObject();
            _vbo.CreateBuffers(this.VertexCoords, this.IndexArray);
            return _vbo;
        }

        internal void DisposeVbo()
        {
            //dispose only VBO
            //and we can create the vbo again
            //from VertexCoord and IndexArray 

            if (_vbo != null)
            {
                _vbo.Dispose();
                _vbo = null;
            }
        }
        public override void Dispose()
        {
            //no use this any more
            VertexCoords = null;
            IndexArray = null;

            if (OwnerPlate != null)
            {
                OwnerPlate.RemoveWordStrip(this);
                OwnerPlate = null;
            }

            DisposeVbo();
            base.Dispose();
        }

#if DEBUG
        public string dbugText;
        public override string ToString()
        {
            if (dbugText != null)
            {
                return dbugText;
            }
            return base.ToString();
        }
        public override string dbugName => "GL";
#endif

    }
    public class GLRenderVxFormattedStringSpan
    {
        DrawingGL.VertexBufferObject _vbo;
        internal GLRenderVxFormattedStringSpan()
        {
        }

        //--------
        public float[] VertexCoords { get; set; }
        public ushort[] IndexArray { get; set; }
        public int IndexArrayCount { get; set; }
        public ushort WordPlateLeft { get; set; }
        public ushort WordPlateTop { get; set; }

        internal RequestFont RequestFont { get; set; }
        internal WordPlate OwnerPlate { get; set; }
        internal bool Delay { get; set; }
        internal bool UseWithWordPlate { get; set; }

        internal void ClearOwnerPlate()
        {
            OwnerPlate = null;
        }

        internal DrawingGL.VertexBufferObject GetVbo()
        {
            if (_vbo != null)
            {
                return _vbo;
            }
            _vbo = new VertexBufferObject();
            _vbo.CreateBuffers(this.VertexCoords, this.IndexArray);
            return _vbo;
        }

        internal void DisposeVbo()
        {
            //dispose only VBO
            //and we can create the vbo again
            //from VertexCoord and IndexArray 

            if (_vbo != null)
            {
                _vbo.Dispose();
                _vbo = null;
            }
        }
        public void Dispose()
        {
            //no use this any more
            VertexCoords = null;
            IndexArray = null;

            //if (OwnerPlate != null)
            //{
            //    OwnerPlate.RemoveWordStrip(this);
            //    OwnerPlate = null;
            //}

            DisposeVbo();

        }

#if DEBUG
        public string dbugText;
        public override string ToString()
        {
            if (dbugText != null)
            {
                return dbugText;
            }
            return base.ToString();
        }
#endif

    }



    public enum GlyphTexturePrinterDrawingTechnique
    {
        Copy,
        Stencil,
        LcdSubPixelRendering,
        Msdf
    }




    public class GLBitmapGlyphTextPrinter : IGLTextPrinter, IDisposable
    {

        MySimpleGLBitmapFontManager _myGLBitmapFontMx;
        SimpleBitmapAtlas _fontAtlas;
        GLPainterCore _pcx;
        GLPainter _painter;
        GLBitmap _glBmp;
        RequestFont _font;

        readonly OpenFontTextService _textServices;
        readonly TextureCoordVboBuilder _vboBuilder = new TextureCoordVboBuilder();

        float _px_scale = 1;

#if DEBUG
        public static GlyphTexturePrinterDrawingTechnique s_dbugDrawTechnique = GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering;
        public static bool s_dbugUseVBO = true;
        public static bool s_dbugShowGlyphTexture = false;
        public static bool s_dbugShowMarkers = false;
#endif
        /// <summary>
        /// use vertex buffer object
        /// </summary>

        public GLBitmapGlyphTextPrinter(GLPainter painter, OpenFontTextService textServices)
        {
            //create text printer for use with canvas painter           
            _painter = painter;
            _pcx = painter.Core;
            _textServices = textServices;

            //_currentTextureKind = TextureKind.Msdf; 
            //_currentTextureKind = TextureKind.StencilGreyScale;

            _myGLBitmapFontMx = new MySimpleGLBitmapFontManager(textServices);

            //LoadFontAtlas("tahoma_set1.multisize_fontAtlas", "tahoma_set1.multisize_fontAtlas.png");

            //test textures...

            //GlyphPosPixelSnapX = GlyphPosPixelSnapKind.Integer;
            //GlyphPosPixelSnapY = GlyphPosPixelSnapKind.Integer;
            //**
            ChangeFont(painter.CurrentFont);
            //
            //TextDrawingTechnique = GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering; //default 
            TextDrawingTechnique = GlyphTexturePrinterDrawingTechnique.Stencil; //default
            UseVBO = true;

            TextBaseline = TextBaseline.Top;
            //TextBaseline = TextBaseline.Alphabetic;
            //TextBaseline = TextBaseline.Bottom;


            //temp fix
            var myAlternativeTypefaceSelector = new MyAlternativeTypefaceSelector();
            var preferTypefaceList = new MyAlternativeTypefaceSelector.PreferTypefaceList();
            preferTypefaceList.AddTypefaceName("Source Sans Pro");
            preferTypefaceList.AddTypefaceName("Sarabun");
            myAlternativeTypefaceSelector.SetPreferTypefaces(ScriptTagDefs.Latin, preferTypefaceList);
            AlternativeTypefaceSelector = myAlternativeTypefaceSelector;

        }
        public void LoadFontAtlas(string fontTextureInfoFile, string atlasImgFilename)
        {
            //TODO: extension method
            if (PixelFarm.Platforms.StorageService.Provider.DataExists(fontTextureInfoFile) &&
                PixelFarm.Platforms.StorageService.Provider.DataExists(atlasImgFilename))
            {
                using (System.IO.Stream fontTextureInfoStream = PixelFarm.Platforms.StorageService.Provider.ReadDataStream(fontTextureInfoFile))
                using (System.IO.Stream fontTextureImgStream = PixelFarm.Platforms.StorageService.Provider.ReadDataStream(atlasImgFilename))
                {
                    try
                    {
                        BitmapAtlasFile fontAtlas = new BitmapAtlasFile();
                        fontAtlas.Read(fontTextureInfoStream);
                        SimpleBitmapAtlas[] resultAtlases = fontAtlas.AtlasList.ToArray();
                        _myGLBitmapFontMx.AddSimpleFontAtlas(resultAtlases, fontTextureImgStream);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

        }
        public bool UseVBO { get; set; }

        GlyphTexturePrinterDrawingTechnique _drawingTech;
        public GlyphTexturePrinterDrawingTechnique TextDrawingTechnique
        {
            get => _drawingTech;
            set
            {
#if DEBUG
                if (value == GlyphTexturePrinterDrawingTechnique.Stencil)
                {

                }
#endif
                _drawingTech = value;
            }
        }
        public void ChangeFillColor(Color color)
        {
            //called by owner painter  
            _painter.FontFillColor = color;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: implementation here
        }

        public TextBaseline TextBaseline { get; set; }

        public void ChangeFont(RequestFont font)
        {
            if (_font == font || (_font != null && _font.FontKey == font.FontKey))
            {
                //not change -> then return
                return;
            }

            //_loadedFont = _loadFonts.RegisterFont(font);
            //System.Diagnostics.Debug.WriteLine(font.Name + font.SizeInPoints);

            //LoadedFont loadFont = _loadFonts.RegisterFont(font);
            //font has been changed, 
            //resolve for the new one 
            //check if we have this texture-font atlas in our MySimpleGLBitmapFontManager 
            //if not-> request the MySimpleGLBitmapFontManager to create a newone 
            _fontAtlas = _myGLBitmapFontMx.GetFontAtlas(font, out _glBmp);
            _font = font;
            _px_scale = _textServices.CalculateScaleToPixelsFromPoint(font);
        }
        public void Dispose()
        {
            if (_myGLBitmapFontMx != null)
            {
                _myGLBitmapFontMx.Clear();
                _myGLBitmapFontMx = null;
            }


            if (_glBmp != null)
            {
                _glBmp.Dispose();
                _glBmp = null;
            }
        }
        public void MeasureString(char[] buffer, int startAt, int len, out int w, out int h)
        {

            var textBufferSpan = new TextBufferSpan(buffer, startAt, len);
            Size s = _textServices.MeasureString(textBufferSpan, _painter.CurrentFont);
            w = s.Width;
            h = s.Height;
        }
        //        void InnerDrawString(char[] textBuffer, int startAt, int len, float x, float y)
        //        {

        //#if DEBUG
        //            if (textBuffer.Length > 3)
        //            {

        //            }
        //#endif 

        //            UpdateGlyphLayoutSettings();

        //            //unscale layout, with design unit scale
        //            var buffSpan = new TextBufferSpan(textBuffer, startAt, len);

        //            float xpos = x;
        //            float ypos = y;

        //            if (!EnableMultiTypefaces)
        //            {
        //                GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(buffSpan, _currentTypeface, FontSizeInPoints);
        //                DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
        //            }
        //            else
        //            {
        //                //a single string may be broken into many glyph-plan-seq
        //                using (ILineSegmentList segments = _textServices.BreakToLineSegments(buffSpan))
        //                {
        //                    int count = segments.Count;

        //                    ClearTempFormattedGlyphPlanSeq();

        //                    bool needRightToLeftArr = false;

        //                    Typeface defaultTypeface = _currentTypeface;
        //                    Typeface curTypeface = defaultTypeface;

        //                    for (int i = 0; i < count; ++i)
        //                    {
        //                        //
        //                        ILineSegment line_seg = segments[i];
        //                        SpanBreakInfo spBreakInfo = (SpanBreakInfo)line_seg.SpanBreakInfo;
        //                        TextBufferSpan buff = new TextBufferSpan(textBuffer, line_seg.StartAt, line_seg.Length);
        //                        if (spBreakInfo.RightToLeft)
        //                        {
        //                            needRightToLeftArr = true;
        //                        }

        //                        //each line segment may have different unicode range 
        //                        //and the current typeface may not support that range
        //                        //so we need to ensure that we get a proper typeface,
        //                        //if not => alternative typeface

        //                        ushort glyphIndex = 0;
        //                        char sample_char = textBuffer[line_seg.StartAt];
        //                        bool contains_surrogate_pair = false;
        //                        if (line_seg.Length > 1)
        //                        {
        //                            //high serogate pair or not
        //                            int codepoint = sample_char;
        //                            if (sample_char >= 0xd800 && sample_char <= 0xdbff) //high surrogate 
        //                            {
        //                                char nextCh = textBuffer[line_seg.StartAt + 1];
        //                                if (nextCh >= 0xdc00 && nextCh <= 0xdfff) //low surrogate
        //                                {
        //                                    codepoint = char.ConvertToUtf32(sample_char, nextCh);
        //                                    contains_surrogate_pair = true;
        //                                }
        //                            }

        //                            glyphIndex = curTypeface.GetGlyphIndex(codepoint);
        //                        }
        //                        else
        //                        {
        //                            glyphIndex = curTypeface.GetGlyphIndex(sample_char);
        //                        }


        //                        if (glyphIndex == 0)
        //                        {
        //                            //not found then => find other typeface                    
        //                            //we need more information about line seg layout

        //                            if (AlternativeTypefaceSelector != null)
        //                            {
        //                                AlternativeTypefaceSelector.LatestTypeface = curTypeface;
        //                            }

        //                            if (_textServices.TryGetAlternativeTypefaceFromChar(sample_char, AlternativeTypefaceSelector, out Typeface alternative))
        //                            {
        //                                curTypeface = alternative;

        //                            }
        //                            else
        //                            {
        //#if DEBUG
        //                                if (sample_char >= 0 && sample_char < 255)
        //                                {


        //                                }
        //#endif
        //                            }
        //                        }


        //                        _textServices.CurrentScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);
        //                        GlyphPlanSequence seq = _textServices.CreateGlyphPlanSeq(buff, curTypeface, FontSizeInPoints);
        //                        seq.IsRightToLeft = spBreakInfo.RightToLeft;

        //                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _pool.GetFreeFmtGlyphPlanSeqs();
        //                        formattedGlyphPlanSeq.seq = seq;
        //                        formattedGlyphPlanSeq.Typeface = curTypeface;
        //                        formattedGlyphPlanSeq.ContainsSurrogatePair = contains_surrogate_pair;

        //                        _tmpGlyphPlanSeqs.Add(formattedGlyphPlanSeq);

        //                        curTypeface = defaultTypeface;//switch back to default

        //                        //restore latest script lang?
        //                    }

        //                    if (needRightToLeftArr)
        //                    {
        //                        //special arr left-to-right
        //                        for (int i = count - 1; i >= 0; --i)
        //                        {
        //                            FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

        //                            Typeface = formattedGlyphPlanSeq.Typeface;

        //                            DrawFromGlyphPlans(formattedGlyphPlanSeq.seq, xpos, y);


        //                            //xpos += (glyphPlanSeq.CalculateWidth() * _currentFontSizePxScale);
        //                            xpos += LatestAccumulateWidth;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        for (int i = 0; i < count; ++i)
        //                        {
        //                            FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

        //                            //change typeface                            
        //                            Typeface = formattedGlyphPlanSeq.Typeface;
        //                            //update pxscale size                             
        //                            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

        //                            DrawFromGlyphPlans(formattedGlyphPlanSeq.seq, xpos, y);
        //                            xpos += LatestAccumulateWidth;

        //                        }
        //                    }

        //                    Typeface = defaultTypeface;
        //                    ClearTempFormattedGlyphPlanSeq();
        //                }
        //            }
        //        }

        readonly TextPrinterLineSegmentList<TextPrinterLineSegment> _textPrinterLineSegs = new TextPrinterLineSegmentList<TextPrinterLineSegment>();
        readonly TextPrinterWordVisitor _textPrinterWordVisitor = new TextPrinterWordVisitor();

        void InnerDrawI18NString(char[] buffer, int startAt, int len, double left, double top)
        {
            //input string may not be only Eng+ Num
            //it may contains characters from other unicode ranges (eg. Emoji)
            //to print it correctly we need to split it to multiple part
            //and choose a proper typeface for each part
            //-----------------

            var textBufferSpan = new TextBufferSpan(buffer, startAt, len);

            _textPrinterLineSegs.Clear();
            _textPrinterWordVisitor.SetLineSegmentList(_textPrinterLineSegs);
            _textServices.BreakToLineSegments(textBufferSpan, _textPrinterWordVisitor);
            _textPrinterWordVisitor.SetLineSegmentList(null);//TODO: not need to set this,

            //check each split segment


            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(textBufferSpan, _font);
            //-----------------

            _vboBuilder.Clear();
            _vboBuilder.SetTextureInfo(_glBmp.Width, _glBmp.Height, _glBmp.IsYFlipped, _pcx.OriginKind);

            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font

            float px_scale = _px_scale;
            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again      

            float scaleFromTexture = _font.SizeInPoints / _fontAtlas.OriginalFontSizePts;

            TextureKind textureKind = _fontAtlas.TextureKind;

            float g_left = 0;
            float g_top = 0;



            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y 

#if DEBUG
            if (s_dbugShowMarkers)
            {
                if (s_dbugShowGlyphTexture)
                {
                    //show original glyph texture at top 
                    _pcx.DrawImage(_glBmp, 0, 0);
                }
                //draw red-line-marker for baseLine
                _painter.StrokeColor = Color.Red;
                int baseLine = (int)Math.Round((float)top + _font.AscentInPixels);
                _painter.DrawLine(left, baseLine, left + 200, baseLine);
                //
                //draw magenta-line-marker for bottom line
                _painter.StrokeColor = Color.Magenta;
                int bottomLine = (int)Math.Round((float)top + _font.LineSpacingInPixels);
                _painter.DrawLine(left, bottomLine, left + 200, bottomLine);
                //draw blue-line-marker for top line
                _painter.StrokeColor = Color.Blue;
                _painter.DrawLine(0, top, left + 200, top);
            }

            //DrawingTechnique = s_dbugDrawTechnique;//for debug only
            //UseVBO = s_dbugUseVBO;//for debug only 
#endif

            if (textureKind == TextureKind.Msdf)
            {
                TextDrawingTechnique = GlyphTexturePrinterDrawingTechnique.Msdf;
            }


            //----------
            float bottom = (float)top + _font.AscentInPixels - _font.DescentInPixels;
            int seqLen = glyphPlanSeq.Count;
            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];
                if (!_fontAtlas.TryGetItem(glyph.glyphIndex, out AtlasItem atlasItem))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //--------------------------------------
                //paint src rect

                var srcRect = new Rectangle(atlasItem.Left, atlasItem.Top, atlasItem.Width, atlasItem.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * px_scale - atlasItem.TextureXOffset * scaleFromTexture);
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * px_scale - atlasItem.TextureYOffset * scaleFromTexture) + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(bottom - y_offset); //***

                switch (TextBaseline)
                {
                    default:
                    case TextBaseline.Alphabetic:
                        //nothing todo
                        break;
                    case TextBaseline.Top:
                        g_top += _font.DescentInPixels;
                        break;
                    case TextBaseline.Bottom:

                        break;
                }

                acc_x += (float)Math.Round(glyph.AdvanceX * px_scale);
                g_top = (float)Math.Ceiling(g_top);//adjust to integer num *** 

#if DEBUG
                if (s_dbugShowMarkers)
                {

                    if (s_dbugShowGlyphTexture)
                    {
                        //draw yellow-rect-marker on original texture
                        _painter.DrawRectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, Color.Yellow);
                    }

                    //draw debug-rect box at target glyph position
                    _painter.DrawRectangle(g_left, g_top, srcRect.Width, srcRect.Height, Color.Black);
                    _painter.StrokeColor = Color.Blue; //restore
                }


                //System.Diagnostics.Debug.WriteLine(
                //    "ds:" + buffer[0] + "o=(" + left + "," + top + ")" +
                //    "g=(" + g_left + "," + g_top + ")" + "srcRect=" + srcRect);

#endif

                if (UseVBO)
                {
                    _vboBuilder.WriteRect(
                           srcRect,
                           g_left, g_top, scaleFromTexture);
                }
                else
                {
                    switch (TextDrawingTechnique)
                    {
                        case GlyphTexturePrinterDrawingTechnique.Msdf:
                            _pcx.DrawSubImageWithMsdf(_glBmp,
                                  srcRect,
                                 g_left,
                                 g_top,
                                 scaleFromTexture);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Stencil:
                            //stencil gray scale with fill-color
                            _pcx.DrawGlyphImageWithStecil(_glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                scaleFromTexture);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Copy:
                            _pcx.DrawSubImage(_glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                1);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                            _pcx.DrawGlyphImageWithSubPixelRenderingTechnique2_GlyphByGlyph(
                                _glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                1);
                            break;
                    }
                }

            }
            //-------------------------------------------
            //

            if (UseVBO)
            {
                switch (TextDrawingTechnique)
                {
                    case GlyphTexturePrinterDrawingTechnique.Copy:
                        _pcx.DrawGlyphImageWithCopy_VBO(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                        _pcx.DrawGlyphImageWithSubPixelRenderingTechnique3_DrawElements(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.Stencil:
                        _pcx.DrawGlyphImageWithStecil_VBO(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.Msdf:
                        _pcx.DrawImagesWithMsdf_VBO(_glBmp, _vboBuilder);
                        break;
                }

                _vboBuilder.Clear();
            }
        }
        public void DrawString(char[] buffer, int startAt, int len, double left, double top)
        {
            //input string may not be only Eng+ Num
            //it may contains characters from other unicode ranges (eg. Emoji)
            //to print it correctly we need to split it to multiple part
            //and choose a proper typeface for each part


            InnerDrawI18NString(buffer, startAt, len, left, top);
            return;

            //OLD

            //create temp buffer span that describe the part of a whole char buffer
            //-----------------
            var textBufferSpan = new TextBufferSpan(buffer, startAt, len);

            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(textBufferSpan, _font);
            //-----------------

            _vboBuilder.Clear();
            _vboBuilder.SetTextureInfo(_glBmp.Width, _glBmp.Height, _glBmp.IsYFlipped, _pcx.OriginKind);

            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font

            float px_scale = _px_scale;
            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again      

            float scaleFromTexture = _font.SizeInPoints / _fontAtlas.OriginalFontSizePts;

            TextureKind textureKind = _fontAtlas.TextureKind;

            float g_left = 0;
            float g_top = 0;



            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y 

#if DEBUG
            if (s_dbugShowMarkers)
            {
                if (s_dbugShowGlyphTexture)
                {
                    //show original glyph texture at top 
                    _pcx.DrawImage(_glBmp, 0, 0);
                }
                //draw red-line-marker for baseLine
                _painter.StrokeColor = Color.Red;
                int baseLine = (int)Math.Round((float)top + _font.AscentInPixels);
                _painter.DrawLine(left, baseLine, left + 200, baseLine);
                //
                //draw magenta-line-marker for bottom line
                _painter.StrokeColor = Color.Magenta;
                int bottomLine = (int)Math.Round((float)top + _font.LineSpacingInPixels);
                _painter.DrawLine(left, bottomLine, left + 200, bottomLine);
                //draw blue-line-marker for top line
                _painter.StrokeColor = Color.Blue;
                _painter.DrawLine(0, top, left + 200, top);
            }

            //DrawingTechnique = s_dbugDrawTechnique;//for debug only
            //UseVBO = s_dbugUseVBO;//for debug only 
#endif

            if (textureKind == TextureKind.Msdf)
            {
                TextDrawingTechnique = GlyphTexturePrinterDrawingTechnique.Msdf;
            }


            //----------
            float bottom = (float)top + _font.AscentInPixels - _font.DescentInPixels;
            int seqLen = glyphPlanSeq.Count;
            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];
                if (!_fontAtlas.TryGetItem(glyph.glyphIndex, out AtlasItem atlasItem))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //--------------------------------------
                //paint src rect

                var srcRect = new Rectangle(atlasItem.Left, atlasItem.Top, atlasItem.Width, atlasItem.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * px_scale - atlasItem.TextureXOffset * scaleFromTexture);
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * px_scale - atlasItem.TextureYOffset * scaleFromTexture) + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(bottom - y_offset); //***

                switch (TextBaseline)
                {
                    default:
                    case TextBaseline.Alphabetic:
                        //nothing todo
                        break;
                    case TextBaseline.Top:
                        g_top += _font.DescentInPixels;
                        break;
                    case TextBaseline.Bottom:

                        break;
                }

                acc_x += (float)Math.Round(glyph.AdvanceX * px_scale);
                g_top = (float)Math.Ceiling(g_top);//adjust to integer num *** 

#if DEBUG
                if (s_dbugShowMarkers)
                {

                    if (s_dbugShowGlyphTexture)
                    {
                        //draw yellow-rect-marker on original texture
                        _painter.DrawRectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, Color.Yellow);
                    }

                    //draw debug-rect box at target glyph position
                    _painter.DrawRectangle(g_left, g_top, srcRect.Width, srcRect.Height, Color.Black);
                    _painter.StrokeColor = Color.Blue; //restore
                }


                //System.Diagnostics.Debug.WriteLine(
                //    "ds:" + buffer[0] + "o=(" + left + "," + top + ")" +
                //    "g=(" + g_left + "," + g_top + ")" + "srcRect=" + srcRect);

#endif

                if (UseVBO)
                {
                    _vboBuilder.WriteRect(
                           srcRect,
                           g_left, g_top, scaleFromTexture);
                }
                else
                {
                    switch (TextDrawingTechnique)
                    {
                        case GlyphTexturePrinterDrawingTechnique.Msdf:
                            _pcx.DrawSubImageWithMsdf(_glBmp,
                                  srcRect,
                                 g_left,
                                 g_top,
                                 scaleFromTexture);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Stencil:
                            //stencil gray scale with fill-color
                            _pcx.DrawGlyphImageWithStecil(_glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                scaleFromTexture);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Copy:
                            _pcx.DrawSubImage(_glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                1);
                            break;
                        case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                            _pcx.DrawGlyphImageWithSubPixelRenderingTechnique2_GlyphByGlyph(
                                _glBmp,
                                srcRect,
                                g_left,
                                g_top,
                                1);
                            break;
                    }
                }

            }
            //-------------------------------------------
            //

            if (UseVBO)
            {
                switch (TextDrawingTechnique)
                {
                    case GlyphTexturePrinterDrawingTechnique.Copy:
                        _pcx.DrawGlyphImageWithCopy_VBO(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                        _pcx.DrawGlyphImageWithSubPixelRenderingTechnique3_DrawElements(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.Stencil:
                        _pcx.DrawGlyphImageWithStecil_VBO(_glBmp, _vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.Msdf:
                        _pcx.DrawImagesWithMsdf_VBO(_glBmp, _vboBuilder);
                        break;
                }

                _vboBuilder.Clear();
            }
        }

        public void DrawString(GLRenderVxFormattedString vxFmtStr, double x, double y)
        {
            _pcx.FontFillColor = _painter.FontFillColor;

            switch (TextDrawingTechnique)
            {
                case GlyphTexturePrinterDrawingTechnique.Copy:
                    {
                        //eg. bitmap glyph
                        if (vxFmtStr.Delay && vxFmtStr.OwnerPlate == null)
                        {
                            //add this to queue to create                              
                            return;
                        }

                        float base_offset = 0;

                        if (!vxFmtStr.UseWithWordPlate)
                        {
                            _pcx.DrawGlyphImageWithCopyTech_FromVBO(
                                _glBmp,
                                   vxFmtStr.GetVbo(),
                                   vxFmtStr.IndexArrayCount,
                                   (float)Math.Round(x),
                                   (float)Math.Floor(y)
                                );
                            return;
                        }
                        //---------
                        //use word plate 
                        if (vxFmtStr.OwnerPlate == null)
                        {
                            //UseWithWordPlate=> this renderVx has beed assign to wordplate,
                            //but when WordPlateId=0, this mean the wordplate was disposed.
                            //so create it again
                            _painter.CreateWordStrip(vxFmtStr);
                        }

                        switch (TextBaseline)
                        {
                            case TextBaseline.Alphabetic:
                                //base_offset = -(vxFmtStr.SpanHeight + vxFmtStr.DescendingInPx);
                                break;
                            case TextBaseline.Top:
                                base_offset = vxFmtStr.DescendingInPx;
                                break;
                            case TextBaseline.Bottom:
                                base_offset = -vxFmtStr.SpanHeight;
                                break;
                        }


                        //eval again 
                        if (vxFmtStr.OwnerPlate != null)
                        {
                            _pcx.DrawWordSpanWithCopyTechnique((GLBitmap)vxFmtStr.OwnerPlate._backBuffer.GetImage(),
                                vxFmtStr.WordPlateLeft, -vxFmtStr.WordPlateTop - vxFmtStr.SpanHeight,
                                vxFmtStr.Width, vxFmtStr.SpanHeight,
                                (float)Math.Round(x),
                                (float)Math.Floor(y + base_offset));
                        }
                        else
                        {
                            //can't create at this time
                            //render with vbo 

                            _pcx.DrawGlyphImageWithCopyTech_FromVBO(
                                _glBmp,
                                   vxFmtStr.GetVbo(),
                                   vxFmtStr.IndexArrayCount,
                                   (float)Math.Round(x),
                                   (float)Math.Floor(y + base_offset)
                                );


                            //_pcx.DrawGlyphImageWithStencilRenderingTechnique4_FromVBO(
                            //     _glBmp,
                            //     vxFmtStr.GetVbo(),
                            //     vxFmtStr.IndexArrayCount,
                            //     (float)Math.Round(x),
                            //     (float)Math.Floor(y + base_offset));
                        }
                    }
                    break;
                case GlyphTexturePrinterDrawingTechnique.Stencil:
                    {
                        if (vxFmtStr.Delay && vxFmtStr.OwnerPlate == null)
                        {
                            //add this to queue to create                              
                            return;
                        }

                        float base_offset = 0;

                        if (!vxFmtStr.UseWithWordPlate)
                        {
                            _pcx.DrawGlyphImageWithStencilRenderingTechnique4_FromVBO(
                                   _glBmp,
                                   vxFmtStr.GetVbo(),
                                   vxFmtStr.IndexArrayCount,
                                   (float)Math.Round(x),
                                   (float)Math.Floor(y));
                            return;
                        }
                        //---------
                        //use word plate 
                        if (vxFmtStr.OwnerPlate == null)
                        {
                            //UseWithWordPlate=> this renderVx has beed assign to wordplate,
                            //but when WordPlateId=0, this mean the wordplate was disposed.
                            //so create it again
                            _painter.CreateWordStrip(vxFmtStr);
                        }

                        switch (TextBaseline)
                        {
                            case TextBaseline.Alphabetic:
                                //base_offset = -(vxFmtStr.SpanHeight + vxFmtStr.DescendingInPx);
                                break;
                            case TextBaseline.Top:
                                base_offset = vxFmtStr.DescendingInPx;
                                break;
                            case TextBaseline.Bottom:
                                base_offset = -vxFmtStr.SpanHeight;
                                break;
                        }


                        //eval again 
                        if (vxFmtStr.OwnerPlate != null)
                        {
                            _pcx.DrawWordSpanWithStencilTechnique((GLBitmap)vxFmtStr.OwnerPlate._backBuffer.GetImage(),
                                vxFmtStr.WordPlateLeft, -vxFmtStr.WordPlateTop - vxFmtStr.SpanHeight,
                                vxFmtStr.Width, vxFmtStr.SpanHeight,
                                (float)Math.Round(x),
                                (float)Math.Floor(y + base_offset));
                        }
                        else
                        {
                            //can't create at this time
                            //render with vbo 
                            _pcx.DrawGlyphImageWithStencilRenderingTechnique4_FromVBO(
                                 _glBmp,
                                 vxFmtStr.GetVbo(),
                                 vxFmtStr.IndexArrayCount,
                                 (float)Math.Round(x),
                                 (float)Math.Floor(y + base_offset));
                        }
                    }
                    break;
                case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                    {
                        if (vxFmtStr.Delay && vxFmtStr.OwnerPlate == null)
                        {
                            //add this to queue to create                              
                            return;
                        }

                        float base_offset = 0;
                        switch (TextBaseline)
                        {
                            case TextBaseline.Alphabetic:
                                //base_offset = -(vxFmtStr.SpanHeight + vxFmtStr.DescendingInPx);
                                break;
                            case TextBaseline.Top:
                                base_offset = vxFmtStr.DescendingInPx;
                                break;
                            case TextBaseline.Bottom:
                                base_offset = -vxFmtStr.SpanHeight;
                                break;
                        }

                        //LCD-Effect****
                        if (!vxFmtStr.UseWithWordPlate)
                        {
                            if (_painter.PreparingWordStrip)
                            {
                                //
#if DEBUG
                                //ensure text bg hint
#endif


                                _pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_ForWordStrip_FromVBO(
                                 _glBmp,
                                 vxFmtStr.GetVbo(),
                                 vxFmtStr.IndexArrayCount,
                                 (float)Math.Round(x),
                                 (float)Math.Floor(y + base_offset));
                            }
                            else
                            {
                                _pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_FromVBO(
                                 _glBmp,
                                 vxFmtStr.GetVbo(),
                                 vxFmtStr.IndexArrayCount,
                                 (float)Math.Round(x),
                                 (float)Math.Floor(y + base_offset));
                            }

                            return;
                        }

                        //use word plate 
                        if (vxFmtStr.OwnerPlate == null)
                        {
                            //UseWithWordPlate=> this renderVx has beed assign to wordplate,
                            //but when WordPlateId=0, this mean the wordplate was disposed.
                            //so create it again
                            _painter.CreateWordStrip(vxFmtStr);
                        }


                        //eval again                         
                        if (vxFmtStr.OwnerPlate != null)
                        {
                            //depend on current owner plate bg color***
                            //                
                            Color bgColorHint = _painter.TextBgColorHint;
                            if (bgColorHint.A == 255)
                            {
                                //solid bg color
                                //TODO: configure this value to range 
                                //since this works with since some light color (near white) too

                                //_pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_FromVBO(
                                //   _glBmp,
                                //   vxFmtStr.GetVbo(),
                                //   vxFmtStr.IndexArrayCount,
                                //   (float)Math.Round(x),
                                //   (float)Math.Floor(y + base_offset));

                                _pcx.DrawWordSpanWithLcdSubpixForSolidBgColor((GLBitmap)vxFmtStr.OwnerPlate._backBuffer.GetImage(),
                                    vxFmtStr.WordPlateLeft, -vxFmtStr.WordPlateTop - vxFmtStr.SpanHeight - base_offset,
                                    vxFmtStr.Width, vxFmtStr.SpanHeight,
                                    (float)Math.Round(x),
                                    (float)Math.Floor(y + base_offset),
                                    bgColorHint);
                            }
                            else
                            {
                                _pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_FromVBO(
                                 _glBmp,
                                 vxFmtStr.GetVbo(),
                                 vxFmtStr.IndexArrayCount,
                                 (float)Math.Round(x),
                                 (float)Math.Floor(y + base_offset));

                            }
                        }
                        else
                        {
                            //can't create at this time or we 
                            //render with vbo

                            _pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_FromVBO(
                                _glBmp,
                                vxFmtStr.GetVbo(),
                                vxFmtStr.IndexArrayCount,
                                (float)Math.Round(x),
                                (float)Math.Floor(y + base_offset));
                        }

                    }
                    break;
            }
        }
#if DEBUG
        static int _dbugCount;
#endif



        void CreateTextCoords(GLRenderVxFormattedString vxFmtStr, TextBufferSpan textBufferSpan)
        {
            int top = 0;//simulate top
            int left = 0;//simulate left

            _vboBuilder.Clear();
            _vboBuilder.SetTextureInfo(_glBmp.Width, _glBmp.Height, _glBmp.IsYFlipped, _pcx.OriginKind);

            //create temp buffer span that describe the part of a whole char buffer 
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font  

            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(textBufferSpan, _font);
            float px_scale = _px_scale;
            float scaleFromTexture = 1; //TODO: support msdf auto scale

            //-------------------------- 

            //TextureKind textureKind = _fontAtlas.TextureKind;
            float g_left = 0;
            float g_top = 0;

            //int baseLine = (int)Math.Round((float)top + _font.AscentInPixels);
            //int bottom = (int)Math.Round((float)top + _font.AscentInPixels - _font.DescentInPixels);
            float bottom = (float)top + _font.AscentInPixels - _font.DescentInPixels;
            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y  

            int seqLen = glyphPlanSeq.Count;

            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];

                if (!_fontAtlas.TryGetItem(glyph.glyphIndex,
                    out AtlasItem atlasItem))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //--------------------------------------  
                //paint src rect

                var srcRect = new Rectangle(atlasItem.Left, atlasItem.Top, atlasItem.Width, atlasItem.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * px_scale - atlasItem.TextureXOffset);
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * px_scale - atlasItem.TextureYOffset) + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(bottom - y_offset); //***

                acc_x += (float)Math.Round(glyph.AdvanceX * px_scale);
                //g_x = (float)Math.Round(g_x); //***
                g_top = (float)Math.Floor(g_top);//adjust to integer num *** 
                //
                _vboBuilder.WriteRect(srcRect, g_left, g_top, scaleFromTexture);
            }

            if (seqLen > 1)
            {
                _vboBuilder.AppendDegenerativeTriangle();
            }

            //---
            //copy vbo result and store into  renderVx  
            //TODO: review here
            vxFmtStr.IndexArrayCount = _vboBuilder._indexList.Count;
            vxFmtStr.IndexArray = _vboBuilder._indexList.ToArray();
            vxFmtStr.VertexCoords = _vboBuilder._buffer.ToArray();
            vxFmtStr.Width = acc_x;
            vxFmtStr.SpanHeight = _font.LineSpacingInPixels;
            vxFmtStr.DescendingInPx = (short)_font.DescentInPixels;

            _vboBuilder.Clear();
        }

        public AlternativeTypefaceSelector AlternativeTypefaceSelector { get; set; }
        public void PrepareStringForRenderVx(GLRenderVxFormattedString vxFmtStr, char[] buffer, int startAt, int len)
        {
            //we need to parse string 
            //since it may contains glyph from multiple font (eg. eng, emoji etc.)
            //see VxsTextPrinter 

            var buffSpan = new TextBufferSpan(buffer, startAt, len);

            RequestFont reqFont = vxFmtStr.RequestFont;
            if (reqFont == null)
            {
                //use default
                reqFont = _painter.CurrentFont;
            }

            //resolve this type face
            Typeface defaultTypeface = _textServices.ResolveTypeface(reqFont);
            Typeface curTypeface = defaultTypeface;

            bool needRightToLeftArr = false;

            _textPrinterLineSegs.Clear();
            _textPrinterWordVisitor.SetLineSegmentList(_textPrinterLineSegs);
            _textServices.BreakToLineSegments(buffSpan, _textPrinterWordVisitor);
            _textPrinterWordVisitor.SetLineSegmentList(null);
            //typeface may not have a glyph for some char
            //eg eng font + emoji

            int count = _textPrinterLineSegs.Count;
            for (int i = 0; i < count; ++i)
            {
                TextPrinterLineSegment line_seg = _textPrinterLineSegs.GetLineSegment(i);
                //find a proper font for each segment
                //so we need to check 
                SpanBreakInfo spBreakInfo = line_seg.breakInfo;

                TextBufferSpan buff = new TextBufferSpan(buffer, line_seg.StartAt, line_seg.Length);
                if (spBreakInfo.RightToLeft)
                {
                    needRightToLeftArr = true;
                }
                //each line segment may have different unicode range 
                //and the current typeface may not support that range
                //so we need to ensure that we get a proper typeface,
                //if not => alternative typeface

                ushort glyphIndex = 0;
                char sample_char = buffer[line_seg.StartAt];
                bool contains_surrogate_pair = false;
                if (line_seg.Length > 1)
                {
                    //high serogate pair or not
                    int codepoint = sample_char;
                    if (sample_char >= 0xd800 && sample_char <= 0xdbff) //high surrogate 
                    {
                        char nextCh = buffer[line_seg.StartAt + 1];
                        if (nextCh >= 0xdc00 && nextCh <= 0xdfff) //low surrogate
                        {
                            codepoint = char.ConvertToUtf32(sample_char, nextCh);
                            contains_surrogate_pair = true;
                        }
                    }

                    glyphIndex = curTypeface.GetGlyphIndex(codepoint);
                }
                else
                {
                    glyphIndex = curTypeface.GetGlyphIndex(sample_char);
                }

                //------------
                if (glyphIndex == 0)
                {
                    //not found then => find other typeface                    
                    //we need more information about line seg layout
                    if (AlternativeTypefaceSelector != null)
                    {
                        AlternativeTypefaceSelector.LatestTypeface = curTypeface;
                    }

                    if (_textServices.TryGetAlternativeTypefaceFromChar(sample_char, AlternativeTypefaceSelector, out Typeface alternative))
                    {
                        curTypeface = alternative;

                    }
                    else
                    {
#if DEBUG
                        if (sample_char >= 0 && sample_char < 255)
                        {


                        }
#endif
                    }
                }

                //
                _textServices.CurrentScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);
                GlyphPlanSequence seq = _textServices.CreateGlyphPlanSeq(buff, curTypeface, reqFont.SizeInPoints);
                seq.IsRightToLeft = spBreakInfo.RightToLeft;

                FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlanPool.GetFreeFmtGlyphPlanSeqs();
                formattedGlyphPlanSeq.seq = seq;
                formattedGlyphPlanSeq.Typeface = curTypeface;
                formattedGlyphPlanSeq.ContainsSurrogatePair = contains_surrogate_pair;

                _tmpGlyphPlanSeqs.Add(formattedGlyphPlanSeq);
            }

            CreateTextCoords(vxFmtStr, buffSpan);

            if (vxFmtStr.Delay)
            {
                //when we use delay mode
                //we need to save current font setting  of the _painter
                //with the render vx---
                vxFmtStr.RequestFont = _painter.CurrentFont;

                //TODO: review here again
                vxFmtStr.BmpOnTransparentBackground = defaultTypeface.HasSvgTable() || defaultTypeface.IsBitmapFont || defaultTypeface.HasColorTable();

            }
            else
            {
                //TODO: review here again 

                vxFmtStr.BmpOnTransparentBackground = defaultTypeface.HasSvgTable() || defaultTypeface.IsBitmapFont || defaultTypeface.HasColorTable();
                _painter.CreateWordStrip(vxFmtStr);
            }
        }


        FormattedGlyphPlanSeqPool _fmtGlyphPlanPool = new FormattedGlyphPlanSeqPool();
        List<FormattedGlyphPlanSeq> _tmpGlyphPlanSeqs = new List<FormattedGlyphPlanSeq>();
        public void ClearTempFormattedGlyphPlanSeq()
        {
            for (int i = _tmpGlyphPlanSeqs.Count - 1; i >= 0; --i)
            {
                _fmtGlyphPlanPool.ReleaseFmtGlyphPlanSeqs(_tmpGlyphPlanSeqs[i]);
            }
            _tmpGlyphPlanSeqs.Clear();
        }
    }






    class WordPlateMx
    {

        Dictionary<ushort, WordPlate> _wordPlates = new Dictionary<ushort, WordPlate>();
        //**dictionay not guarantee sorted id**
        Queue<WordPlate> _wordPlatesQueue = new Queue<WordPlate>();
        WordPlate _latestPlate;

        int _defaultPlateW = 800;
        int _defaultPlateH = 600;

        static ushort s_totalPlateId = 0;

        public WordPlateMx()
        {
            MaxPlateCount = 20; //*** important!
            AutoRemoveOldestPlate = true;
        }

        public bool AutoRemoveOldestPlate { get; set; }
        public int MaxPlateCount { get; set; }
        public void SetDefaultPlateSize(int w, int h)
        {
            _defaultPlateH = h;
            _defaultPlateW = w;
        }
        public void ClearAllPlates()
        {
            foreach (WordPlate wordPlate in _wordPlates.Values)
            {
                wordPlate.Dispose();
            }
            _wordPlates.Clear();
            _wordPlatesQueue.Clear();
        }
        /// <summary>
        /// get current wordplate if there is avilable space, or create new one
        /// </summary>
        /// <param name="fmtPlate"></param>
        /// <returns></returns>
        public WordPlate GetWordPlate(GLRenderVxFormattedString fmtPlate)
        {
            if (_latestPlate != null &&
                _latestPlate.HasAvailableSpace(fmtPlate))
            {
                return _latestPlate;
            }
            return GetNewWordPlate();
        }
        public WordPlate GetNewWordPlate()
        {
            //create new and register  
            if (_wordPlates.Count == MaxPlateCount)
            {
                if (AutoRemoveOldestPlate)
                {
                    //**dictionay not guarantee sorted id**
                    //so we use queue, (TODO: use priority queue) 
                    WordPlate oldest = _wordPlatesQueue.Dequeue();
                    _wordPlates.Remove(oldest._plateId);
#if DEBUG
                    if (oldest.dbugUsedCount < 50)
                    {

                    }
                    //oldest.dbugSaveBackBuffer("word_plate_" + oldest._plateId + ".png");
#endif

                    oldest.Dispose();
                    oldest = null;
                }
            }

            if (s_totalPlateId + 1 >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }

            s_totalPlateId++;  //so plate_id starts at 1 

            WordPlate wordPlate = new WordPlate(s_totalPlateId, _defaultPlateW, _defaultPlateH);
            _wordPlates.Add(s_totalPlateId, wordPlate);
            _wordPlatesQueue.Enqueue(wordPlate);

#if DEBUG
            wordPlate.Cleared += WordPlate_Cleared;
#endif
            return _latestPlate = wordPlate;
        }
#if DEBUG
        private void WordPlate_Cleared(WordPlate obj)
        {

        }
#endif
    }

    public class WordPlate : IDisposable
    {
        bool _isInitBg;
        int _currentX;
        int _currentY;
        int _currentLineHeightMax;
        readonly int _plateWidth;
        readonly int _plateHeight;
        bool _full;

        internal readonly ushort _plateId;
        Dictionary<GLRenderVxFormattedString, bool> _wordStrips = new Dictionary<GLRenderVxFormattedString, bool>();
        internal Drawing.GLES2.MyGLBackbuffer _backBuffer;

        public event Action<WordPlate> Cleared;

        public WordPlate(ushort plateId, int w, int h)
        {
            _plateId = plateId;
            _plateWidth = w;
            _plateHeight = h;
            _backBuffer = new Drawing.GLES2.MyGLBackbuffer(w, h);
        }
#if DEBUG
        internal int dbugUsedCount;
        public void dbugSaveBackBuffer(string filename)
        {
            //save output
            using (Image img = _backBuffer.CopyToNewMemBitmap())
            {
                if (img is MemBitmap memBmp)
                {
                    memBmp.SaveImage(filename);
                }
            }
        }
#endif

        const int INTERLINE_SPACE = 1; //px
        const int INTERWORD_SPACE = 1; //px

        public void Dispose()
        {
            //clear all
            if (_backBuffer != null)
            {
                _backBuffer.Dispose();
                _backBuffer = null;
            }
            foreach (GLRenderVxFormattedString k in _wordStrips.Keys)
            {
                //essential!
                k.ClearOwnerPlate();
            }
            _wordStrips.Clear();
        }


        public bool Full => _full;

        public bool HasAvailableSpace(GLRenderVxFormattedString renderVxFormattedString)
        {
            //check if we have avaliable space for this?

            float width = renderVxFormattedString.Width;
            float previewY = _currentY;
#if DEBUG
            float previewX = _currentX;
#endif
            if (_currentX + width > _plateWidth)
            {
                //move to newline                    
                previewY += _currentLineHeightMax + INTERLINE_SPACE;

            }

            return previewY + renderVxFormattedString.SpanHeight < _plateHeight;
        }
        public bool CreateWordStrip(GLPainter painter, GLRenderVxFormattedString renderVxFormattedString)
        {
            //--------------
            //create stencil text buffer                  
            //we use white glyphs on black bg
            //--------------
            if (!_isInitBg)
            {
                //by default, we init bg to black for stencil buffer
                _isInitBg = true;
                painter.Clear(Color.Black);
            }



            float width = renderVxFormattedString.Width;

            if (_currentX + width > _plateWidth)
            {
                //move to newline
                _currentY += _currentLineHeightMax + INTERLINE_SPACE;//interspace =4 px
                _currentX = 0;
                //new line
                _currentLineHeightMax = (int)Math.Ceiling(renderVxFormattedString.SpanHeight);
            }

            //on current line
            //check available height
            if (_currentY + renderVxFormattedString.SpanHeight > _plateHeight)
            {
                _full = true;
                return false;
            }
            //----------------------------------


            if (renderVxFormattedString.SpanHeight > _currentLineHeightMax)
            {
                _currentLineHeightMax = (int)Math.Ceiling(renderVxFormattedString.SpanHeight);
            }
            //draw string with renderVxFormattedString                
            //float width = renderVxFormattedString.CalculateWidth();

            //PixelFarm.Drawing.GLES2.GLES2Platform.TextService.MeasureString()

            //we need to go to newline or not

            Color prevColor = painter.FontFillColor;
            Color prevTextBgHint = painter.TextBgColorHint;
            bool prevPreparingWordStrip = painter.PreparingWordStrip;
            GlyphTexturePrinterDrawingTechnique prevTextDrawing = painter.TextPrinterDrawingTechnique;

            painter.TextBgColorHint = Color.Black;
            painter.FontFillColor = Color.White;
            painter.PreparingWordStrip = true;
            renderVxFormattedString.UseWithWordPlate = false;

            //----
            RequestFont reqFont = painter.CurrentFont;


            if (reqFont.Name.Contains("Emoji"))
            {
                //some font is color font,
                //eg some bitmap font, some svg, or color glyph
                //we will send background rgn for it to transparent bg
                //painter.ClearRect(Color.Transparent, _currentX, _currentY, width, _currentLineHeightMax);
                //painter.ClearRect(Color.Red, _currentX, _currentY, 200, 200);
                painter.TextPrinterDrawingTechnique = GlyphTexturePrinterDrawingTechnique.Copy;
                //painter.Clear(Color.Transparent);

                //choice 1
                //painter.ClearRect(Color.Red, _currentX, _currentY, width, _currentLineHeightMax);

                //--
                //choice 2
                Rectangle currentClip = painter.ClipBox;
                painter.SetClipBox(_currentX, _currentY, (int)(_currentX + width), _currentY + _currentLineHeightMax);
                painter.Clear(Color.Transparent);
                painter.SetClipBox(currentClip.Left, currentClip.Top, currentClip.Right, currentClip.Bottom); //restore
                //--
            }
            //else
            //{
            //    painter.ClearRect(Color.Black, _currentX, _currentY, 200, 200);
            //}
            //----


            painter.DrawString(renderVxFormattedString, _currentX, _currentY);

            renderVxFormattedString.UseWithWordPlate = true;//restore
            painter.FontFillColor = prevColor;//restore
            painter.TextBgColorHint = prevTextBgHint;//restore
            painter.PreparingWordStrip = prevPreparingWordStrip;
            painter.TextPrinterDrawingTechnique = prevTextDrawing;
            //in this case we can dispose vbo inside renderVx
            //(we can recreate that vbo later)
            renderVxFormattedString.DisposeVbo();

            renderVxFormattedString.OwnerPlate = this;
            renderVxFormattedString.WordPlateLeft = (ushort)_currentX;
            renderVxFormattedString.WordPlateTop = (ushort)_currentY;
            renderVxFormattedString.UseWithWordPlate = true;

#if DEBUG
            dbugUsedCount++;
#endif
            _wordStrips.Add(renderVxFormattedString, true);
            //--------

            _currentX += (int)Math.Ceiling(renderVxFormattedString.Width) + INTERWORD_SPACE; //interspace x 1px

            return true;
        }

        public void RemoveWordStrip(GLRenderVxFormattedString vx)
        {
            _wordStrips.Remove(vx);
            if (_full && _wordStrips.Count == 0)
            {
                Cleared?.Invoke(this);
            }
        }
    }


}



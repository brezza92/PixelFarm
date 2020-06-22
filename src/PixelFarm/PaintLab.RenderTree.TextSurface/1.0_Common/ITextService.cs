﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using Typography.TextServices;
using Typography.TextBreak;
using Typography.OpenFont;
using System.Runtime.Remoting.Messaging;

namespace LayoutFarm
{

    public interface IAdvanceTextService
    {
        bool SupportsWordBreak { get; }
        void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
                RequestFont font,
                ref TextSpanMeasureResult result);

        ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan);

        void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, ILineSegmentList lineSegs,
              RequestFont font,
              ref TextSpanMeasureResult result);
    }



    class AdvanceTextServiceImpl : IAdvanceTextService
    { 
        readonly OpenFontTextService _textService;
        public AdvanceTextServiceImpl(OpenFontTextService textService) => _textService = textService;

        public bool SupportsWordBreak => true;

        public ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan)
        {
            return _textService.BreakToLineSegments(textBufferSpan);
        }

        public void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan, RequestFont font, ref TextSpanMeasureResult measureResult)
        {
            _textService.CalculateUserCharGlyphAdvancePos(textBufferSpan, font, ref measureResult);
        }

        public void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
            ILineSegmentList lineSegs,
            RequestFont font,
            ref TextSpanMeasureResult measureResult)
        {
            _textService.CalculateUserCharGlyphAdvancePos(textBufferSpan,
                lineSegs,
                font,
                ref measureResult);
        } 
    } 
    public static class GlobalTextService
    {

        public static IAdvanceTextService AdvanceTextService { get; private set; }

        static ITextService s_textServices;
        static OpenFontTextService s_openFontTextService;
        public static OpenFontTextService TextService2
        {
            get => s_openFontTextService;
            set
            {
                s_openFontTextService = value;
                AdvanceTextService = (value != null) ? new AdvanceTextServiceImpl(value) : null;
            }
        }

        public static ITextService TextService
        {
            get => s_textServices;
            set
            {
#if DEBUG
                if (s_textServices != null)
                {

                }
#endif
                s_textServices = value;

            }
        }
    }
}

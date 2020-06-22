﻿//Apache2, 2014-present, WinterDev


using PixelFarm.Drawing;

namespace LayoutFarm.TextEditing
{
    public class RunStyle
    {
        public RunStyle()
        {

        }
        //
        public byte ContentHAlign;
        //
        public RequestFont ReqFont { get; set; }
        public Color FontColor { get; set; }
        //
        internal Size MeasureString(in TextBufferSpan textBufferSpan)
        {
            return GlobalTextService.TextService.MeasureString(textBufferSpan, ReqFont);
        }
        internal float MeasureBlankLineHeight()
        {
            return GlobalTextService.TextService.MeasureBlankLineHeight(ReqFont);
        }
        internal bool SupportsWordBreak => GlobalTextService.AdvanceTextService.SupportsWordBreak;
        internal ILineSegmentList BreakToLineSegments(in TextBufferSpan textBufferSpan)
        {
            return GlobalTextService.AdvanceTextService.BreakToLineSegments(textBufferSpan);
        }

        internal void CalculateUserCharGlyphAdvancePos(
            in TextBufferSpan textBufferSpan,
            ref TextSpanMeasureResult measureResult)
        {
            GlobalTextService.AdvanceTextService.CalculateUserCharGlyphAdvancePos(
                    textBufferSpan,
                    ReqFont,
                    ref measureResult);
        }

        internal void CalculateUserCharGlyphAdvancePos(in TextBufferSpan textBufferSpan,
            ILineSegmentList lineSegs,
            ref TextSpanMeasureResult measureResult)
        {
            GlobalTextService.AdvanceTextService.CalculateUserCharGlyphAdvancePos(
                textBufferSpan,
                lineSegs,
                ReqFont,
                ref measureResult);
        }

    }

}
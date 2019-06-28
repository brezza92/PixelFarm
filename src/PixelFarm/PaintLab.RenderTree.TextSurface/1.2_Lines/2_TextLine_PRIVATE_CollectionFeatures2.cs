﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
namespace LayoutFarm.TextEditing
{
    partial class TextLine
    {

        public void AddLineBreakAfter(Run afterTextRun)
        {
            if (afterTextRun == null)
            {
                this.EndWithLineBreak = true;
                TextLine newline = _textFlowLayer.InsertNewLine(_currentLineNumber + 1);
                //
                if (_textFlowLayer.LineCount - 1 != newline.LineNumber)
                {
                    newline.EndWithLineBreak = true;
                }
            }
            else if (afterTextRun.NextRun == null)
            {
                this.EndWithLineBreak = true;
                TextLine newline = _textFlowLayer.InsertNewLine(_currentLineNumber + 1);
                //
                if (_textFlowLayer.LineCount - 1 != newline.LineNumber)
                {
                    newline.EndWithLineBreak = true;
                }
            }
            else
            {
                
                //TODO: use pool
                List<Run> tempTextRuns = new List<Run>(this.RunCount);
                if (afterTextRun != null)
                {
                    foreach (Run t in GetVisualElementForward(afterTextRun.NextRun))
                    {
                        tempTextRuns.Add(t);
                    }
                }

                this.EndWithLineBreak = true;
                this.LocalSuspendLineReArrange();

                TextLine newTextline = _textFlowLayer.InsertNewLine(_currentLineNumber + 1);
                //
                int j = tempTextRuns.Count;
                newTextline.LocalSuspendLineReArrange();
                int cx = 0;
                for (int i = 0; i < j; ++i)
                {
                    Run t = tempTextRuns[i];
                    this.Remove(t);
                    newTextline.AddLast(t);
                    Run.DirectSetLocation(t, cx, 0);
                    cx += t.Width;
                }

                newTextline.LocalResumeLineReArrange();
                this.LocalResumeLineReArrange();
            }
        }
        void AddLineBreakBefore(Run beforeTextRun)
        {
            if (beforeTextRun == null)
            {
                this.EndWithLineBreak = true;
                _textFlowLayer.InsertNewLine(_currentLineNumber + 1);
            }
            else
            {
                //TODO: use pool
                List<Run> tempTextRuns = new List<Run>();
                if (beforeTextRun != null)
                {
                    foreach (Run t in GetVisualElementForward(beforeTextRun))
                    {
                        tempTextRuns.Add(t);
                    }
                }
                this.EndWithLineBreak = true;
                TextLine newTextline = _textFlowLayer.InsertNewLine(_currentLineNumber + 1);
                //
                this.LocalSuspendLineReArrange();
                newTextline.LocalSuspendLineReArrange();
                int j = tempTextRuns.Count;
                for (int i = 0; i < j; ++i)
                {
                    Run t = tempTextRuns[i];
                    this.Remove(t);
                    newTextline.AddLast(t);
                }
                this.LocalResumeLineReArrange();
                newTextline.LocalResumeLineReArrange();
            }
        }

        void RemoveLeft(Run t)
        {
            if (t != null)
            {

                LinkedList<Run> tobeRemoveTextRuns = CollectLeftRuns(t);
                LinkedListNode<Run> curNode = tobeRemoveTextRuns.First;
                LocalSuspendLineReArrange();
                while (curNode != null)
                {
                    Remove(curNode.Value);
                    curNode = curNode.Next;
                }
                LocalResumeLineReArrange();
            }
        }
        void RemoveRight(Run t)
        {

            LinkedList<Run> tobeRemoveTextRuns = CollectRightRuns(t);
            LinkedListNode<Run> curNode = tobeRemoveTextRuns.First;
            LocalSuspendLineReArrange();
            while (curNode != null)
            {
                Remove(curNode.Value);
                curNode = curNode.Next;
            }
            LocalResumeLineReArrange();
        }

        LinkedList<Run> CollectLeftRuns(Run t)
        {

            LinkedList<Run> colllectRun = new LinkedList<Run>();
            foreach (Run r in GetVisualElementForward(this.FirstRun, t))
            {
                colllectRun.AddLast(r);
            }
            return colllectRun;
        }
        LinkedList<Run> CollectRightRuns(Run t)
        {

            LinkedList<Run> colllectRun = new LinkedList<Run>();
            foreach (Run r in _textFlowLayer.TextRunForward(t, this.LastRun))
            {
                colllectRun.AddLast(r);
            }
            return colllectRun;
        }
        public void ReplaceAll(IEnumerable<Run> textRuns)
        {
            this.Clear();
            this.LocalSuspendLineReArrange();
            if (textRuns != null)
            {
                foreach (Run r in textRuns)
                {
                    this.AddLast(r);
                }
            }

            this.LocalResumeLineReArrange();
        }
    }
}
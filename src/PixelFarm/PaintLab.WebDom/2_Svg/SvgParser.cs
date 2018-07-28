﻿//MIT, 2018-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.3
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
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
//
// SVG parser.
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

using LayoutFarm.HtmlBoxes; //temp
using LayoutFarm.Svg;
using LayoutFarm.WebDom;
using LayoutFarm.WebDom.Parser;
using LayoutFarm.WebLexer;
 


namespace PaintLab.Svg
{




    public abstract class XmlParserBase
    {
        int parseState = 0;
        protected TextSnapshot _textSnapshot;
        MyXmlLexer myXmlLexer = new MyXmlLexer();
        string waitingAttrName;
        string currentNodeName;
        Stack<string> openEltStack = new Stack<string>();

        TextSpan nodeNamePrefix;
        bool hasNodeNamePrefix;

        TextSpan attrName;
        TextSpan attrPrefix;
        bool hasAttrPrefix;

        protected struct TextSpan
        {
            public readonly int startIndex;
            public readonly int len;
            public TextSpan(int startIndex, int len)
            {
                this.startIndex = startIndex;
                this.len = len;
            }
#if DEBUG
            public override string ToString()
            {
                return startIndex + "," + len;
            }
#endif
            public static readonly TextSpan Empty = new TextSpan();
        }


        public XmlParserBase()
        {
            myXmlLexer.LexStateChanged += MyXmlLexer_LexStateChanged;
        }

        private void MyXmlLexer_LexStateChanged(XmlLexerEvent lexEvent, int startIndex, int len)
        {

            switch (lexEvent)
            {
                default:
                    {
                        throw new NotSupportedException();
                    }
                case XmlLexerEvent.VisitOpenAngle:
                    {
                        //enter new context
                    }
                    break;
                case XmlLexerEvent.CommentContent:
                    {

                    }
                    break;
                case XmlLexerEvent.NamePrefix:
                    {
                        //name prefix of 

#if DEBUG
                        string testStr = _textSnapshot.Substring(startIndex, len);
#endif

                        switch (parseState)
                        {
                            default:
                                throw new NotSupportedException();
                            case 0:
                                nodeNamePrefix = new TextSpan(startIndex, len);
                                hasNodeNamePrefix = true;
                                break;
                            case 1:
                                //attribute part
                                attrPrefix = new TextSpan(startIndex, len);
                                hasAttrPrefix = true;
                                break;
                            case 2: //   </a
                                nodeNamePrefix = new TextSpan(startIndex, len);
                                hasNodeNamePrefix = true;
                                break;
                        }
                    }
                    break;
                case XmlLexerEvent.FromContentPart:
                    {

                        //text content of the element 
                        OnTextNode(new TextSpan(startIndex, len));
                    }
                    break;
                case XmlLexerEvent.AttributeValueAsLiteralString:
                    {
                        //assign value and add to parent
                        //string attrValue = textSnapshot.Substring(startIndex, len);
                        if (parseState == 11)
                        {
                            //doctype node
                            //add to its parameter
                        }
                        else
                        {
                            //add value to current attribute node
                            parseState = 1;
                            OnAttribute(attrName, new TextSpan(startIndex, len));
                        }
                    }
                    break;
                case XmlLexerEvent.Attribute:
                    {
                        //create attribute node and wait for its value
                        attrName = new TextSpan(startIndex, len);
                        //string attrName = textSnapshot.Substring(startIndex, len);
                    }
                    break;
                case XmlLexerEvent.NodeNameOrAttribute:
                    {
                        //the lexer dose not store state of element name or attribute name
                        //so we use parseState to decide here

                        string name = _textSnapshot.Substring(startIndex, len);
                        switch (parseState)
                        {
                            case 0:
                                {
                                    //element name=> create element 
                                    if (currentNodeName != null)
                                    {
                                        OnEnteringElementBody();
                                        openEltStack.Push(currentNodeName);
                                    }

                                    currentNodeName = name;
                                    //enter new node                                   
                                    OnVisitNewElement(new TextSpan(startIndex, len));

                                    parseState = 1; //enter attribute 
                                    waitingAttrName = null;
                                }
                                break;
                            case 1:
                                {
                                    //wait for attr value 
                                    if (waitingAttrName != null)
                                    {
                                        //push waiting attr
                                        //create new attribute

                                        //eg. in html
                                        //but this is not valid in Xml

                                        throw new NotSupportedException();
                                    }
                                    waitingAttrName = name;
                                }
                                break;
                            case 2:
                                {
                                    //****
                                    //node name after open slash  </
                                    //TODO: review here,avoid direct string comparison
                                    if (currentNodeName == name)
                                    {
                                        OnExitingElementBody();

                                        if (openEltStack.Count > 0)
                                        {
                                            waitingAttrName = null;
                                            currentNodeName = openEltStack.Pop();
                                        }
                                        parseState = 3;
                                    }
                                    else
                                    {
                                        //eg. in html
                                        //but this is not valid in Xml
                                        //not match open-close tag
                                        throw new NotSupportedException();
                                    }
                                }
                                break;
                            case 4:
                                {
                                    //attribute value as id ***
                                    //eg. in Html, but not for general Xml
                                    throw new NotSupportedException();
                                }

                            case 10:
                                {
                                    //eg <! 
                                    parseState = 11;
                                }
                                break;
                            case 11:
                                {
                                    //comment node

                                }
                                break;
                            default:
                                {
                                }
                                break;
                        }
                    }
                    break;
                case XmlLexerEvent.VisitCloseAngle:
                    {
                        //close angle of current new node
                        //enter into its content 
                        if (parseState == 11)
                        {
                            //add doctype to html 
                        }
                        else
                        {

                        }
                        waitingAttrName = null;
                        parseState = 0;
                    }
                    break;
                case XmlLexerEvent.VisitAttrAssign:
                    {

                        parseState = 4;
                    }
                    break;
                case XmlLexerEvent.VisitOpenSlashAngle:
                    {
                        parseState = 2;
                    }
                    break;
                case XmlLexerEvent.VisitCloseSlashAngle:
                    {
                        //   />
                        if (openEltStack.Count > 0)
                        {
                            OnExitingElementBody();
                            //curTextNode = null;
                            //curAttr = null;
                            waitingAttrName = null;
                            currentNodeName = openEltStack.Pop();
                        }
                        parseState = 0;
                    }
                    break;
                case XmlLexerEvent.VisitOpenAngleExclimation:
                    {
                        parseState = 10;
                    }
                    break;

            }
        }

        public virtual void ParseDocument(TextSnapshot textSnapshot)
        {
            this._textSnapshot = textSnapshot;


            OnBegin();
            //reset
            openEltStack.Clear();
            waitingAttrName = null;
            currentNodeName = null;
            parseState = 0;

            //

            myXmlLexer.BeginLex();
            myXmlLexer.Analyze(textSnapshot);
            myXmlLexer.EndLex();

            OnFinish();
        }

        protected virtual void OnBegin()
        {

        }
        public virtual void OnFinish()
        {

        }


        //-------------------
        protected virtual void OnTextNode(TextSpan text) { }
        protected virtual void OnAttribute(TextSpan localAttr, TextSpan value) { }
        protected virtual void OnAttribute(TextSpan ns, TextSpan localAttr, TextSpan value) { }

        protected virtual void OnVisitNewElement(TextSpan ns, TextSpan localName) { }
        protected virtual void OnVisitNewElement(TextSpan localName) { }

        protected virtual void OnEnteringElementBody() { }
        protected virtual void OnExitingElementBody() { }
    }


    public enum WellknownSvgElementName
    {
        Unknown,
        /// <summary>
        /// svg  
        /// </summary>
        Svg,
        /// <summary>
        /// g
        /// </summary>
        Group,
        /// <summary>
        /// path
        /// </summary>
        Path,
        /// <summary>
        /// defs
        /// </summary>
        Defs,
        /// <summary>
        /// line
        /// </summary>
        Line,
        /// <summary>
        /// polyline
        /// </summary>
        Polyline,
        /// <summary>
        /// polygon
        /// </summary>
        Polygon,
        /// <summary>
        /// title
        /// </summary>
        Title,
        /// <summary>
        /// rect
        /// </summary>
        Rect,
        /// <summary>
        /// ellipse
        /// </summary>
        Ellipse,
        /// <summary>
        /// circle
        /// </summary>
        Circle,
        /// <summary>
        /// clipPath
        /// </summary>
        ClipPath,

        Gradient,
        Image,
    }


    public class SvgElement
    {
        readonly WellknownSvgElementName _wellknownName;
        readonly string _unknownElemName;

        public SvgVisualSpec _visualSpec = new SvgVisualSpec();

        List<SvgElement> _childNodes = new List<SvgElement>();

        object _controller;

        public SvgElement(WellknownSvgElementName wellknownName, SvgVisualSpec visualspec = null)
        {
            _wellknownName = wellknownName;
            if (visualspec == null)
            {
                visualspec = new SvgVisualSpec();
            }
            _visualSpec = visualspec;
        }
        public SvgElement(WellknownSvgElementName wellknownName, string name)
        {
            _wellknownName = wellknownName;
            _unknownElemName = name;
        }
        protected void SetController(object controller)
        {
            _controller = controller;
        }

        public static object UnsafeGetController(SvgElement elem)
        {
            return elem._controller;
        }

        public WellknownSvgElementName WellknowElemName { get { return _wellknownName; } }

        public string ElemName
        {
            get
            {
                switch (_wellknownName)
                {
                    default:
                        throw new NotSupportedException();
                    case WellknownSvgElementName.Unknown:
                        return _unknownElemName;
                    case WellknownSvgElementName.Circle: return "circle";
                    case WellknownSvgElementName.ClipPath: return "clipPath";
                    case WellknownSvgElementName.Rect: return "rect";
                    case WellknownSvgElementName.Path: return "path";
                    case WellknownSvgElementName.Polygon: return "polygon";
                    case WellknownSvgElementName.Polyline: return "polyline";
                    case WellknownSvgElementName.Line: return "line";
                    case WellknownSvgElementName.Defs: return "defs";
                    case WellknownSvgElementName.Title: return "title";
                }
            }
        }
        public virtual void AddElement(SvgElement elem)
        {
            _childNodes.Add(elem);
        }
        public virtual void AddChild(SvgElement elem)
        {
            _childNodes.Add(elem);
        }
        public int ChildCount
        {
            get { return _childNodes.Count; }
        }
        public SvgElement GetChild(int index)
        {
            return _childNodes[index];
        }
    }

    public interface ISvgDocBuilder
    {
        void OnBegin();
        void OnVisitNewElement(string elemName);

        void OnAttribute(string attrName, string value);
        void OnEnteringElementBody();
        void OnExtingElementBody();
        void OnEnd();
    }
    //----------------------
    public class SvgHitChain
    {
        float rootGlobalX;
        float rootGlobalY;
        List<SvgHitInfo> svgList = new List<SvgHitInfo>();
        public SvgHitChain()
        {
        }
        public void AddHit(SvgElement svg, float x, float y)
        {
            svgList.Add(new SvgHitInfo(svg, x, y));
        }
        public int Count
        {
            get
            {
                return this.svgList.Count;
            }
        }
        public SvgHitInfo GetHitInfo(int index)
        {
            return this.svgList[index];
        }
        public SvgHitInfo GetLastHitInfo()
        {
            return this.svgList[svgList.Count - 1];
        }
        public void Clear()
        {
            this.rootGlobalX = this.rootGlobalY = 0;
            this.svgList.Clear();
        }
        public void SetRootGlobalPosition(float x, float y)
        {
            this.rootGlobalX = x;
            this.rootGlobalY = y;
        }
    }


    public struct SvgHitInfo
    {
        public readonly SvgElement svg;
        public readonly float x;
        public readonly float y;
        public SvgHitInfo(SvgElement svg, float x, float y)
        {
            this.svg = svg;
            this.x = x;
            this.y = y;
        }
    }

    public class SvgDocument
    {
        SvgElement _rootElement = new SvgElement(WellknownSvgElementName.Svg);
        public SvgElement CreateElement(string elemName)
        {
            //------
            switch (elemName)
            {
                default:
#if DEBUG
                    Console.WriteLine("svg unimplemented element: " + elemName);
#endif
                    return new SvgElement(WellknownSvgElementName.Unknown, elemName);
                case "defs":
                    return new SvgElement(WellknownSvgElementName.Defs);
                case "clipPath":
                    return new SvgElement(WellknownSvgElementName.ClipPath);
                case "svg":
                    return new SvgElement(WellknownSvgElementName.Svg);
                case "g":
                    return new SvgElement(WellknownSvgElementName.Group);
                case "title":
                    return new SvgElement(WellknownSvgElementName.Title);
                case "rect":
                    return new SvgElement(WellknownSvgElementName.Rect);
                case "line":
                    return new SvgElement(WellknownSvgElementName.Line);
                case "polyline":
                    return new SvgElement(WellknownSvgElementName.Polyline);
                case "polygon":
                    return new SvgElement(WellknownSvgElementName.Polygon);
                case "path":
                    return new SvgElement(WellknownSvgElementName.Path, new SvgPathSpec());
            }
        }

        public SvgElement Root
        {
            get { return _rootElement; }
        }
    }

    public class SvgDocBuilder : ISvgDocBuilder
    {
        Stack<SvgElement> _elems = new Stack<SvgElement>();
        CssParser _cssParser = new CssParser();

        SvgElement _currentElem;
        SvgDocument _svgDoc;

        public SvgDocBuilder()
        {
            
        }
        public SvgDocument ResultDocument
        {
            get { return _svgDoc; }
        }
        public void OnBegin()
        {
            _elems.Clear();
            _svgDoc = new SvgDocument();
            _currentElem = _svgDoc.Root;             
        }
        public void OnVisitNewElement(string elemName)
        {
            SvgElement newElem = _svgDoc.CreateElement(elemName);
            if (_currentElem != null)
            {
                _elems.Push(_currentElem);
                _currentElem.AddElement(newElem);
            }
            _currentElem = newElem;

        }

        static void AddClipPathLink(SvgVisualSpec spec, string value)
        {
            //eg. url(#aaa)
            if (value.StartsWith("url("))
            {
                int endAt = value.IndexOf(')', 4);
                if (endAt > -1)
                {
                    //get value 
                    string url_value = value.Substring(4, endAt - 4);
                    if (url_value.StartsWith("#"))
                    {
                        spec.ClipPathLink = new SvgAttributeLink(SvgAttributeLinkKind.Id, url_value.Substring(1));
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            else
            {

            }

        }
        void AddStyle(SvgVisualSpec spec, string cssStyle)
        {
            if (!String.IsNullOrEmpty(cssStyle))
            {


                //***                
                CssRuleSet cssRuleSet = _cssParser.ParseCssPropertyDeclarationList(cssStyle.ToCharArray());

                foreach (CssPropertyDeclaration propDecl in cssRuleSet.GetAssignmentIter())
                {
                    switch (propDecl.UnknownRawName)
                    {

                        default:
                            break;
                        case "fill":
                            {

                                int valueCount = propDecl.ValueCount;
                                //1
                                string value = propDecl.GetPropertyValue(0).ToString();
                                if (value != "none")
                                {
                                    //spec.FillColor = ConvToActualColor(CssValueParser2.GetActualColor(value));
                                    spec.FillColor = CssValueParser2.ParseCssColor(value);
                                }
                            }
                            break;
                        case "fill-opacity":
                            {
                                //TODO:
                                //adjust fill opacity
                            }
                            break;
                        case "stroke-width":
                            {
                                int valueCount = propDecl.ValueCount;
                                //1
                                string value = propDecl.GetPropertyValue(0).ToString();

                                spec.StrokeWidth = UserMapUtil.ParseGenericLength(value);
                            }
                            break;
                        case "stroke":
                            {
                                //TODO:
                                //if (attr.Value != "none")
                                //{
                                //    spec.StrokeColor = ConvToActualColor(CssValueParser2.GetActualColor(attr.Value));
                                //}
                            }
                            break;
                        case "stroke-linecap":
                            //set line-cap and line join again
                            //TODO:
                            break;
                        case "stroke-linejoin":
                            //TODO:
                            break;
                        case "stroke-miterlimit":
                            //TODO:
                            break;
                        case "stroke-opacity":
                            //TODO:
                            break;
                        case "transform":
                            {
                                ////parse trans
                                //ParseTransform(attr.Value, spec);
                            }
                            break;
                    }
                }
            }
        }

        public void OnAttribute(string attrName, string value)
        {
            SvgVisualSpec spec = _currentElem._visualSpec;
            switch (attrName)
            {
                default:
                    {
                        //unknown attribute
                        //some specfic attr for some elem
                        if (_currentElem.WellknowElemName == WellknownSvgElementName.Path)
                        {
                            if (attrName == "d")
                            {
                                SvgPathSpec pathSpec = (SvgPathSpec)spec;
                                pathSpec.D = value;
                            }
                        }
                    }
                    break;
                case "class":
                    spec.Class = value;
                    break;
                case "id":
                    spec.Id = value;
                    break;
                case "style":
                    AddStyle(spec, value);
                    break;
                case "clip-path":
                    AddClipPathLink(spec, value);
                    break;
                case "fill":
                    {
                        if (value != "none")
                        {
                            //spec.FillColor = ConvToActualColor(CssValueParser2.GetActualColor(value));
                            spec.FillColor = CssValueParser2.ParseCssColor(value);
                        }
                    }
                    break;
                case "fill-opacity":
                    {
                        //adjust fill opacity
                        //0f-1f?

                    }
                    break;
                case "stroke-width":
                    {
                        spec.StrokeWidth = UserMapUtil.ParseGenericLength(value);
                    }
                    break;
                case "stroke":
                    {
                        if (value != "none")
                        {
                            //spec.StrokeColor = ConvToActualColor(CssValueParser2.GetActualColor(value));
                            spec.StrokeColor = CssValueParser2.ParseCssColor(value);
                        }
                    }
                    break;
                case "stroke-linecap":
                    //set line-cap and line join again

                    break;
                case "stroke-linejoin":

                    break;
                case "stroke-miterlimit":

                    break;
                case "stroke-opacity":

                    break;
                case "transform":
                    {
                        //parse trans
                        ParseTransform(value, spec);
                    }
                    break;
            }

        }
        public void OnEnteringElementBody()
        {

        }

        public void OnExtingElementBody()
        {
            if (_elems.Count > 0)
            {
                _currentElem = _elems.Pop();
            }
        }

        public void OnEnd()
        {


        }

        static void ParseTransform(string value, SvgVisualSpec spec)
        {
            //TODO: ....

            int openParPos = value.IndexOf('(');
            if (openParPos > -1)
            {
                string right = value.Substring(openParPos + 1, value.Length - (openParPos + 1)).Trim();
                string left = value.Substring(0, openParPos);
                switch (left)
                {
                    default:
                        break;
                    case "matrix":
                        {
                            //read matrix args  
                            spec.Transform = new SvgTransformMatrix(ParseMatrixArgs(right));
                        }
                        break;
                    case "translate":
                        {
                            //translate matrix
                            float[] matrixArgs = ParseMatrixArgs(right);
                            spec.Transform = new SvgTranslate(matrixArgs[0], matrixArgs[1]);
                        }
                        break;
                    case "rotate":
                        {
                            float[] matrixArgs = ParseMatrixArgs(right);
                            spec.Transform = new SvgRotate(matrixArgs[0]);
                        }
                        break;
                    case "scale":
                        {
                            float[] matrixArgs = ParseMatrixArgs(right);
                            spec.Transform = new SvgScale(matrixArgs[0], matrixArgs[1]);
                        }
                        break;
                    case "skewX":
                        {
                            float[] matrixArgs = ParseMatrixArgs(right);
                            spec.Transform = new SvgSkew(matrixArgs[0], 0);
                        }
                        break;
                    case "skewY":
                        {
                            float[] matrixArgs = ParseMatrixArgs(right);
                            spec.Transform = new SvgSkew(0, matrixArgs[1]);
                        }
                        break;
                }
            }
            else
            {
                //?
            }
        }

        static float[] ParseMatrixArgs(string matrixTransformArgs)
        {


            int close_paren = matrixTransformArgs.IndexOf(')');
            matrixTransformArgs = matrixTransformArgs.Substring(0, close_paren);
            string[] elem_string_args = matrixTransformArgs.Split(',');
            int j = elem_string_args.Length;
            float[] elem_values = new float[j];
            for (int i = 0; i < j; ++i)
            {
                elem_values[i] = float.Parse(elem_string_args[i]);
            }
            return elem_values;
        }

    }


    public class SvgParser : XmlParserBase
    {

        ISvgDocBuilder _svgDocBuilder;

        public SvgParser(ISvgDocBuilder svgDocBuilder)
        {
            _svgDocBuilder = svgDocBuilder;
        }

        protected override void OnBegin()
        {
            _svgDocBuilder.OnBegin();
            base.OnBegin();
        }
        public void ReadSvgString(string svgString)
        {
            ParseDocument(new TextSnapshot(svgString));
        }
        public void ReadSvgCharBuffer(char[] svgBuffer)
        {
            ParseDocument(new TextSnapshot(svgBuffer));
        }
        public void ReadSvgFile(string svgFileName)
        {
            ReadSvgString(System.IO.File.ReadAllText(svgFileName));
        }
        protected override void OnVisitNewElement(TextSpan ns, TextSpan localName)
        {
            throw new NotSupportedException();
        }
        protected override void OnVisitNewElement(TextSpan localName)
        {
            string elemName = _textSnapshot.Substring(localName.startIndex, localName.len);
            _svgDocBuilder.OnVisitNewElement(elemName);
        }

        protected override void OnAttribute(TextSpan localAttr, TextSpan value)
        {
            string attrLocalName = _textSnapshot.Substring(localAttr.startIndex, localAttr.len);
            string attrValue = _textSnapshot.Substring(value.startIndex, value.len);
            _svgDocBuilder.OnAttribute(attrLocalName, attrValue);
        }
        protected override void OnAttribute(TextSpan ns, TextSpan localAttr, TextSpan value)
        {
            string attrLocalName = _textSnapshot.Substring(localAttr.startIndex, localAttr.len);
            string attrValue = _textSnapshot.Substring(value.startIndex, value.len);
            _svgDocBuilder.OnAttribute(attrLocalName, attrValue);

        }
        protected override void OnEnteringElementBody()
        {
            _svgDocBuilder.OnEnteringElementBody();
        }
        protected override void OnExitingElementBody()
        {
            _svgDocBuilder.OnExtingElementBody();
        }
    }

}
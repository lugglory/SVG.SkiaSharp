#if !NO_SDC
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Svg
{
    public abstract partial class SvgTextBase : SvgVisualElement
    {
        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        public override SKRect Bounds
        {
            get
            {
                var path = this.Path(null);
                if (path == null) return SKRect.Empty;
                
                using (var combinedPath = new SKPath(path))
                {
                    foreach (var elem in this.Children.OfType<SvgVisualElement>())
                    {
                        if (elem is SvgTextSpan span && string.IsNullOrWhiteSpace(span.Text))
                            continue;

                        var childPath = elem.Path(null);
                        if (childPath != null) combinedPath.AddPath(childPath);
                    }
                    
                    if (Transforms == null || Transforms.Count == 0)
                        return combinedPath.Bounds;

                    var matrix = Transforms.GetMatrix();
                    return matrix.MapRect(combinedPath.Bounds);
                }
            }
        }

        protected internal override void RenderFillAndStroke(ISvgRenderer renderer)
        {
            base.RenderFillAndStroke(renderer);
            RenderChildren(renderer);
        }

        internal virtual IEnumerable<ISvgNode> GetContentNodes()
        {
            return (this.Nodes == null || this.Nodes.Count < 1 ? this.Children.OfType<ISvgNode>().Where(o => !(o is ISvgDescriptiveElement)) : this.Nodes);
        }
        
        protected virtual SKPath GetBaselinePath(ISvgRenderer renderer)
        {
            return null;
        }
        
        protected virtual float GetAuthorPathLength()
        {
            return 0;
        }

        private SKPath _path;

        /// <summary>
        /// Gets the <see cref="SKPath"/> for this element.
        /// </summary>
        public override SKPath Path(ISvgRenderer renderer)
        {
            var nodeCount = GetContentNodes().Count(x => x is SvgContentNode contentNode &&
                                                         string.IsNullOrEmpty(contentNode.Content.Trim(new[]
                                                             { '\r', '\n', '\t' })));

            if (_path == null || IsPathDirty || nodeCount == 1)
            {
                SetPath(new TextDrawingState(renderer, this));
            }
            return _path;
        }

        private void SetPath(TextDrawingState state)
        {
            SetPath(state, true);
        }

        private void SetPath(TextDrawingState state, bool doMeasurements)
        {
            TextDrawingState origState = null;
            bool alignOnBaseline = state.BaselinePath != null && (this.TextAnchor == SvgTextAnchor.Middle || this.TextAnchor == SvgTextAnchor.End);

            if (doMeasurements)
            {
                if (this.TextLength != SvgUnit.None)
                {
                    origState = state.Clone();
                }
                else if (alignOnBaseline)
                {
                    origState = state.Clone();
                    state.BaselinePath = null;
                }
            }

            foreach (var node in GetContentNodes())
            {
                if (node is SvgTextBase textNode)
                {
                    TextDrawingState newState = new TextDrawingState(state, textNode);
                    textNode.SetPath(newState);
                    state.NumChars += newState.NumChars;
                    state.Current = newState.Current;
                }
                else if (!string.IsNullOrEmpty(node.Content))
                {
                    state.DrawString(PrepareText(node.Content));
                }
            }

            var path = state.GetPath() ?? new SKPath();

            if (doMeasurements)
            {
                if (this.TextLength != SvgUnit.None)
                {
                    var specLength = this.TextLength.ToDeviceValue(state.Renderer, UnitRenderingType.Horizontal, this);
                    var actLength = state.TextBounds.Width;
                    var diff = (actLength - specLength);
                    if (Math.Abs(diff) > 1.5)
                    {
                        if (this.LengthAdjust == SvgTextLengthAdjust.Spacing)
                        {
                            if (this.X.Count < 2)
                            {
                                var numCharDiff = state.NumChars - origState.NumChars - 1;
                                if (numCharDiff != 0)
                                {
                                    origState.LetterSpacingAdjust = -1 * diff / numCharDiff;
                                    SetPath(origState, false);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            var matrix = SKMatrix.CreateTranslation(-1 * state.TextBounds.Left, 0);
                            matrix = matrix.PostConcat(SKMatrix.CreateScale(specLength / actLength, 1));
                            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(state.TextBounds.Left, 0));
                            path.Transform(matrix);
                        }
                    }
                }
                else if (alignOnBaseline)
                {
                    var bounds = path.Bounds;
                    if (this.TextAnchor == SvgTextAnchor.Middle)
                    {
                        origState.StartOffsetAdjust = -1 * bounds.Width / 2;
                    }
                    else
                    {
                        origState.StartOffsetAdjust = -1 * bounds.Width;
                    }
                    SetPath(origState, false);
                    return;
                }
            }

            _path = path;
            this.IsPathDirty = false;
        }

        private class FontBoundable : ISvgBoundable
        {
            private IFontDefn _font;
            private float _width = 1;

            public FontBoundable(IFontDefn font)
            {
                _font = font;
            }
            public FontBoundable(IFontDefn font, float width)
            {
                _font = font;
                _width = width;
            }

            public SKPoint Location => SKPoint.Empty;
            public SKSize Size => new SKSize(_width, _font.Size);
            public SKRect Bounds => new SKRect(0, 0, _width, _font.Size);
        }

        private class TextDrawingState
        {
            private float _xAnchor = float.MinValue;
            private IList<SKPath> _anchoredPaths = new List<SKPath>();
            private SKPath _currPath = null;
            private SKPath _finalPath = null;
            private float _authorPathLength = 0;

            public SKPath BaselinePath { get; set; }
            public SKPoint Current { get; set; }
            public SKRect TextBounds { get; set; }
            public SvgTextBase Element { get; set; }
            public float LetterSpacingAdjust { get; set; }
            public int NumChars { get; set; }
            public TextDrawingState Parent { get; set; }
            public ISvgRenderer Renderer { get; set; }
            public float StartOffsetAdjust { get; set; }

            private TextDrawingState() { }
            public TextDrawingState(ISvgRenderer renderer, SvgTextBase element)
            {
                this.Element = element;
                this.Renderer = renderer;
                this.Current = SKPoint.Empty;
                this.TextBounds = SKRect.Empty;
                _xAnchor = 0;
                this.BaselinePath = element.GetBaselinePath(renderer);
                _authorPathLength = element.GetAuthorPathLength();
            }

            public TextDrawingState(TextDrawingState parent, SvgTextBase element)
                : this(parent.Renderer, element)
            {
                this.Parent = parent;
                this.Current = parent.Current;
                this.TextBounds = parent.TextBounds;
                this.BaselinePath = this.BaselinePath ?? parent.BaselinePath;
                if (_authorPathLength == 0)
                    _authorPathLength = parent._authorPathLength;
            }

            public SKPath GetPath()
            {
                FlushPath();
                return _finalPath;
            }

            public TextDrawingState Clone()
            {
                var result = new TextDrawingState();
                result._anchoredPaths = this._anchoredPaths.ToList();
                result.BaselinePath = this.BaselinePath;
                result._xAnchor = this._xAnchor;
                result.Current = this.Current;
                result.TextBounds = this.TextBounds;
                result.Element = this.Element;
                result.NumChars = this.NumChars;
                result.Parent = this.Parent;
                result.Renderer = this.Renderer;
                return result;
            }

            public void DrawString(string value)
            {
                var xAnchors = GetValues(value.Length, e => e._x, UnitRenderingType.HorizontalOffset);
                var yAnchors = GetValues(value.Length, e => e._y, UnitRenderingType.VerticalOffset);
                
                using (var font = this.Element.GetFont(this.Renderer, null))
                {
                    if (font == null) return;
                    
                    var fontBaselineHeight = font.Ascent(this.Renderer);
                    PathStatistics pathStats = null;
                    var pathScale = 1.0f;
                    if (BaselinePath != null)
                    {
                        pathStats = new PathStatistics(BaselinePath);
                        if (_authorPathLength > 0) pathScale = _authorPathLength / (float)pathStats.TotalLength;
                    }

                    IList<float> xOffsets;
                    IList<float> yOffsets;
                    IList<float> rotations;
                    float baselineShift = 0.0f;

                    try
                    {
                        this.Renderer.SetBoundable(new FontBoundable(font, (float)(pathStats == null ? 1 : pathStats.TotalLength)));
                        xOffsets = GetValues(value.Length, e => e._dx, UnitRenderingType.Horizontal);
                        yOffsets = GetValues(value.Length, e => e._dy, UnitRenderingType.Vertical);
                        if (StartOffsetAdjust != 0.0f)
                        {
                            if (xOffsets.Count < 1) xOffsets.Add(StartOffsetAdjust);
                            else xOffsets[0] += StartOffsetAdjust;
                        }

                        if (this.Element.LetterSpacing.Value != 0.0f || this.Element.WordSpacing.Value != 0.0f || this.LetterSpacingAdjust != 0.0f)
                        {
                            var spacing = this.Element.LetterSpacing.ToDeviceValue(this.Renderer, UnitRenderingType.Horizontal, this.Element) + this.LetterSpacingAdjust;
                            var wordSpacing = this.Element.WordSpacing.ToDeviceValue(this.Renderer, UnitRenderingType.Horizontal, this.Element);
                            if (this.Parent == null && this.NumChars == 0 && xOffsets.Count < 1) xOffsets.Add(0);
                            for (int i = (this.Parent == null && this.NumChars == 0 ? 1 : 0); i < value.Length; i++)
                            {
                                if (i >= xOffsets.Count) xOffsets.Add(spacing + (char.IsWhiteSpace(value[i]) ? wordSpacing : 0));
                                else xOffsets[i] += spacing + (char.IsWhiteSpace(value[i]) ? wordSpacing : 0);
                            }
                        }

                        rotations = GetValues(value.Length, e => e._rotations);

                        var baselineShiftText = Element.BaselineShift.Trim().ToLower();
                        if (string.IsNullOrEmpty(baselineShiftText)) baselineShiftText = "baseline";

                        switch (baselineShiftText)
                        {
                            case "baseline": break;
                            case "sub": baselineShift = new SvgUnit(SvgUnitType.Ex, 1).ToDeviceValue(this.Renderer, UnitRenderingType.Vertical, this.Element); break;
                            case "super": baselineShift = -1f * new SvgUnit(SvgUnitType.Ex, 1).ToDeviceValue(this.Renderer, UnitRenderingType.Vertical, this.Element); break;
                            default:
                                var convert = new SvgUnitConverter();
                                var shiftUnit = (SvgUnit)convert.ConvertFromInvariantString(baselineShiftText);
                                baselineShift = -1f * shiftUnit.ToDeviceValue(this.Renderer, UnitRenderingType.Vertical, this.Element);
                                break;
                        }

                        if (baselineShift != 0.0f)
                        {
                            if (yOffsets.Any()) yOffsets[0] += baselineShift;
                            else yOffsets.Add(baselineShift);
                        }
                    }
                    finally
                    {
                        this.Renderer.PopBoundable();
                    }

                    var xTextStart = Current.X;
                    var yPos = Current.Y;
                    for (int i = 0; i < xAnchors.Count - 1; i++)
                    {
                        FlushPath();
                        _xAnchor = xAnchors[i] + (xOffsets.Count > i ? xOffsets[i] : 0);
                        EnsurePath();
                        yPos = (yAnchors.Count > i ? yAnchors[i] : yPos) + (yOffsets.Count > i ? yOffsets[i] : 0);

                        if (xTextStart == Current.X) xTextStart = _xAnchor;
                        DrawStringOnCurrPath(value[i].ToString(), font, new SKPoint(_xAnchor, yPos),
                            fontBaselineHeight, (rotations.Count > i ? rotations[i] : rotations.LastOrDefault()));
                    }

                    var renderChar = 0;
                    var xPos = this.Current.X;
                    if (xAnchors.Any())
                    {
                        FlushPath();
                        renderChar = xAnchors.Count - 1;
                        xPos = xAnchors.Last();
                        _xAnchor = xPos;
                    }
                    EnsurePath();

                    var lastIndividualChar = renderChar + Math.Max(Math.Max(Math.Max(Math.Max(xOffsets.Count, yOffsets.Count), yAnchors.Count), rotations.Count) - renderChar - 1, 0);
                    if (rotations.LastOrDefault() != 0.0f || pathStats != null) lastIndividualChar = value.Length;
                    if (lastIndividualChar > renderChar)
                    {
                        var charBounds = font.MeasureCharacters(this.Renderer, value.Substring(renderChar, Math.Min(lastIndividualChar + 1, value.Length) - renderChar));
                        SKPoint pathPoint;
                        float rotation;
                        float halfWidth;
                        for (int i = renderChar; i < lastIndividualChar; i++)
                        {
                            xPos += (float)pathScale * (xOffsets.Count > i ? xOffsets[i] : 0) + (charBounds[i - renderChar].Left - (i == renderChar ? 0 : charBounds[i - renderChar - 1].Left));
                            yPos = (yAnchors.Count > i ? yAnchors[i] : yPos) + (yOffsets.Count > i ? yOffsets[i] : 0);
                            if (pathStats == null)
                            {
                                if (xTextStart == Current.X) xTextStart = xPos;
                                DrawStringOnCurrPath(value[i].ToString(), font, new SKPoint(xPos, yPos),
                                    fontBaselineHeight, (rotations.Count > i ? rotations[i] : rotations.LastOrDefault()));
                            }
                            else
                            {
                                xPos = Math.Max(xPos, 0);
                                halfWidth = charBounds[i - renderChar].Width / 2;
                                if (pathStats.OffsetOnPath(xPos + halfWidth))
                                {
                                    pathStats.LocationAngleAtOffset(xPos + halfWidth, out pathPoint, out rotation);
                                    pathPoint = new SKPoint((float)(pathPoint.X - halfWidth * Math.Cos(rotation * Math.PI / 180) - (float)pathScale * yPos * Math.Sin(rotation * Math.PI / 180)),
                                        (float)(pathPoint.Y - halfWidth * Math.Sin(rotation * Math.PI / 180) + (float)pathScale * yPos * Math.Cos(rotation * Math.PI / 180)));
                                    if (xTextStart == Current.X) xTextStart = pathPoint.X;
                                    DrawStringOnCurrPath(value[i].ToString(), font, pathPoint, fontBaselineHeight, rotation);
                                }
                            }
                        }

                        if (lastIndividualChar < value.Length) xPos += charBounds[charBounds.Count - 1].Left - charBounds[charBounds.Count - 2].Left;
                        else xPos += charBounds.Last().Width;
                    }

                    if (lastIndividualChar < value.Length)
                    {
                        xPos += (xOffsets.Count > lastIndividualChar ? xOffsets[lastIndividualChar] : 0);
                        yPos = (yAnchors.Count > lastIndividualChar ? yAnchors[lastIndividualChar] : yPos) +
                               (yOffsets.Count > lastIndividualChar ? yOffsets[lastIndividualChar] : 0);
                        if (xTextStart == Current.X) xTextStart = xPos;
                        DrawStringOnCurrPath(value.Substring(lastIndividualChar), font, new SKPoint(xPos, yPos),
                            fontBaselineHeight, rotations.LastOrDefault());
                        var bounds = font.MeasureString(this.Renderer, value.Substring(lastIndividualChar));
                        xPos += bounds.Width;
                    }

                    NumChars += value.Length;
                    this.Current = new SKPoint(xPos, yPos - baselineShift);
                    this.TextBounds = new SKRect(xTextStart, 0, this.Current.X, 0);
                    
                    pathStats?.Dispose();
                }
            }

            private void DrawStringOnCurrPath(string value, IFontDefn font, SKPoint location, float fontBaselineHeight, float rotation)
            {
                var drawPath = _currPath;
                if (rotation != 0.0f) drawPath = new SKPath();
                font.AddStringToPath(this.Renderer, drawPath, value, new SKPoint(location.X, location.Y - fontBaselineHeight));
                if (rotation != 0.0f && drawPath.PointCount > 0)
                {
                    var matrix = SKMatrix.CreateTranslation(-1 * location.X, -1 * location.Y);
                    matrix = matrix.PostConcat(SKMatrix.CreateRotationDegrees(rotation));
                    matrix = matrix.PostConcat(SKMatrix.CreateTranslation(location.X, location.Y));
                    drawPath.Transform(matrix);
                    _currPath.AddPath(drawPath);
                }
            }

            private void EnsurePath()
            {
                if (_currPath == null)
                {
                    _currPath = new SKPath();
                    var currState = this;
                    while (currState != null && currState._xAnchor <= float.MinValue)
                    {
                        currState = currState.Parent;
                    }
                    currState?._anchoredPaths.Add(_currPath);
                }
            }

            private void FlushPath()
            {
                if (_currPath != null)
                {
                    if (_currPath.PointCount < 1)
                    {
                        _anchoredPaths.Clear();
                        _xAnchor = float.MinValue;
                        _currPath = null;
                        return;
                    }

                    if (_xAnchor > float.MinValue)
                    {
                        float minX = float.MaxValue;
                        float maxX = float.MinValue;
                        foreach (var path in _anchoredPaths)
                        {
                            var bounds = path.Bounds;
                            if (bounds.Left < minX) minX = bounds.Left;
                            if (bounds.Right > maxX) maxX = bounds.Right;
                        }

                        var xOffset = 0f;
                        switch (Element.TextAnchor)
                        {
                            case SvgTextAnchor.Middle:
                                if (_anchoredPaths.Count == 1) xOffset -= this.TextBounds.Width / 2;
                                else xOffset -= (maxX - minX) / 2;
                                break;
                            case SvgTextAnchor.End:
                                if (_anchoredPaths.Count == 1) xOffset -= this.TextBounds.Width;
                                else xOffset -= (maxX - minX);
                                break;
                        }

                        if (xOffset != 0)
                        {
                            var matrix = SKMatrix.CreateTranslation(xOffset, 0);
                            foreach (var path in _anchoredPaths) path.Transform(matrix);
                        }

                        _anchoredPaths.Clear();
                        _xAnchor = float.MinValue;
                    }

                    if (_finalPath == null) _finalPath = _currPath;
                    else _finalPath.AddPath(_currPath);

                    _currPath = null;
                }
            }

            private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<float>> listGetter)
            {
                var currState = this;
                int charCount = 0;
                var results = new List<float>();
                int resultCount = 0;

                while (currState != null)
                {
                    charCount += currState.NumChars;
                    results.AddRange(listGetter.Invoke(currState.Element).Skip(charCount).Take(maxCount));
                    if (results.Count > resultCount)
                    {
                        maxCount -= results.Count - resultCount;
                        charCount += results.Count - resultCount;
                        resultCount = results.Count;
                    }
                    if (maxCount < 1) return results;
                    currState = currState.Parent;
                }
                return results;
            }
            
            private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<SvgUnit>> listGetter, UnitRenderingType renderingType)
            {
                var currState = this;
                int charCount = 0;
                var results = new List<float>();
                int resultCount = 0;

                while (currState != null)
                {
                    charCount += currState.NumChars;
                    results.AddRange(listGetter.Invoke(currState.Element).Skip(charCount).Take(maxCount).Select(p => p.ToDeviceValue(currState.Renderer, renderingType, currState.Element)));
                    if (results.Count > resultCount)
                    {
                        maxCount -= results.Count - resultCount;
                        charCount += results.Count - resultCount;
                        resultCount = results.Count;
                    }
                    if (maxCount < 1) return results;
                    currState = currState.Parent;
                }
                return results;
            }
        }
    }
}
#endif
#if !NO_SDC
using System;
using SkiaSharp;
using Svg.Transforms;

namespace Svg
{
    public abstract partial class SvgElement : ISvgElement, ISvgTransformable, ICloneable, ISvgNode
    {
        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        protected internal virtual bool PushTransforms(ISvgRenderer renderer)
        {
            renderer.Save();

            var transforms = Transforms;
            if (transforms == null || transforms.Count == 0)
                return true;

            var transformMatrix = transforms.GetMatrix();
            if (transformMatrix.IsIdentity)
                return true;

            // Apply transformation
            // Assuming PostConcat behavior similar to GDI+ Append
            var current = renderer.Transform;
            var next = current.PostConcat(transformMatrix);
            renderer.Transform = next;

            return true;
        }

        /// <summary>
        /// Removes any previously applied transforms from the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that should have transforms removed.</param>
        protected internal virtual void PopTransforms(ISvgRenderer renderer)
        {
            renderer.Restore();
        }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        void ISvgTransformable.PushTransforms(ISvgRenderer renderer)
        {
            PushTransforms(renderer);
        }

        /// <summary>
        /// Removes any previously applied transforms from the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that should have transforms removed.</param>
        void ISvgTransformable.PopTransforms(ISvgRenderer renderer)
        {
            PopTransforms(renderer);
        }

        /// <summary>
        /// Transforms the given rectangle with the set transformation, if any.
        /// Can be applied to bounds calculated without considering the element transformation.
        /// </summary>
        /// <param name="bounds">The rectangle to be transformed.</param>
        /// <returns>The transformed rectangle, or the original rectangle if no transformation exists.</returns>
        protected SKRect TransformedBounds(SKRect bounds)
        {
            if (Transforms != null && Transforms.Count > 0)
            {
                var matrix = Transforms.GetMatrix();
                return matrix.MapRect(bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Renders this element to the <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that the element should use to render itself.</param>
        public void RenderElement(ISvgRenderer renderer)
        {
            Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected virtual void Render(ISvgRenderer renderer)
        {
            try
            {
                PushTransforms(renderer);
                RenderChildren(renderer);
            }
            finally
            {
                PopTransforms(renderer);
            }
        }

        /// <summary>
        /// Renders the children of this <see cref="SvgElement"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to render the child <see cref="SvgElement"/>s to.</param>
        protected virtual void RenderChildren(ISvgRenderer renderer)
        {
            foreach (var element in Children)
                element.Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        void ISvgElement.Render(ISvgRenderer renderer)
        {
            Render(renderer);
        }

        /// <summary>
        /// Recursive method to add up the paths of all children
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="path"></param>
        protected void AddPaths(SvgElement elem, SKPath path)
        {
            foreach (var child in elem.Children)
            {
                // Skip to avoid double calculate Symbol element
                if (child is SvgSymbol)
                    continue;

                if (child is SvgVisualElement visualElement)
                {
                    if (!(child is SvgGroup))
                    {
                        var childPath = visualElement.Path(null);
                        if (childPath != null)
                        {
                            // Clone path to transform it
                            using (var clonedPath = new SKPath(childPath))
                            {
                                if (child.Transforms != null)
                                {
                                    var matrix = child.Transforms.GetMatrix();
                                    clonedPath.Transform(matrix);
                                }

                                if (clonedPath.PointCount > 0)
                                    path.AddPath(clonedPath);
                            }
                        }
                    }
                }

                if (!(child is SvgPaintServer)) AddPaths(child, path);
            }
        }

        /// <summary>
        /// Recursive method to add up the paths of all children
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="renderer"></param>
        protected SKPath GetPaths(SvgElement elem, ISvgRenderer renderer)
        {
            var ret = new SKPath();

            foreach (var child in elem.Children)
            {
                if (child is SvgVisualElement visualElement)
                {
                    if (child is SvgGroup)
                    {
                        var childPath = GetPaths(child, renderer);
                        if (childPath.PointCount > 0)
                        {
                            if (child.Transforms != null)
                            {
                                var matrix = child.Transforms.GetMatrix();
                                childPath.Transform(matrix);
                            }

                            ret.AddPath(childPath);
                        }
                    }
                    else
                    {
                        var childPath = visualElement.Path(renderer);
                        var pathToAdd = childPath != null ? new SKPath(childPath) : new SKPath();

                        if (child.Children.Count > 0)
                        {
                            var descendantPath = GetPaths(child, renderer);
                            if (descendantPath.PointCount > 0)
                                pathToAdd.AddPath(descendantPath);
                        }

                        if (pathToAdd.PointCount > 0)
                        {
                            if (child.Transforms != null)
                            {
                                var matrix = child.Transforms.GetMatrix();
                                pathToAdd.Transform(matrix);
                            }

                            ret.AddPath(pathToAdd);
                        }
                    }
                }
            }

            return ret;
        }
    }
}
#endif
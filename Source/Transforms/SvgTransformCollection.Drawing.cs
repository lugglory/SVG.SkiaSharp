#if !NO_SDC
using SkiaSharp;

namespace Svg.Transforms
{
    public partial class SvgTransformCollection
    {
        /// <summary>
        /// Multiplies all matrices
        /// </summary>
        /// <returns>The result of all transforms</returns>
        public SKMatrix GetMatrix()
        {
            var transformMatrix = SKMatrix.CreateIdentity();

            foreach (var transform in this)
            {
                var other = transform.Matrix;
                transformMatrix = transformMatrix.PostConcat(other);
            }

            return transformMatrix;
        }
    }
}
#endif

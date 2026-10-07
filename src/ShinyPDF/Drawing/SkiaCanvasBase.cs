using ShinyPDF.Infrastructure;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace ShinyPDF.Drawing
{
    internal abstract class SkiaCanvasBase : ICanvas, IRenderingCanvas
    {
        internal SKCanvas? Canvas { get; set; }

        public abstract void BeginDocument();
        public abstract void EndDocument();
        
        public abstract void BeginPage(Size size);
        public abstract void EndPage();
        
        public void Translate(Position vector)
        {
            if (Canvas == null) return;
            Canvas.Translate(vector.X, vector.Y);
        }

        public void DrawRectangle(Position vector, Size size, string color)
        {
            if (size.Width < Size.Epsilon || size.Height < Size.Epsilon)
                return;

            if (Canvas == null) return;
            var paint = color.ColorToPaint();
            Canvas.DrawRect(vector.X, vector.Y, size.Width, size.Height, paint);
        }

        public void DrawTextDecoration(Position vector, float width, float thickness, string color, TextDecorationStyle style)
        {
            if (width < Size.Epsilon || thickness < Size.Epsilon)
                return;

            if (Canvas == null) return;

            if (style is TextDecorationStyle.Solid or TextDecorationStyle.Double)
            {
                // double is drawn as two solid lines, each with the given thickness and a gap of the same size
                DrawRectangle(vector, new Size(width, thickness), color);

                if (style == TextDecorationStyle.Double)
                    DrawRectangle(new Position(vector.X, vector.Y + 2 * thickness), new Size(width, thickness), color);

                return;
            }

            using var paint = new SKPaint
            {
                Color = SKColor.Parse(color),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = thickness
            };

            var centerY = vector.Y + thickness / 2;

            if (style == TextDecorationStyle.Wavy)
            {
                DrawWavyLine(paint, vector.X, centerY, width, thickness);
                return;
            }

            if (style == TextDecorationStyle.Dotted)
            {
                // zero-length dashes with round caps render as dots
                paint.StrokeCap = SKStrokeCap.Round;
                using var dots = SKPathEffect.CreateDash(new[] { 0f, 2 * thickness }, 0);
                paint.PathEffect = dots;
                Canvas.DrawLine(vector.X + thickness / 2, centerY, vector.X + width, centerY, paint);
                return;
            }

            using var dashes = SKPathEffect.CreateDash(new[] { 3 * thickness, 2 * thickness }, 0);
            paint.PathEffect = dashes;
            Canvas.DrawLine(vector.X, centerY, vector.X + width, centerY, paint);
        }

        private void DrawWavyLine(SKPaint paint, float x, float centerY, float width, float thickness)
        {
            var amplitude = thickness * 1.5f;
            var halfWave = thickness * 3;

            using var path = new SKPath();
            path.MoveTo(x, centerY);

            var direction = -1;

            for (var startX = x; startX < x + width; startX += halfWave)
            {
                // the quadratic control point at twice the amplitude puts the peak at the amplitude
                path.QuadTo(startX + halfWave / 2, centerY + direction * amplitude * 2, startX + halfWave, centerY);
                direction = -direction;
            }

            Canvas!.Save();
            Canvas.ClipRect(new SKRect(x, centerY - amplitude - thickness, x + width, centerY + amplitude + thickness));
            Canvas.DrawPath(path, paint);
            Canvas.Restore();
        }

        public void DrawText(SKTextBlob skTextBlob, Position position, TextStyle style)
        {
            if (Canvas == null) return;
            Canvas.DrawText(skTextBlob, position.X, position.Y, style.ToPaint());
        }

        public void DrawImage(SKImage image, Position vector, Size size)
        {
            if (Canvas == null) return;
            Canvas.DrawImage(image, new SKRect(vector.X, vector.Y, size.Width, size.Height), SKSamplingOptions.Default);
        }

        public void DrawHyperlink(string url, Size size)
        {
            if (Canvas == null) return;
            Canvas.DrawUrlAnnotation(new SKRect(0, 0, size.Width, size.Height), url);
        }
        
        public void DrawSectionLink(string sectionName, Size size)
        {
            if (Canvas == null) return;
            Canvas.DrawLinkDestinationAnnotation(new SKRect(0, 0, size.Width, size.Height), sectionName);
        }

        public void DrawSection(string sectionName)
        {
            if (Canvas == null) return;
            Canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), sectionName);
        }

        public void Rotate(float angle)
        {
            if (Canvas == null) return;
            Canvas.RotateDegrees(angle);
        }

        public void Scale(float scaleX, float scaleY)
        {
            if (Canvas == null) return;
            Canvas.Scale(scaleX, scaleY);
        }
    }
}
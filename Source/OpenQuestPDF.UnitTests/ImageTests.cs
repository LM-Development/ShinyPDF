using NUnit.Framework;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Elements;
using OpenQuestPDF.Fluent;
using OpenQuestPDF.Infrastructure;
using OpenQuestPDF.UnitTests.TestEngine;
using SkiaSharp;
using System;

namespace OpenQuestPDF.UnitTests
{
    [TestFixture]
    public class ImageTests
    {
        private static bool IsSkiaSharpAvailable()
        {
            try
            {
                var info = new SKImageInfo(1, 1);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void Measure_TakesAvailableSpaceRegardlessOfSize()
        {
            if (!IsSkiaSharpAvailable())
            {
                Assert.Ignore("SkiaSharp is not available on this platform");
                return;
            }

            TestPlan
                .For(x => new Image
                {
                    InternalImage = GenerateImage(400, 300)
                })
                .MeasureElement(new Size(300, 200))
                .CheckMeasureResult(SpacePlan.FullRender(300, 200));
        }
        
        [Test]
        public void Draw_TakesAvailableSpaceRegardlessOfSize()
        {
            if (!IsSkiaSharpAvailable())
            {
                Assert.Ignore("SkiaSharp is not available on this platform");
                return;
            }

            TestPlan
                .For(x => new Image
                {
                    InternalImage = GenerateImage(400, 300)
                })
                .DrawElement(new Size(300, 200))
                .ExpectCanvasDrawImage(new Position(0, 0), new Size(300, 200))
                .CheckDrawResult();
        }

        [Test]
        public void Fluent_RecognizesImageProportions()
        {
            if (!IsSkiaSharpAvailable())
            {
                Assert.Ignore("SkiaSharp is not available on this platform");
                return;
            }
            
            var image = GenerateImage(600, 200).Encode(SKEncodedImageFormat.Png, 100).ToArray();
            
            TestPlan
                .For(x =>
                {
                    var container = new Container();
                    container.Image(image);
                    return container;
                })
                .MeasureElement(new Size(300, 200))
                .CheckMeasureResult(SpacePlan.FullRender(300, 100));
        }
        
        SKImage GenerateImage(int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);
            return surface.Snapshot();
        }
    }
}
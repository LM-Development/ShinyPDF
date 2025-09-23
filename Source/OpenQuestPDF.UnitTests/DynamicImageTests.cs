using FluentAssertions;
using NUnit.Framework;
using OpenQuestPDF.Drawing;
using OpenQuestPDF.Elements;
using OpenQuestPDF.Infrastructure;
using OpenQuestPDF.UnitTests.TestEngine;
using SkiaSharp;
using System;

namespace OpenQuestPDF.UnitTests
{
    [TestFixture]
    public class DynamicImageTests
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
                .For(x => new DynamicImage
                {
                    Source = GenerateImage
                })
                .MeasureElement(new Size(300, 200))
                .CheckMeasureResult(SpacePlan.FullRender(300, 200));
        }
        
        [Test]
        public void Draw_HandlesNull()
        {
            TestPlan
                .For(x => new DynamicImage
                {
                    Source = size => null
                })
                .DrawElement(new Size(300, 200))
                .CheckDrawResult();
        }
        
        [Test]
        public void Draw_PreservesSize()
        {
            if (!IsSkiaSharpAvailable())
            {
                Assert.Ignore("SkiaSharp is not available on this platform");
                return;
            }

            TestPlan
                .For(x => new DynamicImage
                {
                    Source = GenerateImage
                })
                .DrawElement(new Size(300, 200))
                .ExpectCanvasDrawImage(Position.Zero, new Size(300, 200))
                .CheckDrawResult();
        }
        
        [Test]
        public void Draw_PassesCorrectSizeToSource()
        {
            if (!IsSkiaSharpAvailable())
            {
                Assert.Ignore("SkiaSharp is not available on this platform");
                return;
            }

            Size passedSize = default;

            TestPlan
                .For(x => new DynamicImage
                {
                    Source = size =>
                    {
                        passedSize = size;
                        return GenerateImage(size);
                    }
                })
                .DrawElement(new Size(400, 300))
                .ExpectCanvasDrawImage(Position.Zero, new Size(400, 300))
                .CheckDrawResult();
            
            passedSize.Should().BeEquivalentTo(new Size(400, 300));
        }
        
        byte[] GenerateImage(Size size)
        {
            var image = GenerateImage((int) size.Width, (int) size.Height);
            return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }
        
        SKImage GenerateImage(int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(imageInfo);
            return surface.Snapshot();
        }
    }
}
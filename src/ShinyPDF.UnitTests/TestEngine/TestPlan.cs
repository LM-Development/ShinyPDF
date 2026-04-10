using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using ShinyPDF.Drawing;
using ShinyPDF.Elements;
using ShinyPDF.Helpers;
using ShinyPDF.Infrastructure;
using ShinyPDF.UnitTests.TestEngine.Operations;

namespace ShinyPDF.UnitTests.TestEngine
{
    internal class TestPlan
    {
        private const string DefaultChildName = "child";

        private static Random Random { get; } = new Random();
        
        private Element Element { get; set; }
        private ICanvas Canvas { get; }
        
        private Size OperationInput { get; set; }
        private Queue<OperationBase> Operations { get; } = new Queue<OperationBase>();

        public TestPlan()
        {
            Canvas = CreateCanvas();
        }
        
        public static TestPlan For(Func<TestPlan, Element> create)
        {
            var plan = new TestPlan();
            plan.Element = create(plan);
            
            return plan;
        }

        private T GetExpected<T>() where T : OperationBase
        {
            if (Operations.TryDequeue(out var value) && value is T result)
                return result;

            var gotType = value?.GetType()?.Name ?? "null";
            Assert.Fail($"Expected: {typeof(T).Name}, got {gotType}: {JsonSerializer.Serialize(value)}");
            return null;
        }
        
        private ICanvas CreateCanvas()
        {
            return new MockCanvas
            {
                TranslateFunc = position =>
                {
                    var expected = GetExpected<CanvasTranslateOperation>();

                    Assert.That(position.X, Is.EqualTo(expected.Position.X), "Translate X");
                    Assert.That(position.Y, Is.EqualTo(expected.Position.Y), "Translate Y");
                },
                RotateFunc = angle =>
                {
                    var expected = GetExpected<CanvasRotateOperation>();

                    Assert.That(angle, Is.EqualTo(expected.Angle), "Rotate angle");
                },
                ScaleFunc = (scaleX, scaleY) =>
                {
                    var expected = GetExpected<CanvasScaleOperation>();

                    Assert.That(scaleX, Is.EqualTo(expected.ScaleX), "Scale X");
                    Assert.That(scaleY, Is.EqualTo(expected.ScaleY), "Scale Y");
                },
                DrawRectFunc = (position, size, color) =>
                {
                    var expected = GetExpected<CanvasDrawRectangleOperation>();
                    
                    Assert.That(position.X, Is.EqualTo(expected.Position.X), "Draw rectangle: X");
                    Assert.That(position.Y, Is.EqualTo(expected.Position.Y), "Draw rectangle: Y");
                    
                    Assert.That(size.Width, Is.EqualTo(expected.Size.Width), "Draw rectangle: width");
                    Assert.That(size.Height, Is.EqualTo(expected.Size.Height), "Draw rectangle: height");
                    
                    Assert.That(color, Is.EqualTo(expected.Color), "Draw rectangle: color");
                },
                DrawImageFunc = (image, position, size) =>
                {
                    var expected = GetExpected<CanvasDrawImageOperation>();
                    
                    Assert.That(position.X, Is.EqualTo(expected.Position.X), "Draw image: X");
                    Assert.That(position.Y, Is.EqualTo(expected.Position.Y), "Draw image: Y");
                    
                    Assert.That(size.Width, Is.EqualTo(expected.Size.Width), "Draw image: width");
                    Assert.That(size.Height, Is.EqualTo(expected.Size.Height), "Draw image: height");
                }
            };
        }

        public Element CreateChild() => CreateChild(DefaultChildName);
        
        public Element CreateChild(string id)
        {
            return new ElementMock
            {
                Id = id,
                MeasureFunc = availableSpace =>
                {
                    var expected = GetExpected<ChildMeasureOperation>();

                    Assert.That(id, Is.EqualTo(expected.ChildId));
                    
                    Assert.That(availableSpace.Width, Is.EqualTo(expected.Input.Width), $"Measure: width of child '{expected.ChildId}'");
                    Assert.That(availableSpace.Height, Is.EqualTo(expected.Input.Height), $"Measure: height of child '{expected.ChildId}'");

                    return expected.Output;
                },
                DrawFunc = availableSpace =>
                {
                    var expected = GetExpected<ChildDrawOperation>();

                    Assert.That(id, Is.EqualTo(expected.ChildId));
                    
                    Assert.That(availableSpace.Width, Is.EqualTo(expected.Input.Width), $"Draw: width of child '{expected.ChildId}'");
                    Assert.That(availableSpace.Height, Is.EqualTo(expected.Input.Height), $"Draw: width of child '{expected.ChildId}'");
                }
            };
        }
        
        public TestPlan MeasureElement(Size input)
        {
            OperationInput = input;
            return this;
        }
        
        public TestPlan DrawElement(Size input)
        {
            OperationInput = input;
            return this;
        }

        private TestPlan AddOperation(OperationBase operationBase)
        {
            Operations.Enqueue(operationBase);
            return this;
        }
        
        public TestPlan ExpectChildMeasure(Size expectedInput, SpacePlan returns)
        {
            return ExpectChildMeasure(DefaultChildName, expectedInput, returns);
        }
        
        public TestPlan ExpectChildMeasure(string child, Size expectedInput, SpacePlan returns)
        {
            return AddOperation(new ChildMeasureOperation(child, expectedInput, returns));
        }
        
        public TestPlan ExpectChildDraw(Size expectedInput)
        {
            return ExpectChildDraw(DefaultChildName, expectedInput);
        }
        
        public TestPlan ExpectChildDraw(string child, Size expectedInput)
        {
            return AddOperation(new ChildDrawOperation(child, expectedInput));
        }

        public TestPlan ExpectCanvasTranslate(Position position)
        {
            return AddOperation(new CanvasTranslateOperation(position));
        }
        
        public TestPlan ExpectCanvasTranslate(float left, float top)
        {
            return AddOperation(new CanvasTranslateOperation(new Position(left, top)));
        }

        public TestPlan ExpectCanvasScale(float scaleX, float scaleY)
        {
            return AddOperation(new CanvasScaleOperation(scaleX, scaleY));
        }
        
        public TestPlan ExpectCanvasRotate(float angle)
        {
            return AddOperation(new CanvasRotateOperation(angle));
        }
        
        public TestPlan ExpectCanvasDrawRectangle(Position position, Size size, string color)
        {
            return AddOperation(new CanvasDrawRectangleOperation(position, size, color));
        }
        
        public TestPlan ExpectCanvasDrawImage(Position position, Size size)
        {
            return AddOperation(new CanvasDrawImageOperation(position, size));
        }
        
        public TestPlan CheckMeasureResult(SpacePlan expected)
        {
            Element.InjectDependencies(null, Canvas);
            
            var actual = Element.Measure(OperationInput);
            
            Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
            
            Assert.That(actual.Width, Is.EqualTo(expected.Width), "Measure: width");
            Assert.That(actual.Height, Is.EqualTo(expected.Height), "Measure: height");
            Assert.That(actual.Type, Is.EqualTo(expected.Type), "Measure: height");
            
            return this;
        }
        
        public TestPlan CheckDrawResult()
        {
            Element.InjectDependencies(null, Canvas);
            Element.Draw(OperationInput);
            return this;
        }

        public TestPlan CheckState(Func<Element, bool> condition)
        {
            Assert.That(condition(Element), Is.True, "Checking condition");
            return this;
        }

        public TestPlan CheckState<T>(Func<T, bool> condition) where T : Element
        {
            Assert.That(Element, Is.InstanceOf<T>());
            Assert.That(condition(Element as T), Is.True, "Checking condition");
            return this;
        }
        
        public static Element CreateUniqueElement()
        {
            return new Constrained
            {
                MinWidth = 90,
                MinHeight = 60,
                
                Child = new DynamicImage
                {
                    Source = Placeholders.Image
                }
            };
        }

        public static void CompareOperations(Element value, Element expected, Size? availableSpace = null)
        {
            CompareMeasureOperations(value, expected, availableSpace);
            CompareDrawOperations(value, expected, availableSpace);
        }
        
        private static void CompareMeasureOperations(Element value, Element expected, Size? availableSpace = null)
        {
            availableSpace ??= new Size(400, 300);
            
            var canvas = new FreeCanvas();
            value.InjectDependencies(null, canvas);
            var valueMeasure = value.Measure(availableSpace.Value);
            
            expected.InjectDependencies(null, canvas);
            var expectedMeasure = expected.Measure(availableSpace.Value);
            
            valueMeasure.Should().BeEquivalentTo(expectedMeasure);
        }
        
        private static void CompareDrawOperations(Element value, Element expected, Size? availableSpace = null)
        {
            availableSpace ??= new Size(400, 300);
            
            var valueCanvas = new OperationRecordingCanvas();
            value.InjectDependencies(null, valueCanvas);
            value.Draw(availableSpace.Value);
            
            var expectedCanvas = new OperationRecordingCanvas();
            expected.InjectDependencies(null, expectedCanvas);
            expected.Draw(availableSpace.Value);

            // Prüfe, ob Operationen vorhanden sind
            Assert.That(valueCanvas.Operations, Is.Not.Null, "valueCanvas.Operations ist null");
            Assert.That(expectedCanvas.Operations, Is.Not.Null, "expectedCanvas.Operations ist null");

            // Prüfe, ob Operationen nicht leer sind
            Assert.That(valueCanvas.Operations.Count, Is.GreaterThan(0), "valueCanvas.Operations ist leer");
            Assert.That(expectedCanvas.Operations.Count, Is.GreaterThan(0), "expectedCanvas.Operations ist leer");

            // Vergleiche die Operationen
            valueCanvas.Operations.Should().BeEquivalentTo(expectedCanvas.Operations, options => options
                .PreferringRuntimeMemberTypes()
                .WithTracing());
        }
    }
}
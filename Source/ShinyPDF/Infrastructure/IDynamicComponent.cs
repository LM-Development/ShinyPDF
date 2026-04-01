using System;
using ShinyPDF.Elements;
using ShinyPDF.Helpers;

namespace ShinyPDF.Infrastructure
{
    internal class DynamicComponentProxy
    {
        // These properties are set by the CreateFrom factory method before use
        internal Action<object> SetState { get; private set; } = null!;
        internal Func<object> GetState { get; private set; } = null!;
        internal Func<DynamicContext, DynamicComponentComposeResult> Compose { get; private set; } = null!;
        
        internal static DynamicComponentProxy CreateFrom<TState>(IDynamicComponent<TState> component) where TState : struct
        {
            return new DynamicComponentProxy
            {
                GetState = () => component.State,
                SetState = x => component.State = (TState)x,
                Compose = component.Compose
            };
        }
    }

    public class DynamicComponentComposeResult
    {
        public required IElement Content { get; set; }
        public bool HasMoreContent { get; set; }
    }
    
    public interface IDynamicComponent<TState> where TState : struct
    {
        TState State { get; set; }
        DynamicComponentComposeResult Compose(DynamicContext context);
    }
}
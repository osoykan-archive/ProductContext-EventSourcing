using System;
using System.Collections.Generic;

namespace ProductContext.Framework
{
    public class ProjectionTypeMapper
    {
        private readonly Dictionary<Type, Type> _handled = new Dictionary<Type, Type>();

        public void HandledBy<TEvent, TProjection>()
        {
            _handled[typeof(TEvent)] = typeof(TProjection);
        }

        public Type WhoHandlesMe(string evenType)
        {
            _handled.TryGetValue(Type.GetType(evenType), out Type projectionType);
            return projectionType;
        }
    }
}

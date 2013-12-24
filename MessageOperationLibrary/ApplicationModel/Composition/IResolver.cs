using System;

namespace MessageOperationLibrary.ApplicationModel.Composition
{
    public interface IResolver
    {
        object Resolve(Type type);
    }
}

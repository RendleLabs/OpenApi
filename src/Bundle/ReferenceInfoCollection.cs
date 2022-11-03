using System.Collections.ObjectModel;
using Microsoft.OpenApi.Interfaces;

namespace RendleLabs.OpenApi.Bundle;

public class ReferenceInfoCollection : KeyedCollection<string, ReferenceInfo>
{
    protected override string GetKeyForItem(ReferenceInfo item) => item.Path;

    public ReferenceInfo<T> GetOrAdd<T>(string path) where T : IOpenApiReferenceable
    {
        if (TryGetValue(path, out var info))
        {
            return (ReferenceInfo<T>)info;
        }

        var referenceInfo = new ReferenceInfo<T>(path);
        Add(referenceInfo);
        return referenceInfo;
    }
}
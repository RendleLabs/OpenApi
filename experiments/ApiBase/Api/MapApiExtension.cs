using System.Reflection;

namespace ApiBase.Api;

public static class MapApiExtension
{
    public static WebApplication MapApi<T>(this WebApplication app)
    {
        var mapMethod = GetMapMethod<T>();
        mapMethod.Invoke(null, new object[]{ app, CreateBuilder<T>() });
        return app;
    }

    private static MethodInfo GetMapMethod<T>()
    {
        var type = typeof(T);
        MethodInfo? mapMethod = null;
        while (mapMethod is null && type is not null)
        {
            mapMethod = type.GetMethod("__Map", BindingFlags.Static | BindingFlags.NonPublic);
            type = type.BaseType;
        }

        if (mapMethod is null) throw new InvalidOperationException();
        return mapMethod.MakeGenericMethod(typeof(T));
    }

    private static Func<IServiceProvider, T> CreateBuilder<T>()
    {
        var ctors = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var ctor = ctors.FirstOrDefault();
        if (ctor is null) return _ => Activator.CreateInstance<T>();
        var parameterTypes = ctor.GetParameters();
        return services =>
        {
            int length = parameterTypes.Length;
            object[] parameters = new object[length];
            for (int i = 0; i < length; i++)
            {
                var parameterType = parameterTypes[i].ParameterType;
                parameters[i] = services.GetRequiredService(parameterType);
            }

            return (T)Activator.CreateInstance(typeof(T), parameters)!;
        };
    }
}
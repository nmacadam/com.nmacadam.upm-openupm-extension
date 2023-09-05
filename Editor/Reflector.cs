using System;
using System.Linq;
using System.Reflection;

namespace PackageWizard.Editor
{
    /// <summary>
    /// Wraps or creates an object instance from a given Type, and provides methods for calling methods and retrieving
    /// fields and properties via reflection.
    /// </summary>
    /// <remarks>
    /// Does not handle reflection errors.
    /// </remarks>
    /// <remarks>
    /// Intended for hacking on Unity internal classes, not for use in production code.
    /// </remarks>
    public class Reflector
    {
        private const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        
        public readonly object Instance;
        public readonly Type Type;

        private readonly BindingFlags _flags;
        
        public Reflector(object instance)
        {
            Instance = instance;
            Type = instance.GetType();
            _flags = AllFlags;
        }
        
        public Reflector(object instance, Type type)
        {
            Instance = instance;
            Type = type;
            _flags = AllFlags;
        }
        
        public Reflector(Type type)
        {
            Instance = null;
            Type = type;
            _flags = AllFlags;
        }
        
        public Reflector(object instance, Type type, BindingFlags flags)
        {
            Instance = instance;
            Type = type;
            _flags = flags;
        }

        public object Call(string methodName, object[] arguments = null)
        {
            var methodInfo = Type.GetMethod(methodName, _flags);
            return methodInfo.Invoke(Instance, arguments);
        }

        public object Call(string methodName, Type[] genericTypes, object[] arguments)
        {
            var methods = Type.GetMethods(_flags);
            var method = methods.First(method => method.IsGenericMethodDefinition && method.Name == methodName);
            var genericMethod = method.MakeGenericMethod(genericTypes.ToArray());
            return genericMethod.Invoke(Instance, arguments);
        }

        public Reflector GetProperty(string propertyName)
        {
            var propertyInfo = Type.GetProperty(propertyName, _flags);
            return new Reflector(propertyInfo.GetValue(Instance));
        }

        public void SetProperty(string propertyName, object value)
        {
            var propertyInfo = Type.GetProperty(propertyName, _flags);
            propertyInfo.SetValue(Instance, value);
        }

        public Reflector GetField(string fieldName)
        {
            var fieldInfo = Type.GetField(fieldName, _flags);
            return new Reflector(fieldInfo.GetValue(Instance));
        }

        public void SetField(string fieldName, object value)
        {
            var fieldInfo = Type.GetField(fieldName, _flags);
            fieldInfo.SetValue(Instance, value);
        }
    }
    
    public static class ReflectorExtensions
    {
        public static Reflector ToReflector(this object obj)
        {
            return new Reflector(obj);
        }
    }
}
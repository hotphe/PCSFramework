using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;
using PCS.Common;

namespace PCS.DI
{
    public enum LifecycleScope
    {
        Scene,
        Project
    }

    public class DIContainer
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        private LifecycleScope _scope;

        public DIContainer()
        {
            _scope = LifecycleScope.Scene;
        }

        public DIContainer(LifecycleScope scope)
        {
            _scope = scope;
        }

        public void Regist(IEnumerable<IDependencyProvider> providers)
        {
            foreach (var provider in providers)
            {
                RegistProvider(provider);
            }
        }

        public void Inject(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            foreach (var monoBehaviour in monoBehaviours)
            {
                if (IsInjectable(monoBehaviour))
                    Inject(monoBehaviour);
            }
        }

        public void Register<T>(T instance)
        {
            registry[typeof(T)] = instance;
        }

        private void RegistProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(bindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var provideAttribute = method.GetCustomAttribute<ProvideAttribute>();

                if (provideAttribute.Scope != _scope) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                {
                    if (registry.ContainsKey(returnType))
                    {
                        Debug.LogWarning($"A provider for {returnType.Name} is already registered.");
                        continue;
                    }
                    registry.Add(returnType, providedInstance);
                    if (EnvProperties.IsDebugMode) Debug.Log($"{returnType} type is Registered.");
                }
                else
                    throw new Exception($"Provider {method.Name} return null into {provider.GetType().Name}");
            }
        }

        private bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }


        private void Inject(object instance)
        {
            var type = instance.GetType();

            var fields = type.GetFields(bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach (var field in fields)
            {
                if (field.GetValue(instance) != null)
                    continue;

                var fieldType = field.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance != null)
                {
                    field.SetValue(instance, resolvedInstance);
                    if(EnvProperties.IsDebugMode) Debug.Log($"{field.Name} field is injected.");
                }
            }

            var properties = type.GetProperties(bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach (var property in properties)
            {
                if (property.GetValue(instance) != null)
                    continue;

                var propertyType = property.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance != null)
                {
                    property.SetValue(instance, resolvedInstance);
                    if (EnvProperties.IsDebugMode) Debug.Log($"{property.Name} property is injected.");
                }
            }

            var methods = type.GetMethods(bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach (var method in methods)
            {
                var parameterTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
                var resolvedInstances = parameterTypes.Select(Resolve).ToArray();
                if (resolvedInstances.Any(member => member == null))
                    continue;

                method.Invoke(instance, resolvedInstances);
                if(EnvProperties.IsDebugMode) Debug.Log($"{method.Name} method is injected.");
            }

        }
        private object Resolve(Type type)
        {
            registry.TryGetValue(type, out var instance);
            return instance;
        }

    }
}
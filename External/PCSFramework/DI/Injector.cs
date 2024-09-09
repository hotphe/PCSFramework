using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PCS.DI
{
    [DefaultExecutionOrder(-9000)]
    public class Injector : MonoBehaviour
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        protected void Awake()
        {
            var monoBehaviours = FindMonoBehaviours();

            var providers = monoBehaviours.OfType<IDependencyProvider>();
            foreach(var provider in providers)
            {
                RegistProvider(provider);
            }

            var injectables = monoBehaviours.Where(IsInjectable);
            foreach(var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        public void Register<T>(T instance)
        {
            registry[typeof(T)] = instance;
        }

        // 프로바이더(객체 제공자) 등록
        private void RegistProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(bindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                    registry.Add(returnType, providedInstance);
                else
                    throw new Exception($"Provider {method.Name} return null into {provider.GetType().Name}");
            }
        }

        //하나라도 Inject Attribute를 사용한 field가 있으면 true 반환
        private bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(bindingFlags);
            return members.Any(member => Attribute.IsDefined(member,typeof(InjectAttribute)));
        }

        
        private void Inject(object instance)
        {
            var type = instance.GetType();

            var fields = type.GetFields(bindingFlags).Where(member => Attribute.IsDefined(member,typeof(InjectAttribute)));
            foreach(var field in fields)
            {
                if (field.GetValue(instance) != null)
                    continue;

                var fieldType = field.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                    throw new Exception($"Failed to inject {fieldType.Name} into {type.Name}.");

                field.SetValue(instance, resolvedInstance);
            }

            var properties = type.GetProperties(bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach(var property in properties)
            {
                if (property.GetValue(instance) != null)
                    continue;

                var propertyType = property.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                    throw new Exception($"Failed to inject {propertyType.Name} into {type.Name}.");

                property.SetValue(instance, resolvedInstance);
            }

            var methods = type.GetMethods(bindingFlags).Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach(var method in methods)
            {
                var parameterTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
                var resolvedInstances = parameterTypes.Select(Resolve).ToArray();
                if(resolvedInstances.Any(member=>member == null))
                    throw new Exception($"Failed to inject {method.Name} into {type.Name}.");

                method.Invoke(instance, resolvedInstances);
            }
        }

        private object Resolve(Type type)
        {
            registry.TryGetValue(type, out var instance);
            return instance;
        }

        private MonoBehaviour[] FindMonoBehaviours() => FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);

    }
}

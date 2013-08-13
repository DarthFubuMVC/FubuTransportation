﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using FubuCore.Descriptions;
using FubuCore.Util;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.Registration
{
    public class HandlerSource : IHandlerSource, DescribesItself
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly StringWriter _description = new StringWriter();

        private readonly ActionMethodFilter _methodFilters;
        private readonly CompositeFilter<Type> _typeFilters = new CompositeFilter<Type>();
        private readonly CompositeFilter<HandlerCall> _callFilters = new CompositeFilter<HandlerCall>();

        public HandlerSource()
        {
            _description.WriteLine("Public methods that follow the 1 in/1 out, 0 in/ 1 out, or 1 in/ 0 out pattern");

            _methodFilters = new ActionMethodFilter();
        }

        public void UseAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
        }

        public void UseThisAssembly()
        {
            UseAssembly(FubuTransportRegistry.FindTheCallingAssembly());
        }

        IEnumerable<HandlerCall> IHandlerSource.FindCalls()
        {
            var types = new TypePool();
            if (_assemblies.Any())
            {
                types.AddAssemblies(_assemblies);
            }
            else
            {
                types.AddAssembly(FubuTransportRegistry.FindTheCallingAssembly());
            }

            return types.TypesMatching(_typeFilters.Matches).SelectMany(actionsFromType);
        }

        private IEnumerable<HandlerCall> actionsFromType(Type type)
        {
            return type.PublicInstanceMethods()
                       .Where(_methodFilters.Matches)
                       .Where(HandlerCall.IsCandidate)
                       .Select(m => buildHandler(type, m))
                       .Where(_callFilters.Matches);
        }

        protected virtual HandlerCall buildHandler(Type type, MethodInfo method)
        {
            return new HandlerCall(type, method);
        }

        /// <summary>
        /// Find Handlers on classes whose name ends on 'Consumer'
        /// </summary>
        public void IncludeClassesSuffixedWithConsumer()
        {
            _description.WriteLine("Public classes that are suffixed by 'Consumer'");
            IncludeTypesNamed(x => x.EndsWith("Consumer"));
        }

        /// <summary>
        /// Find Handlers suffixed with 'Saga' that have public properties of 'State' and 'IsCompleted'
        /// </summary>
        public void IncludeClassesMatchingSagaConvention()
        {
            _description.WriteLine("Public classes suffixed with Saga that have public properties of 'State' and 'IsCompleted'");
            IncludeTypes(x => x.MatchesSagaConvention());
        }

        public void IncludeTypesNamed(Expression<Func<string, bool>> filter)
        {
            _description.WriteLine("Classes that match " + filter.ToString());

            var typeParam = Expression.Parameter(typeof(Type), "type"); // type =>
            var nameProp = Expression.Property(typeParam, "Name");  // type.Name
            var invokeFilter = Expression.Invoke(filter, nameProp); // filter(type.Name)
            var lambda = Expression.Lambda<Func<Type, bool>>(invokeFilter, typeParam); // type => filter(type.Name)

            IncludeTypes(lambda);
        }

        /// <summary>
        /// Find Handlers on types that match on the provided filter
        /// </summary>
        public void IncludeTypes(Expression<Func<Type, bool>> filter)
        {
            _description.WriteLine("Public class that match " + filter.ToString());

            _typeFilters.Includes += filter;
        }

        /// <summary>
        /// Find Handlers on concrete types assignable to T
        /// </summary>
        public void IncludeTypesImplementing<T>()
        {
            _description.WriteLine("Where types are concrete types that implement the {0} interface".ToFormat(typeof(T).FullName));
            IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
        }

        /// <summary>
        /// Handlers that match on the provided filter will be added to the runtime. 
        /// </summary>
        public void IncludeMethods(Expression<Func<MethodInfo, bool>> filter)
        {
            _description.WriteLine("Methods matching " + filter.ToString());
            _methodFilters.Includes += filter;
        }

        /// <summary>
        /// Exclude types that match on the provided filter for finding Handlers
        /// </summary>
        public void ExcludeTypes(Expression<Func<Type, bool>> filter)
        {
            _description.WriteLine("Exclude types matching " + filter.ToString());
            _typeFilters.Excludes += filter;
        }

        /// <summary>
        /// Handlers that match on the provided filter will NOT be added to the runtime. 
        /// </summary>
        public void ExcludeMethods(Expression<Func<MethodInfo, bool>> filter)
        {
            _description.WriteLine("Exclude methods matching " + filter);
            _methodFilters.Excludes += filter;
        }

        /// <summary>
        /// Ignore any methods that are declared by a super type or interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void IgnoreMethodsDeclaredBy<T>()
        {
            _description.WriteLine("Exclude methods declared by type " + typeof(T).FullName);
            _methodFilters.IgnoreMethodsDeclaredBy<T>();
        }

        /// <summary>
        /// Exclude any types that are not concrete
        /// </summary>
        public void ExcludeNonConcreteTypes()
        {
            _description.WriteLine("Excludes non-concrete types");
            _typeFilters.Excludes += type => !type.IsConcrete();
        }

        void DescribesItself.Describe(Description description)
        {
            var list = new BulletList
            {
                Name = "Assemblies"
            };

            _assemblies.Each(assem =>
            {
                list.Children.Add(new Description
                {
                    Title = assem.FullName
                });
            });

            description.Title = "Handler Source";
            description.BulletLists.Add(list);
            description.LongDescription = _description.ToString();

        }
    }
}
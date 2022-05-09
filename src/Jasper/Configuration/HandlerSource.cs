using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using BaselineTypeDiscovery;
using Jasper.Attributes;
using Jasper.Runtime.Handlers;
using Jasper.Util;

namespace Jasper.Configuration;

public sealed class HandlerSource
{
    private readonly IList<Assembly> _assemblies = new List<Assembly>();
    private readonly IList<Type> _explicitTypes = new List<Type>();

    private readonly ActionMethodFilter _methodFilters;
    private readonly CompositeFilter<Type> _typeFilters = new();

    private readonly string[] _validMethods =
        { "Handle", "Handles", "Consume", "Consumes", "Orchestrate", "Orchestrates", "Start", "Starts" };

    private bool _conventionalDiscoveryDisabled;

    public HandlerSource()
    {
        _methodFilters = new ActionMethodFilter();
        _methodFilters.Excludes += m => m.HasAttribute<JasperIgnoreAttribute>();

        _methodFilters.Includes += m => _validMethods.Contains(m.Name);

        IncludeClassesSuffixedWith("Handler");
        IncludeClassesSuffixedWith("Consumer");

        IncludeTypes(x => x.Closes(typeof(StatefulSagaOf<>)));

        _typeFilters.Excludes += t => t.HasAttribute<JasperIgnoreAttribute>();
    }

    /// <summary>
    ///     Disable all conventional discovery of message handlers
    /// </summary>
    public HandlerSource DisableConventionalDiscovery(bool value = true)
    {
        _conventionalDiscoveryDisabled = value;
        return this;
    }

    internal async Task<HandlerCall[]> FindCallsAsync(JasperOptions options)
    {
        if (_conventionalDiscoveryDisabled)
        {
            return _explicitTypes.SelectMany(actionsFromType).ToArray();
        }

        if (options.ApplicationAssembly == null)
        {
            return Array.Empty<HandlerCall>();
        }

        _assemblies.Add(options.ApplicationAssembly);


        var types = await TypeRepository.FindTypes(_assemblies,
                TypeClassification.Concretes | TypeClassification.Closed, type => _typeFilters.Matches(type))
            .ConfigureAwait(false);


        return types
            .Where(x => !x.HasAttribute<JasperIgnoreAttribute>())
            .Concat(_explicitTypes)
            .Distinct()
            .SelectMany(actionsFromType).ToArray();
    }

    private IEnumerable<HandlerCall> actionsFromType(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
            .Where(x => !x.HasAttribute<JasperIgnoreAttribute>())
            .Where(x => x.DeclaringType != typeof(object)).ToArray()
            .Where(_methodFilters.Matches)
            .Where(HandlerCall.IsCandidate)
            .Select(m => buildHandler(type, m));
    }

    private HandlerCall buildHandler(Type type, MethodInfo method)
    {
        return new HandlerCall(type, method);
    }

    /// <summary>
    ///     Find Handlers from concrete classes from the given
    ///     assembly
    /// </summary>
    /// <param name="assembly"></param>
    public void IncludeAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
    }

    /// <summary>
    ///     Find Handlers from concrete classes whose names ends with the suffix
    /// </summary>
    /// <param name="suffix"></param>
    public void IncludeClassesSuffixedWith(string suffix)
    {
        IncludeTypesNamed(x => x.EndsWith(suffix));
    }

    /// <summary>
    ///     Find Handler classes based on the Type name filter supplied
    /// </summary>
    /// <param name="filter"></param>
    public void IncludeTypesNamed(Func<string, bool> filter)
    {
        IncludeTypes(type => filter(type.Name));
    }

    /// <summary>
    ///     Find Handlers on types that match on the provided filter
    /// </summary>
    public void IncludeTypes(Func<Type, bool> filter)
    {
        _typeFilters.Includes += filter;
    }

    /// <summary>
    ///     Find Handlers on concrete types assignable to T
    /// </summary>
    public void IncludeTypesImplementing<T>()
    {
        IncludeTypes(type => !type.IsOpenGeneric() && type.IsConcreteTypeOf<T>());
    }

    /// <summary>
    ///     Exclude types that match on the provided filter for finding Handlers
    /// </summary>
    public void ExcludeTypes(Func<Type, bool> filter)
    {
        _typeFilters.Excludes += filter;
    }

    /// <summary>
    ///     Handlers that match on the provided filter will NOT be added to the runtime.
    /// </summary>
    public void ExcludeMethods(Func<MethodInfo, bool> filter)
    {
        _methodFilters.Excludes += filter;
    }

    /// <summary>
    ///     Exclude any types that are not concrete
    /// </summary>
    public void ExcludeNonConcreteTypes()
    {
        _typeFilters.Excludes += type => !type.IsConcrete();
    }

    /// <summary>
    ///     Include a single type "T"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void IncludeType<T>()
    {
        _explicitTypes.Fill(typeof(T));
    }

    /// <summary>
    ///     Include a single handler type
    /// </summary>
    /// <param name="type"></param>
    public void IncludeType(Type type)
    {
        _explicitTypes.Fill(type);
    }
}

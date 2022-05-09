﻿using System;
using System.Collections.Generic;
using Lamar;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Jasper.Persistence.Postgresql;

public class NpgsqlConnectionInstance : Instance
{
    private Instance? _settings;

    public NpgsqlConnectionInstance(Type serviceType) : base(serviceType, typeof(NpgsqlConnection),
        ServiceLifetime.Scoped)
    {
        Name = Variable.DefaultArgName(serviceType);
    }

    public override bool RequiresServiceProvider => false;

    public override Func<Scope, object> ToResolver(Scope topScope)
    {
        return _ => new NpgsqlConnection(topScope.GetInstance<PostgresqlSettings>().ConnectionString);
    }

    public override object Resolve(Scope scope)
    {
        return new NpgsqlConnection(scope.GetInstance<PostgresqlSettings>().ConnectionString);
    }

    public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
    {
        var settings = variables.Resolve(_settings, mode);
        return new NpgsqlConnectionFrame(settings, this).Connection;
    }

    protected override IEnumerable<Instance> createPlan(ServiceGraph services)
    {
        _settings = services.FindDefault(typeof(PostgresqlSettings));
        yield return _settings;
    }
}

public class NpgsqlConnectionFrame : SyncFrame
{
    private readonly Instance _instance;
    private readonly Variable _settings;

    public NpgsqlConnectionFrame(Variable settings, Instance instance)
    {
        _settings = settings;
        Connection = new ServiceVariable(instance, this);
        _instance = instance;
    }

    public ServiceVariable Connection { get; }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        yield return _settings;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write(
            $"BLOCK:using ({_instance.ServiceType.FullNameInCode()} {Connection.Usage} = new {typeof(NpgsqlConnection).FullName}({_settings.Usage}.{nameof(PostgresqlSettings.ConnectionString)}))");
        Next?.GenerateCode(method, writer);
        writer.FinishBlock();
    }
}

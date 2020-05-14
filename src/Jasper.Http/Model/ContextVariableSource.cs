﻿using System;
using System.Collections.Generic;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Model
{
    public class ContextVariableSource : IVariableSource
    {
        private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();

        public ContextVariableSource()
        {
            foreach (var property in typeof(HttpContext).GetProperties())
            {
                if (property.PropertyType == typeof(string)) continue;

                var variable = new Variable(property.PropertyType, $"{RouteGraph.Context}.{property.Name}");
                _variables.Add(property.PropertyType, variable);
            }
        }

        public bool Matches(Type type)
        {
            return _variables.ContainsKey(type);
        }

        public Variable Create(Type type)
        {
            return _variables[type];
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weasel.Core;
using Weasel.Core.Migrations;

namespace Jasper.Persistence.Database
{
    public abstract partial class DatabaseBackedEnvelopePersistence<T> : IFeatureSchema
    {

        void IFeatureSchema.WritePermissions(Migrator rules, TextWriter writer)
        {
            // Nothing
        }

        IEnumerable<Type> IFeatureSchema.DependentTypes()
        {
            yield break;
        }

        public abstract ISchemaObject[] Objects { get; }

        public override IFeatureSchema[] BuildFeatureSchemas()
        {
            return new IFeatureSchema[] { this };
        }

        Type IFeatureSchema.StorageType => GetType();
    }
}

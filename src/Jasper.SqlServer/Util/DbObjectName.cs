using System;

namespace Jasper.SqlServer.Util
{
    public class DbObjectName
    {
        /// <summary>
        ///     Create a DbObjectName with Schema = "public"
        /// </summary>
        /// <param name="name"></param>
        public DbObjectName(string name) : this("public", name)
        {
        }

        public DbObjectName(string schema, string name)
        {
            Schema = schema;
            Name = name;
            QualifiedName = $"{Schema}.{Name}";
        }

        public string OwnerName => Schema == "dbo" ? Name : QualifiedName;

        public string Schema { get; }
        public string Name { get; }
        public string QualifiedName { get; }

        public DbObjectName ToTempCopyTable()
        {
            return new DbObjectName(Schema, Name + "_temp");
        }

        public override string ToString()
        {
            return QualifiedName;
        }

        protected bool Equals(DbObjectName other)
        {
            return GetType() == other.GetType() &&
                   string.Equals(QualifiedName, other.QualifiedName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DbObjectName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (GetType().GetHashCode() * 397) ^ (QualifiedName?.GetHashCode() ?? 0);
            }
        }
    }
}
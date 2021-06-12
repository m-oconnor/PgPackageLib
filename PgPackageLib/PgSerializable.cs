using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PgPackageLib
{
    /// <summary>
    /// apply this attribute to associate a class with a table (must be PgModel<ChildClass>)
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false)]
    public class Table : Attribute
    {
        public Type type;
        /// <summary>
        /// name of the table associated with this class (default = class name)
        /// </summary>
        public string TableName { get; set; }
    }

    /// <summary>
    /// apply this to a proprty to associate it with a column in the database
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
    public class Column : Attribute
    {
        public Type type;
        public Table table;
        public PropertyInfo property;
        public HasOne relation;
        public string dbColumnName;

        /// <summary>
        /// name of the database column associated with this property (default=property.Name)
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// override the type of the database column associated with this property (default=Pgsql.GetPostgresType)
        /// </summary>
        public string OverrideType { get; set; }
        /// <summary>
        /// index this column
        /// </summary>
        public bool Index { get; set; }
        /// <summary>
        /// add a unique index to this column
        /// </summary>
        public bool UniqueIndex { get; set; }
        /// <summary>
        /// add a unique constraint to this column
        /// </summary>
        public bool Unique { get; set; }
        /// <summary>
        /// add a not null constraint to this column
        /// </summary>
        public bool NotNull { get; set; }
        /// <summary>
        /// add a foreign key constraint to this column for the table associated with the given type (must be paired with ForeignKeyPropertyName)
        /// </summary>
        public Type ForeignKeyTable { get; set; }
        /// <summary>
        /// property of the foregin type to use for foreign key constraint (must be paird with ForeignKeyTable) 
        /// </summary>
        public string ForeignKeyPropertyName { get; set; }
    }

    /// <summary>
    /// add a relation for this column to the table associated with the given type (by default this."<thisColumnName>"=relationTarget."<thisTableName>Id")
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
    public class HasOne : Attribute
    {
        public Column column;
        public Column relationTargetColumn;

        /// <summary>
        /// unique name of this relation, this can be blank, if it is defined it must be passed to HasOne()
        /// </summary>
        public string RelationName { get; set; }

        public HasOne(Type relationTargetTableClass) { RelationTargetTable = relationTargetTableClass; }
        public Type RelationTargetTable { get; }

        /// <summary>
        /// the property of the relation on the relation target class (default="<ThisTableName>Id")
        /// </summary>
        public string RelationTargetPropertyName { get; set; }
    }

    /// <summary>
    /// add a has many relation (or many to many relation using a join class)
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = true)]
    public class HasMany : Attribute
    {
        public Table table;
        public Column onColumn;
        public Column relationTargetColumn;
        public Column joinColumn;
        public HasMany joinRelation;

        /// <summary>
        /// unique name of this relation, this can be blank, if it is defined it must be passed to HasMany()
        /// </summary>
        public string RelationName { get; set; }

        public HasMany(Type relationTargetTableClass) { RelationTargetTable = relationTargetTableClass; }
        public Type RelationTargetTable { get; }
        /// <summary>
        /// the property on the associated relation table (default="<thisTableName>Id") - do not use with JoinTable
        /// </summary>
        public string RelationTargetPropertyName { get; set; }

        /// <summary>
        /// the property on this table that should be used for the relation (default=id)
        /// (this must be defined in the class and tagged as a valid column)
        /// </summary>
        public string OnPropertyName { get; set; }

        /// <summary>
        /// handle the relation through this table (for a many to many relation)
        /// this must be defined on the target relation class as well if it is used
        /// </summary>
        public Type JoinTable { get; set; }
        /// <summary>
        /// the property of the join table representing THIS class (default="<thisTableName>Id")
        /// </summary>
        public string JoinTablePropertyName { get; set; }
    }
}
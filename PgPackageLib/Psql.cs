using Npgsql;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PgPackageLib
{
    public class Psql
    {
        private static string dbConnectionString;
        private static Type[] namespaceTypes;

       
        //Server=127.0.0.1;Port=5432;Database=myDataBase;Userid=myUsername;Password=myPassword;Protocol=3;Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionLifeTime=15;
        public static void Initialize(string dbConfigString, params Type[] types) //"Host=localhost;Username=postgres;Password=password;Database=postgres;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=100;";
        {
            if (types.Length == 0) { throw new Exception("Must pass some type"); }
            dbConnectionString = dbConfigString;
            namespaceTypes = types;
            PrepareAttributes(namespaceTypes);
        }

        public static IEnumerable<Type> GetAllPgModelTypes()
        {
            if (namespaceTypes == null) { throw new Exception("Must initialize"); }
            List<Type> types = new List<Type> { };
            foreach (Type projectType in namespaceTypes)
            {
                types.AddRange(projectType.Assembly.GetTypes().Where(type =>
                    type.BaseType != null && type.BaseType.IsGenericType &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(PgModel<>)));
            }
            return types;
        }

        private static void PrepareAttributes(Type[] projectTypes)
        {
            IEnumerable<Type> types = GetAllPgModelTypes();
            foreach (Type type in types)
            {
                Table table = type.GetCustomAttribute<Table>();
                if (table == null) { continue; }
                table.type = type;
                table.dbTableName = table.TableName != null ? table.TableName : type.Name;
                List<Column> columns = new List<Column> { };

                IEnumerable<HasOne> hasOnes = type.GetCustomAttributes<HasOne>();
                if (hasOnes.Count() != hasOnes.GroupBy(ho => $"{ho.RelationTargetTable}-{ho.RelationName}").Count())
                {
                    throw new Exception($"conflicting duplicate HasOne relations on {type.Name}");
                }

                IEnumerable<HasMany> hasManys = type.GetCustomAttributes<HasMany>();
                if (hasManys.Count() != hasManys.GroupBy(hm => $"{hm.RelationTargetTable}-{hm.RelationName}").Count())
                {
                    throw new Exception($"conflicting duplicate HasMany relations on {type.Name}");
                }

                IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(property => property.GetCustomAttribute(typeof(Column)) != null);

                foreach (PropertyInfo property in properties)
                {
                    Column column = property.GetCustomAttribute<Column>();
                    columns.Add(column);
                    column.property = property;
                    column.type = type;
                    column.table = table;
                    column.dbColumnName = column.ColumnName != null ? column.ColumnName : property.Name;
                    if (column.ForeignKeyTable != null)
                    {
                        string foreignKeyPropertyName = column.ForeignKeyPropertyName != null ? column.ForeignKeyPropertyName : "id";
                        if (column.ForeignKeyTable.GetProperty(foreignKeyPropertyName) == null)
                        {
                            throw new Exception($"Foreign key property ({column.ForeignKeyTable.Name}.{foreignKeyPropertyName}) does not exist");
                        }
                    }

                    HasOne hasOne = property.GetCustomAttribute<HasOne>();
                    if (hasOne != null)
                    {
                        column.relation = hasOne;
                        hasOne.column = column;
                        string relationTargetPropertyName = hasOne.RelationTargetPropertyName != null ? hasOne.RelationTargetPropertyName : "id";
                        PropertyInfo relationTargetProperty = hasOne.RelationTargetTable.GetProperty(relationTargetPropertyName);
                        if (relationTargetProperty == null)
                        {
                            throw new Exception($"missing RelationTargetTable for {type.Name} hasone relation on {hasOne.RelationTargetTable.Name}.{relationTargetPropertyName}");
                        }
                        PgModel.AddHasOneRelation(type, hasOne);
                    }
                }
                PgModel.SetColumns(type, columns.ToArray());
                foreach (HasMany hasMany in hasManys)
                {
                    hasMany.table = table;
                    Type relationType = hasMany.RelationTargetTable;
                    string onPropertyName = hasMany.OnPropertyName != null ? hasMany.OnPropertyName : "id";
                    PropertyInfo onProperty = type.GetProperty(onPropertyName);
                    if (onProperty == null) { throw new Exception($"invalid OnPropertyName ({onPropertyName}) for hasmany relation on {type.Name}"); }
                    Column onColumn = PgModel<Object>.GetColumns(type).Single(col => col.property == onProperty);
                    hasMany.onColumn = onColumn;
                    PgModel.AddHasManyRelation(type, hasMany);
                }
                PgModel.SetTable(type, table);
            }

            // iterate a second time to make sure all of the columns are seeded before we add the more complicated has many stuff
            foreach (Table table in PgModel<Object>.GetAllTables())
            {
                Type type = table.type;
                HasOne[] hasOnes = PgModel<Object>.GetHasOneRelations(type);
                if (hasOnes != null)
                {
                    foreach (HasOne hasOne in hasOnes)
                    {
                        string relationTargetPropertyName = hasOne.RelationTargetPropertyName != null ? hasOne.RelationTargetPropertyName : "id";
                        Column relationTargetColumn = PgModel<Object>.GetColumns(hasOne.RelationTargetTable).Single(col => col.property.Name == relationTargetPropertyName);
                        hasOne.relationTargetColumn = relationTargetColumn;
                    }
                }
                HasMany[] hasManys = PgModel<Object>.GetHasManyRelations(type);
                if (hasManys != null)
                {
                    foreach (HasMany hasMany in hasManys)
                    {
                        Type relationType = hasMany.RelationTargetTable;
                        if (hasMany.JoinTable == null)
                        {
                            string relationPropertyName = hasMany.RelationTargetPropertyName != null ? hasMany.RelationTargetPropertyName : $"{type.Name}Id";
                            PropertyInfo relationProperty = relationType.GetProperty(relationPropertyName);
                            if (relationProperty == null) { throw new Exception($"HasMany.RelationTargetPropertyName ({relationPropertyName}) is not present on '{relationType.Name}'"); }
                            if (relationProperty.GetCustomAttribute<Column>() == null) { throw new Exception($"HasMany.RelationTargetPropertyName ({relationPropertyName}) does not have 'Column' Attribute"); }

                            Column relationColumn = PgModel<Object>.GetColumns(relationType).Single(col => col.property == relationProperty);
                            hasMany.relationTargetColumn = relationColumn;
                        }

                        else
                        {
                            string joinPropertyName = hasMany.JoinTablePropertyName != null ? hasMany.JoinTablePropertyName : $"{type.Name}Id";
                            PropertyInfo joinProperty = hasMany.JoinTable.GetProperty(joinPropertyName);
                            if (joinProperty == null) { throw new Exception($"HasMany joined table ({hasMany.JoinTable.Name}) missing valid property ({joinPropertyName}) for {type.Name}"); }

                            Column joinColumn = PgModel<Object>.GetColumns(hasMany.JoinTable).Single(col => col.property == joinProperty);
                            hasMany.joinColumn = joinColumn;

                            HasMany joinRelation;
                            try
                            {
                                joinRelation = PgModel<Object>.GetHasManyRelations(relationType).Single(hm => hm.RelationTargetTable == type && hm.RelationName == hasMany.RelationName);
                                hasMany.joinRelation = joinRelation;
                            }
                            //TODO why does this throw 2 different exceptions?
                            catch (System.ArgumentNullException) { throw new Exception($"missing hasmany relation {relationType.Name}->{type.Name} (name:{hasMany.RelationName}) (join:{hasMany.JoinTable.Name})"); }
                            catch (System.InvalidOperationException) { throw new Exception($"missing hasmany relation {relationType.Name}->{type.Name} (name:{hasMany.RelationName}) (join:{hasMany.JoinTable.Name})"); }
                        }

                        if (hasMany.RelationTargetPropertyName != null || hasMany.OnPropertyName != null)
                        {
                            throw new Exception("Can not use RelationTargetPropertyName or OnPropertyName with JoinTable");
                        }
                    }
                }
            }
        }


        public static string GetPostgresType(string Type)
        {
            if (Type == "String")
            {
                return "TEXT";
            }
            else if (Type == "Int32")
            {
                return "INTEGER";
            }
            else if (Type == "Single")
            {
                return "REAL";
            }
            else if (Type == "Double")
            {
                return "DOUBLE PRECISION";
            }
            else if (Type == "DateTime")
            {
                return "TIMESTAMP";
            }
            else if (Type == "Boolean")
            {
                return "BOOLEAN";
            }

            throw new Exception("unexpected type");
        }

        public static void ExecuteCommand(string CommandString, object[] vals = default)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(dbConnectionString))
            { 
                NpgsqlCommand command = new NpgsqlCommand(CommandString, conn);
                if (vals != null)
                {
                    for (int i = 0; i < vals.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@{i}", vals[i]);
                    }
                }
                conn.Open();
                command.ExecuteReader();
            }
        }

        public static NpgsqlCommand GetCommand(string CommandString)
        {
            return new NpgsqlCommand(CommandString, new NpgsqlConnection(dbConnectionString));
        }
    }
}

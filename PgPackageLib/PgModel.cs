using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PgPackageLib.PgQuery;

namespace PgPackageLib
{
    public class PgModel
    {
        public static bool autoSave = false;
        protected static Dictionary<Type, Table> cachedTables { get; set; } = new Dictionary<Type, Table> { };
        public static void SetTable(Type tableClass, Table table)
        {
            cachedTables[tableClass] = table;
        }
        public static Table[] GetAllTables()
        {
            return cachedTables.Values.ToArray();
        }

        protected static Dictionary<Type, Dictionary<string, Column>> cachedColumns { get; set; } = new Dictionary<Type, Dictionary<string, Column>> { };
        public static void SetColumns(Type tableClass, Column[] columns)
        {
            cachedColumns[tableClass] = columns.ToDictionary(col => col.property.Name, col => col);
        }

        protected static Dictionary<Type, HasOne[]> cachedHasOneRelations { get; set; } = new Dictionary<Type, HasOne[]> { };
        public static void AddHasOneRelation(Type tableClass, HasOne hasOne)
        {
            List<HasOne> hasOneList = cachedHasOneRelations.ContainsKey(tableClass) ? cachedHasOneRelations[tableClass].ToList() : new List<HasOne> { };
            hasOneList.Add(hasOne);
            cachedHasOneRelations[tableClass] = hasOneList.ToArray();
        }

        protected static Dictionary<Type, HasMany[]> cachedHasManyRelations { get; set; } = new Dictionary<Type, HasMany[]> { };
        public static void AddHasManyRelation(Type tableClass, HasMany hasMany)
        {
            List<HasMany> hasManyList = cachedHasManyRelations.ContainsKey(tableClass) ? cachedHasManyRelations[tableClass].ToList() : new List<HasMany> { };
            hasManyList.Add(hasMany);
            cachedHasManyRelations[tableClass] = hasManyList.ToArray();
        }

    }
    public class PgModel<ChildClass> : PgModel where ChildClass : new()
    {
        [Column(OverrideType = "SERIAL PRIMARY KEY")]
        public int id { get; set; }

        public static Table GetTable(Type tableClass)
        {
            return cachedTables[tableClass];         
        }
        public static Table GetTable()
        {
            return GetTable(typeof(ChildClass));
        }

        public static Column[] GetColumns(Type tableClass)
        {
            try
            {
                return cachedColumns[tableClass].Values.ToArray();
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new Exception($"{tableClass.Name} does not seem to be a valid PgModel Table, the class may be missing the [Table] attribute or may have not initialized properly");
            }
        }
        
        public static Column[] GetColumns()
        {
            return GetColumns(typeof(ChildClass));
        }

        public static HasOne[] GetHasOneRelations(Type tableClass)
        {
            if (cachedHasOneRelations.ContainsKey(tableClass)) { return cachedHasOneRelations[tableClass]; }
            return default;
        }
        public static HasOne[] GetHasOneRelations()
        {
            return GetHasOneRelations(typeof(ChildClass));
        }

        public static HasMany[] GetHasManyRelations(Type tableClass)
        {
            if (cachedHasManyRelations.ContainsKey(tableClass)) { return cachedHasManyRelations[tableClass]; }
            return default;
        }
        public static HasMany[] GetHasManyRelations()
        {
            return GetHasManyRelations(typeof(ChildClass));
        }

        public static string GetTableName(Type type)
        {
            Table table = GetTable(type);
            return table.TableName != null ? table.TableName : type.Name;
        }

        public static string GetTableName()
        {
            return GetTableName(typeof(ChildClass));
        }

        public static Column GetColumn(PropertyInfo property)
        {
            return cachedColumns[property.ReflectedType][property.Name];
        }
        public static Column GetColumn(Type type, string propertyName)
        {
            return cachedColumns[type][propertyName];
        }

        public static void DropTable()
        {
            string commandString = $"DROP TABLE IF EXISTS {GetTableName()} CASCADE;";
            Psql.ExecuteCommand(commandString);
            
        }

        public static void DropAllTables()
        {
            foreach (Table table in GetAllTables())
            {
                string commandString = $"DROP TABLE IF EXISTS {GetTableName(table.type)} CASCADE;";
                Psql.ExecuteCommand(commandString);
            
            }
        }

        private object GetValue(string propertyName)
        {
            return GetValue(GetType().GetRuntimeProperty(propertyName));
        }
        private object GetValue(PropertyInfo property)
        {
            return property.GetValue(this);
        }

        public void Save()
        {
            Column[] columns = GetColumns();
            Dictionary<PropertyInfo, Column> dict = new Dictionary<PropertyInfo, Column> { };
            foreach (Column column in columns)
            {
                PropertyInfo property = column.property;
                if (property.Name == "id") { continue; }
                dict[property] = column;
            }

            object result;
            if (this.id == 0)
            {
                result = Insert<ChildClass>(dict.Values.Select(column => column.dbColumnName).ToArray(), dict.Keys.Select(property => property.GetValue(this)).ToArray())
                                               .Execute().FirstOrDefault();
            }
            else
            {
                result = (Update<ChildClass>(dict.Values.Select(column => column.dbColumnName).ToArray(), dict.Keys.Select(property => property.GetValue(this)).ToArray())
                                               .Where("id", this.id)
                                               .Execute().FirstOrDefault());
            }

            this.id = ((PgModel<ChildClass>)result).id;
        }

        public static IEnumerable<ChildClass> FindAll(int[] queryIds)
        {
            return Select<ChildClass>()
                   .Where("id", queryIds.Cast<object>().ToArray())
                   .Execute<ChildClass>();
        }
        public static ChildClass Find(int queryId)
        {
            return Select<ChildClass>().Where("id", queryId)
                                       .Limit(1)
                                       .Execute<ChildClass>().FirstOrDefault();
        }


        private string GetRelationCacheKey(Type type, string relationMethod, Type relationType, string relationName) { return $"{relationMethod}-{type.Name}-{relationType.Name}-{relationName}"; }
        private string GetRelationCacheKey(string relationMethod, Type relationType, string relationName) { return GetRelationCacheKey(GetType(), relationMethod, relationType, relationName); }

        private Dictionary<string, object> cachedRelationResults = new Dictionary<string, object>() { };
        public RelationTableClass HasOne<RelationTableClass>(bool refresh = false) { return HasOne<RelationTableClass>(null, refresh); }
        public RelationTableClass HasOne<RelationTableClass>(string relationName, bool refresh = false)
        {
            if (this.id == 0)
            {
                if (autoSave) { this.Save(); }
                else { throw new Exception("instance has no id set, it has not been saved to the database, set PgModelBase.autoSave to true to implicitly save in these cases"); }
            }

            Type relationTableType = typeof(RelationTableClass);
            string cacheKey = GetRelationCacheKey("HasOne", relationTableType, relationName);
            if (refresh == false && this.cachedRelationResults.ContainsKey(cacheKey))
            {
                return (RelationTableClass)this.cachedRelationResults[cacheKey];
            }

            HasOne hasOne = GetHasOne(relationTableType, relationName);
            Column columnm = hasOne.column;
            PropertyInfo property = columnm.property;

            object value = property.GetValue(this);

            Column relationColumn = hasOne.relationTargetColumn;
            string dbRelationColumnName = relationColumn.dbColumnName;

            RelationTableClass result = Select<RelationTableClass>()
                            .Where(dbRelationColumnName, value)
                            .Limit(1)
                            .Execute<RelationTableClass>().FirstOrDefault();

            this.cachedRelationResults[cacheKey] = result;
            return result;
        }

        public void Set<RelationTableClass>(RelationTableClass newInstance, string relationName = null)
        {
            if (this.id == 0)
            {
                if (autoSave) { this.Save(); }
                else { throw new Exception("instance has no id set, it has not been saved to the database, set PgModelBase.autoSave to true to implicitly save in these cases"); }
            }

            Type relationTableType = typeof(RelationTableClass);
            HasOne hasOne = GetHasOne(relationTableType, relationName);
            Column onColumn = hasOne.column;
            PropertyInfo onProperty = onColumn.property;

            Column relationColumn = hasOne.relationTargetColumn;
            PropertyInfo relationProperty = relationColumn.property;
            object value = relationProperty.GetValue(newInstance);

            onProperty.SetValue(this, value);
            this.Save();

            // update the cache
            this.HasOne<RelationTableClass>(relationName, true);
        }

        public static HasOne GetHasOne(Type callingClass, Type relationTargetClass, string relationName = null)
        {
            try { return GetHasOneRelations(callingClass).Single(hasOne => hasOne.RelationTargetTable == relationTargetClass && hasOne.RelationName == relationName); }
            catch (System.InvalidOperationException) { throw new Exception($"HasOne relation on {callingClass.Name}->{relationTargetClass.Name} (name:{relationName}) does not exist"); }
        }
        public static HasOne GetHasOne(Type relationTargetClass, string relationName = null)
        {
            return GetHasOne(typeof(ChildClass), relationTargetClass, relationName);
        }

        public static HasMany GetHasMany(Type callingClass, Type relationTargetClass, string relationName = null)
        {
            try { return GetHasManyRelations(callingClass).Single(hasMany => hasMany.RelationTargetTable == relationTargetClass && hasMany.RelationName == relationName); }
            catch (System.InvalidOperationException) { throw new Exception($"HasMany relation on {callingClass.Name}->{relationTargetClass.Name} (name:{relationName}) does not exist"); }
        }
        public static HasMany GetHasMany(Type relationTargetClass, string relationName = null)
        {
            return GetHasMany(typeof(ChildClass), relationTargetClass, relationName);
        }

        public IEnumerable<RelationTableClass> HasMany<RelationTableClass>(bool refresh = false) { return HasMany<RelationTableClass>(null, refresh); }
        public IEnumerable<RelationTableClass> HasMany<RelationTableClass>(string relationName, bool refresh = false)
        {
            if (this.id == 0)
            {
                if (autoSave) { this.Save(); }
                else { throw new Exception("instance has no id set, it has not been saved to the database, set PgModelBase.autoSave to true to implicitly save in these cases"); }
            }

            Type relationTableType = typeof(RelationTableClass);
            string cacheKey = GetRelationCacheKey("HasMany", relationTableType, relationName);
            if (refresh == false && this.cachedRelationResults.ContainsKey(cacheKey))
            {
                return (List<RelationTableClass>)this.cachedRelationResults[cacheKey];
            }

            HasMany hasMany = GetHasMany(relationTableType, relationName);
            Column onColumn = hasMany.onColumn;
            PropertyInfo onProperty = onColumn.property;
            object onValue = onProperty.GetValue(this);

            PgQuery query = Select<RelationTableClass>();

            Type joinTableType = hasMany.JoinTable;
            if (joinTableType == null)
            {
                Column relationColumn = hasMany.relationTargetColumn;
                query.Where(relationColumn.dbColumnName, onValue);
            }
            else
            {
                Column joinColumn = hasMany.joinColumn;

                HasMany joinRelation = hasMany.joinRelation;
                Column relationOnColumn = joinRelation.onColumn;
                Column relationJoinColumn = joinRelation.joinColumn;

                query = query.Join(joinTableType, relationJoinColumn.dbColumnName, relationTableType, relationOnColumn.dbColumnName);
                query = query.Join(this.GetType(), onColumn.dbColumnName, joinTableType, joinColumn.dbColumnName);
                query = query.Where(joinTableType, joinColumn.dbColumnName, onValue);
            }

            IEnumerable<RelationTableClass> result = query.Execute<RelationTableClass>();
            this.cachedRelationResults[cacheKey] = result;

            return result;
        }


        public void Add<RelationTableClass>(RelationTableClass newInstance, string relationName = null)
        {
            if (this.id == 0)
            {
                if (autoSave) { this.Save(); }
                else { throw new Exception("instance has no id set, it has not been saved to the database, set PgModelBase.autoSave to true to implicitly save in these cases"); }
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            Type relationTableType = typeof(RelationTableClass);
            HasMany hasMany = GetHasMany(relationTableType, relationName);
            Column onColumn = hasMany.onColumn;
            object onValue = GetValue(onColumn.property);

            if (hasMany.JoinTable == null)
            {
                Column relationColumn = hasMany.relationTargetColumn;
                PropertyInfo relationProperty = relationColumn.property;
                relationProperty.SetValue(newInstance, onValue);
                relationTableType.GetMethod("Save", flags).Invoke(newInstance, null);

                // update the cache here
                this.HasMany<RelationTableClass>(relationName, true);
            }
            else
            {
                PgQuery query = Select(hasMany.JoinTable);
                Type joinTableType = hasMany.JoinTable;

                Column joinColumn = hasMany.joinColumn;

                HasMany joinRelation = hasMany.joinRelation;
                Column relationOnColumn = joinRelation.onColumn;
                Column relationJoinColumn = joinRelation.joinColumn;
                PropertyInfo relationOnProperty = relationOnColumn.property;
                object relationOnValue = relationOnProperty.GetValue(newInstance);

                object result = Select(joinTableType)
                                .Where(joinTableType, joinColumn.dbColumnName, onValue)
                                .And()
                                .Where(joinTableType, relationJoinColumn.dbColumnName, relationOnValue)
                                .Limit(1)
                                .Execute().FirstOrDefault();

                if (result != null)
                {
                    cachedRelationResults.Remove(GetRelationCacheKey("HasMany", relationTableType, relationName));
                    cachedRelationResults.Remove(GetRelationCacheKey(relationTableType, "HasMany", GetType(), relationName));
                }
                else
                {
                    //Insert(joinTableType);
                    object obj = Activator.CreateInstance(joinTableType);
                    PropertyInfo joinColumnProperty = joinColumn.property;
                    PropertyInfo relationJoincolumnProperty = relationJoinColumn.property;
                    joinColumnProperty.SetValue(obj, onValue);
                    relationJoincolumnProperty.SetValue(obj, relationOnValue);
                    joinTableType.GetMethod("Save").Invoke(obj, null);
                }
            }
        }
    }
}

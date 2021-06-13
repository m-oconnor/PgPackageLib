using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PgPackageLib
{
    public class PgQuery
    {
        // ////////////////////////////////////////
        string queryMethod;
        Type queryClass;
        string queryClassTableName;
        string[] queryFields;
        object[] queryValues;
        private static PgQuery BaseQuery(Type queryTableType, string queryMethodName, string[] fields, object[] values)
        {
            if (queryMethodName == "UPDATE" && values.Length == 0) { throw new Exception($"no values provides for UPDATE on {queryTableType.Name}"); }
            PgQuery pgQuery = new PgQuery();
            pgQuery.queryMethod = queryMethodName;
            pgQuery.queryClass = queryTableType;
            pgQuery.queryClassTableName = PgModel<Object>.GetTableName(queryTableType);
            pgQuery.queryFields = fields;
            pgQuery.queryValues = values.Select(val => val != null ? val : DBNull.Value).ToArray();
            return pgQuery;
        }

        public static PgQuery Select(Type queryTableType) { return BaseQuery(queryTableType, "SELECT", new string[] { "*" }, new object[] { }); }
        public static PgQuery Select<QueryTableClass>() { return Select(typeof(QueryTableClass)); }

        public static PgQuery Insert(Type queryTableType, string[] fields, object[] values) { return BaseQuery(queryTableType, "INSERT", fields, values); }
        public static PgQuery Insert<QueryTableClass>(string[] fields, object[] values) { return BaseQuery(typeof(QueryTableClass), "INSERT", fields, values); }
        public static PgQuery Insert(Type queryTableType, string field, object value) { return BaseQuery(queryTableType, "INSERT", new string[] { field }, new object[] { value }); }
        public static PgQuery Insert<QueryTableClass>(string field, object value) { return BaseQuery(typeof(QueryTableClass), "INSERT", new string[] { field }, new object[] { value }); }

        public static PgQuery Update(Type queryTableType, string[] fields, object[] values) { return BaseQuery(queryTableType, "UPDATE", fields, values); }
        public static PgQuery Update<QueryTableClass>(string[] fields, object[] values) { return BaseQuery(typeof(QueryTableClass), "UPDATE", fields, values); }
        public static PgQuery Update(Type queryTableType, string field, object value) { return BaseQuery(queryTableType, "UPDATE", new string[] { field }, new object[] { value }); }
        public static PgQuery Update<QueryTableClass>(string field, object value) { return BaseQuery(typeof(QueryTableClass), "UPDATE", new string[] { field }, new object[] { value }); }



        // ////////////////////////////////////////
        List<List<object>> whereList;
        private PgQuery BaseWhere(Type whereClass, string[] fields, string opp, object[] values)
        {
            if (this.whereList == null) { whereList = new List<List<object>> { }; }
            else if (this.whereList.Count % 2 == 1) { new List<object> { And() }; }
            this.whereList.Add(new List<object> { whereClass, fields, opp, values });
            return this;
        }

        public PgQuery Where<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, "=", values); }
        public PgQuery Where(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, "=", values); }
        public PgQuery Where(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, "=", values); }
        public PgQuery Where<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, "=", new object[] { value }); }
        public PgQuery Where(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, "=", new object[] { value }); }
        public PgQuery Where(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, "=", new object[] { value }); }

        public PgQuery WhereGt<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, ">", values); }
        public PgQuery WhereGt(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, ">", values); }
        public PgQuery WhereGt(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, ">", values); }
        public PgQuery WhereGt<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, ">", new object[] { value }); }
        public PgQuery WhereGt(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, ">", new object[] { value }); }
        public PgQuery WhereGt(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, ">", new object[] { value }); }

        public PgQuery WhereGtE<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, ">=", values); }
        public PgQuery WhereGtE(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, ">=", values); }
        public PgQuery WhereGtE(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, ">=", values); }
        public PgQuery WhereGtE<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, ">=", new object[] { value }); }
        public PgQuery WhereGtE(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, ">=", new object[] { value }); }
        public PgQuery WhereGtE(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, ">=", new object[] { value }); }

        public PgQuery WhereLt<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, "<", values); }
        public PgQuery WhereLt(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, "<", values); }
        public PgQuery WhereLt(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, "<", values); }
        public PgQuery WhereLt<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, "<", new object[] { value }); }
        public PgQuery WhereLt(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, "<", new object[] { value }); }
        public PgQuery WhereLt(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, "<", new object[] { value }); }

        public PgQuery WhereLtE<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, "<=", values); }
        public PgQuery WhereLtE(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, "<=", values); }
        public PgQuery WhereLtE(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, "<=", values); }
        public PgQuery WhereLtE<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, "<=", new object[] { value }); }
        public PgQuery WhereLtE(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, "<=", new object[] { value }); }
        public PgQuery WhereLtE(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, "<=", new object[] { value }); }

        public PgQuery WhereLike<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, "LIKE", values); }
        public PgQuery WhereLike(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, "LIKE", values); }
        public PgQuery WhereLike(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, "LIKE", values); }
        public PgQuery WhereLike<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, "LIKE", new object[] { value }); }
        public PgQuery WhereLike(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, "LIKE", new object[] { value }); }
        public PgQuery WhereLike(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, "LIKE", new object[] { value }); }

        public PgQuery WhereIn<WhereTable>(string[] fields, object[] values) { return BaseWhere(typeof(WhereTable), fields, "IN", values); }
        public PgQuery WhereIn(Type whereTable, string[] fields, object[] values) { return BaseWhere(whereTable, fields, "IN", values); }
        public PgQuery WhereIn(string[] fields, object[] values) { return BaseWhere(this.queryClass, fields, "IN", values); }
        public PgQuery WhereIn<WhereTable>(string field, object value) { return BaseWhere(typeof(WhereTable), new string[] { field }, "IN", new object[] { value }); }
        public PgQuery WhereIn(Type whereTable, string field, object value) { return BaseWhere(whereTable, new string[] { field }, "IN", new object[] { value }); }
        public PgQuery WhereIn(string field, object value) { return BaseWhere(this.queryClass, new string[] { field }, "IN", new object[] { value }); }

        public PgQuery And() { whereList.Add(new List<object> { "AND" }); return this; }
        public PgQuery Or() { whereList.Add(new List<object> { "OR" }); return this; }



        // ////////////////////////////////////////
        List<List<object>> joinList;
        public PgQuery Join(Type joinTableA, string[] joinFieldsA, Type joinTableB, string[] joinFieldsB)
        {
            if (this.joinList == null) { this.joinList = new List<List<object>> { }; }
            this.joinList.Add(new List<object> { joinTableA, joinFieldsA, joinTableB, joinFieldsB });
            return this;
        }
        public PgQuery Join<JoinTableA, JoinTableB>(string[] joinFieldsA, string[] joinFieldsB) { return Join(typeof(JoinTableA), joinFieldsA, typeof(JoinTableB), joinFieldsB); }
        public PgQuery Join(Type joinTableA, string joinFieldA, Type joinTableB, string joinFieldB) { return Join(joinTableA, new string[] { joinFieldA }, joinTableB, new string[] { joinFieldB }); }
        public PgQuery Join<JoinTableA, JoinTableB>(string joinFieldA, string joinFieldB) { return Join(typeof(JoinTableA), new string[] { joinFieldA }, typeof(JoinTableB), new string[] { joinFieldB }); }



        // ////////////////////////////////////////
        int limit;
        public PgQuery Limit(int limit) { this.limit = limit; return this; }



        // /////////////////////////////////
        private string commandString;
        private int commandValueCount;
        private List<object> commandValuesList;
        // /////////////////////////////////////
        private void handleSelect()
        {
                commandString = $"SELECT {String.Join(", ", this.queryFields.ToList().Select(field => $"{this.queryClassTableName}.{field}"))} FROM {this.queryClassTableName}";
        }
        private void handleUpdate()
        {
            List<string> updateStrings = new List<string> { };
            for (int i = 0; i < this.queryValues.Length; i++)
            {
                updateStrings.Add($"{this.queryFields[i]} = @{commandValueCount}");
                commandValuesList.Add(this.queryValues[i]);
                commandValueCount++;
            }
            commandString = $"UPDATE {this.queryClassTableName} SET {String.Join(", ", updateStrings)}";
        }
        private void handleInsert()
        {
            List<object> insertValueList = new List<object> { };
            for (int i = 0; i < this.queryValues.Length; i++)
            {
                insertValueList.Add($"@{i}");
                commandValuesList.Add(queryValues[i]);
                commandValueCount++;
            }            
            commandString = $"INSERT INTO {this.queryClassTableName}";
            if (insertValueList.Count > 0)
            {
                commandString = $"{commandString} ({String.Join(", ", this.queryFields)})";
                commandString = $"{commandString} VALUES ({String.Join(", ", insertValueList)})";
            }
            else
            {
                commandString = $"{commandString} DEFAULT VALUES";
            }
        }
        private void handleJoin()
        {
            if (joinList == null) { return; }
            List<string> joinStrings = new List<string> { };
            for (int i = 0; i < this.joinList.Count; i++)
            {
                Type joinClassA = (Type)joinList[i][0];
                string joinClassATableName = PgModel<object>.GetTableName(joinClassA);
                string[] joinFieldsA = (string[])joinList[i][1];

                Type joinClassB = (Type)joinList[i][2];
                string joinClassBTableName = PgModel<object>.GetTableName(joinClassB);
                string[] joinFieldsB = (string[])joinList[i][3];
                
                string joinString = $"JOIN {joinClassATableName} ON";
                for (int j = 0; j < joinFieldsA.Length; j++)
                {
                    joinString = $"{joinString} {joinClassATableName}.{joinFieldsA[j]} = {joinClassBTableName}.{joinFieldsB[j]}";
                }
                joinStrings.Add(joinString);
            }
            commandString = $"{commandString} {String.Join(" ", joinStrings)}";
        }
        private void handleWhere()
        {
            if (whereList == null) { return; }
            commandString = $"{commandString} WHERE";
            List<string> whereStringList = new List<string> { };
            for (int i = 0; i < whereList.Count; i++)
            {
                string whereString = "";
                // this is an AND or an OR
                if (whereList[i].Count == 1) { whereString = (string)whereList[i][0]; }
                else
                {
                    Type whereClass = (Type)whereList[i][0];
                    string whereClassTableName = PgModel<object>.GetTableName(whereClass);
                    string[] whereFields = (string[])whereList[i][1];
                    string opp = (string)whereList[i][2];
                    object[] whereValues = (object[])whereList[i][3];
                    for (int j = 0; j < whereFields.Length; j++)
                    {
                        if (whereValues[j].GetType().IsArray)
                        {
                            if (opp == "IN")
                            {
                                whereString = $"{whereString} {whereClassTableName}.{whereFields[j]} {opp}";
                                whereString = $"{whereString} ({String.Join(",", ((object[])whereValues[j]).Select(val => $"@{commandValueCount++}"))})";
                                commandValuesList.AddRange((object[])whereValues[j]);
                            }
                            else
                            {
                                whereString = $"{whereString} ({String.Join("AND ", ((object[])whereValues[j]).Select(val => $"{whereFields[j]} {opp} @{commandValueCount++}"))})";
                                commandValuesList.AddRange((object[])whereValues[j]);
                            }
                        }
                        else
                        {
                            if (opp == "IN")
                            {
                                whereString = $"{whereString} {whereClassTableName}.{whereFields[j]} {opp} @{commandValueCount++}";
                                commandValuesList.AddRange(new object[] { whereValues[j] });
                            }
                            else
                            {
                                whereString = $"{whereString} {whereClassTableName}.{whereFields[j]} {opp} @{commandValueCount++}";
                                commandValuesList.Add(whereValues[j]);
                            }
                        }
                    }
                    whereString = $"({String.Join("AND ", whereString)})";
                }
                whereStringList.Add(whereString);
            }
            commandString = $"{commandString} {String.Join(" ", whereStringList)}";
        }     
        private void handleLimit()
        {
            if (this.limit > 0) { commandString = $"{commandString} LIMIT {this.limit}"; }
        }
        private void handleReturning()
        {
            if (this.queryMethod == "INSERT" || this.queryMethod == "UPDATE") { commandString = $"{commandString} RETURNING {this.queryClassTableName}.*"; }
        }
        private List<Table> handleQuery<Table>()
        {
            NpgsqlCommand command = Psql.GetCommand(commandString);
            for (int i = 0; i < commandValuesList.Count; i++)
            {
                command.Parameters.AddWithValue($"@{i}", commandValuesList[i]);
            }

            List<object> data = new List<object>();
            
            Column[] columns = PgModel<object>.GetColumns(this.queryClass);
            command.Connection.Open();
            NpgsqlDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                object obj = Activator.CreateInstance(this.queryClass);
                foreach (Column column in columns)
                {
                    PropertyInfo property = column.property;
                    
                    string fieldName = column.ColumnName != null ? column.ColumnName : property.Name;
                    if (result[fieldName] != null && result[fieldName].GetType() != typeof(DBNull))
                    {
                        property.SetValue(obj, result[fieldName]);
                    }
                }
                data.Add(obj);
            }
            result.Close();
            command.Connection.Close();
            return data.Cast<Table>().ToList();
        }
    


        // //////////////////////////////////////////
        public List<object> Execute() { return Execute<object>(); }
        public List<Table> Execute<Table>()
        {
            commandString = "";
            commandValueCount = 0;
            commandValuesList = new List<object> { };

            if (this.queryMethod == "SELECT")
            {
                handleSelect();
            }
            else if (this.queryMethod == "UPDATE")
            {
                handleUpdate();
            }
            else if (this.queryMethod == "INSERT")
            {
                handleInsert();
            }

            handleJoin();
            handleWhere();

            handleLimit();
            handleReturning();

            return handleQuery<Table>();
        }
    }
}
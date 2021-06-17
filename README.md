# PgPackageLib
An easy to use system for creating database models using postgres and Npgsql
## PgModel
Define a class
```C#
publc ClassName : PgModel<ClassName>
```
Add 'Table' attribute
```C#
[Table]
publc ClassName : PgModel<ClassName>
```
Define a property and add a 'Column' tag
```C#
[Table]
publc ClassName : PgModel<ClassName>
{
[Column]
public string Name { get; set; }
}
```
You now have access to a database model anmd all of the features/methods present in the library
## PgQuery
You can perform basic sql commands with your new class
```C#
PgQuery.Select<ClassName>().Where("id", 3).Limit(1).Execute<ClassName>();
```
You can also perform more complex queries.
```C#
PgQuery.Select<OtherClass>().Join<ClassName, OtherClass>("OtherClassId", "Id").Where("id", 3).Limit(1).Execute<OtherClass>();
```
You can also update or insert.

#Advanced attributes
The attribute tags have more advanced features as well.
### [Table]
```C#
[Table(TableName="SomeName")]
```
TableName: define an alternate table name for the associated table in postgres.
### [Column]
```C#
[Column(ColumnName="SomeName"]
...
[Column(OverrideType="VARCHAR(64)"]
...
[Column(Index=true)]
...
[Column(UniqueIndex=true)]
...
[Column(Unique=true)]
...
[Column(NotNull=true)]
...
[Column(ForeignKeyTable=typeof(OtherClass), ForeignKeyPropertyName="OtherClassPropertyName")]
```
ColumnName: define an alternate column name for the associated property in postgres.
OverrideType: override the default postgres type for the C# type.
Index/UniqueIndex/Unique/NotNull: create the given index/constraint.
ForeignKeyTable: add a foreign key constraint for this property for the table associated with the given class.
ForeignKeyPropertyName: define what field of the given foreign key table should be used for the constraint.

### Relations
There are also tags that can be used to define relations

```C#

```



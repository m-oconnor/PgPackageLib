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

## Relations
There are also tags that can be used to define relations
# [HasOne]
Place this below the column tag for the derired property.
```C#
[Table]
publc ClassName : PgModel<ClassName>
{
[Column]
[HasOne(typeof(OtherClass))]
public int otherClassId Name { get; set; }
}
```
Defining this relation will give you access to two new methods.
# HasOne
```C#
ClassName className = ClassName.Find(1);
OtherClass otherClass = className.HasOne<OtherClass>();
```
These are results cached, you can force an updated query by passing true;
```C#
OtherClass otherClass = className.HasOne<OtherClass>(true);
```
# Set
```C#
className.Set<OtherClass>(otherClassInstance);
```
Invoking Set will update the HasOne cached result.
You can also define a more direct protery name:
```C#
[Column]
[HasOne(typeof(OtherClass))]
public int otherClassId { get; set; }
public OtherClass OtherClass 
{
  get { return HasOne<OtherClass>(); }
  set { Set<OtherClass>(value); }
}
...
ClassName className = ClassName.Find(1);
OtherClass = className.OtherClass;
```

# Advanced
There are also more advanced options for the HasOne attribute.
```C#
[HasOne(RelationName="SomeName"]
```
RelationName: if this is defined you must pass this value to both HasOne and Set. This allows you to have multiple relations of the same type.
```C#
className.HasOne<OtherClass>("SomeName");
...
className.HasOne<OtherClass>("SomeName", true);  // refresh cached result
...
className.Set<OtherClass>(otherClassInstance, "SomeName");
```

```C#
[HasOne(RelationTargetPropertyName="SomePropertyName")]
```
RelationTargetPropertyName: an alternative property to use for your relation, by default it will be expected that the class associated with your relation contains a property named <YourClassName>Id. This allows you to override that and use any existing property.
  

using Dapper;
using System.Reflection;

namespace MealSync.Infrastructure.Services.Dapper;

public class TypeMap<T> : SqlMapper.ITypeMap
{
    private readonly ColumnTypeMapper<T> defaultTypeMap = new ColumnTypeMapper<T>();

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    => this.defaultTypeMap.FindConstructor(names, types);

    public ConstructorInfo? FindExplicitConstructor()
    => this.defaultTypeMap.FindExplicitConstructor();

    public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
    => this.defaultTypeMap.GetConstructorParameter(constructor, columnName);

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        List<SqlMapper.ITypeMap> fallbackMappers = new List<SqlMapper.ITypeMap>();
        fallbackMappers.Add(this.defaultTypeMap);

        FallbackTypeMapper fallbackMapper = new FallbackTypeMapper(fallbackMappers);

        var member = fallbackMapper.GetMember(columnName);
        if (member == null)
        {
            throw new Exception($"Column {columnName} cannot map to an object");
        }

        return member;
    }
}

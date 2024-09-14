using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MealSync.Infrastructure.Services.Dapper;

public class ColumnTypeMapper<T> : FallbackTypeMapper
{
    public ColumnTypeMapper()
        : base(new SqlMapper.ITypeMap[]
        {
            new CustomPropertyTypeMap(
                typeof(T),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop => prop.GetCustomAttributes(false).OfType<ColumnAttribute>()
                        .Any(attr => attr.Name == columnName))), new DefaultTypeMap(typeof(T)),

        })
    {
    }
}

public class FallbackTypeMapper : SqlMapper.ITypeMap
{
    private readonly IEnumerable<SqlMapper.ITypeMap> mappers;

    public FallbackTypeMapper(IEnumerable<SqlMapper.ITypeMap> mappers)
    {
        this.mappers = mappers;
    }

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    {
        foreach (var mapper in mappers)
        {
            try
            {
                ConstructorInfo result = mapper.FindConstructor(names, types);
                if (result != null)
                {
                    return result;
                }
            }
            catch (NotSupportedException)
            {
            }
        }

        return null;
    }

    public ConstructorInfo? FindExplicitConstructor()
    {
        return this.mappers.Select(mapper => mapper.FindExplicitConstructor())
            .FirstOrDefault(result => result != null);
    }

    public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
    {
        foreach (var mapper in mappers.Reverse())
        {
            try
            {
                var result = mapper.GetConstructorParameter(constructor, columnName);
                if (result != null)
                {
                    return result;
                }
            }
            catch (NotSupportedException)
            {
            }
        }

        return null;
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        foreach (var mapper in mappers)
        {
            try
            {
                var result = mapper.GetMember(columnName);
                if (result != null)
                {
                    return result;
                }
            }
            catch (NotSupportedException)
            {
            }
        }

        return null;
    }
}
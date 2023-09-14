using Ardalis.Specification;
using LinqToDB.Include;
using System.Linq.Expressions;
using System.Reflection;

namespace Ardalis.Specification.LinqToDb;

public class IncludeEvaluator : IEvaluator
{
    private static readonly MethodInfo _includeMethodInfo = typeof(IncludeExtensions)
        .GetTypeInfo().GetDeclaredMethods(nameof(IncludeExtensions.Include))
        .Single(mi => mi.GetGenericArguments().Length == 2
            && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>)
            && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
           )
        ;


    private static readonly CachedReadConcurrentDictionary<(Type EntityType, Type PropertyType, Type? PreviousPropertyType), Lazy<Func<IQueryable, LambdaExpression, IQueryable>>> _delegatesCache = new();

    private readonly bool _cacheEnabled;

    private IncludeEvaluator(bool cacheEnabled)
    {
        _cacheEnabled = cacheEnabled;
    }

    /// <summary>
    /// <see cref="IncludeEvaluator"/> instance without any additional features.
    /// </summary>
    public static IncludeEvaluator Default { get; } = new IncludeEvaluator(false);

    /// <summary>
    /// <see cref="IncludeEvaluator"/> instance with caching to provide better performance.
    /// </summary>
    public static IncludeEvaluator Cached { get; } = new IncludeEvaluator(true);

    public bool IsCriteriaEvaluator => false;

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        foreach (var includeString in specification.IncludeStrings)
        {
            //query = query.Include(includeString);
        }

        foreach (var includeInfo in specification.IncludeExpressions)
        {
            if (includeInfo.Type == IncludeTypeEnum.Include)
            {
                query = BuildInclude<T>(query, includeInfo);
            }
            else if (includeInfo.Type == IncludeTypeEnum.ThenInclude)
            {
                query = BuildInclude<T>(query, includeInfo);
            }
        }

        return query;
    }

    private IQueryable<T> BuildInclude<T>(IQueryable query, IncludeExpressionInfo includeInfo)
    {
        _ = includeInfo ?? throw new ArgumentNullException(nameof(includeInfo));

        if (!_cacheEnabled)
        {
            var args = _includeMethodInfo.GetGenericArguments();
            var ps = _includeMethodInfo.GetParameters();
            var result = _includeMethodInfo.MakeGenericMethod(includeInfo.EntityType, includeInfo.PropertyType).Invoke(null, new object[] { query, includeInfo.LambdaExpression, null });

            _ = result ?? throw new TargetException();

            return (IQueryable<T>)result;
        }

        var include = _delegatesCache.GetOrAdd((includeInfo.EntityType, includeInfo.PropertyType, null), CreateIncludeDelegate).Value;

        return (IQueryable<T>)include(query, includeInfo.LambdaExpression);
    }

   

    // (source, selector) => EntityFrameworkQueryableExtensions.Include<TEntity, TProperty>((IQueryable<TEntity>)source, (Expression<Func<TEntity, TProperty>>)selector);
    private static Lazy<Func<IQueryable, LambdaExpression, IQueryable>> CreateIncludeDelegate((Type EntityType, Type PropertyType, Type? PreviousPropertyType) cacheKey)
        => new(() =>
        {
            var concreteInclude = _includeMethodInfo.MakeGenericMethod(cacheKey.EntityType, cacheKey.PropertyType, null);
            var sourceParameter = Expression.Parameter(typeof(IQueryable));
            var selectorParameter = Expression.Parameter(typeof(LambdaExpression));

            var call = Expression.Call(
                  concreteInclude,
                  Expression.Convert(sourceParameter, typeof(IQueryable<>).MakeGenericType(cacheKey.EntityType)),
                  Expression.Convert(selectorParameter, typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(cacheKey.EntityType, cacheKey.PropertyType))));

            var lambda = Expression.Lambda<Func<IQueryable, LambdaExpression, IQueryable>>(call, sourceParameter, selectorParameter);

            return lambda.Compile();
        });


    private static bool IsGenericEnumerable(Type type, out Type propertyType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            propertyType = type.GenericTypeArguments[0];

            return true;
        }

        propertyType = type;

        return false;
    }
}

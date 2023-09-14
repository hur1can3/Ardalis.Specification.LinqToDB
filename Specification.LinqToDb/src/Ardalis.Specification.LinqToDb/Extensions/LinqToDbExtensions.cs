using LinqToDB;
using System.Linq.Expressions;

namespace Ardalis.Specification.LinqToDb
{
    public static class LinqToDbExtensions
    {
        public static TEntity? ObjectGetById<TEntity, TId>(this IDataContext dc, TId id, CancellationToken cancellationToken = default)
     where TEntity : class
        {
            var ed = dc.MappingSchema.GetEntityDescriptor(typeof(TEntity));

            // assume that we have only one PK Column
            var pk = ed.Columns.Single(c => c.IsPrimaryKey);

            var param = Expression.Parameter(typeof(TEntity), "e");

            var memberExpr = Expression.MakeMemberAccess(param, pk.MemberInfo);
            var idExpr = (Expression)Expression.Constant(id);

            if (idExpr.Type != memberExpr.Type)
                idExpr = Expression.Convert(idExpr, memberExpr.Type);

            // generating filter
            var filter = Expression.Equal(memberExpr, idExpr);
            var filterLambda = Expression.Lambda<Func<TEntity, bool>>(filter, param);

            return  dc.GetTable<TEntity>().FirstOrDefault(filterLambda);
        }

        public static async Task<TEntity?> ObjectGetByIdAsync<TEntity,TId>(this IDataContext dc, TId id, CancellationToken cancellationToken = default)
       where TEntity : class
        {
            var ed = dc.MappingSchema.GetEntityDescriptor(typeof(TEntity));

            // assume that we have only one PK Column
            var pk = ed.Columns.Single(c => c.IsPrimaryKey);

            var param = Expression.Parameter(typeof(TEntity), "e");

            var memberExpr = Expression.MakeMemberAccess(param, pk.MemberInfo);
            var idExpr = (Expression)Expression.Constant(id);

            if (idExpr.Type != memberExpr.Type)
                idExpr = Expression.Convert(idExpr, memberExpr.Type);

            // generating filter
            var filter = Expression.Equal(memberExpr, idExpr);
            var filterLambda = Expression.Lambda<Func<TEntity, bool>>(filter, param);

            return await dc.GetTable<TEntity>().FirstOrDefaultAsync(filterLambda);
        }
    }
}

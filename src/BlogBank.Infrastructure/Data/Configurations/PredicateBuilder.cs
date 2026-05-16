using System.Linq.Expressions;

namespace BlogBank.Infrastructure.Data.Configurations;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> New<T>(bool defaultVal = true)
        => defaultVal
            ? _ => true
            : _ => false;

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param   = left.Parameters[0];
        var visitor = new ReplaceParameterVisitor(right.Parameters[0], param);
        var body    = Expression.AndAlso(left.Body, visitor.Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param   = left.Parameters[0];
        var visitor = new ReplaceParameterVisitor(right.Parameters[0], param);
        var body    = Expression.OrElse(left.Body, visitor.Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

// 替换参数访问者（把两个 lambda 的参数统一成同一个）
public class ReplaceParameterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _old;
    private readonly ParameterExpression _new;

    public ReplaceParameterVisitor(ParameterExpression old, ParameterExpression @new)
    {
        _old = old;
        _new = @new;
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _old ? _new : base.VisitParameter(node);
}
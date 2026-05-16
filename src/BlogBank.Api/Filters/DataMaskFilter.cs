using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using BlogBank.Api.Tool;
using BlogBank.Core.Attributes;
using BlogBank.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogBank.Api.Filters;

// result filter 阶段，重新赋值结果，将结果进行加密
public class DataMaskFilter : IResultFilter
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, SensitiveAttribute?> _attrCache = new();

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: not null } objectResult)
            ProcessMask(objectResult.Value);
    }

    public void OnResultExecuted(ResultExecutedContext context) { }

    private void ProcessMask(object obj)
        => ProcessMask(obj, new HashSet<object>(ReferenceEqualityComparer.Instance));

    private void ProcessMask(object obj, HashSet<object> visited)
    {
        if (obj == null) return;

        var type = obj.GetType();

        // ✅ 扩展早退：涵盖所有不可能含敏感属性的类型
        if (type.IsPrimitive 
            || type == typeof(string) 
            || type.IsEnum
            || _skipTypes.Contains(type)) return;

        // ✅ 值类型也加入 visited（接受装箱开销，防止自引用属性）
        if (!visited.Add(obj)) return;

        if (obj is IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
                if (item != null) ProcessMask(item, visited);
            return;
        }

        var props = _propCache.GetOrAdd(type,
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead)
                .ToArray());

        foreach (var prop in props)
        {
            object? value;
            try { value = prop.GetValue(obj); }
            catch { continue; }

            if (value == null) continue;

            var maskAttr = _attrCache.GetOrAdd(prop,
                p => p.GetCustomAttribute<SensitiveAttribute>());

            if (maskAttr != null && value is string strVal)
            {
                if (prop.CanWrite)
                    prop.SetValue(obj, Masker.Mask(strVal, maskAttr));
            }
            else
            {
                ProcessMask(value, visited);
            }
        }
    }
    
    private static readonly HashSet<Type> _skipTypes = new()
    {
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
    };
}
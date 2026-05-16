using System.Reflection;
using BlogBank.Api.Tool;
using BlogBank.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogBank.Api.Filters;

public class DataMaskFilter: IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: not null } objectResult)
        {
            ProcessMask(objectResult.Value);
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        
    }
    
    private void ProcessMask(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();

        if (obj is System.Collections.IEnumerable enumerable && obj is not string)
        {
            foreach (var item in enumerable)
                ProcessMask(item);
            return;
        }

        if (type.IsPrimitive || type == typeof(string) || type.IsEnum) return;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // ✅ 跳过索引器属性（有参数的属性，如 this[int index]）
            if (prop.GetIndexParameters().Length > 0) continue;

            // ✅ 跳过只写属性
            if (!prop.CanRead) continue;

            object? value;
            try
            {
                value = prop.GetValue(obj);
            }
            catch
            {
                continue; // 取值失败就跳过，不影响其他属性
            }

            var maskAttr = prop.GetCustomAttribute<SensitiveAttribute>();

            if (maskAttr != null && value is string strVal)
            {
                if (prop.CanWrite)
                    prop.SetValue(obj, Masker.Mask(strVal, maskAttr));
            }
            else if (value != null)
            {
                ProcessMask(value);
            }
        }
    }
}
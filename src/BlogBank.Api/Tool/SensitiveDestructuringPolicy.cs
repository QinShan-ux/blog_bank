using System.Reflection;
using BlogBank.Api.Filters;
using Serilog.Core;
using Serilog.Events;

namespace BlogBank.Api.Tool;

public class SensitiveDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory factory,
        out LogEventPropertyValue result)
    {
        var type = value.GetType();

        // 只处理含有 SensitiveAttribute 的类
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var hasSensitive = properties.Any(p => p.GetCustomAttribute<SensitiveAttribute>() != null);

        if (!hasSensitive)
        {
            result = null;
            return false; // 交给 Serilog 默认处理
        }

        // 逐字段处理
        var logProps = new List<LogEventProperty>();

        foreach (var prop in properties)
        {   
            if (!prop.CanRead) continue;

            var rawValue = prop.GetValue(value);
            var sensitiveAttr = prop.GetCustomAttribute<SensitiveAttribute>();

            if (sensitiveAttr != null && rawValue is string strValue)
            {
                // 脱敏字段
                var masked = Masker.Mask(strValue, sensitiveAttr);
                logProps.Add(new LogEventProperty(prop.Name, new ScalarValue(masked)));
            }
            else
            {
                // 非敏感字段正常解构
                logProps.Add(new LogEventProperty(
                    prop.Name,
                    factory.CreatePropertyValue(rawValue, destructureObjects: true)));
            }
        }

        result = new StructureValue(logProps);
        return true;
    }
}
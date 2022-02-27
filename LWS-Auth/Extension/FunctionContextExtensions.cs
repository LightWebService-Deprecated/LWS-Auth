using System.Reflection;
using Microsoft.Azure.Functions.Worker;

namespace LWS_Auth.Extension;

public static class FunctionContextExtensions
{
    public static MethodInfo GetMethodInfo(this FunctionContext context)
    {
        var functionEntryPoint = context.FunctionDefinition.EntryPoint;
        var assembly = Assembly.LoadFrom(context.FunctionDefinition.PathToAssembly);

        var typeName = functionEntryPoint.Substring(0, functionEntryPoint.LastIndexOf('.'));
        var classType = assembly.GetType(typeName);

        var methodName = functionEntryPoint.Substring(functionEntryPoint.LastIndexOf('.') + 1);
        return classType.GetMethod(methodName);
    }

    public static void SetAccountId(this FunctionContext functionContext, string accountId)
    {
        functionContext.Items.Add("accountId", accountId);
    }
}
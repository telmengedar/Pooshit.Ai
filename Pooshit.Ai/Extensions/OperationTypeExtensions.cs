using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Extensions;

public static class OperationTypeExtensions {

    /// <summary>
    /// converts an operation to a display string
    /// </summary>
    /// <param name="type">operation type to convert</param>
    /// <returns>display string</returns>
    public static string ToDisplay(this OperationType type) {
        switch (type) {
            case OperationType.Add:
                return "+";
            case OperationType.Div:
                return "/";
            case OperationType.Multiply:
                return "*";
            case OperationType.Sub:
                return "-";
            case OperationType.Pow:
                return "^";
            case OperationType.InvPow:
                return "-^";
            case OperationType.Max:
                return"MAX";
            case OperationType.Min:
                return "MIN";
            default:
                return "?";
        }
    }
}
using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Extensions;

public static class OperationTypeExtensions {

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
            default:
                return "?";
        }
    }
}
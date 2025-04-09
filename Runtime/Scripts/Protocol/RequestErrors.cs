using System;

namespace ElympicsPlayPad.Protocol
{
    internal static class RequestErrors
    {
        public const int Unknown = 1;
        public const int AddressNotFound = 404;
        public const int NotImplementedOnWebComponent = 501;
        public const int SendTransactionFailure = 502;
        public const int FeatureUnavailable = 600;
        public const int ModalUnavailable = 700;
        public const int UserRejectedTheRequest = 4001;


        public static string GetErrorMessage(int code, string responseType) => code switch
        {
            0 => throw new ArgumentException($"Status code {responseType} is not an error."),
            Unknown => $"Unknown message type {responseType}",
            AddressNotFound => "Address not found",
            NotImplementedOnWebComponent => $"{responseType} is not implemented on web game component.",
            UserRejectedTheRequest => "User rejected the request",
            SendTransactionFailure => "Error executing chain transaction.",
            FeatureUnavailable => "Feature not available.",
            ModalUnavailable => "Cannot find the specified modal.",
            _ => $"Undefined error: {responseType}."
        };
    }
}

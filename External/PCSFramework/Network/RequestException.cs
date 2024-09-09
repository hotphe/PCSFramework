using System;
using Cysharp.Threading.Tasks;

namespace PCS.Network
{
    public enum ErrorCode
    {
        TimeOut = 0,
        Ban = 1,
        Mainttenance = 2,
        RequireAppUpdate = 3,
        RequireResourceUpdate = 4,
        InvalidResponseHash=5,
        UnknownError = 100
    }

    public class RequestException : Exception
    {
        public uint Code { get; }
        public CommonResponse CommonResponse { get; }
        public string ToUrl { get; }
        public string DisplayMessage { get; }

        public RequestException(uint code, string message, CommonResponse commonResponse = null, string toUrl = null, string displayMessage = null) : base(message)
        {
            Code = code;
            CommonResponse = commonResponse;
            ToUrl = toUrl;
            DisplayMessage = displayMessage;
        }
    }

    public class CompletedException : Exception
    {
        public CompletedException(Exception e) : base(e.Message, e) { }
    }
}

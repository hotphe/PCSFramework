using MessagePack;


namespace PCS.Network
{
    [MessagePackObject]
    public partial class Request
    {
        [Key(0)]public CommonRequest Common { get; set; }
    }

    [MessagePackObject]
    public partial class Response
    {
        [Key(0)] public CommonResponse Common { get; set; }
    }

    [MessagePackObject]
    public partial class CommonRequest
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string ApplicationVersion { get; set; }
        [Key(2)] public long UnixTime { get; set; }
        /// <summary> 1:Adnroid  2:IOS </summary>
        [Key(3)] public uint OS { get; set; }

    }

    [MessagePackObject]
    public partial class CommonResponse
    {
        [Key(0)] public long UnixTime { get; set; }
    }

    [MessagePackObject]
    public partial class Error
    {
        /// <summary> RequestException ErrorCode /// </summary>
        [Key(0)] public uint Code { get; set; }
        [Key(1)] public string Message { get; set; }
        [Key(2)] public CommonResponse Common { get; set; }
        [Key(3)] public string ToUrl { get; set; }
        [Key(4)] public string DisplayMessage { get; set; }
    }
}




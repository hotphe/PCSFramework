using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Security.Cryptography;
using PCS.Crypto;
using UniRx;
using Cysharp.Threading.Tasks;
using MessagePack;
using PCS.Common;

namespace PCS.Network
{
    public partial class Request
    {
        protected static readonly CommonRequest commonCache;
        protected static readonly HmacConfig config;
        protected static HMAC hmac;
        private static readonly CryptoManager crypto;
        private static readonly IntReactiveProperty ConnectingCount = new IntReactiveProperty(0);
        private static readonly Dictionary<uint, Action<RequestException>> errorHandlers;
        private static readonly Dictionary<uint, Func<RequestException, UniTask<bool>>> errorRetryHandlers;

        public static Action<CommonResponse> CommonResponseHandler;

        /// <summary>
        /// Request는 공통 데이터이기 때문에 static 생성자 사용(Request 객체 생성 시 최초 1회만 호출)
        /// </summary>
        static Request()
        {
            commonCache = new CommonRequest();
            config = BaseConfig.Instance.NetworkConfig;
            hmac = new HMACSHA256(Convert.FromBase64String(config.HmacKey));
            crypto = new CryptoManager(config.CryptoKey);
            errorHandlers = new Dictionary<uint, Action<RequestException>>();
            errorRetryHandlers = new Dictionary<uint, Func<RequestException, UniTask<bool>>>();
        }
        /// <summary>
        /// 확인 기능이 달린 에러 알림 팝업용
        /// </summary>
        public static void AddErrorHandler(uint errorCode, Action<RequestException> handler) => errorHandlers[errorCode] = handler;

        /// <summary>
        /// 재시도와 취소 기능이 달린 에러 알림 팝업용
        /// </summary>
        public static void AddErrorHandler(uint errorCode, Func<RequestException, UniTask<bool>> handler) => errorRetryHandlers[errorCode] = handler;

        protected async UniTask<ResponseType> ConnectAsync<RequestType, ResponseType>(string apiName) where RequestType : Request where ResponseType : Response
        {
            var unixTime = UnixTime.Now;
            while(true)
            {
                var req = CreateRequest<RequestType>(apiName, unixTime);
                ConnectingCount.Value++;
                try
                {
                    await req.SendWebRequest();
                }catch
                {
                    // Error handling is done below.
                }
                ConnectingCount.Value--;

                try
                {
                    var statusCode = (HttpStatusCode)req.responseCode;
                    if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.BadRequest) 
                    {
                        throw new RequestException((uint)req.responseCode, req.downloadHandler.text);
                    }

                    var bytes = crypto.Decrypt(req.downloadHandler.data);
                    if(statusCode == HttpStatusCode.BadRequest)
                    {
                        var error = MessagePackSerializer.Deserialize<Error>(bytes);

                        if(error.Code == (uint)ErrorCode.TimeOut && UnixTime.Now - unixTime > config.Timeout)
                        {
                            Debug.LogWarning($"TimeOut. Last:{unixTime}, Current:{UnixTime.Now}. Retry Connect.");
                            unixTime = UnixTime.Now;
                            continue;
                        }
                        throw new RequestException(error.Code, error.Message,error.Common,error.ToUrl,error.DisplayMessage);
                    }

                    if(GetHash(bytes) != req.GetResponseHeader(config.HmacName))
                    {
                        throw new RequestException((uint)ErrorCode.InvalidResponseHash, ErrorCode.InvalidResponseHash.ToString());
                    }

                    ResponseType res;
                    try
                    {
                        res = MessagePackSerializer.Deserialize<ResponseType>(bytes);
                    }catch(Exception e)
                    {
                        Debug.LogError($"Deserialize Failed. url : {apiName} , data : {MessagePackSerializer.ConvertToJson(bytes)} , error : {e}");
                        throw;
                    }
                    // 응답 시간 저장.
                    UnixTime.Now = res.Common.UnixTime;

                    CommonResponseHandler?.Invoke(res.Common);

                    return res;
                }catch(Exception e)
                {
                    // 요청 에러 캐스팅. 캐스팅에 실패 할 경우 UnknownError로 생성.
                    var exception = e as RequestException ?? new RequestException((uint)ErrorCode.UnknownError, ErrorCode.UnknownError.ToString());

                    if(errorHandlers.ContainsKey(exception.Code))
                    {
                        errorHandlers[exception.Code].Invoke(exception);
                        throw new CompletedException(e);
                    }
                    if(errorRetryHandlers.ContainsKey(exception.Code)) 
                    {
                        // 에러 발생 후 재시도시, true를 반환. 취소시 false를 반환.
                        if (await errorRetryHandlers[exception.Code].Invoke(exception)) 
                            continue;
                        throw new CompletedException(e);
                    }

                    // Unknown 에러 처리(팝업창 띄움)
                    errorHandlers[(uint)ErrorCode.UnknownError]?.Invoke(exception);
                    throw new CompletedException(e);
                }
            }
        }

        private UnityWebRequest CreateRequest<RequestType>(string apiName, long unixTime) where RequestType : Request
        {
            Common = new CommonRequest();
            Common.UserId = commonCache.UserId;
            Common.OS = commonCache.OS;
            Common.ApplicationVersion = Application.version;
            Common.UnixTime = unixTime;

            var bytes = MessagePackSerializer.Serialize(this as RequestType);

            var req = new UnityWebRequest(config.BaseURL + apiName, "POST");
            req.timeout = config.Timeout;
            req.uploadHandler = new UploadHandlerRaw(crypto.Encrypt(bytes));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader(config.HmacName, GetHash(bytes));
            return req;
        }

        private string GetHash(byte[] bytes) => Convert.ToBase64String(hmac.ComputeHash(bytes));
 

    }
}

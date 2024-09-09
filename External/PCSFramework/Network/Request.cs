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
        /// Request�� ���� �������̱� ������ static ������ ���(Request ��ü ���� �� ���� 1ȸ�� ȣ��)
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
        /// Ȯ�� ����� �޸� ���� �˸� �˾���
        /// </summary>
        public static void AddErrorHandler(uint errorCode, Action<RequestException> handler) => errorHandlers[errorCode] = handler;

        /// <summary>
        /// ��õ��� ��� ����� �޸� ���� �˸� �˾���
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
                    // ���� �ð� ����.
                    UnixTime.Now = res.Common.UnixTime;

                    CommonResponseHandler?.Invoke(res.Common);

                    return res;
                }catch(Exception e)
                {
                    // ��û ���� ĳ����. ĳ���ÿ� ���� �� ��� UnknownError�� ����.
                    var exception = e as RequestException ?? new RequestException((uint)ErrorCode.UnknownError, ErrorCode.UnknownError.ToString());

                    if(errorHandlers.ContainsKey(exception.Code))
                    {
                        errorHandlers[exception.Code].Invoke(exception);
                        throw new CompletedException(e);
                    }
                    if(errorRetryHandlers.ContainsKey(exception.Code)) 
                    {
                        // ���� �߻� �� ��õ���, true�� ��ȯ. ��ҽ� false�� ��ȯ.
                        if (await errorRetryHandlers[exception.Code].Invoke(exception)) 
                            continue;
                        throw new CompletedException(e);
                    }

                    // Unknown ���� ó��(�˾�â ���)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Security.Cryptography;
using PCS.Crypto;
using Cysharp.Threading.Tasks;
using MessagePack;
using PCS.Common;

namespace PCS.Network
{
    public partial class Request
    {
        protected static readonly CommonRequest _commonCache;
        protected static readonly HmacConfig _config;
        protected static HMAC _hmac;
        private static readonly CryptoManager _crypto;
        private static int _connectingCount;
        private static readonly Dictionary<uint, Action<RequestException>> _errorHandlers;
        private static readonly Dictionary<uint, Func<RequestException, UniTask<bool>>> _errorRetryHandlers;

        public static Action<CommonResponse> CommonResponseHandler;
        public static bool IsConnecting => _connectingCount > 0;
        /// <summary>
        /// Request�� ���� �������̱� ������ static ������ ���(Request ��ü ���� �� ���� 1ȸ�� ȣ��)
        /// </summary>
        static Request()
        {
            _commonCache = new CommonRequest();
            _config = BaseConfig.Instance.NetworkConfig;
            _hmac = new HMACSHA256(Convert.FromBase64String(_config.HmacKey));
            _crypto = new CryptoManager(_config.CryptoKey);
            _errorHandlers = new Dictionary<uint, Action<RequestException>>();
            _errorRetryHandlers = new Dictionary<uint, Func<RequestException, UniTask<bool>>>();
        }
        /// <summary>
        /// Ȯ�� ����� �޸� ���� �˸� �˾���
        /// </summary>
        public static void AddErrorHandler(uint errorCode, Action<RequestException> handler) => _errorHandlers[errorCode] = handler;

        /// <summary>
        /// ��õ��� ��� ����� �޸� ���� �˸� �˾���
        /// </summary>
        public static void AddErrorHandler(uint errorCode, Func<RequestException, UniTask<bool>> handler) => _errorRetryHandlers[errorCode] = handler;

        protected async UniTask<ResponseType> ConnectAsync<RequestType, ResponseType>(string apiName) where RequestType : Request where ResponseType : Response
        {
            var unixTime = UnixTime.Now;
            while(true)
            {
                var req = CreateRequest<RequestType>(apiName, unixTime);
                _connectingCount++;
                try
                {
                    await req.SendWebRequest();
                }catch
                {
                    // Error handling is done below.
                }
                _connectingCount--;

                try
                {
                    var statusCode = (HttpStatusCode)req.responseCode;
                    if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.BadRequest) 
                    {
                        throw new RequestException((uint)req.responseCode, req.downloadHandler.text);
                    }

                    var bytes = _crypto.Decrypt(req.downloadHandler.data);
                    if(statusCode == HttpStatusCode.BadRequest)
                    {
                        var error = MessagePackSerializer.Deserialize<Error>(bytes);

                        if(error.Code == (uint)ErrorCode.TimeOut && UnixTime.Now - unixTime > _config.Timeout)
                        {
                            Debug.LogWarning($"TimeOut. Last:{unixTime}, Current:{UnixTime.Now}. Retry Connect.");
                            unixTime = UnixTime.Now;
                            continue;
                        }
                        throw new RequestException(error.Code, error.Message,error.Common,error.ToUrl,error.DisplayMessage);
                    }

                    if(GetHash(bytes) != req.GetResponseHeader(_config.HmacName))
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

                    if(_errorHandlers.ContainsKey(exception.Code))
                    {
                        _errorHandlers[exception.Code].Invoke(exception);
                        throw new CompletedException(e);
                    }
                    if(_errorRetryHandlers.ContainsKey(exception.Code)) 
                    {
                        // ���� �߻� �� ��õ���, true�� ��ȯ. ��ҽ� false�� ��ȯ.
                        if (await _errorRetryHandlers[exception.Code].Invoke(exception)) 
                            continue;
                        throw new CompletedException(e);
                    }

                    // Unknown ���� ó��(�˾�â ���)
                    _errorHandlers[(uint)ErrorCode.UnknownError]?.Invoke(exception);
                    throw new CompletedException(e);
                }
            }
        }

        private UnityWebRequest CreateRequest<RequestType>(string apiName, long unixTime) where RequestType : Request
        {
            Common = new CommonRequest();
            Common.UserId = _commonCache.UserId;
            Common.OS = _commonCache.OS;
            Common.ApplicationVersion = Application.version;
            Common.UnixTime = unixTime;

            var bytes = MessagePackSerializer.Serialize(this as RequestType);

            var req = new UnityWebRequest(_config.BaseURL + apiName, "POST");
            req.timeout = _config.Timeout;
            req.uploadHandler = new UploadHandlerRaw(_crypto.Encrypt(bytes));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader(_config.HmacName, GetHash(bytes));
            return req;
        }

        private string GetHash(byte[] bytes) => Convert.ToBase64String(_hmac.ComputeHash(bytes));
 

    }
}

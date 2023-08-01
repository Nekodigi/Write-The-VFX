using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace ChatGPT {
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;

        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
    [System.Serializable]
    public class CompletionRequest
    {
        public string model;
        public List<Message> messages;
    }
    [System.Serializable]
    public class Response
    {
        public string id;
        public string @object;
        public int created;
        public Choice[] choices;
        public Usage usage;
        [System.Serializable]
        public class Choice
        {
            public int index;
            public Message message;
            public string finish_reason;
        }
        [System.Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

    public class ChatGPTBase : MonoBehaviour
    {


        public static void Request(List<Message> messages, Action<string> action)
        {
            var reqJson = JsonUtility.ToJson(new CompletionRequest()
            {
                model = "gpt-3.5-turbo",
                messages = messages
            }, true);
            Debug.Log(reqJson);
            var headers = new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + APIKEY.CHATGPT_KEY},
                { "Content-type", "application/json"},
                { "X-Slack-No-Retry", "1"}
            };
            var apiUrl = "https://api.openai.com/v1/chat/completions";
            var request = new UnityWebRequest(apiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(reqJson)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            String res = "";
            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (operation.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                           operation.webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(operation.webRequest.error);
                    Debug.LogError(operation.webRequest.result);

                    throw new Exception();
                }
                else
                {
                    var responseString = operation.webRequest.downloadHandler.text;
                    var responseObject = JsonUtility.FromJson<Response>(responseString);
                    res = responseObject.choices[0].message.content;
                    action(responseObject.choices[0].message.content);
                }
                request.Dispose();
            };
        }
    }
}
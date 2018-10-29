using UnityEngine;
using System;
#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
#else //Unity
using WebSocketSharp;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;

// class for rostopic publish
#region
public class Advertise
{
    public string op = "advertise";
    public string topic;
    public string type;

    public Advertise(string topic, string type)
    {
        this.topic = topic;
        this.type = type;
    }
}

public class UnAdvertise
{
    public string op = "unadvertise";
    public string topic;

    public UnAdvertise(string topic)
    {
        this.topic = topic;
    }
}

public class Publish
{
    public string op = "publish";
    public string topic;
    public object msg;

    public Publish(string topic, object msg)
    {
        this.topic = topic;
        this.msg = msg;
    }
}
#endregion

// class for rostopic subscribe
#region
public class Subscribe
{
    public string op = "subscribe";
    public string topic;
    public string type;

    public Subscribe(string topic, string type)
    {
        this.topic = topic;
        this.type = type;
    }
}

public class UnSubscribe
{
    public string op = "unsubscribe";
    public string topic;
    
    public UnSubscribe(string topic)
    {
        this.topic = topic;
    }
}
#endregion

//class for rosservice server
#region 
public class AdvertiseService
{
    public string op = "advertise_service";
    public string service;
    public string type;
    
    public AdvertiseService(string service, string type)
    {
        this.service = service;
        this.type = type;
    }
}

public class UnAdvertiseService
{
    public string op = "unadvertise_service";
    public string service;
    
    public UnAdvertiseService(string service)
    {
        this.service = service;
    }
}

public class ServiceResponse
{
    public string op = "service_response";
    public string service;
    public string id;
    public object values;
    public bool result;

    public ServiceResponse(string service, string id, object values, bool result)
    {
        this.service = service;
        this.id = id;
        this.values = values;
        this.result = result;
    }
}
#endregion

// class for rosservice client
public class ServiceCall
{
    public string op = "call_service";
    public string service;
    public object args;
    
    public ServiceCall(string service, object args)
    {
        this.service = service;
        this.args = args;
    }
}

public class RosSocketClient : MonoBehaviour
{
#if WINDOWS_UWP
    private MessageWebSocket ws;
    private DataWriter writer;
#else
    private WebSocket ws;
#endif
    private string address = "ws://192.168.4.168:9090";
    private JObject receiveJson, topicJson, srvResJson, srvReqJson;
    private List<string[]> namesService, namesPubTopic, namesSubTopic;

    //*****************************************
    // function to be run first

    void Awake()
    {
        // initialize
        namesService = new List<string[]>();
        namesPubTopic = new List<string[]>();
        namesSubTopic = new List<string[]>();

        // connect another PC via websocket
        Connect();
    }

    //*****************************************
    // function to connect another PC(ROS)
    // can connect Unity and UWP environment

    public RosSocketClient()
    {
#if WINDOWS_UWP
        ws = new MessageWebSocket();
        writer = new DataWriter(ws.OutputStream);
        ws.Control.MessageType = SocketMessageType.Utf8;
        ws.MessageReceived += MessageReceived;
        //CoreApplication.EnteredBackground += AppEnteredBackground;
        //CoreApplication.LeavingBackground += AppLeavingBackground;
#else
        ws = new WebSocket(address);
        //open message
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("*********** Websocket connected ***********");
        };
        //close message
        ws.OnClose += (sender, e) =>
        {
            Debug.Log("*********** Websocket disconnected ***********");
        };
        //error message
        ws.OnError += (sender, e) =>
        {
            Debug.Log("Error : " + e.Message);
        };
        OnMessage();
#endif
    }

    public void Connect()
    {
#if WINDOWS_UWP
        var task = Task.Run(async () => {
            await ws.ConnectAsync(new Uri(address));
        });
        task.Wait();
#else
        ws.Connect();
#endif
    }

    public void Close()
    {
#if WINDOWS_UWP
        ws.Close(1000, "Disconnect function is called.");
        ws = null;
#else
        ws.Close();
        ws = null;
#endif
    }

    //*****************************************
    // function to receive msg from ROS
    // the msg is saved as receiveMsg

    public bool IsReceiveMsg()
    {
        if (receiveJson != null)
            return true;
        else
            return false;
    }

    public bool IsReceiveTopic()
    {
        if (topicJson != null)
            return true;
        else
            return false;
    }

    public bool IsReceiveSrvRes()
    {
        if (srvResJson != null)
            return true;
        else
            return false;
    }

    public bool IsReceiveSrvReq()
    {
        if (srvReqJson != null)
            return true;
        else
            return false;
    }

    public JObject GetReceiveMsg()
    {
        JObject temp = receiveJson;
        receiveJson = null;
        return temp;
    }

    public JObject GetTopicMsg()
    {
        JObject temp = topicJson;
        topicJson = null;
        return temp;
    }

    public JObject GetSrvResMsg()
    {
        JObject temp = srvResJson;
        srvResJson = null;
        return temp;
    }

    public JObject GetSrvReqMsg()
    {
        JObject temp = srvReqJson;
        srvReqJson = null;
        return temp;
    }

    public string GetValue(string key)
    {
        string value = (string)receiveJson.SelectToken(key);
        return value;
    }

    public string GetTopicValue(string key)
    {
        string value = (string)topicJson.SelectToken(key);
        return value;
    }

    public string GetSrvResValue(string key)
    {
        string value = (string)srvResJson.SelectToken(key);
        return value;
    }

    public string GetSrvReqValue(string key)
    {
        string value = (string)srvReqJson.SelectToken(key);
        return value;
    }

    private void ClassificationMsg()
    {
        string op = GetValue("op");
        switch (op)
        {
            case "publish":
                topicJson = receiveJson;
                break;
            case "service_response":
                srvResJson = receiveJson;
                break;
            case "call_service":
                srvReqJson = receiveJson;
                break;
            default:
                break;
        }
    }

#if WINDOWS_UWP
    private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
        var reader = args.GetDataReader();
        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
        receiveJson = JsonConvert.DeserializeObject<JObject>(reader.ReadString(reader.UnconsumedBufferLength));
        ClassificationMsg();
    }
#else
    private void OnMessage()
    {
        ws.OnMessage += (sender, e) =>
        {
            receiveJson = JsonConvert.DeserializeObject<JObject>(e.Data);
            ClassificationMsg();
        };
    }
#endif

    //*****************************************
    // function to send operation of ROS
    // publish and subscribe topic
    // call and advertise and response service

    public void Advertiser(string topicName, string topicType)
    {
        namesPubTopic.Add(new string[] { topicName, topicType });
        Advertise temp = new Advertise(topicName, topicType);
        SendOpMsg(temp);
    }

    public void UnAdvertiser(string topicName)
    {
        UnAdvertise temp = new UnAdvertise(topicName);
        SendOpMsg(temp);
    }

    public void Publisher(string topicName, object msg)
    {
        Publish temp = new Publish(topicName, msg);
        SendOpMsg(temp);
    }

    public void Subscriber(string topicName, string topicType)
    {
        namesSubTopic.Add(new string[] { topicName, topicType });
        Subscribe temp = new Subscribe(topicName, topicType);
        SendOpMsg(temp);
    }

    public void UnSubscriber(string topicName)
    {
        UnSubscribe temp = new UnSubscribe(topicName);
        SendOpMsg(temp);
    }

    public void ServiceCaller(string serviceName, object args)
    {
        ServiceCall temp = new ServiceCall(serviceName, args);
        SendOpMsg(temp);
    }

    public void ServiceAdvertiser(string serviceName, string serviceType)
    {
        namesService.Add(new string[] { serviceName, serviceType });
        AdvertiseService temp = new AdvertiseService(serviceName, serviceType);
        SendOpMsg(temp);
    }

    public void ServiceUnAdvertiser(string serviceName)
    {
        UnAdvertiseService temp = new UnAdvertiseService(serviceName);
        SendOpMsg(temp);
    }

    public void ServiceResponse(string serviceName, string id, object values, bool result)
    {
        ServiceResponse temp = new ServiceResponse(serviceName, id, values, result);
        SendOpMsg(temp);
    }

    public void SendOpMsg(object temp)
    {
        string msg = JsonConvert.SerializeObject(temp, Formatting.Indented);
#if WINDOWS_UWP
        var task = Task.Run(async () => {
            writer.WriteString(msg);
            await writer.StoreAsync();
        });
        task.Wait();
#else
        Debug.Log(msg);
        ws.SendAsync(msg, OnSendComplete);
#endif
    }

#if UNITY_EDITOR
    private void OnSendComplete(bool success)
    {
        if (!success)
        {
            Debug.Log("!-------------- Sent operation is failed --------------!");
        }
    }
#endif

#if WINDOWS_UWP
    private void AppEnteredBackground(object sender, EnteredBackgroundEventArgs e)
    {
        int[] size = new int[3];
        size[0] = namesService.Count;
        size[1] = namesPubTopic.Count;
        size[2] = namesSubTopic.Count;

        // UnAdvertiseService
        for (int i = 0; i < size[0]; i++)
        {
            ServiceUnAdvertiser(namesService[i][0]);
        }

        // UnAdvertiseTopic
        for (int i = 0; i < size[1]; i++)
        {
            UnAdvertiser(namesPubTopic[i][0]);
        }

        // UnSubscribeTopic
        for (int i = 0; i < size[2]; i++)
        {
            UnSubscriber(namesSubTopic[i][0]);
        }

        Close();
    }

    private void AppLeavingBackground(object sender, LeavingBackgroundEventArgs e)
    {
        ws = new MessageWebSocket();
        ws.Control.MessageType = SocketMessageType.Utf8;
        ws.MessageReceived += MessageReceived;
        
        Connect();

        int[] size = new int[3];
        size[0] = namesService.Count;
        size[1] = namesPubTopic.Count;
        size[2] = namesSubTopic.Count;

        // ReAdvertiseService
        for (int i = 0; i < size[0]; i++)
        {
            ServiceAdvertiser(namesService[0][0], namesService[0][1]);
            namesService.RemoveAt(0);
        }

        // ReAdvertiseTopic
        for (int i = 0; i < size[1]; i++)
        {
            Advertiser(namesPubTopic[0][0], namesPubTopic[0][1]);
            namesPubTopic.RemoveAt(0);
        }

        // ReSubscribeTopic
        for (int i = 0; i < size[2]; i++)
        {
            Subscriber(namesSubTopic[0][0], namesSubTopic[0][1]);
            namesSubTopic.RemoveAt(0);
        }
    }
#endif
}
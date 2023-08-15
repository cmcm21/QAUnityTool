using System;
using TagwizzQASniffer.Network;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField msgInput;
    [SerializeField] private TextMeshProUGUI feedbackMsg;

    private HubClient _client;
    
    public string GetPort => portInput.text;
    public string GetIP => ipInput.text;
    public string GetMsg => msgInput.text;

    private void Start()
    {
        _client = new HubClient();
    }

    private void PutFeedbackMsg(string message)
    {
        feedbackMsg.text = message;
    }

    public void Connect()
    {
        _client.StartClient(GetIP,int.Parse(GetPort));
        PutFeedbackMsg("Connection made");
    }

    public void SendMessage()
    {
        _client.SendMsgToServer(GetMsg);
        PutFeedbackMsg($"Message {GetMsg} was send correctly");
    }

    public void OnDisable()
    {
        _client.StopClient();
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;
public class Message 
{
    public string text;
    public Text textObject;
    public MessageType messageType;
    public enum MessageType 
    { 
        playerMessage,
        info
    }
    
}

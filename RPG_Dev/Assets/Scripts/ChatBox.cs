using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class ChatBox : MonoBehaviourPun
{
    public TextMeshProUGUI chatLogText;
    public TMP_InputField chatInput;

    //instance
    public static ChatBox instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        //allows enter to make the chat active or send chat messages
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(EventSystem.current.currentSelectedGameObject == chatInput.gameObject)
            {
                OnChatInputSend();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            }
        }
    }

    public void OnChatInputSend()
    {
        if(chatInput.text.Length > 0)
        {
            //send message and clear input
            photonView.RPC("Log", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, chatInput.text);
            chatInput.text = "";
        }

        //deselects the chat on send
        EventSystem.current.SetSelectedGameObject(null);
    }

    [PunRPC]
    private void Log(string playerName, string message)
    {
        //cats the chat message
        chatLogText.text += string.Format("<br>{0}:</b> {1}", playerName, message);

        //resizes the text box that the chat is stored in
        chatLogText.rectTransform.sizeDelta = new Vector2(chatLogText.rectTransform.sizeDelta.x, chatLogText.mesh.bounds.size.y + 20);
    }
}

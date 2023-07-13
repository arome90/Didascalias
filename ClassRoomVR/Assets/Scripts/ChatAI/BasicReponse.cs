using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI.Chat;
using OpenAI;
using OpenAI.Models;

public class BasicReponse : MonoBehaviour
{

    public async void Awake()
    {
        var api = new OpenAIClient("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");
        var messages = new List<Message>
        {
            //new Message(Role.System,
            //"Quiero que asumas el Rol de ser un estudiante ")
        
            ////Añadir descripcion de cada estudiante para tener diferentes respuestas 
    new Message(Role.System, "You are a helpful assistant."),
    new Message(Role.User, "Who won the world series in 2020?"),
    new Message(Role.Assistant, "The Los Angeles Dodgers won the World Series in 2020."),
    new Message(Role.User, "Where was it played?"),
        };

        //messages.Add(new Message(Role.User, "Lo que diga el profesor"));

        // var chatrequest;

        var chatRequest = new ChatRequest(messages, Model.GPT3_5_Turbo);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        Debug.Log($"{result.FirstChoice.Message.Role}: {result.FirstChoice.Message.Content}");
    }

}


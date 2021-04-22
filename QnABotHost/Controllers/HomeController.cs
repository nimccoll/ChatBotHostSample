//===============================================================================
// Microsoft FastTrack for Azure
// Azure Bot Framework web host sample
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QnABotHost.Models;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace QnABotHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _directLineSecret = "{your bot direct line secret goes here - get this from the portal}";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            DirectLineToken directLineToken = null;
            string waterMark = string.Empty;
            string conversationId = this.HttpContext.Session.GetString("ConversationId");
            // Do we have an existing conversation with the chat bot?
            if (string.IsNullOrEmpty(conversationId))
            {
                //directLineToken = GetDirectLineToken();
                directLineToken = StartConversation(); // Start a new conversation with the chat bot
                // Preserve the watermark and conversation ID
                waterMark = Guid.NewGuid().ToString();
                this.HttpContext.Session.SetString("ConversationId", directLineToken.conversationId);
                this.HttpContext.Session.SetString("Watermark", waterMark);
            }
            else
            {
                // Re-connect to the existing conversation
                waterMark = this.HttpContext.Session.GetString("Watermark");
                directLineToken = ReconnectConversation(conversationId, waterMark);
            }
            // Pass the conversation values to the WebChat control
            ViewBag.DirectLineToken = directLineToken.token;
            ViewBag.ConversationId = directLineToken.conversationId;
            ViewBag.StreamUrl = directLineToken.streamUrl ?? string.Empty;
            ViewBag.Watermark = waterMark;
            return View();
        }

        public IActionResult Privacy()
        {
            DirectLineToken directLineToken = null;
            string waterMark = string.Empty;
            string conversationId = this.HttpContext.Session.GetString("ConversationId");
            // Do we have an existing conversation with the chat bot?
            if (string.IsNullOrEmpty(conversationId))
            {
                //directLineToken = GetDirectLineToken();
                directLineToken = StartConversation(); // Start a new conversation with the chat bot
                // Preserve the watermark and conversation ID
                waterMark = Guid.NewGuid().ToString();
                this.HttpContext.Session.SetString("ConversationId", directLineToken.conversationId);
                this.HttpContext.Session.SetString("Watermark", waterMark);
            }
            else
            {
                // Re-connect to the existing conversation
                waterMark = this.HttpContext.Session.GetString("Watermark");
                directLineToken = ReconnectConversation(conversationId, waterMark);
            }
            // Pass the conversation values to the iFrame
            ViewBag.DirectLineToken = directLineToken.token;
            ViewBag.ConversationId = directLineToken.conversationId;
            ViewBag.StreamUrl = directLineToken.streamUrl;
            ViewBag.Watermark = waterMark;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private DirectLineToken GetDirectLineToken()
        {
            // Retrieve a direct line token to communicate with the chat bot.
            // This method does not start a new conversation.
            DirectLineToken directLineToken = null;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _directLineSecret);
            HttpResponseMessage httpResponseMessage = httpClient.PostAsync("https://directline.botframework.com/v3/directline/tokens/generate", null).Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                directLineToken = JsonConvert.DeserializeObject<DirectLineToken>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            }
            return directLineToken;
        }

        private DirectLineToken StartConversation()
        {
            // Start a new conversation with the chat bot. Pass in a watermark value so you can
            // re-connect to the conversation later and replay the conversation history in the chat window.
            DirectLineToken directLineToken = null;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _directLineSecret);
            HttpResponseMessage httpResponseMessage = httpClient.PostAsync("https://directline.botframework.com/v3/directline/conversations", null).Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                directLineToken = JsonConvert.DeserializeObject<DirectLineToken>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            }
            return directLineToken;
        }

        private DirectLineToken ReconnectConversation(string conversationId, string waterMark)
        {
            // Re-connect to an existing conversation with the chat bot. Pass in the conversation ID and
            // watermark value so you can replay the conversation history in the chat window.
            DirectLineToken directLineToken = null;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _directLineSecret);
            HttpResponseMessage httpResponseMessage = httpClient.GetAsync($"https://directline.botframework.com/v3/directline/conversations/{conversationId}?watermark={waterMark}").Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                directLineToken = JsonConvert.DeserializeObject<DirectLineToken>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            }

            return directLineToken;
        }
    }
}

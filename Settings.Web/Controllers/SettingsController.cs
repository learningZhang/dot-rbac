﻿using Grit.Utility.Security;
using Newtonsoft.Json;
using Settings.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Settings.Web.Controllers
{
    public class SettingsController : ApiController
    {
        public SettingsController(ISettingsService settingsService,
            Grit.Tree.ITreeService treeService)
        {
            this.SettingsService = settingsService;
            this.TreeService = treeService;
        }

        private Grit.Tree.ITreeService TreeService { get; set; }
        private ISettingsService SettingsService  {get;set;}

        [HttpGet]
        [Route("api/settings/{client}")]
        public HttpResponseMessage Index(string client)
        {
            var aClient = SettingsService.GetClient(client);
            if (aClient == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "客户不存在");
            }

            var tree = TreeService.GetTree(Constants.TREE_NODE);
            ClientSettings settings = SettingsService.GetClientSettings(aClient, tree);
            string json = JsonConvert.SerializeObject(settings);

            Envelope resp = EnvelopeService.Encrypt(aClient.Name, json, aClient.PublicKey);
            return Request.CreateResponse(HttpStatusCode.OK, resp);
        }
    }
}
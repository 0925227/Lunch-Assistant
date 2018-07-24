using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace aws_speech
{
    public class Function
    {
        public List<ResponseResource> GetResources()
        {
            List<ResponseResource> resources = new List<ResponseResource>();
            ResponseResource enUSResource = new ResponseResource("en-US");
            enUSResource.SkillName = "Lunch Assistant";
            enUSResource.GetResponsesMessage = "Here's your lunch tip for the day: ";
            enUSResource.HelpMessage = "Some help message";
            enUSResource.HelpReprompt = "Some help reprompt";
            enUSResource.StopMessage = "Goodbye!";

            enUSResource.Responses.Add("I made a sandwich with cheese, tomato's and white bread.");

            resources.Add(enUSResource);
            return resources;
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Lunch Helper");
                innerResponse = new PlainTextOutputSpeech();
                NeuralNetworkSetup();
                (innerResponse as PlainTextOutputSpeech).Text = emitResponse(resource, true);

            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitResponse(resource, false);
                        break;
                    case "GetNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitResponse(resource, false);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public void NeuralNetworkSetup()
        {
            //NeuralNetwork.NeuralNetwork neuralNetwork = new NeuralNetwork.NeuralNetwork();

            //neuralNetwork.Setup();
        }


        public string emitResponse(ResponseResource resource, bool withPreface)
        {
            Random r = new Random();
            if (withPreface)
                return resource.GetResponsesMessage + resource.Responses[r.Next(resource.Responses.Count)];
            return resource.Responses[r.Next(resource.Responses.Count)];
        }

    }

    public class ResponseResource
    {
        public ResponseResource(string language)
        {
            this.Language = language;
            this.Responses = new List<string>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public List<string> Responses { get; set; }
        public string GetResponsesMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;

namespace SeerAzureWebAPI.Controllers
{
    [ApiController]
    [Route("api/workitems")]
    public class TaskController : ControllerBase
    {
        private readonly string _uri;
        private readonly string _personalAccessToken;
        private readonly string _project;
        private readonly IConfiguration _configuration;

        public TaskController(IConfiguration configuration)
        {
            _configuration = configuration;

            _uri = configuration.GetSection("AzureDevOpsSettings:Uri").Value;
            _personalAccessToken = configuration.GetSection("AzureDevOpsSettings:PersonalAccessToken").Value;
            _project = configuration.GetSection("AzureDevOpsSettings:Project").Value;
        }

        [HttpGet]
        public IActionResult GetWorkItems()
        {
            try
            {
                var workItem = CreateTaskUsingClientLib();

                if (workItem != null)
                {
                    return Ok(workItem);
                }
                else
                {
                    return BadRequest("Failed to create task");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private WorkItem CreateTaskUsingClientLib()
        {
            Uri uri = new Uri(_uri);
            string personalAccessToken = _personalAccessToken;
            string project = _project;

            VssBasicCredential credentials = new VssBasicCredential("", _personalAccessToken);
            JsonPatchDocument patchDocument = new JsonPatchDocument();

            // Add fields and their values to your patch document
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = "Task Created Using API"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.TCM.ReproSteps",
                    Value = "Our authorization logic needs to allow for users with Microsoft accounts (formerly Live Ids) - http:// msdn.microsoft.com/library/live/hh826547.aspx"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Priority",
                    Value = "1"
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Severity",
                    Value = "2 - High"
                }
            );

            VssConnection connection = new VssConnection(uri, credentials);
            WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                WorkItem result = workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, project, "Task").Result;
                Console.WriteLine("Task Successfully Created: Task #{0}", result.Id);
                return result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error creating Task: {0}", ex.InnerException.Message);
                return null;
            }
        }
    }
}


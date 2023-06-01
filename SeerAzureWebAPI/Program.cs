namespace SeerAzureWebAPI;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using SeerAzureWebAPI.Controllers;

public class Program
{
    public static void Main(string[] args)
    {
        //instantiates an instance of the CreateTask class and calls the CreateTaskUsingClientLib method.
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;


        var createTask = new CreateTask(configuration);
        createTask.CreateTaskUsingClientLib(configuration);


        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}

public class CreateTask
{
    readonly string _uri;
    readonly string _personalAccessToken;
    readonly string _project;
    readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor. Manually set values to match your organization. 
    /// </summary>


    public CreateTask(IConfiguration configuration)
    {
        _configuration = configuration;

        _uri = configuration.GetSection("AzureDevOpsSettings:Uri").Value;
        _personalAccessToken = configuration.GetSection("AzureDevOpsSettings:PersonalAccessToken").Value;
        _project = configuration.GetSection("AzureDevOpsSettings:Project").Value;
    }

    /// <summary>
    /// Create a Task using the .NET client library
    /// </summary>
    /// <returns>Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem</returns>    
    public WorkItem CreateTaskUsingClientLib(IConfiguration configuration)
    {
        Uri uri = new Uri(_uri);
        string personalAccessToken = _personalAccessToken;
        string project = _project;

        VssBasicCredential credentials = new VssBasicCredential("", _personalAccessToken);
        JsonPatchDocument patchDocument = new JsonPatchDocument();

        //patchDocument is populated with the desired changes to the task. The JsonPatchOperation objects are added to the `patchDocument
        //add fields and their values to your patch document
        patchDocument.Add(
            new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = "Task Created Using API 1- Meth"
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
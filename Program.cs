using Microsoft.Extensions.Configuration;
using static System.Console;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using AsanaAssignToMe;
using System.Net.Http.Json;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string apiKey = config["apiKey"];
string baseUrl = config["baseUrl"];

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var user = await GetAsync<User>("users").ConfigureAwait(false);
string userGID = user.data[0].gid;

var projects = await GetAsync<Project>("projects").ConfigureAwait(false);

foreach (var project in projects.data)
{
    WriteLine(project.name);
    string taskUrl = $"projects/{project.gid}/tasks?opt_fields=gid,name,assignee,completed&limit=25";
    string url = taskUrl;
    while (true)
    {
        WriteLine($"Getting tasks, url={url}");
        TaskSummary tasks = await GetAsync<TaskSummary>(url).ConfigureAwait(false);
        foreach (var taskSummary in tasks.data)
        {
            if (taskSummary.completed) { continue; }
            if (taskSummary.assignee?.gid != userGID)
            {
                WriteLine($"Task: {taskSummary.name}");
                WriteLine($"Assignee is now null");
                var taskResponse = await PutAsync<AssignUserRequest, AssignUserResponse>(
                    $"tasks/{taskSummary.gid}", new AssignUserRequest(new AssignUserRequestData(userGID))).ConfigureAwait(false);
                WriteLine($"Assignee is now {taskResponse.data.assignee?.name}");
            }
        }

        if (!string.IsNullOrWhiteSpace(tasks.next_page?.offset))
        {
            url = $"{taskUrl}&offset={tasks.next_page.offset}";
        }
        else
        {
            break;
        }
    }
}

async Task<T> GetAsync<T>(string suffix)
{
    await Task.Delay(500); // API Limit of 150 requests per minute
    var response = await client.GetAsync(GetUrl(suffix)).ConfigureAwait(false);
    return (await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false))!;
}

async Task<TResponse> PutAsync<TRequest, TResponse>(string suffix, TRequest body)
{
    await Task.Delay(500); // API Limit of 150 requests per minute
    var response = await client.PutAsJsonAsync(GetUrl(suffix), body).ConfigureAwait(false);
    return (await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false))!;
}

string GetUrl(string suffix) => $"{baseUrl}{suffix.TrimStart('/')}";

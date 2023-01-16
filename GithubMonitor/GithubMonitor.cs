using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace GithubMonitor
{
    public static class GithubMonitor
    {
        [FunctionName("GithubMonitor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var azureBusConnectionString = config["GithubBusConnectionString"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RootObject githubData = JsonConvert.DeserializeObject<RootObject>(requestBody);
            log.LogInformation(githubData?.commits?.FirstOrDefault()?.message?.ToString() ?? "No github data");

            await SendMessageAsync(githubData, azureBusConnectionString, "githubmessages");

            return new OkResult();
        }

        public static async Task SendMessageAsync<T>(T message, string queueConnectionString, string queueName)
        {
            var queueClient = new QueueClient(queueConnectionString, queueName);
            
            string messageBodyJson = JsonConvert.SerializeObject(message);
            var queueMessage = new Message(Encoding.UTF8.GetBytes(messageBodyJson));

            await queueClient.SendAsync(queueMessage);
        }
    }
}
public class RootObject
{
    public string _ref { get; set; }
    public string before { get; set; }
    public string after { get; set; }
    public Repository repository { get; set; }
    public Pusher pusher { get; set; }
    public Sender sender { get; set; }
    public bool created { get; set; }
    public bool deleted { get; set; }
    public bool forced { get; set; }
    public object base_ref { get; set; }
    public string compare { get; set; }
    public Commit[] commits { get; set; }
    public Head_Commit head_commit { get; set; }
}

public class Repository
{
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public string full_name { get; set; }
    public bool _private { get; set; }
    public Owner owner { get; set; }
    public string html_url { get; set; }
    public string description { get; set; }
    public bool fork { get; set; }
    public string url { get; set; }
    public string forks_url { get; set; }
    public string keys_url { get; set; }
    public string collaborators_url { get; set; }
    public string teams_url { get; set; }
    public string hooks_url { get; set; }
    public string issue_events_url { get; set; }
    public string events_url { get; set; }
    public string assignees_url { get; set; }
    public string branches_url { get; set; }
    public string tags_url { get; set; }
    public string blobs_url { get; set; }
    public string git_tags_url { get; set; }
    public string git_refs_url { get; set; }
    public string trees_url { get; set; }
    public string statuses_url { get; set; }
    public string languages_url { get; set; }
    public string stargazers_url { get; set; }
    public string contributors_url { get; set; }
    public string subscribers_url { get; set; }
    public string subscription_url { get; set; }
    public string commits_url { get; set; }
    public string git_commits_url { get; set; }
    public string comments_url { get; set; }
    public string issue_comment_url { get; set; }
    public string contents_url { get; set; }
    public string compare_url { get; set; }
    public string merges_url { get; set; }
    public string archive_url { get; set; }
    public string downloads_url { get; set; }
    public string issues_url { get; set; }
    public string pulls_url { get; set; }
    public string milestones_url { get; set; }
    public string notifications_url { get; set; }
    public string labels_url { get; set; }
    public string releases_url { get; set; }
    public string deployments_url { get; set; }
    public int created_at { get; set; }
    public DateTime updated_at { get; set; }
    public int pushed_at { get; set; }
    public string git_url { get; set; }
    public string ssh_url { get; set; }
    public string clone_url { get; set; }
    public string svn_url { get; set; }
    public string homepage { get; set; }
    public int size { get; set; }
    public int stargazers_count { get; set; }
    public int watchers_count { get; set; }
    public string language { get; set; }
    public bool has_issues { get; set; }
    public bool has_projects { get; set; }
    public bool has_downloads { get; set; }
    public bool has_wiki { get; set; }
    public bool has_pages { get; set; }
    public bool has_discussions { get; set; }
    public int forks_count { get; set; }
    public object mirror_url { get; set; }
    public bool archived { get; set; }
    public bool disabled { get; set; }
    public int open_issues_count { get; set; }
    public object license { get; set; }
    public bool allow_forking { get; set; }
    public bool is_template { get; set; }
    public bool web_commit_signoff_required { get; set; }
    public object[] topics { get; set; }
    public string visibility { get; set; }
    public int forks { get; set; }
    public int open_issues { get; set; }
    public int watchers { get; set; }
    public string default_branch { get; set; }
    public int stargazers { get; set; }
    public string master_branch { get; set; }
}

public class Owner
{
    public string name { get; set; }
    public string email { get; set; }
    public string login { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public bool site_admin { get; set; }
}

public class Pusher
{
    public string name { get; set; }
    public string email { get; set; }
}

public class Sender
{
    public string login { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public bool site_admin { get; set; }
}

public class Head_Commit
{
    public string id { get; set; }
    public string tree_id { get; set; }
    public bool distinct { get; set; }
    public string message { get; set; }
    public DateTime timestamp { get; set; }
    public string url { get; set; }
    public Author author { get; set; }
    public Committer committer { get; set; }
    public string[] added { get; set; }
    public object[] removed { get; set; }
    public object[] modified { get; set; }
}

public class Author
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Committer
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Commit
{
    public string id { get; set; }
    public string tree_id { get; set; }
    public bool distinct { get; set; }
    public string message { get; set; }
    public DateTime timestamp { get; set; }
    public string url { get; set; }
    public Author1 author { get; set; }
    public Committer1 committer { get; set; }
    public string[] added { get; set; }
    public object[] removed { get; set; }
    public object[] modified { get; set; }
}

public class Author1
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Committer1
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}


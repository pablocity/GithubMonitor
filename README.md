# Github Monitor documentation

Github monitor is a project that takes advantage of Azure infrustructure to monitor chosen Github repository
and process data actions that are executed in that repo (for example who pushed some latest commits etc.) 
It uses webhook from Github to call an Azure function which creates a Azure Service Bus message and push it to the queue. Then if GithubProcessor Azure function is available it consumes the message and adds prepared data to Azure Cosmos DB.
From now on it is possible for Django WebApp to read the data and display to the end user.

Infrustructure could be simplified but using two azure functions and service bus has big advantages. It does not matter whether processor function or cosmos db are available. When these services are down, message waits in the queue and is consumed as soon as services are back online. Even if only Cosmos DB is unavailable, consuming service bus message will be unsuccessfull which means that message will be back in the queue to be processed at later time.

## Application components

1. Azure function (GithubMonitor)
2. Azure Service Bus
3. Azure Cosmos DB
4. Azure Key Vault
5. Azure Managed Identity
6. Azure function (GithubProcessor)
7. Django WebApp

## Azure function (GithubMonitor)

GithubMonitor is called by a webhook and takes Github data and puts them into message queue. It is worth mentioning that this is only text data which is not really big, Service bus should not be used to store big messages.

```
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
```
## Azure function (GithubProcessor)

It is an service bus trigger function. Whenever new message is added to the queue, this function is called to consume such message. In this case it takes the message and puts github data to Cosmos DB. In the snippet below there is a code responsible only for adding commits to database.

```
CosmosClient cosmosClient = new CosmosClient(cosmosDbConnectionString);
            var db = cosmosClient.GetDatabase("GithubMonitor");
            var container = db.GetContainer("Commits");

            var tasks = new List<Task>();

            foreach (var item in githubData.commits)
            {
                tasks.Add(container.CreateItemAsync<Commit>(item, new PartitionKey(item.id)));
            }

            await Task.WhenAll(tasks);
```

## Azure configuration components

Besides azure functions and service bus there are a few things going on behind the scene. For exmple Azure Key Vault is used to store Secret key to the service bus and as you can see in the GithubMonitor snippet connection string is taken directly from the configuration. This means that locally anything that is given in local.settings.json is taken but when published in azure it is configured to take secret from key vault.
Additionally it Azure Managed Identity needed to be configured for the Azure function to have access to key vault.
Azure Cosmos DB was also configured directly on the Azure portal.

## Django WebApp

Web application in Django acts purely as a data receiver. It takes data from cosmos db and display it to the user. Altough it is quite simple with existing infrustructure it is easy to extend its functionality for example to add real time push notifications when new data is pushed to Cosmos DB.

Here is part of the code responsible for getting the data and passing it to html template. Additionally initial configuration for push notifications was added.
```
client = CosmosClient(url=endpoint, credential=key)

db = client.get_database_client("GithubMonitor")

container = db.get_container_client("Commits")
queryAll = "SELECT * FROM Commits"

result = container.query_items(query = queryAll, enable_cross_partition_query=True)

commits = []

for  item  in  result:

dumped = json.dumps(item, indent=True)
commitData = json.loads(dumped)
commits.append(commitData)
  
push_settings = getattr(settings, 'WEBPUSH_SETTINGS', {})
vapid_key = push_settings.get('VAPID_PUBLIC_KEY')
user = request.user

  
template = loader.get_template('monitor/index.html')

context = {
'commits': commits,
'vapid_key': vapid_key,
'user': user
}
 

return  HttpResponse(template.render(context, request))
```

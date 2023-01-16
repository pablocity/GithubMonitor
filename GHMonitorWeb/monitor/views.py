from django.shortcuts import render
from django.http import HttpResponse
from django.template import loader
from azure.cosmos import CosmosClient, PartitionKey
import json
from django.http.response import JsonResponse, HttpResponse
from django.views.decorators.http import require_GET, require_POST
from django.shortcuts import get_object_or_404
from django.contrib.auth.models import User
from django.views.decorators.csrf import csrf_exempt
from webpush import send_user_notification
from django.conf import settings


def index(request):
    endpoint = "https://azurecertaccount.documents.azure.com:443/"
    key = "4MQ3nJatGPmRukecco5OpYHuPfSib4jZbfnGp1zgOukHSkSho3Mn0u6WtESaKkWO7QZSBrf7iDqwACDbe6hvZg=="
    client = CosmosClient(url=endpoint, credential=key)
    db = client.get_database_client("GithubMonitor")
    container = db.get_container_client("Commits")

    queryAll = "SELECT * FROM Commits"

    result = container.query_items(query = queryAll, enable_cross_partition_query=True)
    commits = []

    for item in result:
        #commits.append(json.loads(json.dumps(commits, indent=True)))
        t = json.dumps(item, indent=True)
        r = json.loads(t)
        commits.append(r)

    push_settings = getattr(settings, 'WEBPUSH_SETTINGS', {})
    vapid_key = push_settings.get('VAPID_PUBLIC_KEY')
    user = request.user

    template = loader.get_template('monitor/index.html')
    context = {
        'commits': commits,
        'vapid_key': vapid_key,
        'user': user
    }

    return HttpResponse(template.render(context, request))
    #return render(request, 'monitor/index.html', context)


# @require_POST
# @csrf_exempt
def send_push(request):
    # try:
    #     body = request.body
    #     data = json.loads(body)

    #     if 'head' not in data or 'body' not in data or 'id' not in data:
    #         return JsonResponse(status=400, data={"message": "Invalid data format"})

    #     user_id = data['id']
    #     user = get_object_or_404(User, pk=user_id)
    #     payload = {'head': data['head'], 'body': data['body']}
    #     send_user_notification(user=user, payload=payload, ttl=1000)

    #     return JsonResponse(status=200, data={"message": "Web push successful"})
    # except TypeError:
    #     return JsonResponse(status=500, data={"message": "An error occurred"})
    return HttpResponse("Push notification!")
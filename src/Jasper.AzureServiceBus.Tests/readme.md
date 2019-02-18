## Configuration

To run these tests, you need to add an *appsettings.json* file to the project with your Azure credentials like this:


```
{
    "AzureServiceBus": {
      "ConnectionString": "the Azure Service Bus connection to your namespace",
      "TenantId": "See https://docs.microsoft.com/en-us/onedrive/find-your-office-365-tenant-id",
      "ClientId": "See https://www.netiq.com/communities/cool-solutions/creating-application-client-id-client-secret-microsoft-azure-new-portal/",
      "ClientSecret": "See https://www.netiq.com/communities/cool-solutions/creating-application-client-id-client-secret-microsoft-azure-new-portal",
      "SubscriptionId": "See http://scug.be/wim/2018/03/12/azure-tip-how-to-find-your-subscription-id-guid/",
      "DataCenterLocation": "West US",
      "ServiceBusSku": "Standard"
    }
}


```

The test harness will generate all the necessary queues, topics, and subscriptions on the fly

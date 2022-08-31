module.exports = {
    title: 'Jasper',
    description: 'Next Generation Command and Message Bus for .NET',
    head: [],
    themeConfig: {
        logo: null,
        repo: 'JasperFx/jasper',
        docsDir: 'docs',
        docsBranch: 'master',
        editLinks: true,
        editLinkText: 'Suggest changes to this page',

        nav: [
            { text: 'Guide', link: '/guide/' },
            { text: 'Gitter | Join Chat', link: 'https://gitter.im/JasperFx/jasper?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge' }
        ],

        algolia: {
            appId: '2V5OM390DF',
            apiKey: '674cd4f3e6b6ebe232a980c7cc5a0270',
            indexName: 'jasper_index'
        },

        sidebar: [
            {
                text: 'Getting Started',
                link: '/guide/',
                children: tableOfContents()
            }
        ]
    },
    markdown: {
        linkify: false
    }
}

function tableOfContents() {
    return [
      {text: "As Mediator", link: '/guide/mediator'},
      {text: "As Command Bus", link: '/guide/in-memory-bus'},
      {
        text: "As Messaging Bus",
        link: '/guide/messaging/',
        children: [
          {text: "Configuring Messaging", link: '/guide/messaging/configuration'},
          {text: "Message Routing", link: '/guide/messaging/routing'},
          {text: "Publishing and Sending", link: '/guide/messaging/pubsub'},
          {text: "Message Expiration", link: '/guide/messaging/expiration'},
          {text: "Transports", link: '/guide/messaging/transports/', children: [
              {text: "With Rabbit MQ", link: '/guide/messaging/rabbitmq'},
              {text: "With Pulsar", link: '/guide/messaging/pulsar'},
              {text: "With TCP", link: '/guide/messaging/tcp'},
              {text: "MassTransit Interop", link: '/guide/messaging/masstransit'}
            ]},
          {text: "Scheduled Delivery", link: '/guide/messaging/scheduled'},
          {text: "Message Correlation", link: '/guide/messaging/correlation'},
        ]


      },
      {
        text: "Durable Messaging",
        link: '/guide/durability/',
        children: [
          {text: "Stateful Sagas", link: '/guide/durability/sagas'},
          {text: "With Marten", link: '/guide/durability/marten'},
          {text: "With Entity Framework Core", link: '/guide/durability/efcore'}
        ]
      },
      {
        text: "Messages and Message Handlers",
        link: '/guide/messages/',
        children: [
          {text: "Handlers", link: '/guide/messages/handlers'},
          {text: "Discovery", link: '/guide/messages/discovery'},
          {text: "Middleware", link: '/guide/messages/middleware'},
          {text: "Versioning", link: '/guide/messages/versioning'},
          {text: "Serialization", link: '/guide/messages/serialization'},
          {text: "Error Handling", link: '/guide/messages/error-handling'},

        ]
      },
      {text: "Scheduled", link: '/guide/scheduled'},
      {text: "Configuration", link: '/guide/configuration'},
      {text: "Instrumentation, Diagnostics, and Logging", link: '/guide/logging'},
      {text: "Test Automation Support", link: '/guide/testing'},
      {text: "Command Line Integration", link: '/guide/command-line'},
      {text: "Best Practices", link: '/guide/best-practices'},
      {text: "Extensions", link: '/guide/extensions'}

    ]
}

/*


        {text: 'Alba Setup', link: '/guide/hosting'},
        {text: 'Integrating with xUnit.Net', link: '/guide/xunit'},
        {text: 'Integrating with NUnit', link: '/guide/nunit'},
        {text: 'Extension Model', link: '/guide/extensions'},
        {text: 'Security Extensions', link: '/guide/security'}
 */


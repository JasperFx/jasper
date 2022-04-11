module.exports = {
    title: 'Jasper',
    description: 'WHAT SHOULD BE HERE?',
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
      {text: "As In Memory Message Bus", link: '/guide/in-memory-bus'},
      {
        text: "As Messaging Bus",
        link: '/guide/messaging/',
        children: [
          {text: "With Rabbit MQ", link: '/guide/messaging/rabbitmq'},
          {text: "With Pulsar", link: '/guide/messaging/pulsar'},
          {text: "With TCP", link: '/guide/messaging/tcp'},
          {text: "MassTransit Interop", link: '/guide/messaging/masstransit'}

        ]
      },
      {
        text: "Persistent Messaging",
        link: '/guide/persistence/',
        children: [
          {text: "Stateful Sagas", link: '/guide/persistence/sagas'},
          {text: "With Postgresql", link: '/guide/persistence/postgresql'},
          {text: "With Sql Server", link: '/guide/persistence/sqlserver'},
          {text: "With Marten", link: '/guide/persistence/marten'},
          {text: "With Entity Framework Core", link: '/guide/persistence/efcore'}
        ]
      },
      {
        text: "Messages and Message Handlers",
        link: '/guide/messaging/',
        children: [
          {text: "Discovery", link: '/guide/messaging/discovery'},
          {text: "Middleware", link: '/guide/messaging/middleware'},
          {text: "Versioning", link: '/guide/messaging/versioning'},
          {text: "Serialization", link: '/guide/messaging/serialization'},
          {text: "Error Handling", link: '/guide/messaging/error-handling'},

        ]
      },
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

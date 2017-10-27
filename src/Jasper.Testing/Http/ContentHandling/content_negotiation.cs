using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Alba;
using AlbaForJasper;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.Model;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class content_negotiation : IDisposable
    {
        private readonly JasperRuntime _runtime;

        public content_negotiation()
        {
            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Services.For<IMessageDeserializer>().Add<XmlReader<SpecialInput>>();
                _.Services.For<IMessageSerializer>().Add<XmlWriter<SpecialOutput>>();

            });

            var sourceCode = _runtime.Get<RouteGraph>().First(x => x.InputType == typeof(SpecialInput)).SourceCode;
            Console.WriteLine(sourceCode);
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private Task<IScenarioResult> scenario(Action<Scenario> configure)
        {
            return _runtime.Scenario(configure);
        }

        [Fact]
        public Task send_and_receive_against_specific_reader_writer()
        {
            return scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("text/special")
                    .Accepts("text/special");
                _.ContentShouldBe("Tamba Hali");
                _.ContentTypeShouldBe("text/special");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public Task send_and_receive_against_specific_reader_writer_2()
        {
            return scenario(_ =>
            {
                _.Post.Json(new SpecialInput
                    {
                        Name = "Justin Houston"
                    }).ToUrl("/special/output")
                    .Accepts("text/special");
                _.ContentShouldBe("Justin Houston");
                _.ContentTypeShouldBe("text/special");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public Task send_and_receive_against_specific_reader_writer_with_preference()
        {
            return scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("text/special")
                    .Accepts("text/special,application/json");
                _.ContentShouldBe("Tamba Hali");
                _.ContentTypeShouldBe("text/special");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public Task send_and_receive_against_specific_reader_writer_with_preference_using_first_valid()
        {
            return scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("text/special")
                    .Accepts("garbage/else,text/special,application/json");
                _.ContentShouldBe("Tamba Hali");
                _.ContentTypeShouldBe("text/special");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task send_and_receive_against_specific_reader_writer_with_json()
        {
            var result = await scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("text/special")
                    .Accepts("application/json, text/special");

                _.ContentTypeShouldBe("application/json");
                _.StatusCodeShouldBeOk();
            });

            result.ResponseBody.ReadAsJson<SpecialOutput>()
                .Value.ShouldBe("Tamba Hali");
        }

        [Fact]
        public async Task return_415_with_invalid_content_type()
        {
            var result = await scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("nonexistent")
                    .Accepts("application/json, text/special");

                _.StatusCodeShouldBe(415);
            });
        }

        [Fact]
        public async Task return_406_with_invalid_accept()
        {
            var result = await scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output")
                    .ContentType("text/special")
                    .Accepts("text/garbage");

                _.StatusCodeShouldBe(406);
            });
        }
    }



    public class XmlWriter<T> : IMessageSerializer
    {
        public string MessageType { get; } = typeof(T).ToMessageAlias();
        public Type DotNetType { get; } = typeof(T);
        public string ContentType { get; } = "text/xml";


        public byte[] Write(object model)
        {
            throw new NotSupportedException();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(response.Body, model);

            return Task.CompletedTask;
        }
    }

    public class XmlReader<T> : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(T).ToMessageAlias();
        public Type DotNetType { get; } = typeof(T);
        public string ContentType { get; } = "text/xml";
        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

        public Task<T1> ReadFromRequest<T1>(HttpRequest request)
        {
            var serializer = new XmlSerializer(typeof(T1));
            var model = serializer.Deserialize(request.Body).As<T1>();

            return Task.FromResult(model);
        }
    }


}

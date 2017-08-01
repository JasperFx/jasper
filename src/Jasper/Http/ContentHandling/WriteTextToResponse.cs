using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Baseline.Reflection;
using Jasper.Codegen;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.ContentHandling
{
    public class WriteTextToResponse : MethodCall
    {
        private static readonly MethodInfo _writeMethod =
            ReflectionHelper
                .GetMethod<Stream>(x => x.WriteAsync(new byte[0], 0, 0, default(CancellationToken)));

        public WriteTextToResponse(GetBytes bytes) : base(typeof(Stream), _writeMethod)
        {
            Variables[0] = bytes.Bytes;
            Variables[1] = new Variable(typeof(int), "0");
            Variables[2] = new Variable(typeof(int), $"{bytes.Bytes.Usage}.{nameof(Array.Length)}");
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            var response = chain.FindVariable(typeof(HttpResponse));
            yield return response;

            Target = new Variable(typeof(Stream), $"{response.Usage}.{nameof(HttpResponse.Body)}");

            foreach (var variable in base.resolveVariables(chain))
            {
                yield return variable;
            }
        }
    }
}
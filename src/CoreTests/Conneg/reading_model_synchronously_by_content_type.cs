using System;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace CoreTests.Conneg
{


    public class ConnegMessage
    {
        public string ContentType { get; set; }
        public string Name { get; set; }
    }
}

﻿using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class StatusCodeWriter : IWriterRule
    {
        public bool TryToApply(RouteChain chain)
        {
            if (chain.ResourceType != typeof(int)) return false;


            chain.Postprocessors.Add(new SetStatusCode(chain));
            return true;
        }
    }
}

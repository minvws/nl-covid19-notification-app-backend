using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi.Tests
{
    [TestClass()]
    public class AssemblyStuffTests
    {
        [TestMethod()]
        public void GetCustomAttributeTest()
        {
            Trace.Write(string.Join(Environment.NewLine, GetType().Assembly.Dump()));
        }


    }
}
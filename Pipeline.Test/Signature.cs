using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Cfg.Net.Shorthand;
using Pipeline.Command;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class TestSignature {

        [Test(Description = "Validator")]
        public void Validator() {
            const string xml = @"
<cfg>
  <processes>
    <add name='TestSignature'>
      <data-sets>
        <add name='TestData'>
          <rows>
            <add args='10,0' />
          </rows>
        </add>
      </data-sets>
      <entities>
        <add name='TestData'>
          <fields>
            <add name='args' length='128'>
                <transforms>
                    <add method='fromsplit' separator=','>
                        <fields>
                            <add name='TotalWidth' />
                            <add name='PaddingChar' />
                        </fields>
                    </add>
                </transforms>
            </add>
          </fields>
          <calculated-fields>
            <add name='length' type='int' t='copy(args).splitlength(\,)' />
            <add name='TotalWidthCheck' type='string' t='copy(TotalWidth).is(int)' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>";

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(xml, File.ReadAllText(@"Files\Shorthand.xml")));
            var container = builder.Build();

            var root = container.Resolve<Root>();

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            builder = new ContainerBuilder();
            builder.RegisterModule(new PipelineModule(root, LogLevel.Info));
            container = builder.Build();

            var process = root.Processes.First();

            var output = container.ResolveNamed<IProcessController>(process.Key).Run().ToArray();

            var field = process.Entities.First().CalculatedFields.First(cf => cf.Name == "length");
            Assert.AreEqual(2, output[0][field]);

            foreach (var row in output) {
                Console.WriteLine(row);
            }
        }
    }
}

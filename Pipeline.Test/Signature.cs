using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class TestSignature {

        [Test(Description = "Validator")]
        public void Validator() {
            var xml = @"
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

            var module = new PipelineModule(xml);
            if (module.Root.Errors().Any()) {
                foreach (var error in module.Root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            var container = builder.Build();
            var process = module.Root.Processes.First();

            var output = container.ResolveNamed<IEnumerable<IPipeline>>(process.Key).First().Run().ToArray();

            Assert.AreEqual(2, output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "length")]);

            foreach (var row in output) {
                Console.WriteLine(row);
            }
        }
    }
}

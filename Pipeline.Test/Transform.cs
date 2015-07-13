using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class TestTransform {

        [Test(Description = "Format Transformation")]
        public void FormatTransformer()
        {

            var xml = @"
<cfg>
  <processes>
    <add name='TestProcess'>
      <data-sets>
        <add name='TestData'>
          <rows>
            <add Field1='1' Field2='2' Field3='3' />
          </rows>
        </add>
      </data-sets>
      <entities>
        <add name='TestData' pipeline='streams'>
          <fields>
            <add name='Field1' />
            <add name='Field2' />
            <add name='Field3' />
          </fields>
          <calculated-fields>
            <add name='Format' t='copy(Field1,Field2,Field3).format({0}-{1}+{2})' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>
            ".Replace('\'', '"');


            var builder = new ContainerBuilder();
            var module = new PipelineModule(xml);

            if (module.Root.Errors().Any())
            {
                foreach (var error in module.Root.Errors())
                {
                    Console.WriteLine(error);
                }
                throw new Exception("Configuration Errors");
            }

            builder.RegisterModule(module);
            var container = builder.Build();
            var process = module.Root.Processes.First();

            var output = container.ResolveNamed<IEnumerable<IPipeline>>(process.Key).First().Run().ToArray();

            Assert.AreEqual("1-2+3", output[0][process.Entities.First().CalculatedFields.First()]);

        }
    }
}

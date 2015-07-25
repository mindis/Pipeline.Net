using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;

namespace Pipeline.Test {

    [TestFixture]
    public class JavascriptTransform {

        [Test(Description = "Javascript Transform")]
        public void JavascriptTransformAdd() {

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
            <add name='Format' t='copy(Field1,Field2,Field3).javascript(Field1+Field2+Field3)' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>
            ".Replace('\'', '"');


            var builder = new ContainerBuilder();
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var module = new PipelineModule(xml, shorthand);

            if (module.Root.Errors().Any()) {
                foreach (var error in module.Root.Errors()) {
                    Console.WriteLine(error);
                }
                throw new Exception("Configuration Errors");
            }

            builder.RegisterModule(module);
            var container = builder.Build();
            var process = module.Root.Processes.First();

            var output = container.ResolveNamed<IEnumerable<IEntityPipeline>>(process.Key).First().Run().ToArray();

            Assert.AreEqual("123", output[0][process.Entities.First().CalculatedFields.First()]);

        }
    }
}

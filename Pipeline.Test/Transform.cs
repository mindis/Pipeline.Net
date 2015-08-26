using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Configuration;
using Pipeline.Interfaces;

namespace Pipeline.Test {

    [TestFixture]
    public class TestTransform {

        [Test(Description = "Format Transformation")]
        public void FormatTransformer() {

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
            <add name='Format' t='copy(Field1,Field2,Field3).format({0}-{1}+{2} ).trim()' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>
            ".Replace('\'', '"');


            var builder = new ContainerBuilder();
            var composer = new PipelineComposer();
            var controller = composer.Compose(xml);
            var output = controller.EntityPipelines.First().Run().ToArray();

            Assert.AreEqual("1-2+3", output[0][composer.Root.Processes.First().Entities.First().CalculatedFields.First()]);

        }
    }
}

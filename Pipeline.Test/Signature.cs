using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Command.Modules;
using Pipeline.Interfaces;
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

            var composer = new PipelineComposer();
            var controller = composer.Compose(xml);
            var process = composer.Process;
            var output = controller.Run().ToArray();

            var field = process.Entities.First().CalculatedFields.First(cf => cf.Name == "length");
            Assert.AreEqual(2, output[0][field]);

            foreach (var row in output) {
                Console.WriteLine(row);
            }
        }
    }
}

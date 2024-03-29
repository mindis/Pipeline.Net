﻿using System.Linq;
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


            var composer = new PipelineComposer();
            var controller = composer.Compose(xml);

            var output = controller.Run().ToArray();

            Assert.AreEqual("123", output[0][composer.Process.Entities.First().CalculatedFields.First()]);

        }
    }
}

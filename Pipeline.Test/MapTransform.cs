﻿using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Command.Modules;
using Pipeline.Interfaces;
using Pipeline.Logging;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class MapTransformTester {

        [Test(Description = "Map Transform")]
        public void MapTransformAdd() {

            var xml = @"
<cfg>
  <processes>
    <add name='TestProcess'>
      <data-sets>
        <add name='TestData'>
          <rows>
            <add Field1='1' Field3='^' />
            <add Field1='2' Field3='#' />
            <add Field1='3' Field3='$THREE$' />
            <add Field1='4' Field3='@' />
          </rows>
        </add>
      </data-sets>
      <maps>
         <add name='Map'>
            <items>
               <add from='1' to='One' />
               <add from='2' to='Two' />
               <add from='3' parameter='Field3' />
            </items>
         </add>
      </maps>
      <entities>
        <add name='TestData' pipeline='streams'>
          <fields>
            <add name='Field1' />
            <add name='Field2' />
            <add name='Field3' />
          </fields>
          <calculated-fields>
            <add name='Map' t='copy(Field1).map(map)' default='None' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>";

            var composer = new PipelineComposer();
            var controller = composer.Compose(xml);
            var field = composer.Process.Entities.First().CalculatedFields.First();
            var output = controller.Run().ToArray();

            Assert.AreEqual("One", output[0][field]);
            Assert.AreEqual("Two", output[1][field]);
            Assert.AreEqual("$THREE$", output[2][field]);
            Assert.AreEqual("None", output[3][field]);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Configuration;

namespace Pipeline.Test {

    [TestFixture]
    public class Validate {

        [Test(Description = "Contains Validator")]
        public void ContainsValidator() {
            var xml = @"
<cfg>
  <processes>
    <add name='TestProcess'>
      <data-sets>
        <add name='TestData'>
          <rows>
            <add Field1='11' Field2='12' Field3='13' />
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
            <add name='c1' type='bool' t='copy(Field1).contains(1)' />
            <add name='c2' type='string' t='copy(Field1).contains(2)' />
            <add name='c3' type='bool' t='copy(Field2).contains(1).contains(2)' />
            <add name='c4' type='bool' t='copy(Field2).contains(1).contains(3)' />
            <add name='c5' t='copy(Field2).contains(1).contains(2)' />
            <add name='c6' t='copy(Field2).contains(1).contains(3)' />
          </calculated-fields>
        </add>
      </entities>
    </add>
  </processes>
</cfg>
            ".Replace('\'', '"');

            var root = new Root(xml);

            if (root.Errors().Any()) {
                foreach (var error in root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                System.Environment.Exit(1);
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(new PipelineModule(root));
            var container = builder.Build();
            var process = root.Processes.First();

            var output = container.ResolveNamed<IEnumerable<IPipeline>>(process.Key).First().Run().ToArray();

            Assert.AreEqual(true, output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "c1")]);
            Assert.AreEqual("Field1 does not contain 2.", output[0][process.Entities.First().CalculatedFields.First(cf=>cf.Name=="c2")]);

            Assert.AreEqual(true, output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "c3")]);
            Assert.AreEqual(false, output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "c4")]);

            Assert.AreEqual(string.Empty, output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "c5")]);
            Assert.AreEqual("Field2 does not contain 3.", output[0][process.Entities.First().CalculatedFields.First(cf => cf.Name == "c6")]);

        }
    }
}

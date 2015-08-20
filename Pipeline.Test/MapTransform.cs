using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Interfaces;

namespace Pipeline.Test {

   [TestFixture]
   public class MapTransformTester {

      [Test(Description = "Map Transform")]
      public void MapTransformAdd(){

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

         var output = container.ResolveNamed<IProcessController>(process.Key).EntityPipelines.First().Run().ToArray();

         var field = process.Entities.First().CalculatedFields.First();

         Assert.AreEqual("One", output[0][field]);
         Assert.AreEqual("Two", output[1][field]);
         Assert.AreEqual("$THREE$", output[2][field]);
         Assert.AreEqual("None", output[3][field]);


      }
   }
}

using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Command;
using Pipeline.Configuration;
using Pipeline.Provider.SqlServer;
using PoorMansTSqlFormatterLib;

namespace Pipeline.Test {

    [TestFixture]
    public class Inventory {


        [Test]
        public void InventoryQuery() {
            const string expected = @"SELECT [InventoryKey]
	,[Id]
	,[Timestamp]
	,[StatusChangeTimestamp]
	,[PartKey]
	,[StorageLocationKey]
	,[SerialNo1]
	,[SerialNo2]
	,[SerialNo3]
	,[SerialNo4]
	,[Pallet]
	,[Lot]
	,[ShipmentOrder]
	,[DateReceived]
	,[DateInstalled]
	,[LocationInstalled]
	,[Notes]
	,[InventoryStatusId]
	,[Hide]
	,[SS_RowVersion]
FROM [Inventory]
WHERE ([InventoryStatusId] = 80)
";

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(@"C:\temp\Inventory.xml", @"Files\Shorthand.xml"));
            var container = builder.Build();

            var root = container.Resolve<Root>();
            var process = root.Processes[0];
            var context = new PipelineContext(new ConsoleLogger(), process, process.Entities[0]);
            process.Entities[0].Filter.Add(new Filter { Left = "InventoryStatusId", Right = "80" });
            var sql = new SqlFormattingManager().Format(context.SqlSelectInput(process.Entities[0].GetAllFields().Where(f => f.Input).ToArray()));

            Assert.AreEqual(expected, sql);
        }

        [Test]
        [Ignore("Integration testing")]
        public void InventoryTesting() {

            var composer = new PipelineComposer();
            var controller = composer.Compose(@"C:\temp\Inventory.xml?Mode=init");
            controller.PreExecute();
            controller.Execute();
            controller.PostExecute();

        }
    }


}

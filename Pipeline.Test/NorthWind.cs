using NUnit.Framework;
using Pipeline.Provider.SqlServer;
using PoorMansTSqlFormatterLib;

namespace Pipeline.Test {

    [TestFixture(Description = "Local Northwind Integration Testing")]
    public class Northwind {

        [Test]
        //[Ignore("Integration testing")]
        public void NorthwindIntegrationTesting() {

            var composer = new PipelineComposer();
            var controller = composer.Compose(@"Files\Northwind.xml?Mode=init");

            controller.PreExecute();
            controller.Execute();
            controller.PostExecute();

            Assert.IsNotNull(composer);

        }

        [Test]
        public void StarSql() {
            var composer = new PipelineComposer();
            var controller = composer.Compose(@"Files\Northwind.xml");

            Assert.AreEqual(0, composer.Root.Errors().Length);

            var pipe = new PipelineContext(new TraceLogger(), composer.Process);
            var context = new OutputContext(pipe, new Incrementer(pipe));
            var actual = new SqlFormattingManager().Format(context.SqlCreateStarView());

            Assert.IsNotNull(controller);
            const string expected = @"CREATE VIEW [NorthWindStar]
AS
SELECT A.[A1] AS [OrderDetailsDiscount]
	,A.[A2] AS [OrderDetailsOrderID]
	,A.[A3] AS [OrderDetailsProductID]
	,A.[A4] AS [OrderDetailsQuantity]
	,A.[A5] AS [OrderDetailsRowVersion]
	,A.[A6] AS [OrderDetailsUnitPrice]
	,A.[A7] AS [OrderDetailsExtendedPrice]
	,A.[A8] AS [TflHashCode]
	,A.[A9] AS [CountryExchange]
	,A.[B1] AS [OrdersCustomerID]
	,A.[C1] AS [CustomersAddress]
	,A.[C2] AS [CustomersCity]
	,A.[C3] AS [CustomersCompanyName]
	,A.[C4] AS [CustomersContactName]
	,A.[C5] AS [CustomersContactTitle]
	,A.[C6] AS [CustomersCountry]
	,A.[C8] AS [CustomersFax]
	,A.[C9] AS [CustomersPhone]
	,A.[C10] AS [CustomersPostalCode]
	,A.[C11] AS [CustomersRegion]
	,A.[C12] AS [CustomersRowVersion]
	,A.[B2] AS [OrdersEmployeeID]
	,A.[E8] AS [ProductsSupplierID]
	,A.[E1] AS [ProductsCategoryID]
	,A.[B15] AS [OrdersShipVia]
	,ISNULL(B.[B3], 0.0) AS [OrdersFreight]
	,ISNULL(B.[B4], '12/31/9999 11:59:59 PM') AS [OrdersOrderDate]
	,ISNULL(B.[B6], '12/31/9999 11:59:59 PM') AS [OrdersRequiredDate]
	,ISNULL(B.[B8], '') AS [OrdersShipAddress]
	,ISNULL(B.[B9], '') AS [OrdersShipCity]
	,ISNULL(B.[B10], '') AS [OrdersShipCountry]
	,ISNULL(B.[B11], '') AS [OrdersShipName]
	,ISNULL(B.[B12], '12/31/9999 11:59:59 PM') AS [OrdersShippedDate]
	,ISNULL(B.[B13], '') AS [OrdersShipPostalCode]
	,ISNULL(B.[B14], '') AS [OrdersShipRegion]
	,ISNULL(B.[B16], '12-DEC') AS [TimeOrderMonth]
	,ISNULL(B.[B17], '9999-12-31') AS [TimeOrderDate]
	,ISNULL(B.[B18], '9999') AS [TimeOrderYear]
	,ISNULL(D.[D1], '') AS [EmployeesAddress]
	,ISNULL(D.[D2], '12/31/9999 11:59:59 PM') AS [EmployeesBirthDate]
	,ISNULL(D.[D3], '') AS [EmployeesCity]
	,ISNULL(D.[D4], '') AS [EmployeesCountry]
	,ISNULL(D.[D6], '') AS [EmployeesExtension]
	,ISNULL(D.[D7], '') AS [EmployeesFirstName]
	,ISNULL(D.[D8], '12/31/9999 11:59:59 PM') AS [EmployeesHireDate]
	,ISNULL(D.[D9], '') AS [EmployeesHomePhone]
	,ISNULL(D.[D10], '') AS [EmployeesLastName]
	,ISNULL(D.[D11], '') AS [EmployeesNotes]
	,ISNULL(D.[D14], '') AS [EmployeesPostalCode]
	,ISNULL(D.[D15], '') AS [EmployeesRegion]
	,ISNULL(D.[D17], '') AS [EmployeesTitle]
	,ISNULL(D.[D18], '') AS [EmployeesTitleOfCourtesy]
	,ISNULL(D.[D19], 0) AS [EmployeesReportsTo]
	,ISNULL(D.[D20], '') AS [EmployeesManager]
	,ISNULL(D.[D21], '') AS [Employee]
	,ISNULL(E.[E2], 0) AS [ProductsDiscontinued]
	,ISNULL(E.[E4], '') AS [ProductsProductName]
	,ISNULL(E.[E5], '') AS [ProductsQuantityPerUnit]
	,ISNULL(E.[E6], 0) AS [ProductsReorderLevel]
	,ISNULL(E.[E9], 0.0) AS [ProductsUnitPrice]
	,ISNULL(E.[E10], 0) AS [ProductsUnitsInStock]
	,ISNULL(E.[E11], 0) AS [ProductsUnitsOnOrder]
	,ISNULL(F.[F1], '') AS [SuppliersAddress]
	,ISNULL(F.[F2], '') AS [SuppliersCity]
	,ISNULL(F.[F3], '') AS [SuppliersCompanyName]
	,ISNULL(F.[F4], '') AS [SuppliersContactName]
	,ISNULL(F.[F5], '') AS [SuppliersContactTitle]
	,ISNULL(F.[F6], '') AS [SuppliersCountry]
	,ISNULL(F.[F7], '') AS [SuppliersFax]
	,ISNULL(F.[F8], '') AS [SuppliersHomePage]
	,ISNULL(F.[F9], '') AS [SuppliersPhone]
	,ISNULL(F.[F10], '') AS [SuppliersPostalCode]
	,ISNULL(F.[F11], '') AS [SuppliersRegion]
	,ISNULL(G.[G2], '') AS [CategoriesCategoryName]
	,ISNULL(G.[G3], '') AS [CategoriesDescription]
	,ISNULL(H.[H1], '') AS [ShippersCompanyName]
	,ISNULL(H.[H2], '') AS [ShippersPhone]
	,A.TflBatchId
	,A.TflKey
FROM [NorthWindOrder DetailsTable] A WITH (NOLOCK)
LEFT OUTER JOIN [NorthWindOrdersTable] B WITH (NOLOCK) ON (A.A2 = B.B5)
LEFT OUTER JOIN [NorthWindCustomersTable] C WITH (NOLOCK) ON (A.B1 = C.C7)
LEFT OUTER JOIN [NorthWindEmployeesTable] D WITH (NOLOCK) ON (A.B2 = D.D5)
LEFT OUTER JOIN [NorthWindProductsTable] E WITH (NOLOCK) ON (A.A3 = E.E3)
LEFT OUTER JOIN [NorthWindSuppliersTable] F WITH (NOLOCK) ON (A.E8 = F.F13)
LEFT OUTER JOIN [NorthWindCategoriesTable] G WITH (NOLOCK) ON (A.E1 = G.G1)
LEFT OUTER JOIN [NorthWindShippersTable] H WITH (NOLOCK) ON (A.B15 = H.H4);
";

            Assert.AreEqual(expected, actual);
        }
    }


}

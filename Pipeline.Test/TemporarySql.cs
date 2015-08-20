using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Pipeline.Logging;
using Pipeline.Interfaces;

namespace Pipeline.Test {

    [TestFixture(Description = "Proof of concept")]
    public class TemporarySql {

        [Test(Description = "Integration Testing (Sql Server)")]
        //[Ignore("Integration testing")]
        public void SqlServerIntegrationTesting() {

            const string xml = @"
<cfg>
    <processes>
        <add name='test'>
            <connections>
                <add name='input' 
                     provider='sqlserver' 
                     server='localhost' 
                     database='ClevestAclara' />
                <add name='output' provider='sqlserver' server='localhost' database='TflAclara' />
            </connections>
            <entities>  
                <add name='WorkOrder' 
                     log-interval='1000' 
                     pipeline='linq' 
                     version='SS_RowVersion'>
                    <fields>

                        <add name='WorkOrderKey' type='guid' primary-key='true' label='Work Order Key'/>
                        <add name='Id' type='int64'/>
                        <add name='Timestamp' type='datetime' />
                        <add name='ParentOrderKey' type='guid' label='Parent Order Key'/>
                        <add name='OrderNumber' length='50' label='Order Number'/>
                        <add name='HostOrderNumber' length='50' label='Host Order Number'/>
                        <add name='AuditOrderNumber' length='50' label='Audit Order Number'/>
                        <add name='OrderStatusId' type='int32' label='Order Status Id'/>
                        <add name='CreationDatetime' type='datetime' label='Creation Datetime' />
                        <add name='OrderTypeKey' type='guid' label='Order Type Key'/>
                        <add name='ParentOrderTypeKey' type='guid' label='Parent Order Type Key'/>
                        <add name='CategoryKey' type='guid' label='Category Key'/>
                        <add name='JobCodeKey' type='guid' label='Job Code Key'/>
                        <add name='JobDurDay' type='decimal' precision='18' label='Job Dur Day'/>
                        <add name='JobDurSec' type='int32' label='Job Dur Sec'/>
                        <add name='PriorityKey' type='guid' label='Priority Key'/>
                        <add name='EmergencyFlag' type='byte' label='Emergency Flag'/>
                        <add name='Degree' type='int32'/>
                        <add name='TimeZoneIndex' type='int32' label='Time Zone Index'/>
                        <add name='SuspectFlag' type='byte' label='Suspect Flag'/>
                        <add name='SuspectReason' length='255' label='Suspect Reason'/>
                        <add name='CancelReason' length='255' label='Cancel Reason'/>
                        <add name='SentBackToHostFlag' type='byte' label='Sent Back To Host Flag'/>
                        <add name='SentBackToHostDatetime' type='datetime' label='Sent Back To Host Datetime' />
                        <add name='BusinessGroupKey' type='guid' label='Business Group Key'/>
                        <add name='FinalCompletionDatetime' type='datetime' label='Final Completion Datetime' />
                        <add name='ResolvedDatetime' type='datetime' label='Resolved Datetime' />
                        <add name='RequestStartDatetime' type='datetime' label='Request Start Datetime' />
                        <add name='RequestEndDatetime' type='datetime' label='Request End Datetime' />
                        <add name='DueByDate' type='datetime' label='Due By Date' />
                        <add name='SpecialIndicatorKey' type='guid' label='Special Indicator Key'/>
                        <add name='AuditPassed' type='byte' label='Audit Passed'/>
                        <add name='AttachmentCount' type='int32' label='Attachment Count'/>
                        <add name='CustomerAddress1' length='100' label='Customer Address1' search-type='search-display'/>
                        <add name='CustomerAddress2' length='100' label='Customer Address2'/>
                        <add name='CustomerCity' length='100' label='Customer City'/>
                        <add name='CustomerState' length='100' label='Customer State'/>
                        <add name='CustomerCounty' length='100' label='Customer County'/>
                        <add name='CustomerTown' length='100' label='Customer Town'/>
                        <add name='CustomerZip' length='100' label='Customer Zip'/>
                        <add name='CustomerPhone' length='100' label='Customer Phone'/>
                        <add name='CustomerEmail' length='100' label='Customer Email'/>
                        <add name='Latitude' type='decimal' precision='12' scale='9'/>
                        <add name='Longitude' type='decimal' precision='12' scale='9'/>
                        <add name='AreaKey' type='guid' label='Area Key'/>
                        <add name='BlackoutRegionKey' type='guid' label='Blackout Region Key'/>
                        <add name='OpenWindowDatetime' type='datetime' label='Open Window Datetime' />
                        <add name='CloseWindowDatetime' type='datetime' label='Close Window Datetime' />
                        <add name='BlackoutExpiryDatetime' type='datetime' label='Blackout Expiry Datetime' />
                        <add name='WorkableDates' length='1000' label='Workable Dates'/>
                        <add name='NonWorkableDates' length='1000' label='Non Workable Dates'/>
                        <add name='LiftBlackoutFlag' type='byte' label='Lift Blackout Flag'/>
                        <add name='LocationMatch' length='100' label='Location Match'/>
                        <add name='SuspendReason' label='Suspend Reason'/>
                        <add name='SuspendComment' length='255' label='Suspend Comment'/>
                        <add name='OrderData' type='string' length='max' label='Order Data' length='max' output='false'>
                          <transforms>
                            <add method='fromxml' root='Order'>
                              <fields>

                                <add name='FcNdCustomerPhone' label='Customer Provided Phone' />

                                <add name='FhThAccountNumber1' label='Account Number' />
                                <add name='FhThAccountNumber2' label='Account Number2' />
                                <add name='FhThAddrHouseNumber' label='House Number' />
                                <add name='FhThAddrStreetName' label='Street Name' />
                                <add name='FhThCustomerName1' label='Customer Name' search-type='search-display' />
                                <add name='FhThCustomerPhone2' label='Phone 2' />
                                <add name='FhThCustomerPhone3' label='Phone 3' />
                                <add name='FhThCycle' label='Cycle' default='None' />
                                <add name='FhThExpId1' label='Existing Meter SN' />
                                <add name='FhThExpId3' label='Existing Meter SN2' />
                                <add name='FhThExpMTUID' label='Existing MTU ID' />
                                <add name='FhThMtrDescription' type='string' length='100' label='Meter Description' />
                                <add name='FhThMtrStatus' length='64' label='Meter Status' />
                                <add name='FhThMtrTypeId1' label='Meter Type ID' />
                                <add name='FhThMtrTypeId2' label='Meter Type ID2' />
                                <add name='FhThMtuLocation' label='MTU Location' />
                                <add name='FhThMtuType' type='int32' length='64' label='MTU Type' />
                                <add name='FhThPremiseId' label='Premise ID'>
                                  <transforms>
                                    <add method='padleft' total-width='8' padding-char='0' />
                                  </transforms>
                                </add>
                                <add name='FhThPremiseId2' label='Premise ID2' t='padleft(8,0)' />
                                <add name='FhThQuarterSection' label='Quarter Section' />
                                <add name='SysNdBaseHostOrderNumber' label='Base Host Order Number' />
                                <add name='SysNdNewAssetPropType' label='Proposed Type' />

                                <add name='SysNdPrimaryPhoneDisconnected' type='boolean' label='Primary Phone Disconnected' default='false' />
                                <add name='SysNdSecondaryPhoneDisconnected' type='boolean' label='Secondary Phone Disconnected' default='false' />

                              </fields>
                            </add>
                          </transforms>
                        </add>
                        <add name='SS_RowVersion' type='byte[]' length='8' variable-length='false' label='S S Row Version'/>
                    </fields>
                </add>

                <add name='OrderStatus' version='SS_RowVersion'>
                    <fields>
                        <add name='Id' alias='OrderStatusId' type='int' primary-key='true' />
                        <add name='Name' alias='OrderStatus' length='255' denormalize='true' />
                        <add name='SS_RowVersion' alias='OrderStatusVersion' type='byte[]' length='8' variable-length='false' search-type='none' />
                    </fields>
                </add>

            </entities>

            <relationships>
                <add left-entity='WorkOrder' left-field='OrderStatusId' right-entity='OrderStatus' right-field='OrderStatusId'/>
            </relationships>

        </add>
    </processes>
</cfg>";
            var shorthand = File.ReadAllText(@"Files\Shorthand.xml");
            var module = new PipelineModule(xml, shorthand, LogLevel.Info);

            if (module.Root.Errors().Any()){ 
                foreach (var error in module.Root.Errors()) {
                    Console.Error.WriteLine(error);
                }
                throw new Exception("Configuration Error(s)");
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            var container = builder.Build();

            // resolve and run
            foreach (var process in module.Root.Processes) {
                var controller = container.ResolveNamed<IProcessController>(process.Key);
                controller.Initialize();
                foreach (var pipeline in controller.EntityPipelines) {
                    pipeline.Initialize();
                    pipeline.Execute();
                }
            }

            // release
            container.Dispose();

            //Assert.AreEqual(20088, output.Count());

            //foreach (var row in output.Take(10)) {
            //    Console.WriteLine(row);
            //}
        }
    }


}

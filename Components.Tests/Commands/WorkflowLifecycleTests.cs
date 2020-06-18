// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Components.Tests.Commands
//{
//    [TestClass]
//    public class WorkflowLifecycleTests
//    {
//        //Very badly designed test that cannot run in parallel with another developer or itself
//        //NB Does not appear to be supported by the CosmosDb Emulator
//        [TestMethod]
//        public void StartHere()
//        {
//            var WorkflowSubmitted = new Workflow
//            {
//                Token = "12345",
//                Keys = new[] {
//                    new WorkflowKey
//                    {
//                        Seed = "kTIseJvN3Ro2mNDgPnxdOewOzBwzWmXGbg8FEKPNPG0=",
//                        Epoch = 56446554
//                    },
//                    new WorkflowKey
//                    {
//                        Seed = "voKfxC9vmfhnAMSHDa+EL1C6onAwkHtJDQ2KYcKP78Y=",
//                        Epoch = 98733
//                    },
//                }
//            };

//            var localTestConfigProvider = new McsTestConfigProvider();
//            var standardUtcDateTimeProvider = new StandardUtcDateTimeProvider();

//            var newWorkflow = new WorkflowCreateCommand(localTestConfigProvider, standardUtcDateTimeProvider).Execute(WorkflowSubmitted);
//            Assert.AreEqual(false, newWorkflow.Authorised);
//            Assert.AreEqual(2, newWorkflow.WorkflowKeys.Length);
//            Assert.AreEqual("kTIseJvN3Ro2mNDgPnxdOewOzBwzWmXGbg8FEKPNPG0=", newWorkflow.WorkflowKeys.Single(x => x.Epoch == 56446554).Seed);

//            var authorisedWorkflow = new WorkflowAuthoriseCommand(localTestConfigProvider).Execute(WorkflowSubmitted.Token);
//            Assert.AreEqual(true, authorisedWorkflow.Authorised);

//            var snapshot = new WorkflowGetAuthorisedSnapshotCommand(localTestConfigProvider).Execute();
//            Assert.AreEqual(true, snapshot.Any());

//            new WorkflowDeleteSnapshotCommand(localTestConfigProvider).Execute(snapshot);

//            //Test for deletion?
//            var theMissing = new WorkflowGetAuthorisedSnapshotCommand(localTestConfigProvider).Execute();
//            Assert.AreEqual(false, theMissing.Any());
//        }
//    }

//}
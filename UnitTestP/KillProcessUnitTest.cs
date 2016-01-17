using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideApp;

namespace UnitTestP
{
    [TestClass]
    public class KillProcessUnitTest
    {
        KillProcess killprocee = new KillProcess();

        [TestMethod]
        public void TestLoad()
        {
            KillProcess.Load();
        }
        [TestMethod]
        public void TestRun()
        {
            KillProcess.Run();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideApp;
using System.Collections.Generic;

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
        [TestMethod]
        public void TestLoopWindow()
        {
            List<string> windows = new List<string>();
            windows.Add("IEFrame");
            KillProcess.LoopWindows(windows);
        }
    }
}

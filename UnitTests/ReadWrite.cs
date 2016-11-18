using CritterShell.Critters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CritterShell.UnitTests
{
    [TestClass]
    public class ReadWrite : SpreadsheetReaderWriter
    {
        [TestMethod]
        public void VerifyHeader()
        {
            List<string> mockColumnsFromFile = new List<string>() { "Optional", "Required", "Extra" };
            ReadOnlyCollection<ColumnDefinition> knownColumns = new List<ColumnDefinition>()
                {
                    new ColumnDefinition("Optional"),
                    new ColumnDefinition("Required", true)
                }.AsReadOnly();
            FileReadResult readResult = this.VerifyHeader(mockColumnsFromFile, knownColumns);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(readResult.Verbose.Count == 1);
            Assert.IsTrue(readResult.Warnings.Count == 0);

            mockColumnsFromFile.Remove("Optional");
            readResult = this.VerifyHeader(mockColumnsFromFile, knownColumns);
            Assert.IsFalse(readResult.Failed);
            Assert.IsTrue(readResult.Verbose.Count == 1);
            Assert.IsTrue(readResult.Warnings.Count == 1);

            mockColumnsFromFile.Remove("Required");
            readResult = this.VerifyHeader(mockColumnsFromFile, knownColumns);
            Assert.IsTrue(readResult.Failed);
            Assert.IsTrue(readResult.Verbose.Count == 1);
            Assert.IsTrue(readResult.Warnings.Count == 2);
        }

        protected override FileReadResult TryRead(Func<List<string>> readLine)
        {
            throw new NotImplementedException();
        }

        public override void WriteCsv(string filePath)
        {
            throw new NotImplementedException();
        }

        public override void WriteXlsx(string filePath, string worksheetName)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Jdn45Common;


namespace Jdn45CommonUnitTests
{
    [TestClass]
    public class CollectionUtilTests
    {
        [TestMethod]
        public void HaveSameElementsTest1_SameElements_SameOrder()
        {
            List<string> listA = new List<string>(new string[] { "a", "B", "See" });
            List<string> listB = new List<string>(new string[] { "a", "B", "See" });

            Assert.IsTrue(CollectionUtil<string>.HaveSameElements(listA, listB));
        }

        [TestMethod]
        public void HaveSameElementsTest1_SameElements_DifferentOrder()
        {
            List<string> listA = new List<string>(new string[] { "a", "B", "See" });
            List<string> listB = new List<string>(new string[] { "B", "See", "a" });

            Assert.IsTrue(CollectionUtil<string>.HaveSameElements(listA, listB));
        }

    }
}

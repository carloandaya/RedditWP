using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Collections.Generic;

namespace TestRedditWP
{
    [TestClass]
    public class AnonymousUser
    {
        [TestMethod]
        public void Log_In()
        {
            // arrange
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            // act

            // assert
            Assert.IsInstanceOfType(username.GetType(), typeof(string));
        }

        
    }
}

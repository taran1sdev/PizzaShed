using NUnit.Framework;
using Moq;
using Microsoft.Data.SqlClient;
using PizzaShed.Services.Data;
using PizzaShed.Services.Logging;

namespace PizzaShedTests
{
    [TestFixture]
    public class DatabaseManagerTests
    {
        private DatabaseManager _databaseManager;

        [OneTimeSetUp]
        // Database manager is a singleton class so we only want it to initialize once
        public void OneTimeSetUp()
        {
            _databaseManager = DatabaseManager.Instance;
        }

        [Test]
        // Check that ExecuteQuery returns expected result on success
        public void ExecuteQuery_Success()
        {
            const string expectedResult = "Success";

            string result = _databaseManager.ExecuteQuery<string>(conn =>
            {
                Assert.That(conn, Is.TypeOf<SqlConnection>(), "conn should be an SqlConnection object");

                return expectedResult;
            });

            Assert.That(result, Is.EqualTo(expectedResult), "ExecuteQuery should return the result defined in the input function");
        }

        [Test]
        // Check that ExecuteQuery returns default object on exception
        public void ExecuteQuery_Exception_ReturnsDefault()
        {
            bool result = _databaseManager.ExecuteQuery<bool>(conn =>
            {
                throw new Exception("Simulate exception");
            });

            Assert.That(result, Is.EqualTo(default(bool)), "Execute Query should return default of return type on exception");
        }        
    }
}

using System.Data;
using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class ParameterTests
{
   [SetUp]
   public void SetUp()
   {
		   
   }
	   
   [Test]
   public void Parameter_WhenNew_ShouldSetPropertiesToDefaultValues()
   {
	  var parameter = new Parameter();
	  Assert.That(parameter.Value, Is.Not.Null);
	  Assert.That(parameter.Precision, Is.EqualTo(default(byte)));
	  Assert.That(parameter.Scale, Is.EqualTo(default(byte)));
	  Assert.That(parameter.Size, Is.EqualTo(default(int)));
	  Assert.That(parameter.DbType, Is.EqualTo(default(DbType)));
	  Assert.That(parameter.Direction, Is.EqualTo(default(ParameterDirection)));
	  Assert.That(parameter.IsNullable, Is.False);
	  Assert.That(parameter.ParameterName, Is.EqualTo(string.Empty));
	  Assert.That(parameter.SourceColumn, Is.EqualTo(string.Empty));
	  Assert.That(parameter.SourceVersion, Is.EqualTo(default(DataRowVersion)));
   }
	   
   [Test]
   public void Value_WhenChanged_ShouldReturnSameValue()
   {
	  var parameter = new Parameter { Value = 123 };
	  Assert.That(parameter.Value, Is.EqualTo(123));
		  
	  parameter.Value = "abc";
	  Assert.That(parameter.Value, Is.EqualTo("abc"));
		  
	  parameter.Value = null;
	  Assert.That(parameter.Value, Is.Null);
   }
	   
   [TestCase(DbType.AnsiString)]
   [TestCase(DbType.Int32)]
   [TestCase(DbType.Double)]
   public void DbType_WhenAssigned_ShouldReturnSameAssignedDbType(DbType dbType)
   {
	  var parameter = new Parameter { DbType = dbType };
	  Assert.That(parameter.DbType, Is.EqualTo(dbType));
   }
	   
   [TestCase(ParameterDirection.Input)]
   [TestCase(ParameterDirection.Output)]
   public void Direction_WhenAssigned_ShouldReturnSameAssignedDirection(ParameterDirection direction)
   {
	  var parameter = new Parameter { Direction = direction };
	  Assert.That(parameter.Direction, Is.EqualTo(direction));
   }
	   
   [Test]
   public void Precision_WhenAssigned_ShouldReturnSameAssignedPrecision()
   {
	  var parameter = new Parameter { Precision = 10 };
	  Assert.That(parameter.Precision, Is.EqualTo(10));
   }
	   
   [Test]
   public void Scale_WhenAssigned_ShouldReturnSameAssignedScale()
   {
	  var parameter = new Parameter { Scale = 2 };
	  Assert.That(parameter.Scale, Is.EqualTo(2));
   }
	   
   [Test]
   public void Size_WhenAssigned_ShouldReturnSameAssignedSize()
   {
	  var parameter = new Parameter { Size = 512 };
	  Assert.That(parameter.Size, Is.EqualTo(512));
   }
	   
   [TestCase("", false)]
   [TestCase(null, true)]
   [TestCase("myParam", false)]
   public void ParameterName_WhenAssigned_ShouldReturnSameAssignedParameterName(string parameterName, bool shouldBeNull)
   {
	  var parameter = new Parameter { ParameterName = parameterName };
	  Assert.That(parameter.ParameterName, shouldBeNull ? Is.Null : Is.EqualTo(parameterName));
   }

   [TestCase("", false)]
   [TestCase(null, true)]
   [TestCase("col1", false)]
   public void SourceColumn_WhenAssigned_ShouldReturnSameAssignedSourceColumn(string sourceColumn, bool shouldBeNull)
   {
	  var parameter = new Parameter { SourceColumn = sourceColumn };
	  Assert.That(parameter.SourceColumn, shouldBeNull ? Is.Null : Is.EqualTo(sourceColumn));
   }
}
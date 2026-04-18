using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Courses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.CourseDtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace AYA_UIS.Application.Queries.Courses.UnitTests
{
    [TestClass]
    public class GetDepartmentCoursesQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the DepartmentId property for a variety of integer inputs.
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: The constructed object's DepartmentId equals the provided input and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void GetDepartmentCoursesQuery_Constructor_AssignsDepartmentId_ForVariousValues()
        {
            // Arrange
            int[] testValues = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int value in testValues)
            {
                // Act
                var query = new GetDepartmentCoursesQuery(value);

                // Assert
                Assert.AreEqual(value, query.DepartmentId, $"Constructor should set DepartmentId to {value}.");
            }
        }

        /// <summary>
        /// Ensures the query type implements MediatR.IRequest&lt;IEnumerable&lt;CourseDto&gt;&gt; as declared.
        /// Input: typical department id (42).
        /// Expected: The instance is assignable to IRequest&lt;IEnumerable&lt;CourseDto&gt;&gt;.
        /// </summary>
        [TestMethod]
        public void GetDepartmentCoursesQuery_ImplementsIRequestOfIEnumerableCourseDto()
        {
            // Arrange
            int departmentId = 42;

            // Act
            var query = new GetDepartmentCoursesQuery(departmentId);

            // Assert
            Assert.IsTrue(query is IRequest<IEnumerable<CourseDto>>, "GetDepartmentCoursesQuery should implement IRequest<IEnumerable<CourseDto>>.");
        }
    }
}
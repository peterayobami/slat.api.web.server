using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slat.Core;
using System.Net;

namespace Slat.Api.Web.Server
{
    /// <summary>
    /// Manages standard Web API
    /// </summary>
    public class StudentsController : ControllerBase
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        private readonly ApplicationDbContext _context;

        #endregion

        #region Controller

        /// <summary>
        /// Default controller
        /// </summary>
        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        /// <summary>
        /// Fetches a student by the unique matric number
        /// </summary>
        /// <param name="matricNumber">The specified matric number</param>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchStudent)]
        public async Task<ApiResponse> FetchStudentAsync([FromQuery] string matricNumber)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            object result = default;

            try
            {
                // If matric number was not provided...
                if (string.IsNullOrEmpty(matricNumber))
                    // Throw exception
                    throw new Exception("Student's matric number is required!");

                // Fetch the student
                var student = await _context.Students.FirstOrDefaultAsync(student => student.MatricNo == matricNumber);

                // Set the result
                result = student ?? throw new Exception($"The matric number: {matricNumber} does not match an existing student");
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return response
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Fetches all students
        /// </summary>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchStudents)]
        public async Task<ApiResponse> FetchStudentsAsync()
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            object result = default;

            try
            {
                // Fetch the student
                var students = await _context.Students.ToListAsync();

                // Set the result
                result = students;
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return response
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Updates a student's photo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.UpdateStudentPhoto)]
        public async Task<ApiResponse> UpdateStudentPhotoAsync([FromBody] UpdateStudentPhotoApiModel model)
        {
            // Initialize error message
            string errorMessage = default;

            try
            {
                // If photo was not provided...
                if (string.IsNullOrEmpty(model.EncodedPhoto))
                    // Throw exception
                    throw new Exception("This operation require a student\'s photo base 64 encoded format.");

                // Fetch the student
                var student = await _context.Students.FindAsync(model.Id);

                // If student was not found...
                if (student == null)
                    // Throw exception
                    throw new Exception("The specified id does not match a student");

                // Set the photo
                student.Photo = model.EncodedPhoto;

                // Update the student
                _context.Students.Update(student);
                
                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return response
            return new ApiResponse
            {
                ErrorMessage = errorMessage
            };
        }
    }
}

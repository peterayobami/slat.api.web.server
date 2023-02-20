using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slat.Core;
using System.Net;
using System.Net.Mail;

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

        /// <summary>
        /// The DI instance of <see cref="IConfiguration"/>
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The DI instance of the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<StudentsController> _logger;

        #endregion

        #region Controller

        /// <summary>
        /// Default controller
        /// </summary>
        public StudentsController(ApplicationDbContext context, IConfiguration configuration, ILogger<StudentsController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
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

        /// <summary>
        /// Validates the specified student's access by sending a code for further verification
        /// </summary>
        /// <param name="matricNo">The specified student's email</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.ValidateStudentAccess)]
        public async Task<ApiResponse> ValidateStudentAccessAsync([FromQuery] string matricNo)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(object);

            // If the student's matric number was not provided...
            if (string.IsNullOrEmpty(matricNo))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "Student's matric number is required"
                };
            }

            // Fetch the matric
            var student = await _context.Students
                .FirstOrDefaultAsync(student => student.MatricNo == matricNo);

            // If student was not found...
            if (student is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "A student with the specified matric number does not exist."
                };
            }

            try
            {
                // Update the access code
                student.AccessCode = new Random().Next(minValue: 100001, maxValue: 999999);

                // Update the student
                _context.Students.Update(student);

                // Save changes
                await _context.SaveChangesAsync();

                try
                {
                    // Set the username
                    var userName = _configuration["Email:Username"];

                    // Set the password
                    var password = _configuration["Email:Password"];

                    // Set the message body
                    string messageBody = @$"<p>Hello {student.FirstName},</p> <p>Your access code is {student.AccessCode}.</p>";

                    // Create the message instance
                    var message = new MailMessage(new MailAddress(userName, "Slat | Yaba College of Technology"),
                        new MailAddress(student.Email))
                    {
                        Subject = "Verify Your Access",
                        Body = messageBody,
                        IsBodyHtml = true
                    };

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Credentials = new NetworkCredential(userName, password);
                        smtpClient.Host = "smtp.outlook.com";
                        smtpClient.Port = 587;
                        smtpClient.EnableSsl = true;

                        smtpClient.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    _logger.LogError(ex.Message);

                    // Set the status code
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "An error occurred while trying to send a validation email."
                    };
                }

                // Set the result
                result = new { id = student.Id, email = student.Email, firstName = student.FirstName, lastName = student.LastName, photo = student.Photo };
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return the response
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Verifies the specified access
        /// </summary>
        /// <param name="matricNo">The student's email</param>
        /// <param name="accessCode">The access code</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.VerifyStudentAccess)]
        public async Task<ApiResponse> VerifyLecturerAccessAsync([FromQuery] string matricNo, [FromQuery] int accessCode)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(object);

            try
            {
                // If the student's email was not provided...
                if (string.IsNullOrEmpty(matricNo))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "Student's email is required"
                    };
                }

                // If the access code was not provided...
                if (string.IsNullOrEmpty(accessCode.ToString()))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "Access code is required"
                    };
                }

                // Fetch the student
                var student = await _context.Students.FirstOrDefaultAsync(student => student.MatricNo == matricNo);

                // If lecturer was not found...
                if (student is null)
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "A student with the specified matric number does not exist."
                    };
                }

                // If the access code does not match the specified lecturer...
                if (student.AccessCode != accessCode)
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                    // Throw exception
                    throw new Exception("Invalid access code");
                }

                // Set the result
                result = "Valid access code";
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return error response
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }
    }
}

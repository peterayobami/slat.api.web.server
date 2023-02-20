using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slat.Core;
using System.Net;
using static Duende.IdentityServer.Models.IdentityResources;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Slat.Api.Web.Server
{
    /// <summary>
    /// Manages standard Web API
    /// </summary>
    public class LecturersController : ControllerBase
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
        /// The DI instance of the <see cref="ILogger"/>
        /// </summary>
        private readonly ILogger<LecturersController> _logger;

        #endregion

        #region Controller

        /// <summary>
        /// Default controller
        /// </summary>
        public LecturersController(ApplicationDbContext context, IConfiguration configuration, ILogger<LecturersController> logger)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        #endregion

        /// <summary>
        /// Fetches the specified lecturer
        /// </summary>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchLecturerByEmail)]
        public async Task<ApiResponse> FetchLecturerAsync([FromQuery] string lecturerEmail)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(FetchLecturerApiModel);

            // If lecturer id was not specified...
            if (string.IsNullOrEmpty(lecturerEmail))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Lecturer's email is required"
                };
            }

            try
            {
                // Fetch the lecturer
                var lecturer = await _context.Lecturers
                    .Select(lecturer => new FetchLecturerApiModel
                    {
                        Id = lecturer.Id,
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName,
                        Email = lecturer.Email,
                        Photo = lecturer.Photo
                    })
                    .FirstOrDefaultAsync(lecturer => lecturer.Email == lecturerEmail);

                // If we find a lecturer...
                if (lecturer is not null)
                {
                    // Match the lecturer courses
                    var lecturerCourses = await _context.LecturerCourses
                        .Include(lc => lc.Course)
                        .Where(lc => lc.LecturerId == lecturer.Id)
                        .ToListAsync();

                    // Fetch the courses pertaining to the lecturer
                    var courses = lecturerCourses
                        .Select(lc => new CourseApiModel
                        {
                            CourseId = lc.Course.Id,
                            CourseTitle = lc.Course.Title,
                            CourseCode = lc.Course.Code,
                            CourseUnit = lc.Course.Unit,
                            CourseDescription = lc.Course.Description
                        })
                        .ToList();

                    // Set the courses
                    lecturer.Courses = lecturerCourses.Any() ? courses : null;

                    // Set the result
                    result = lecturer;
                }
            }
            catch (Exception ex)
            {
                // Set status code
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
        /// Validates the specified lecturer's access by sending a code for further verification
        /// </summary>
        /// <param name="lecturerEmail">The specified lecturer's email</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.ValidateLecturerAccess)]
        public async Task<ApiResponse> ValidateLecturerAccessAsync([FromQuery] string lecturerEmail)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(object);

            // If the lecturer's email was not provided...
            if (string.IsNullOrEmpty(lecturerEmail))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "Lecturer's email is required"
                };
            }

            // Fetch the lecturer
            var lecturer = await _context.Lecturers
                .Include(lec => lec.Courses)
                .FirstOrDefaultAsync(lec => lec.Email == lecturerEmail);

            // If lecturer was not found...
            if (lecturer is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "A lecturer with the specified email does not exist."
                };
            }

            try
            {
                // Update the access code
                lecturer.AccessCode = new Random().Next(minValue: 100001, maxValue: 999999);

                // Update the lecturer
                _context.Lecturers.Update(lecturer);

                // Save changes
                await _context.SaveChangesAsync();

                try
                {
                    // Set the username
                    var userName = _configuration["Email:Username"];

                    // Set the password
                    var password = _configuration["Email:Password"];

                    // Set the message body
                    string messageBody = @$"<p>Hello {lecturer.FirstName},</p> <p>Your access code is {lecturer.AccessCode}.</p>";

                    // Create the message instance
                    var message = new MailMessage(new MailAddress(userName, "Slat | Yaba College of Technology"),
                        new MailAddress(lecturerEmail))
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
                result = new { id = lecturer.Id, email = lecturer.Email, firstName = lecturer.FirstName, lastName = lecturer.LastName, photo = lecturer.Photo };
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
        /// <param name="lecturerEmail">The lecturer's email</param>
        /// <param name="accessCode">The access code</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.VerifyLecturerAccess)]
        public async Task<ApiResponse> VerifyLecturerAccessAsync([FromQuery] string lecturerEmail, [FromQuery] int accessCode)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(object);

            try
            {
                // If the lecturer's email was not provided...
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "Lecturer's email is required"
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

                // Fetch the lecturer
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(lec => lec.Email == lecturerEmail);

                // If lecturer was not found...
                if (lecturer is null)
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    // Return error response
                    return new ApiResponse
                    {
                        ErrorMessage = "A lecturer with the specified email does not exist."
                    };
                }

                // If the access code does not match the specified lecturer...
                if (lecturer.AccessCode != accessCode)
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

        /// <summary>
        /// Creates a lecture with provided credentials
        /// </summary>
        /// <param name="lectureCredentials">The <see cref="CreateLecturerApiModel"/></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.CreateLecture)]
        public async Task<ApiResponse> CreateLectureAsync([FromBody] CreateLectureApiModel lectureCredentials)
        {
            // Initialize error message
            string errorMessage = default;

            // If lecturer id was not provided...
            if (string.IsNullOrEmpty(lectureCredentials.LecturerId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Lecturer id is required"
                };
            }

            // If course id was not provided...
            if (string.IsNullOrEmpty(lectureCredentials.CourseId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Course id is required"
                };
            }

            // Retrieve the lecturer
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(lec => lec.Id == lectureCredentials.LecturerId);

            // If lecturer was not found...
            if (lecturer is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "The specified lecturer was not found. Please provide a valid lecturer id"
                };
            }

            // Retrieve the course
            var course = await _context.Courses.FirstOrDefaultAsync(course => course.Id == lectureCredentials.CourseId);

            // If course was not found...
            if (course is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "The specified course was not found. Please provide a valid course id"
                };
            }

            // If lecturer does not own specified course...
            if (!await _context.LecturerCourses.AnyAsync(lc => lc.LecturerId == lecturer.Id && lc.CourseId == course.Id))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.Forbidden;

                // Return error response
                return new ApiResponse
                {
                    ErrorResult = new ErrorResult
                    {
                        Errors = new List<IssuesApiModel>
                        {
                            new IssuesApiModel
                            {
                                Status = (int)HttpStatusCode.Forbidden,
                                Code = 19941,
                                Title = "Lecturer and Course Mismatch",
                                Detail = "The specified lecturer does not own the specified course"
                            }
                        }
                    },
                    ErrorMessage = "The specified lecturer does not own the specified course"
                };
            }

            // Initialize the result
            var result = default(LectureApiModel);

            try
            {
                // Get the total count of lectures on current course
                var currentCount = await _context.Lectures.CountAsync(lec => lec.CourseId == lectureCredentials.CourseId);

                // Set the next count
                int nextCount = ++currentCount;

                // Set the lecture credentials
                var lecture = new LecturesDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    LecturerId = lectureCredentials.LecturerId,
                    CourseId = lectureCredentials.CourseId,
                    Title = lectureCredentials.Title ?? $"Lecture {nextCount} | {course.Title} ({course.Code})",
                    Description = lectureCredentials.Description ?? $"This is lecture number {nextCount} on {course.Title} taken by {lecturer.FirstName} {lecturer.LastName}"
                };

                // Create the lecture
                await _context.Lectures.AddAsync(lecture);

                // Save changes
                var succeeded = await _context.SaveChangesAsync() > 0;

                // If succeeded...
                if (succeeded)
                {
                    // Set the result
                    result = new LectureApiModel
                    {
                        Id = lecture.Id,
                        Title = lecture.Title,
                        Description = lecture.Description,
                        DateCreated = lecture.DateCreated
                    };
                }
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
        /// Marks attendance of specified student for specified lecture
        /// </summary>
        /// <param name="attendanceCredentials">The attendance api model</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpPost(ApiRoutes.MarkAttendance)]
        public async Task<ApiResponse> MarkAttendanceAsync([FromBody] AttendanceApiModel attendanceCredentials)
        {
            // Initialize error message
            string errorMessage = default;

            // If student's matric no was not provided
            if (string.IsNullOrEmpty(attendanceCredentials.MatricNo))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "The student's matric number is required"
                };
            }

            // If lecture's id was not provided...
            if (string.IsNullOrEmpty(attendanceCredentials.LectureId))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "Lecture's id is required"
                };
            }

            // Fetch the student
            var student = await _context.Students.FirstOrDefaultAsync(st => st.MatricNo == attendanceCredentials.MatricNo);

            // If student was not found...
            if (student is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = $"Student with matric no: {attendanceCredentials.MatricNo} was not found"
                };
            }

            // Fetch the lecture
            var lecture = await _context.Lectures
                .Include(lec => lec.Course)
                .FirstOrDefaultAsync(lec => lec.Id == attendanceCredentials.LectureId);

            // If lecture was not found...
            if (lecture is null)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = $"Lecture with id: {attendanceCredentials.LectureId} was not found"
                };
            }

            // If student does not own specified course...
            if (!await _context.StudentCourses.AnyAsync(sc => sc.StudentId == student.Id && sc.CourseId == lecture.CourseId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.Forbidden;

                // Return error response
                return new ApiResponse
                {
                    ErrorResult = new ErrorResult
                    {
                        Errors = new List<IssuesApiModel>
                        {
                            new IssuesApiModel
                            {
                                Status = (int)HttpStatusCode.Forbidden,
                                Code = 19042,
                                Title = "Student was not Register for Specified Course",
                                Detail = $"The specified student, {student.FirstName} {student.LastName} ({student.MatricNo}) was not registered for {lecture.Course.Title} ({lecture.Course.Code})"
                            }
                        }
                    },
                    ErrorMessage = $"Student was not Register for Specified Course"
                };
            }

            // If student has been marked before...
            if (await _context.Attendees.AnyAsync(at => at.StudentId == student.Id && at.LectureId == lecture.Id))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.Created;

                // Return response
                return new ApiResponse
                {
                    Result = "Student attendance already taken"
                };
            }

            try
            {
                // Set attendee credentials
                var attendee = new AttendeesDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    LectureId = lecture.Id,
                    StudentId = student.Id
                };

                // Create student as lecture attendee
                await _context.Attendees.AddAsync(attendee);

                // Save changes
                var succeeded = await _context.SaveChangesAsync() > 0;

                // If failed...
                if (!succeeded)
                    // Set error message
                    errorMessage = "Failed to mark attendance due to an error";
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

        [HttpGet(ApiRoutes.FetchLectureAttendees)]
        public async Task<ApiResponse> FetchLectureAttendeesAsync([FromQuery] string lectureId)
        {
            // Initialize error message
            string errorMessage = default;

            // If lecture id was not specified...
            if (string.IsNullOrEmpty(lectureId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "The lecture id is required"
                };
            }

            // If we don't find a lecture...
            if (!await _context.Lectures.AnyAsync(l => l.Id == lectureId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.NotFound;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = $"The specified lecture id: {lectureId} does not match an existing lecture"
                };
            }

            // Initialize result
            var result = new List<LectureAttendeeApiModel> { };

            try
            {
                // Fetch attendees by lecture id
                var attendees = await _context.Attendees
                    .Include(at => at.Student)
                    .Select(at => new LectureAttendeeApiModel
                    {
                        MatricNo = at.Student.MatricNo,
                        Email = at.Student.Email,
                        FirstName = at.Student.FirstName,
                        LastName = at.Student.LastName
                    })
                    .ToListAsync();

                // Set the result
                result = attendees;
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Log the error message
                _logger.LogError(ex.Message);

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
        /// Retrieves attendance records for a lecturer's courses
        /// </summary>
        /// <param name="lecturerId">The lecturer's id</param>
        /// <returns>The <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.FetchLecturerAttendanceRecords)]
        public async Task<ApiResponse> FetchLecturerAttendanceRecordsAsync([FromQuery] string lecturerId)
        {
            // Initialize error message
            string errorMessage = default;

            // Validate lecturer

            // Initialize result
            var result = new FetchLecturerAttendanceRecordApiModel
            {
                Courses = new List<CoursesApiModel> { }
            };

            try
            {
                // Fetch all the courses, course lectures, and
                // lecture attendees of the specified lecturer
                var lc = await _context.LecturerCourses
                    .Include(lc => lc.Course)
                    .ThenInclude(course => course.Lectures)
                    .ThenInclude(lec => lec.Attendees)
                    .ThenInclude(at => at.Student)
                    .Where(lec => lec.LecturerId == lecturerId)
                    .ToListAsync();

                // For each courses...
                foreach (var item in lc)
                {
                    // Set the lectures
                    var lectures = item.Course.Lectures.Select(lec => new CourseLectureApiModel()
                    {
                        Id = lec.Id,
                        Title = lec.Title,
                        Description = lec.Description,
                        Attendees = lec.Attendees.Select(attendee => new LectureAttendeeApiModel
                        {
                            MatricNo = attendee.Student.MatricNo,
                            Email = attendee.Student.Email,
                            FirstName = attendee.Student.FirstName,
                            LastName = attendee.Student.LastName
                        }),
                        DateCreated = lec.DateCreated
                    });

                    // Add the lectures and lecture attendees
                    result.Courses.Add(new CoursesApiModel
                    {
                        CourseId = item.Course.Id,
                        CourseTitle = item.Course.Title,
                        CourseCode = item.Course.Code,
                        CourseUnit = item.Course.Unit,
                        CourseDescription = item.Course.Description,
                        Lectures = lectures.OrderBy(lecture => lecture.DateCreated)
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex.Message);

                // Set the error message
                errorMessage = ex.Message;

                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            // Return response
            return new ApiResponse
            {
                Result = result,
                ErrorMessage = errorMessage
            };
        }
    }
}

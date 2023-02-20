using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slat.Core;
using System.Net;

namespace Slat.Api.Web.Server
{
    /// <summary>
    /// Manages standard Web API
    /// </summary>
    public class AdminController : ControllerBase
    {
        #region Private Members

        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// The DI instance of the <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<AdminController> _logger;

        #endregion

        #region Controller

        /// <summary>
        /// Default controller
        /// </summary>
        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion

        /// <summary>
        /// Creates a course with provided credentials
        /// </summary>
        /// <param name="courseCredentials">The course credentials</param>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpPost(ApiRoutes.CreateCourse)]
        public async Task<ApiResponse> CreateCourseAsync([FromBody] CreateCourseApiModel courseCredentials)
        {
            // Initialize error message
            string errorMessage = default;

            // If course code was not specified...
            if (string.IsNullOrEmpty(courseCredentials.CourseCode))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Course code cannot be null"
                };
            }

            // If course title was not specified...
            if (string.IsNullOrEmpty(courseCredentials.CourseTitle))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Course title cannot be null"
                };
            }

            // If course unit is less than or equal to zero...
            if (courseCredentials.CourseUnit <= 0)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Course unit cannot be less than or equal to zero"
                };
            }

            // Initialize the result
            var result = default(CourseApiModel);

            try
            {
                // Set the course credentials
                var course = new CoursesDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = courseCredentials.CourseCode,
                    Title = courseCredentials.CourseTitle,
                    Unit = courseCredentials.CourseUnit,
                    Description = courseCredentials.CourseDescription
                };

                // Create the course
                await _context.Courses.AddAsync(course);

                // Save changes
                var succeeded = await _context.SaveChangesAsync() > 0;

                // If succeeded...
                if (succeeded)
                {
                    // Set the result
                    result = new CourseApiModel
                    {
                        CourseId = course.Id,
                        CourseUnit = course.Unit,
                        CourseCode = course.Code,
                        CourseDescription = course.Description
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
        /// Creates a collection of courses
        /// </summary>
        /// <param name="model">The collection of courses</param>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>

        [HttpPost(ApiRoutes.CreateCourses)]
        public async Task<ApiResponse> CreateCoursesAsync([FromBody] List<CreateCourseApiModel> model)
        {
            // Initialize error message
            string errorMessage = default;

            // For each course credentials
            foreach (var courseCredentials in model)
            {
                // If course code was not specified...
                if (string.IsNullOrEmpty(courseCredentials.CourseCode))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return response
                    return new ApiResponse
                    {
                        ErrorMessage = "Course code cannot be null"
                    };
                }

                // If course title was not specified...
                if (string.IsNullOrEmpty(courseCredentials.CourseTitle))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return response
                    return new ApiResponse
                    {
                        ErrorMessage = "Course title cannot be null"
                    };
                }

                // If course unit is less than or equal to zero...
                if (courseCredentials.CourseUnit <= 0)
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return response
                    return new ApiResponse
                    {
                        ErrorMessage = "Course unit cannot be less than or equal to zero"
                    };
                }
            }

            // Initialize the result
            var result = default(List<CourseApiModel>);

            try
            {
                // Initialize courses
                var courses = new List<CoursesDataModel>();

                // For each course in model...
                foreach (var courseCredentials in model)
                {
                    // Set the course credentials
                    var course = new CoursesDataModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = courseCredentials.CourseCode,
                        Title = courseCredentials.CourseTitle,
                        Unit = courseCredentials.CourseUnit,
                        Description = courseCredentials.CourseDescription
                    };

                    // Add the course to courses
                    courses.Add(course);
                }

                // Create the course
                await _context.Courses.AddRangeAsync(courses);

                // Save changes
                var succeeded = await _context.SaveChangesAsync() > 0;

                // If succeeded...
                if (succeeded)
                {
                    // For each courses...
                    foreach (var course in courses)
                    {
                        // Set the course item
                        var courseItem = new CourseApiModel
                        {
                            CourseId = course.Id,
                            CourseTitle = course.Title,
                            CourseUnit = course.Unit,
                            CourseCode = course.Code,
                            CourseDescription = course.Description
                        };

                        // Set result to a new instance
                        result = new List<CourseApiModel> { };

                        // Add to the result
                        result.Add(courseItem);
                    }
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
        /// Creates a lecturer with provided credentials
        /// </summary>
        /// <param name="model">The <see cref="CreateLecturerApiModel"/></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.CreateLecturer)]
        public async Task<ApiResponse> CreateLecturerAsync([FromBody] CreateLecturerApiModel model)
        {
            // Initialize error message
            string errorMessage = default;

            // If email was not provided...
            if (string.IsNullOrEmpty(model.Email))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Email cannot be null"
                };
            }

            // If first name was not provided...
            if (string.IsNullOrEmpty(model.FirstName))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "First name cannot be null"
                };
            }

            // If last name was not provided...
            if (string.IsNullOrEmpty(model.LastName))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Last name cannot be null"
                };
            }

            // If lecturer with specified email already exist...
            if (await _context.Lecturers.AnyAsync(lec => lec.Email == model.Email))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.Forbidden;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = "Email address already exist"
                };
            }

            // Initialize the result
            var result = default(LecturerApiModel);

            try
            {
                var lecturer = new LecturersDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Photo = model.Photo
                };

                // Create the lecturer
                await _context.Lecturers.AddAsync(lecturer);

                // Save changes
                var succeeded = await _context.SaveChangesAsync() > 0;

                // If succeeded...
                if (succeeded)
                {
                    // Set the result
                    result = new LecturerApiModel
                    {
                        Id = lecturer.Id,
                        Email = lecturer.Email,
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName,
                        Photo = lecturer.Photo
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
        /// Creates a collection of lecturers with given credentials
        /// </summary>
        /// <param name="model">The collection of lecturers</param>
        /// <returns>The <see cref="ApiResponse"/> of this transaction</returns>
        [HttpPost(ApiRoutes.CreateLecturers)]
        public async Task<ApiResponse> CreateLecturersAsync([FromBody] List<CreateLecturerApiModel> model)
        {
            // Initialize error message
            string errorMessage = default;

            try
            {
                // Initialize students
                var lecturers = new List<LecturersDataModel>();

                // For each student...
                foreach (var credentials in model)
                {
                    // If lecturer with specified email already exist...
                    if (await _context.Lecturers.AnyAsync(lec => lec.Email == credentials.Email))
                    {
                        // Set status code
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;

                        // Return error response
                        return new ApiResponse
                        {
                            ErrorMessage = $"Email address {credentials.Email}, already exist"
                        };
                    }

                    // Add to the list
                    lecturers.Add(new LecturersDataModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = credentials.Email,
                        FirstName = credentials.FirstName,
                        LastName = credentials.LastName,
                        Photo = credentials.Photo
                    });
                }

                // Create students
                await _context.Lecturers.AddRangeAsync(lecturers);

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return the result
            return new ApiResponse
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Creates collection of students with their credentials
        /// </summary>
        /// <param name="studentCredentials"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.CreateStudent)]
        public async Task<ApiResponse> CreateStudentAsync([FromBody] CreateStudentApiModel studentCredentials)
        {
            // Initialize error message
            string errorMessage = default;

            // If either email or matric number is duplicate...
            if (await _context.Students.AnyAsync(st => st.MatricNo == studentCredentials.MatricNo || st.Email == studentCredentials.Email))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return error response
                return new ApiResponse
                {
                    ErrorMessage = $"Email or matric number already exist for {studentCredentials.Email}"
                };
            }

            try
            {
                // Create student
                await _context.Students.AddRangeAsync(new StudentsDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = studentCredentials.Email,
                    FirstName = studentCredentials.FirstName,
                    LastName = studentCredentials.LastName,
                    MatricNo = studentCredentials.MatricNo,
                    Photo = studentCredentials.Photo
                });

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return the result
            return new ApiResponse
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Creates collection of students with their credentials
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.CreateStudents)]
        public async Task<ApiResponse> CreateStudentsAsync([FromBody] List<CreateStudentApiModel> model)
        {
            // Initialize error message
            string errorMessage = default;

            var newStudents = new List<CreateStudentApiModel> { };

            // Initialize warning
            var warningResult = new WarningResult
            {
                Warnings = new List<IssuesApiModel>
                {

                }
            };

            // For each student...
            foreach (var student in model)
            {
                // If either email or matric number is duplicate...
                if (await _context.Students.AnyAsync(st => st.MatricNo == student.MatricNo || st.Email == student.Email))
                {
                    // Create a warning
                    warningResult.Warnings.Add(new IssuesApiModel
                    {
                        Status = (int)HttpStatusCode.Forbidden,
                        Code = 11249,
                        Title = "Duplicate Email or Matric Number",
                        Detail = $"Email or matric number already exist for {student.Email}"
                    });

                    // Move to next iteration
                    continue;
                }
                // Otherwise...
                else
                {
                    // Add to the collection
                    newStudents.Add(student);
                }
            }

            // If the collection is empty
            if (!newStudents.Any())
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return the response
                return new ApiResponse
                {
                    WarningResult = warningResult,
                    ErrorMessage = "Unable to create any record because of a bad request"
                };
            }

            try
            {
                // Initialize students
                var students = new List<StudentsDataModel>();

                // For each student...
                foreach (var credentials in model)
                {
                    // Add to the list
                    students.Add(new StudentsDataModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = credentials.Email,
                        FirstName = credentials.FirstName,
                        LastName = credentials.LastName,
                        MatricNo = credentials.MatricNo,
                        Photo = credentials.Photo
                    });
                }

                // Create students
                await _context.Students.AddRangeAsync(students);

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return the result
            return new ApiResponse
            {
                WarningResult = warningResult,
                ErrorMessage = errorMessage
            };
        }


        [HttpPost(ApiRoutes.AssignLecturerToCourse)]
        public async Task<ApiResponse> AddLecturerToCourse([FromBody] AssignLecturerToCourseApiModel model)
        {
            // Initialize error message
            string errorMessage = default;

            try
            {
                // If this course and lecturer have been paired before...
                if (await _context.LecturerCourses.AnyAsync(lc => lc.LecturerId == model.LecturerId && lc.CourseId == model.CourseId))
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Return the response
                    return new ApiResponse
                    {
                        ErrorMessage = "The specified course was formerly assigned to the specified lecturer"
                    };
                }

                // Pair the lecturer and course
                await _context.LecturerCourses.AddAsync(new LecturerCoursesDataModel
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseId = model.CourseId,
                    LecturerId = model.LecturerId
                });

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Set the status code
                Response.StatusCode = (int )HttpStatusCode.InternalServerError;

                // Set the error message
                errorMessage = ex.Message;
            }

            // Return the result
            return new ApiResponse
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Fetches a specific course
        /// </summary>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchCourse)]
        public async Task<ApiResponse> FetchCourseAsync([FromQuery] string courseId)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(FetchCoursesApiModel);

            // If course id was not provided...
            if (string.IsNullOrEmpty(courseId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Course id is required"
                };
            }

            try
            {
                // Fetch the courses
                var course = await _context.Courses
                    .Select(course => new FetchCoursesApiModel
                    {
                        CourseId = course.Id,
                        CourseCode = course.Code,
                        CourseTitle = course.Title,
                        CourseUnit = course.Unit,
                        CourseDescription = course.Description,
                        DateCreated = course.DateCreated
                    })
                    .FirstOrDefaultAsync(course => course.CourseId == courseId);

                // If we found a match...
                if (course is not null)
                {
                    // Fetch the lecturer courses
                    var lecturerCourses = await _context.LecturerCourses
                        .Include(lc => lc.Lecturer)
                        .Where(lc => lc.CourseId == course.CourseId)
                        .ToListAsync();

                    // Fetch the lecturers
                    var lecturers = lecturerCourses
                        .Select(lc => new LecturerApiModel
                        {
                            Id = lc.Lecturer.Id,
                            FirstName = lc.Lecturer.FirstName,
                            LastName = lc.Lecturer.LastName,
                            Email = lc.Lecturer.Email,
                            Photo = lc.Lecturer.Photo
                        })
                        .ToList();

                    // Set the lecturers
                    course.Lecturers = lecturers;

                    // Set the result
                    result = course;
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
        /// Fetches all courses
        /// </summary>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchCourses)]
        public async Task<ApiResponse> FetchCoursesAsync()
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(List<FetchCoursesApiModel>);

            try
            {
                // Fetch the courses
                var courses = await _context.Courses
                    .Select(course => new FetchCoursesApiModel
                    {
                        CourseId = course.Id,
                        CourseCode = course.Code,
                        CourseTitle = course.Title,
                        CourseUnit = course.Unit,
                        CourseDescription = course.Description,
                        DateCreated = course.DateCreated
                    })
                    .ToListAsync();

                // If we have courses...
                if (courses.Any())
                {
                    // For each courses...
                    foreach (var course in courses)
                    {
                        // Match the lecturer courses
                        var lecturerCourses = await _context.LecturerCourses
                            .Include(lc => lc.Lecturer)
                            .Where(lc => lc.CourseId == course.CourseId)
                            .ToListAsync();

                        // Fetch the lecturers
                        var lecturers = lecturerCourses
                            .Select(lc => new LecturerApiModel
                            {
                                Id = lc.Lecturer.Id,
                                FirstName = lc.Lecturer.FirstName,
                                LastName = lc.Lecturer.LastName,
                                Email = lc.Lecturer.Email,
                                Photo = lc.Lecturer.Photo
                            })
                            .ToList();

                        // Set the lecturers
                        course.Lecturers = lecturers;
                    }

                    // Set the result
                    result = courses;
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
        /// Fetches the specified lecturer
        /// </summary>
        /// <returns>The <see cref="ApiResponse"/> for this transaction</returns>
        [HttpGet(ApiRoutes.FetchLecturer)]
        public async Task<ApiResponse> FetchLecturerAsync([FromQuery] string lecturerId)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(FetchLecturerApiModel);

            // If lecturer id was not specified...
            if (string.IsNullOrEmpty(lecturerId))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse
                {
                    ErrorMessage = "Lecturer id is required"
                };
            }

            try
            {
                // Fetch the lecturers
                var lecturer = await _context.Lecturers
                    .Select(lecturer => new FetchLecturerApiModel
                    {
                        Id = lecturer.Id,
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName,
                        Email = lecturer.Email,
                        Photo = lecturer.Photo
                    })
                    .FirstOrDefaultAsync(lecturer => lecturer.Id == lecturerId);

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
        /// Fetches lecturers and thier courses
        /// </summary>
        /// <returns></returns>
        [HttpGet(ApiRoutes.FetchLecturers)]
        public async Task<ApiResponse> FetchLecturersAsync()
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize result
            var result = default(List<FetchLecturerApiModel>);

            try
            {
                // Fetch the lecturers
                var lecturers = await _context.Lecturers
                    .Select(lecturer => new FetchLecturerApiModel
                    {
                        Id = lecturer.Id,
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName,
                        Email = lecturer.Email,
                        Photo = lecturer.Photo
                    })
                    .ToListAsync();

                // If we have lecturers...
                if (lecturers.Any())
                {
                    // For each lecturer...
                    foreach (var lecturer in lecturers)
                    {
                        // Match the lecturer courses
                        var lecturerCourses = await _context.LecturerCourses
                            .Include(lc => lc.Course)
                            .Where(lc => lc.LecturerId == lecturer.Id)
                            .ToListAsync();

                        // If we don't find a match...
                        if (!lecturerCourses.Any())
                            continue;

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
                        lecturer.Courses = courses;
                    }

                    // Set the result
                    result = lecturers;
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
        /// Registers a student course
        /// </summary>
        /// <param name="model">The api model</param>
        /// <returns>The <see cref="ApiResponse{TResult, TWarningResult, TErrorResult}"/> for this transaction</returns>
        [HttpPost(ApiRoutes.RegisterStudentCourse)]
        public async Task<ApiResponse<object, WarningResult, ErrorResult>> RegisterStudentCourses([FromBody] RegisterStudentCoursesApiModel model)
        {
            // Initialize error message
            string errorMessage = default;

            // Initialize warning result
            var warningResult = new WarningResult
            {
                Warnings = new List<IssuesApiModel> { }
            };

            // If student id was not specified...
            if (string.IsNullOrEmpty(model.StudentId))
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse<object, WarningResult, ErrorResult>
                {
                    ErrorMessage = "Student id is required"
                };
            }

            // If we don't have at least one course specified...
            if (model.CourseIds.Length < 1)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse<object, WarningResult, ErrorResult>
                {
                    ErrorMessage = "A minimum of one course is required."
                };
            }

            // If we have any null or empty id...
            if (model.CourseIds.Any(c => string.IsNullOrWhiteSpace(c)))
            {
                // Set the status code
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Return response
                return new ApiResponse<object, WarningResult, ErrorResult>
                {
                    ErrorMessage = "Not all course id are valid"
                };
            }

            try
            {
                // Fetch a student
                var student = await _context.Students.FirstOrDefaultAsync(st => st.Id == model.StudentId);

                // If student was not found...
                if (student is null)
                {
                    // Set status code
                    Response.StatusCode = (int)HttpStatusCode.NotFound;

                    // Return the response
                    return new ApiResponse<object, WarningResult, ErrorResult>
                    {
                        ErrorMessage = $"Student with id: {model.StudentId} could be not found"
                    };
                }

                // For each course...
                foreach (var id in model.CourseIds)
                {
                    // Fetch the course
                    var course = await _context.Courses
                        .FirstOrDefaultAsync(course => course.Id == id);

                    // If course was not found...
                    if (course is null)
                    {
                        // Add to warning result
                        warningResult.Warnings.Add(new IssuesApiModel
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Code = 11745,
                            Title = $"Course Not Found",
                            Detail = $"Course with id: {id}, is not part of the available courses."
                        });

                        // Skip this iteration
                        continue;
                    }

                    // If this course has been registered before...
                    if (await _context.StudentCourses.AnyAsync(st => st.StudentId == student.Id && st.CourseId == id))
                    {
                        // Add warning...
                        warningResult.Warnings.Add(new IssuesApiModel
                        {
                            Status = (int)HttpStatusCode.Ambiguous,
                            Title = "Already Registered",
                            Code = 17405,
                            Detail = $"The course with id: {id} has been formerly registered for student with id: {student.Id}"
                        });

                        // Skip this iteration
                        continue;
                    }

                    // Register the student's course
                    await _context.StudentCourses.AddAsync(new StudentCoursesDataModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        StudentId = model.StudentId,
                        CourseId = id
                    });
                }

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

            // Return the response
            return new ApiResponse<object, WarningResult, ErrorResult>
            {
                WarningResult = warningResult,
                ErrorMessage = errorMessage
            };
        }

        #region Insights

        /// <summary>
        /// Retrieves the attendance record of students in higher ranking order
        /// </summary>
        /// <returns>TThe <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.RetrieveStudentsAttendanceRanking)]
        public async Task<ApiResponse> RetrieveStudentsAttendanceRankingAsync()
        {
            // Set the error message
            string errorMessage = default;

            // Initialize result
            var result = new List<StudentAttendanceRecordApiModel> { };

            try
            {
                // Initialize result
                var records = new List<StudentAttendanceRecordApiModel> { };

                // Fetch attendees
                var attendees = await _context.Attendees.ToListAsync();

                // Fetch students
                var students = await _context.Students.ToListAsync();

                foreach (var student in students)
                {
                    records.Add(new StudentAttendanceRecordApiModel
                    {
                        Email = student.Email,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        MatricNo = student.MatricNo,
                        AttendanceCount = attendees.Count(at => at.StudentId == student.Id)
                    });
                }

                // Set the result
                result = records.OrderByDescending(st => st.AttendanceCount).ToList();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Log the error
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
        /// Retrieves the attendance record of courses in higher ranking order
        /// </summary>
        /// <returns>TThe <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.RetrieveCoursesAttendanceRanking)]
        public async Task<ApiResponse> RetrieveCoursesAttendanceRankingAsync()
        {
            // Set the error message
            string errorMessage = default;

            // Initialize result
            var result = new List<CourseAttendanceRecordApiModel> { };

            try
            {
                // Initialize result
                var records = new List<CourseAttendanceRecordApiModel> { };

                // Fetch courses
                var courses = await _context.Courses
                    .Include(course => course.Lectures)
                    .ThenInclude(lec => lec.Attendees)
                    .ToListAsync();

                foreach (var course in courses)
                {
                    int aCount = 0;

                    foreach (var lecture in course.Lectures)
                    {
                        aCount += lecture.Attendees.Count(at => at.LectureId == lecture.Id);
                    }

                    records.Add(new CourseAttendanceRecordApiModel
                    {
                        CourseTitle = course.Title,
                        CourseCode = course.Code,
                        CourseUnit = course.Unit,
                        Lectures = course.Lectures.Select(lec => new LectureAttendanceRecordApiModel
                        {
                            LectureTitle = lec.Title,
                            AttendanceCount = lec.Attendees.Count(at => at.LectureId == lec.Id)
                        }),
                        AttendanceCount = aCount
                    });
                }

                // Set the result
                result = records.OrderByDescending(st => st.AttendanceCount).ToList();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Log the error
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
        /// Retrieves the attendance record of courses in higher ranking order
        /// </summary>
        /// <returns>TThe <see cref="ApiResponse"/> for this request</returns>
        [HttpGet(ApiRoutes.RetrieveLecturersAttendanceRanking)]
        public async Task<ApiResponse> RetrieveLecturerAttendanceRankingAsync()
        {
            // Set the error message
            string errorMessage = default;

            // Initialize result
            var result = new List<LecturerAttendanceRecordApiModel> { };

            try
            {
                // Initialize result
                var records = new List<LecturerAttendanceRecordApiModel> { };

                // Fetch lecturers
                var lecturers = await _context.Lecturers
                    .Include(course => course.Lectures)
                    .ThenInclude(lec => lec.Attendees)
                    .ToListAsync();

                foreach (var lecturer in lecturers)
                {
                    int attendanceCount = 0;

                    foreach (var lecture in lecturer.Lectures)
                    {
                        attendanceCount += lecture.Attendees.Count(at => at.LectureId == lecture.Id);
                    }

                    records.Add(new LecturerAttendanceRecordApiModel
                    {
                        FirstName = lecturer.FirstName,
                        LastName = lecturer.LastName,
                        LecturerEmail = lecturer.Email,
                        Lectures = lecturer.Lectures.Select(lec => new LectureAttendanceRecordApiModel
                        {
                            LectureTitle = lec.Title,
                            AttendanceCount = lec.Attendees.Count(at => at.LectureId == lec.Id)
                        }),
                        AttendanceCount = attendanceCount
                    });
                }

                // Set the result
                result = records.OrderByDescending(st => st.AttendanceCount).ToList();
            }
            catch (Exception ex)
            {
                // Set status code
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Log the error
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

        #endregion
    }
}

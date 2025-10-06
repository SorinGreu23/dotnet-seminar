using ConsoleApp1;

// Ex. 1
CloneStudents();

// Ex. 2
CreateAndDisplayInstructor();

// Ex. 3
AddStudents();

// Ex. 4
PrintObjectInfo(new Student(1, "Andy", 22, [new Course("Introduction to .NET", 3)]));
PrintObjectInfo(new Course("Advanced Programming", 6));
PrintObjectInfo("Hello, World!");

// Ex. 5
FilterCourses();

#region Helper Methods

void FilterCourses()
{
    var courses = new List<Course>
    {
        new Course("Math", 5),
        new Course("Software Engineering", 6),
        new Course("Functional Programming", 2),
        new Course("English", 3),
        new Course("Databases", 4)
    };

    Func<Course, bool> filter = c => c.Credits > 3;
    var filteredCourses = courses.Where(filter).ToList();

    Console.WriteLine("Courses with more than 3 credits:");
    foreach (var c in filteredCourses)
    {
        Console.WriteLine($"- {c.Title} ({c.Credits} credits)");
    }
}

void PrintObjectInfo(object? obj)
{
    switch (obj)
    {
        case null:
            Console.WriteLine("Null object.");
            break;
        case Student s:
            Console.WriteLine($"Student: {s.Name} (Courses no: {s.Courses.Count})");
            break;
        case Course c:
            Console.WriteLine($"Course: {c.Title} ({c.Credits} credits)");
            break;
        default:
        {
            if (obj is not Student && obj is not Course)
            {
                Console.WriteLine("Unknown type.");
            }

            break;
        }
    }
}

void CreateAndDisplayInstructor()
{
    var instructor = new Instructor{ Name = "Andrei", Department = "Computer Science", Email = "andrei@info.uaic.ro" };
    Console.WriteLine(instructor);
}

void AddStudents()
{
    var students = new List<Student>();
    var nextId = 1;
    while (true)
    {
        Console.Write($"Nume #{nextId}: ");
        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
            break;
    
        var newStudent = new Student(nextId++, name,20, []);
        students.Add(newStudent);

        Console.WriteLine($"Added: {newStudent.Name}");
    }

    Console.WriteLine("\n--- Complete students list: ---");
    if (!students.Any())
    {
        Console.WriteLine("No students added.");
    }
    else
    {
        foreach (var s in students)
        {
            Console.WriteLine($"#{s.Id}: {s.Name}");
        }
    }
}

static void CloneStudents()
{
    var student = new Student(1, "Sorin", 22, [new Course("Introduction to .NET", 3)]);
    var studentClone = student with
    {
        Name = "Iulian",
        Courses = student.Courses.Append(new Course("TPM", 4)).ToList()
    };

    PrintStudentInfo(student);
    PrintStudentInfo(studentClone);
}

static void PrintStudentInfo(Student s)
{
    Console.WriteLine($"Student {s.Id}: {s.Name}, {s.Age} ani");
    Console.WriteLine("Courses: " + string.Join(", ", s.Courses.Select(c => c.Title)));
}

#endregion Helper Methods

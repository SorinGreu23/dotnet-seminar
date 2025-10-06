namespace ConsoleApp1;

public class Instructor
{
    public string Name { get; init;  }
    public string Department { get; init; }
    public string Email { get; init; }

    public override string ToString()
    {
        return $"Instructor: {Name}, Department: {Department}, Email: {Email}";
    }
}
// BreakPoint: to add break point goto the line where you want to start, press F9
// step over: F10, can also drag, step into F11
// Watch Window: Debug -> Windows -> Watch -> Watch 1
// Call stack: Debug -> Windows -> call stack (Ctrl + D, ctrl + C)
// Autos: Debug -> Windows -> Autos (Ctrl + D, ctrl + A)
// Locals: Debug -> Windows -> Locals (Ctrl + D, ctrl + L)
namespace _01_10_Debugging_Applications
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Debugging01.run();
            Debugging02.run();
            CallStack01.run();
        }
    }
}

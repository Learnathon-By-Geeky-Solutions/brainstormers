using System;
using Fundamentals;

class Program
{
    static void Main(string[] args)
    {
        var john = new Person();
        john.FirstName = "John";
        john.LastName = "Smith";
        john.Intro();

        var max = new Person("Max");
        max.LastName = "Smith";
        max.Intro();

        var order = new Order();
        var customer = new Customer("Max");
        customer.Orders.Add(order);
        customer.Intro();

        // UpCasting
        Circle circle = new Circle();
        Shape shape = circle;

        circle.width = 200;
        shape.width = 100;
        Console.WriteLine(circle.width);

        // DownCasting
        //Circle circle1 = (Circle)shape;
        Circle circle1 = shape as Circle;
        if(circle1 != null){
            circle1.Draw();
        }

        //Boxing
        int number = 10;
        object obj = number;

        //UnBoxing
        number = (int) obj;

        var shapes = new List<Shape1>();
        shapes.Add(new Shape1 { Width = 100, Height = 100});
        shapes.Add(new Circle1());
        shapes.Add(new Rectangle());
        shapes.Add(new Triangle());

        var canvas = new Canvas();
        canvas.DrawShapes(shapes);
        
        //Extensibility Interface
        var dbMigrator = new DbMigrator(new ConsoleLogger());
        dbMigrator.Migrate();

        dbMigrator = new DbMigrator(
            new FileLogger("Log\\log.txt"));
        dbMigrator.Migrate();

        //Interface & Polymorphism
        var encoder = new VideoEncoder();
        encoder.RegisterNotificationChannel(new MailNotificationChannel());
        encoder.RegisterNotificationChannel(new SmsNotificationChannel());
        encoder.Encode(new Video());

    }
}

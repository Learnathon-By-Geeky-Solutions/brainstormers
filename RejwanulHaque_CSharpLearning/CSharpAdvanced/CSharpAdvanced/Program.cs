using System;
using System.Collections.Generic;
using CSharpAdvanced.Delegates;
using CSharpAdvanced.Events;
using CSharpAdvanced.EventsWithParameters;
using CSharpAdvanced.ExceptionHandling;
using CSharpAdvanced.Generics;
using CSharpAdvanced.LambdaExpressions;
using CSharpAdvanced.LINQ;


namespace CSharpAdvanced
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Generics: ");
            GenericsDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Delegates: ");
            DelegatesDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("LambdaExpressions: ");
            LambdaExpressionsDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Events: ");
            EventsDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Events with parameter: ");
            EventsDemoWithParameter.Learn();
            Console.WriteLine();

            Console.WriteLine("LINQ: ");
            LINQDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("ExceptionHandling: ");
            ExceptionHandlingDemo.Learn();
            Console.WriteLine();


        }
    }
}
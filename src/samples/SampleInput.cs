using System;

namespace SampleCode
{
    public class Calculator
    {
        // Method with single parameter - should be modified
        public int Square(int number)
        {
            return number * number;
        }

        // Method with single parameter - should be modified
        public string FormatText(string value)
        {
            return value.ToUpper();
        }

        // Method with no parameters - should remain unchanged
        public void Reset()
        {
            Console.WriteLine("Calculator reset");
        }

        // Method with multiple parameters - should remain unchanged
        public int Add(int a, int b)
        {
            return a + b;
        }

        // Method with single parameter using common naming pattern - should get semantic suggestion
        public bool ValidateInput(string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        // Method with single parameter ending in number - should increment number
        public void ProcessItem1(object item1)
        {
            Console.WriteLine($"Processing: {item1}");
        }

        // Method with single parameter using "is" prefix - should get semantic suggestion
        public void CheckStatus(bool isActive)
        {
            if (isActive)
                Console.WriteLine("Status is active");
        }
    }

    public class DataProcessor
    {
        // Static method with single parameter - should be modified
        public static void ProcessData(byte[] data)
        {
            Console.WriteLine($"Processing {data.Length} bytes");
        }

        // Generic method with single parameter - should be modified
        public T Transform<T>(T item)
            where T : class
        {
            return item;
        }

        // Method with single parameter having modifiers - should be modified
        public void UpdateReference(ref int count)
        {
            count++;
        }
    }
}

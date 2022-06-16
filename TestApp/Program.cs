using System;
using System.Threading.Tasks;
using CSVFileReader;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ICsvFileReader csvFileReader = new CsvFileReader();

            Task<ICsvFile> task = csvFileReader.ReadFileAsync(System.IO.Directory.GetCurrentDirectory() + @"\cities.csv");
            task.Start();
            ICsvFile file = task.Result;
            Console.WriteLine("Total Lines: " + file.RowsCount.ToString());
            var testEnum = file.GetEnumerator();
            while (testEnum.MoveNext())
            {
                string cityName = testEnum.Current["city"];
                Console.WriteLine(cityName);
                var testColumnEnum = testEnum.Current.GetEnumerator();
                while (testColumnEnum.MoveNext())
                {
                    Console.WriteLine("Column Name:" + testColumnEnum.Current.column + " Value:" + testColumnEnum.Current.value);
                }
            }
            Console.ReadLine();
        }
    }
}
